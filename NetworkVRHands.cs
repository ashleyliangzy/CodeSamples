using UnityEngine;
using UnityEngine.Networking;
using Valve.VR;
using System.Collections;
using System.Collections.Generic;

public enum HandSide
{
    Left,
    Right
}

public class NetworkVRHands : NetworkBehaviour
{

    public float speedFac;

    [SyncVar]
    public HandSide side;

    [SyncVar]
    public NetworkInstanceId ownerId;
    public int nid;

    private InteractableObject touchedObject;
    private InteractableObject objectInUse;
    private bool hasTriggerBeenPressedThisFrame;
    private bool hasGripBeenPressedThisFrame;
    private bool hasTriggerBeenReleasedThisFrame;
    private bool hasGripBeenReleasedThisFrame;
    private bool isTriggerPressed;
    private bool isGripPressed;

    private InteractableObject grabbedObject;

    private GameObject trackedController;

    private SteamVR_Controller.Device steamDevice;

    private VRPlayerController localPlayer;
    private Vector3 currentVelocity;

    public GameObject ballPrefab1;
    public GameObject ballPrefab2;

    public bool withWeapon;

    public float coolDown;

    private float cdTime = 0f;
    private int counter;
    
    
    public GameObject ballInHand;

    [SyncVar]
    Vector3 vel;

    private bool onRumble;

    GameObject ballClone;
    [SerializeField] int totalNum = 10;
    Vector3[] speeds;
    Vector3[] speeds2;
    Vector3 totalSpeed;
    Vector3 totalSpeed2;
    int k = 0;
    Vector3 prePosition;

    public Coroutine currentCor;
    public bool gameStart;

   

    public HealthManager hm;
    

    void Start()
    {
        touchedObject = null;


        speeds = new Vector3[totalNum];

        prePosition = Vector3.zero;
        totalSpeed = Vector3.zero;
        gameStart = false;
        hm = FindObjectOfType<HealthManager>();
        
    }

    public override void OnStartAuthority()
    {
        // attach the controller model to the tracked controller object on the local client

        if (hasAuthority)
        {
            trackedController = GameObject.Find(string.Format("Controller ({0})", side.ToString("G").ToLowerInvariant()));

            Helper.AttachAtGrip(trackedController.transform, transform);

            localPlayer = ClientScene.FindLocalObject(ownerId).GetComponent<VRPlayerController>();

            steamDevice = SteamVR_Controller.Input((int)trackedController.GetComponent<SteamVR_TrackedObject>().index);



        }
    }

    private void Update()
    {

        if (gameStart == false) return;
        
        if (hm.player1.health < 0.1f || hm.player2.health < 0.1f) {
            gameStart = false;
                      

        }

        if (!isClient)
			return;
        if (!hasAuthority) return;
        
        //if (isServer) return;


        if (side == HandSide.Left)
        {
            checkLeft();
        }
        else if (side == HandSide.Right)
        {
            
            checkRight();
            if (k < totalNum)
            {
                speeds[k] = (transform.GetChild(3).position - prePosition) / Time.deltaTime;
                prePosition = transform.GetChild(3).position;
                //speeds2[k] = steamDevice.velocity;
                k++;
            }
            else
            {
                totalSpeed = Vector3.zero;
                totalSpeed2 = Vector3.zero;
                for (int i = 0; i < totalNum - 1; i++)
                {
                    speeds[i] = speeds[i + 1];
                    totalSpeed += speeds[i];
                    //speeds2[i] = speeds2[i+1];
                    //totalSpeed2 += speeds2[i];
                }
                speeds[totalNum - 1] = (transform.GetChild(3).position - prePosition) / Time.deltaTime;
                prePosition = transform.GetChild(3).position;
                totalSpeed += speeds[totalNum - 1];
                //totalSpeed2 += speeds2[totalNum -1];

            }


        }


    }



    [Command]
    private void CmdShieldOn(){
        RpcShieldOn();
    }
    [ClientRpc]    
    private void RpcShieldOn(){
        transform.GetChild(2).gameObject.GetComponent<ShieldController>().ShieldOn();
    }

    [Command]
    private void CmdShieldOff(){
        RpcShieldOff();
    }
    [ClientRpc]    
    private void RpcShieldOff(){
        transform.GetChild(2).gameObject.GetComponent<ShieldController>().ShieldOff();
    }

    private void checkLeft()
    {

        if (steamDevice.GetHairTrigger() && transform.GetChild(2).gameObject.GetComponent<ShieldController>().shieldEnergy>0)
        {
            //Debug.Log(gameObject.name + " Trigger Press");

            //currentCor = StartCoroutine(RumbleControllerRoutine(1000));
            CmdShieldOn();
            transform.GetChild(2).gameObject.GetComponent<ShieldController>().ShieldOn();
            
            if (transform.GetChild(2).gameObject.GetComponent<ShieldController>().isShieldOpen == false) {
                RumbleTime(1000,0.01f);
            }
        }
        else {
            
          
            //StopCoroutine(currentCor);
            CmdShieldOff();
            //transform.GetChild(2).gameObject.GetComponent<ShieldController>().ShieldOff();
        }

       

        

    }

    private void checkRight()
    {
        if (withWeapon == false)
        {
            if (cdTime <= 0f)
            {
                //add new ball
                cdTime = coolDown;
                withWeapon = true;
				CmdsetAct();
				RumbleTime(2000f, 0.1f);
                CmdCreateBall(transform.GetChild(3).position, localPlayer.gameObject.GetComponent<NetworkIdentity>());

            }
            else
            {
                cdTime = cdTime - Time.deltaTime;
				if (steamDevice.GetHairTriggerDown()) {
					//RumbleTime(4000f, 0.2f);
				}
            }
        }
        else
        {
            
            vel = steamDevice.velocity;
            if (steamDevice.GetHairTriggerUp())
            {
                //CmdreleaseBall(totalSpeed * 8f);
                if (ballInHand.GetComponent<ballController>().hasAuthority) {
                    ballInHand.GetComponent<ballController>().follow = false;
        

        
                   
                    Rigidbody rb = ballInHand.GetComponent<Rigidbody>();
                    rb.isKinematic = false;
                    Vector3 vv = totalSpeed / totalNum * speedFac;
                    if (vv.magnitude > 15) vv = 15 * Vector3.Normalize(vv);
                    rb.velocity = vv;
                    rb.angularVelocity = Vector3.zero;
                    ballInHand = null;
                    withWeapon = false;
                }
				//RumbleTime(4000, 0.1f);
				//Instantiate(ballPrefab,transform.GetChild(3).position, Quaternion.identity);
                counter = 0;
                

            }



            //get the velocity

            //Rigidbody rb = GetComponent<Rigidbody>();
            // if (vel.magnitude > 1.8 && vel.magnitude < 20) {
            // 	counter++;
            // }
            // else counter = 0;


        }
    }

	[Command]
	void CmdsetAct() {
		RpcsetAct();
	}
	[ClientRpc]
	void RpcsetAct() {
		transform.GetChild(3).gameObject.SetActive(true);
	}
    [Command]
    void CmdCreateBall(Vector3 pos, NetworkIdentity playernid) {
        GameObject ballInstance;
        if (ownerId.Value == 1)
        {
            ballInstance = Instantiate(ballPrefab1, pos, Quaternion.identity);
        }
        else
        {
            ballInstance = Instantiate(ballPrefab2, pos, Quaternion.identity);
        }
        
       
        
       
        print(ballInstance);
        ballInHand = ballInstance;
        
        ballInstance.GetComponent<ballController>().hand = gameObject;
        NetworkServer.SpawnWithClientAuthority(ballInstance, playernid.connectionToClient);
        RpcCreateBall(gameObject.GetComponent<NetworkIdentity>().netId,ballInstance.GetComponent<NetworkIdentity>().netId);
        ballInstance.GetComponent<ballController>().follow = true;
        ballInstance.GetComponent<ballController>().ownerId = ownerId;

    }
    [ClientRpc]
    void RpcCreateBall(NetworkInstanceId hid,NetworkInstanceId bid) {
        GameObject hh = ClientScene.FindLocalObject(hid); //hand
        GameObject bb = ClientScene.FindLocalObject(bid); //ball
        bb.GetComponent<ballController>().hand = hh;
        hh.GetComponent<NetworkVRHands>().ballInHand = bb;


    }
 
    // [Command]
    // void CmdreleaseBall(Vector3 velo)
    // {
    //     print("release");
        
        
    //     ballInHand.GetComponent<ballController>().follow = false;
        

        
    //     Rpcgivev(ballInHand.GetComponent<NetworkIdentity>().netId, velo);
    //     Rigidbody rb = ballInHand.GetComponent<Rigidbody>();
    //     rb.isKinematic = false;
    //     rb.velocity = velo * 5;
    //     ballInHand = null;      




    // }
    // [ClientRpc]
    // void Rpcgivev(NetworkInstanceId ballId, Vector3 vv)
    // {
    //     GameObject ball = ClientScene.FindLocalObject(ballId);
    //     ball.GetComponent<ballController>().ownerId = ownerId;
    //     Rigidbody rb = ball.GetComponent<Rigidbody>();
    //     rb.isKinematic = false;
    //     rb.velocity = vv * 5;
	// 	transform.GetChild(3).gameObject.SetActive(false);

    // }




    

    IEnumerator RumbleControllerRoutine(float strength)
    {
        strength = Mathf.Clamp01(strength);


        while (onRumble == true)
        {
            int valveStrength = Mathf.RoundToInt(Mathf.Lerp(0, 3999, strength));

            steamDevice.TriggerHapticPulse((ushort)valveStrength);

            yield return null;

        }
        yield return null;
    }

	public void RumbleTime(float strength, float rumbleTime)
    {
        StartCoroutine(RumbleTimeRoutine(strength,rumbleTime));

    }

    IEnumerator RumbleTimeRoutine(float strength, float rumbleTime)
    {
        strength = Mathf.Clamp01(strength);

		float startTime = Time.time;
        while (Time.time <= startTime + rumbleTime)
        {
            int valveStrength = Mathf.RoundToInt(Mathf.Lerp(0, 3999, strength));

            steamDevice.TriggerHapticPulse((ushort)valveStrength);

            yield return null;

        }
        
    }


 

}
