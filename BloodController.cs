using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class BloodController : MonoBehaviour {
	float preHealth = 1;
	[SerializeField] Animator animator;
	public float health = 1;
	float currentHealth =1;
	[SerializeField] Color startColor, endColor, middleColor;
	Color currentColor;
	[SerializeField] GameObject healthBar;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(preHealth!= health){
			float dist = Vector3.Distance(transform.position, Camera.main.gameObject.transform.position);
			Debug.Log(dist);
			// int i;
			// if (gameObject.name == "Blood1") {
			// 	i = 1;
			// }
			// else {
			// 	i = 2;
			// }
			if(dist < 4){
				HealthManager healthController = FindObjectOfType<HealthManager>();
				healthController.BloodEffect();
				if (health < 0.1f) {
					if(transform.name == "Blood1"){
						Debug.Log("end game 1");
						healthController.endGame(1);
						
					}else{
						Debug.Log("end game 2");
						healthController.endGame(2);
					}
				}
		
			}
			else {
				HealthManager healthController = FindObjectOfType<HealthManager>();
				healthController.AddCount();
				if(transform.name == "Blood1"){
					Debug.Log("trigger text message1");
					StartCoroutine(healthController.TriggerTextMessage(1));
					
				}else{
					Debug.Log("trigger text message2");
					StartCoroutine(healthController.TriggerTextMessage(2));
				}
				
			}
		}
		health = Mathf.Clamp(health, 0, 1f);
		currentHealth += (health - currentHealth) * 0.1f;
		animator.SetFloat ("bloodBar", currentHealth);
		if(health >0.5f){
			currentColor = Color.Lerp(middleColor, startColor, (health-0.5f)*2f);
		}else{
			currentColor = Color.Lerp( endColor,middleColor, health*2f);
		}
		
	    healthBar.GetComponent<Renderer>().material.SetColor("_Color",currentColor);
		preHealth = health;
	}


	public void healthEffect(int i) {
		GameObject cam = GameObject.Find("[CameraRig](Clone)");
		GameObject healthController = GameObject.Find("healthbarController");
		if (i == 1) {
			if ((int)cam.transform.GetChild(2).GetChild(3).gameObject.GetComponent<NetworkIdentity>().netId.Value == 1) {
				Debug.Log("client hit in host");
				StartCoroutine(healthController.GetComponent<HealthManager>().TriggerTextMessage(i));
			}
			else {
				Debug.Log("client hit in client");
				if (GetComponent<BloodController>().health < 0.1f) {
					healthController.GetComponent<HealthManager>().endGame(i);
				}
				healthController.GetComponent<HealthManager>().BloodEffect();
			}
		}
		else {
			if ((int)cam.transform.GetChild(2).GetChild(3).gameObject.GetComponent<NetworkIdentity>().netId.Value != 1) {
				Debug.Log("host hit in client");
				StartCoroutine(healthController.GetComponent<HealthManager>().TriggerTextMessage(i));
			}
			else {
				Debug.Log("host hit in host");
				if (GetComponent<BloodController>().health < 0.1f) {
					healthController.GetComponent<HealthManager>().endGame(i);
				}
				healthController.GetComponent<HealthManager>().BloodEffect();
			}
		}
		
	}
}
