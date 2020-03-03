using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using Valve.VR;

public class VRPlayerController : NetworkBehaviour
{
	
	public GameObject vrCameraRig;
	public GameObject leftHandPrefab;
    public GameObject rightHandPrefab;
    private GameObject vrCameraRigInstance;
	private int nid;
	//public GameObject steamVRPrefab;
	//private GameObject steamVRInstance;
	
	private GameObject bodyInstance;
	[SyncVar]
	public int hitCount;
	public GameObject hitSheildParticle;
	
	

	public override void OnStartLocalPlayer ()
	{
		if (!isClient)
			return;
		// delete main camera
		try {
			DestroyImmediate (Camera.main.gameObject);
		}
		catch {
			Debug.Log("No main camera");
		}

		// create camera rig and attach player model to it
		vrCameraRigInstance = (GameObject)Instantiate (
			vrCameraRig,
			transform.position,
			transform.rotation);

		// steamVRInstance = (GameObject)Instantiate (
		// 	steamVRPrefab,
		// 	transform.position,
		// 	transform.rotation);
		nid = (int)GetComponent<NetworkIdentity>().netId.Value;

		if (nid == 1) {
			GameObject pos = GameObject.Find("Player1Pos");
			vrCameraRigInstance.transform.position = pos.transform.position;
			vrCameraRigInstance.transform.eulerAngles = new Vector3(0f, 90f, 0f);
		}
		else {
            GameObject pos = GameObject.Find("Player2Pos");
            vrCameraRigInstance.transform.position = pos.transform.position;
            vrCameraRigInstance.transform.eulerAngles = new Vector3(0f, -90f, 0f);
        }
		

		Transform bodyOfVrPlayer = transform.Find ("Player");
		//bodyOfVrPlayer.eulerAngles = new Vector3(0,-90,0);
		
		if (bodyOfVrPlayer != null)
			bodyOfVrPlayer.parent = null;
		
		bodyInstance = transform.GetChild(3).gameObject;
		
		
		
		//bodyInstance.transform.parent = null;
		
		

		GameObject head = vrCameraRigInstance.GetComponentInChildren<SteamVR_Camera> ().gameObject;
		transform.parent = head.transform;
		
        transform.localPosition = new Vector3(0.029f, -0.231f, -0.186f);
		transform.localEulerAngles = new Vector3(0,0,0);

		

		TryDetectControllers ();
	}

	[Command]
	private void CmdGiveBody(NetworkInstanceId nn,Vector3 vv, float ang) {
		RpcGiveBody(nn, vv, ang);
	}
	[ClientRpc]
	private void RpcGiveBody(NetworkInstanceId nn, Vector3 vv, float ang) {
		GameObject body = ClientScene.FindLocalObject(nn);
		body.transform.GetChild(3).eulerAngles = new Vector3(0, ang, 0);
		body.transform.GetChild(3).position = vv + new Vector3(0f, -2.6f, 0f);
	}

	
	

	private void Update() {

		if (!isClient)
			return;
		if (bodyInstance != null) {
			if (isServer) {
				RpcGiveBody(bodyInstance.transform.parent.gameObject.GetComponent<NetworkIdentity>().netId ,transform.position, transform.eulerAngles.y);
				bodyInstance.transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
				bodyInstance.transform.position = transform.position + new Vector3(0f, -2.6f, 0f);
			}
			else {
				CmdGiveBody(bodyInstance.transform.parent.gameObject.GetComponent<NetworkIdentity>().netId ,transform.position, transform.eulerAngles.y);
			}
			
		}
		
		
	}

	

	void TryDetectControllers ()
	{
		var controllers = vrCameraRigInstance.GetComponentsInChildren<SteamVR_TrackedObject> ();
        if (controllers != null && controllers.Length == 2 && controllers[1] != null && controllers[0] != null)
        {
			CmdSpawnHands(netId);
        }
        else
        {
            Invoke("TryDetectControllers", 2f);
        }
	}

	[Command]
	void CmdSpawnHands(NetworkInstanceId playerId)
	{
        // instantiate controllers
        // tell the server, to spawn two new networked controller model prefabs on all clients
        // give the local player authority over the newly created controller models
        GameObject leftHand = Instantiate(leftHandPrefab);
		GameObject rightHand = Instantiate(rightHandPrefab);
		// leftHand.GetComponent<handController>().side = 0;
		// rightHand.GetComponent<handController>().side = 1;
		// leftHand.GetComponent<handController>().ownerId = playerId;
		// rightHand.GetComponent<handController>().ownerId = playerId;
		

		var leftVRHand = leftHand.GetComponent<NetworkVRHands> ();
		var rightVRHand = rightHand.GetComponent<NetworkVRHands> ();

		leftVRHand.side = HandSide.Left;
		rightVRHand.side = HandSide.Right;
        leftVRHand.ownerId = playerId;
		rightVRHand.ownerId = playerId;

		NetworkServer.SpawnWithClientAuthority (leftHand, base.connectionToClient);
		NetworkServer.SpawnWithClientAuthority (rightHand, base.connectionToClient);
	}

	[Command]
	public void CmdGrab(NetworkInstanceId objectId, NetworkInstanceId controllerId)
	{
		var iObject = NetworkServer.FindLocalObject (objectId);
		var networkIdentity = iObject.GetComponent<NetworkIdentity> ();
        networkIdentity.AssignClientAuthority(connectionToClient);

        var interactableObject = iObject.GetComponent<InteractableObject>();
        interactableObject.RpcAttachToHand (controllerId);    // client-side
        var hand = NetworkServer.FindLocalObject(controllerId);
        interactableObject.AttachToHand(hand);    // server-side
    }

	[Command]
	public void CmdDrop(NetworkInstanceId objectId, Vector3 currentHolderVelocity)
	{
		var iObject = NetworkServer.FindLocalObject (objectId);
		var networkIdentity = iObject.GetComponent<NetworkIdentity> ();
        networkIdentity.RemoveClientAuthority(connectionToClient);
        
        var interactableObject = iObject.GetComponent<InteractableObject>();
        interactableObject.RpcDetachFromHand(currentHolderVelocity); // client-side
        interactableObject.DetachFromHand(currentHolderVelocity); // server-side
    }


	
	[Command]
	public void CmdSetVal(int i, float f) {
		
		RpcSetVal(i,f);
	}
	[ClientRpc]
	void RpcSetVal(int i, float f) {
		GameObject trigger = GameObject.Find("Trigger1");
		if (i == 1) {
			trigger.GetComponent<triggerController>().img1.fillAmount = f / 2;
			
		}
		else {
			trigger.GetComponent<triggerController>().img2.fillAmount = f / 2;
			
		}
	}

	[ClientRpc]
	public void RpcStartGame() {
		GameObject trigger = GameObject.Find("Trigger1");
		trigger.GetComponent<triggerController>().StartGame();

	}

	[Command]
	public void CmdShield(Vector3 p, Quaternion ang, NetworkInstanceId hid) {
		RpcShield(p, ang, hid);
	}

	[ClientRpc]
	private void RpcShield(Vector3 p, Quaternion ang, NetworkInstanceId hid) {
		//GameObject sh = ClientScene.FindLocalObject(hid).transform.GetChild(2).GetChild(2).gameObject;
		GameObject hitShieldParticleClone = Instantiate(hitSheildParticle, p, ang);
		
		
		Destroy(hitShieldParticleClone, 3f);
	}

	[Command]
	public void CmdRumble(NetworkInstanceId pid) {
		RpcRumble(pid);
	}

	[ClientRpc]
	private void RpcRumble(NetworkInstanceId pid) {
		if (netId != pid) {
			vrCameraRigInstance.transform.GetChild(0).gameObject.GetComponentInChildren<NetworkVRHands>().RumbleTime(1000,0.5f);
		}
	}


	[Command]
	public void Cmdhealth(int i) {
		if (i == 1) {
			GameObject s = GameObject.Find("Blood1");
			s.GetComponent<BloodController>().health = 0;
			

			

		}
		else {
			GameObject s = GameObject.Find("Blood2");
			s.GetComponent<BloodController>().health = 0;
			// s.GetComponent<BloodController>().healthEffect(2);
		}
	}
}
