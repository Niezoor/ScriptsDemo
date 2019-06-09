using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageNotify : MonoBehaviour {
	public Canvas MessageCanvas;
	public Text Message;
	public Animation AnimationComponent;
	public string ShowAnimationName;
	public string HideAnimationName;
	public Button Btn;
	[Tooltip("Delay in seconds")]
	public int NotifyAutoHideDelay = 3;

	private bool skipAnim = false;
	// Use this for initialization
	void Start () {
		MessageCanvas.enabled = false;
		skipAnim = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ShowNotifyMessage(string message) {
		Message.text = message;
		if (MessageCanvas.enabled) {
			skipAnim = true;
			AnimationComponent.Stop();
		}
		MessageCanvas.enabled = true;
		AnimationComponent.Play (ShowAnimationName);
		StartCoroutine(HideNotifyWithDelay ());
	}

	public void HideNotify() {
		MessageCanvas.enabled = false;
	}

	private IEnumerator HideNotifyWithDelay() {
		yield return new WaitForSeconds (NotifyAutoHideDelay);
		if (!skipAnim) {
			AnimationComponent.Play (HideAnimationName);
		}
		do {
			yield return new WaitForSeconds (0.1f);
			if (skipAnim) {
				break;
			}
		} while (AnimationComponent.isPlaying);
		if (skipAnim) {
			skipAnim = false;
		} else {
			MessageCanvas.enabled = false;
		}
	}
}
