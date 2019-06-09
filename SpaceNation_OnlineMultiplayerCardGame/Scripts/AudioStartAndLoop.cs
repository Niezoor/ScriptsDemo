using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioStartAndLoop : MonoBehaviour {
	public AudioClip engineStartClip;
	public AudioClip engineLoopClip;
	public AudioSource AudioManager;
	private Coroutine PlayTask;

	void Start()
	{
		AudioManager = GetComponent<AudioSource> ();
		PlayMusic ();
	}

	public void PlayMusic() {
		PlayTask = StartCoroutine(playEngineSound());
	}

	public void StopMusic() {
		if (PlayTask != null) {
			StopCoroutine (PlayTask);
		}
		AudioManager.Stop ();
		AudioManager.loop = false;
	}

	IEnumerator playEngineSound()
	{
		AudioManager.clip = engineStartClip;
		AudioManager.loop = false;
		AudioManager.Play();
		yield return new WaitForSeconds(AudioManager.clip.length);
		AudioManager.clip = engineLoopClip;
		AudioManager.loop = true;
		AudioManager.Play();
	}
}
