using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSound : MonoBehaviour {
	public AudioClip sound;
	private Button btn { get { return GetComponent<Button> (); } }
	private AudioSource source { get { return GetComponent<AudioSource> (); } }
	// Use this for initialization
	void Start () {
		gameObject.AddComponent<AudioSource> ();
		source.clip = sound;
		source.playOnAwake = false;
		btn.onClick.AddListener(() =>Playsound());
	}

	void Playsound () {
		if (sound != null) {
			source.PlayOneShot (sound);
		}
	}
}
