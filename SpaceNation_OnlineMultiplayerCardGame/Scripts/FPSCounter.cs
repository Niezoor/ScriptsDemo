using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour {
	public bool EnableFPSCounter = false;

	// Use this for initialization
	void Start () {
		if (EnableFPSCounter) {
			StartFPSCounter ();
		}
	}
	
	private void StartFPSCounter() {
		EnableFPSCounter = true;
		StartCoroutine (FPSCounterTask ());
	}

	private IEnumerator FPSCounterTask() {
		Text OutputText  = GetComponent<Text> ();;
		while (EnableFPSCounter) {
			int fps = (int) (1 / Time.deltaTime);
			OutputText.text = fps.ToString ();
			yield return new WaitForSeconds (0.5f);
		}
		OutputText.text = "";
		yield return null;
	}
}
