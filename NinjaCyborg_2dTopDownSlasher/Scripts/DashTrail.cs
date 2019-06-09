using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashTrail : MonoBehaviour {
	public Transform[] imagesTransform;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Show(float angle) {
		this.gameObject.SetActive (true);
		Quaternion newRot = Quaternion.identity;
		newRot.eulerAngles = new Vector3 (0, 0, (-angle) + 90);
		for (int i = 0; i < imagesTransform.Length; i++) {
			imagesTransform [i].transform.localRotation = newRot;
		}
	}

	public void Hide() {
		this.gameObject.SetActive (false);
	}
}
