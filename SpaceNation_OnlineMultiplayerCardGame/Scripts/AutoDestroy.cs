using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroy : MonoBehaviour {
	public float inSeconds = 10;
	// Use this for initialization
	void Start () {
		Destroy(this.gameObject, inSeconds);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
