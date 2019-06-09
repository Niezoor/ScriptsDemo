using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepOriginalRotation : MonoBehaviour {
	public Vector3 OriginalRotaion = new Vector3(0,0,0);
	public bool KeepingActive = false;
	public bool KeepX = false;
	public bool KeepY = false;
	public bool KeepZ = false;

	// Use this for initialization
	void Start () {
		//OriginalRotaion = transform.rotation;
	}

	void Update() {
		if (KeepingActive) {
			Vector3 rotNow = transform.eulerAngles;
			if (KeepX) {
				rotNow.x = OriginalRotaion.x;
			}
			if (KeepY) {
				rotNow.y = OriginalRotaion.y;
			}
			if (KeepZ) {
				rotNow.z = 0;
			}
			rotNow.x = 20;
			transform.eulerAngles = rotNow;
			//transform.localRotation = rotNow;
		} else {
			//OriginalRotaion = transform.rotation;
		}
	}

	public void SetKeepingActive(bool active) {
		KeepingActive = active;
		//OriginalRotaion = transform.rotation;
	}
}
