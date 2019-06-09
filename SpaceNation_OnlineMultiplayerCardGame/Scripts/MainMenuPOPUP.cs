using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuPOPUP : MonoBehaviour {
	public Text MessageTextOutput;
	public Text YesButtonText;
	public Text NoButtonText;
	public Button YesButton;
	public Button NoButton;
	public Button BackgroundButton;

	public delegate void POPOUPEventDelegate ();

	private POPOUPEventDelegate YesEvent;
	private POPOUPEventDelegate NoEvent;
	private POPOUPEventDelegate HideEvent;
	private Canvas POPUPCanvas;

	// Use this for initialization
	void Start () {
		POPUPCanvas = GetComponent<Canvas> ();
		YesButton.onClick.RemoveAllListeners ();
		YesButton.onClick.AddListener (YesAction);
		YesButton.onClick.AddListener (HidePOPUP);
		NoButton.onClick.RemoveAllListeners ();
		NoButton.onClick.AddListener (NoAction);
		NoButton.onClick.AddListener (HidePOPUP);
		BackgroundButton.onClick.RemoveAllListeners ();
		BackgroundButton.onClick.AddListener (HideAction);
		BackgroundButton.onClick.AddListener (HidePOPUP);
	}

	// Update is called once per frame
	void Update () {
		
	}

	private void ShowPOPUP() {
		POPUPCanvas.enabled = true;
	}

	public void HidePOPUP() {
		POPUPCanvas.enabled = false;
		YesButtonText.text = "TAK";
		NoButtonText.text = "NIE";
	}

	public void SetupDialogPOPUPWindow(string message, POPOUPEventDelegate yesEvent, POPOUPEventDelegate noEvent) {
		MessageTextOutput.text = message;
		YesEvent = yesEvent;
		NoEvent = noEvent;
		HideEvent = null;
		ShowPOPUP ();
	}

	public void SetupDialogPOPUPWindow(string message, POPOUPEventDelegate yesEvent, POPOUPEventDelegate noEvent, POPOUPEventDelegate hideEvent) {
		MessageTextOutput.text = message;
		YesEvent = yesEvent;
		NoEvent = noEvent;
		HideEvent = hideEvent;
		ShowPOPUP ();
	}

	public void SetupDialogPOPUPWindow(string message, string yesText, string noText, POPOUPEventDelegate yesEvent, POPOUPEventDelegate noEvent) {
		YesButtonText.text = yesText;
		NoButtonText.text = noText;
		SetupDialogPOPUPWindow (message, yesEvent, noEvent);
	}

	public void SetupDialogPOPUPWindow(string message, string yesText, string noText, POPOUPEventDelegate yesEvent, POPOUPEventDelegate noEvent, POPOUPEventDelegate hideEvent) {
		YesButtonText.text = yesText;
		NoButtonText.text = noText;
		SetupDialogPOPUPWindow (message, yesEvent, noEvent);
		HideEvent = hideEvent;
	}

	private void YesAction() {
		if (YesEvent != null)
			YesEvent ();
	}

	private void NoAction() {
		if (NoEvent != null)
			NoEvent ();
	}

	private void HideAction() {
		if (HideEvent != null)
			HideEvent ();
	}
}
