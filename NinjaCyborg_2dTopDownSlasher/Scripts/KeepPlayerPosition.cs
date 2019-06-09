using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepPlayerPosition : MonoBehaviour {
	public Transform playerTransform;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void LateUpdate () {
		if (playerTransform != null) {
			this.transform.position = playerTransform.position;
		}
	}

	public void SetPlayerTransform(Transform trans) {
		playerTransform = trans;
	}
}
