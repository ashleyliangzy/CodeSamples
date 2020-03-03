using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Collections.ObjectModel;

using Valve.VR.InteractionSystem;


public class ThrowBallController : MonoBehaviour
{
    // how many frames are tracked
    [SerializeField] int totalNum = 20;
    [SerializeField] GameObject ballPrefab;
    bool hasBall;
    bool preToThrow;
    GameObject ballClone;
    Vector3[] speeds;
    Vector3 totalSpeed;
    int k = 0;
    Vector3 prePosition;
    private SteamVR_TrackedObject trackedObj;
    // 2
    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }
    private Hand hand;
    // Use this for initialization
    void Start()
    {
        // ballClone = Instantiate(ballPrefab, transform.position, transform.rotation);
        // gameObject.GetComponent<FixedJoint>().connectedBody = ballClone.GetComponent<Rigidbody>();
        hasBall = false;
        preToThrow = false;
        speeds = new Vector3[totalNum];
        prePosition = transform.position;
        totalSpeed = Vector3.zero;

        hand = GetComponent<Hand>();
    }

    // Update is called once per frame
    void Update()
    {
        //keep calculating the speed of ball throughout the last few frames
        if (k < totalNum)
        {
            speeds[k] = transform.position - prePosition;
            prePosition = transform.position;
            k++;
        }
        else
        {
            totalSpeed = Vector3.zero;
            for (int i = 0; i < totalNum - 1; i++)
            {
                speeds[i] = speeds[i + 1];
                totalSpeed += speeds[i];
            }
            speeds[totalNum - 1] = transform.position - prePosition;
            prePosition = transform.position;
            totalSpeed += speeds[totalNum - 1];

        }
        //Debug.Log(totalSpeed.magnitude);

        if (Controller.GetHairTriggerDown())
        {
			if(!hasBall){
			   CreateNewBall();
			}
           
            preToThrow = true;
            Debug.Log("trigger is pressed");
        }
		Debug.Log("hasBall: " + hasBall);

        if (Controller.GetHairTriggerUp())
        {
            if (hasBall && preToThrow)
            {
                ThrowBall();
            }
            Debug.Log("trigger is released");
        }


    }
    void CreateNewBall()
    {
        if (!hasBall)
        {
            ballClone = Instantiate(ballPrefab, transform.position, transform.rotation);
            gameObject.GetComponent<FixedJoint>().connectedBody = ballClone.GetComponent<Rigidbody>();
            hasBall = true;

        }
    }
    void ThrowBall()
    {
        Destroy(ballClone.GetComponent<FixedJoint>());
		ballClone.GetComponent<SphereCollider>().enabled = true;
        hasBall = false;
        preToThrow = false;
        ballClone.GetComponent<Rigidbody>().velocity = totalSpeed;
    }

}
