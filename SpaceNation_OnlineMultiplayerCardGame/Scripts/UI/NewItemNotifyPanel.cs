using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewItemNotifyPanel : MonoBehaviour {
	public Image ItemImage;
	public Text NotifyTitle;
	public Text NotifyDesc;
	public Animator AnimationController;
	public AudioClip SFX;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ShowNotify(Sprite image, string title, string desc, float delay) {
		if (image != null) {
			ItemImage.sprite = image;
		}
		if (title != null) {
			NotifyTitle.text = title;
		}
		if (desc != null) {
			NotifyDesc.text = desc;
		}
		StartCoroutine (ShowNotifyTask (delay));
	}

	private IEnumerator ShowNotifyTask(float delay) {
		yield return new WaitForSeconds (delay);
		AnimationController.SetTrigger ("Show");
	}

	public void PlaySoundFX() {
		GetComponent<AudioSource> ().PlayOneShot (SFX);
	}

	public void DestroyPanel() {
		Destroy (this.gameObject);
	}
}
