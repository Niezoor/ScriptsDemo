using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shuriken : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		//Quaternion newRot = transform.rotation;
		//newRot.z += 300 * Time.deltaTime;
		//transform.rotation = newRot;
		transform.Rotate (Vector3.forward * 1200 * Time.deltaTime, Space.World);
	}
}
