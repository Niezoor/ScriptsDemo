using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardPack : MonoBehaviour {
	public AudioClip PackDropSound;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void PlayDropSound() {
		GetComponent<AudioSource> ().PlayOneShot (PackDropSound);
	}
}
