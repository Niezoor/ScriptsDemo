using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class FriendLobby : MonoBehaviour {
	public Button ReadyButton;
	public Text PlayerReadySignal;
	public Text FriendReadySignal;

	public Color ReadyColor;
	public Color NotReadyColor;
	public Color TextReadyColor;
	public Color TextNotReadyColor;

	public MyNetworkManager NetworkManager;
	// Use this for initialization
	void Start () {
		PlayerReadySignal.color = TextNotReadyColor;
		FriendReadySignal.color = TextNotReadyColor;

		if (NetworkManager == null && GameObject.Find ("NetworkManager") != null) {
			NetworkManager = GameObject.Find ("NetworkManager").GetComponent<MyNetworkManager> ();
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	[PunRPC]
	public void ReceiveSignalReady(bool ready) {
		Debug.Log ("Receive signal ready " + ready);
		if (NetworkManager != null) {
			NetworkManager.ReceiveSignalReady (ready);
		}
	}

	public void ButtonReady() {
		NetworkManager.SignalFriendReady ();
	}

	public void SetPlayerReady(bool ready) {
		if (ready) {
			ReadyButton.image.color = NotReadyColor;
			PlayerReadySignal.color = TextReadyColor;
		} else {
			ReadyButton.image.color = ReadyColor;
			PlayerReadySignal.color = TextNotReadyColor;
		}
	}

	public void SetFriendReady(bool ready) {
		if (ready) {
			FriendReadySignal.color = TextReadyColor;
		} else {
			FriendReadySignal.color = TextNotReadyColor;
		}
	}
}
