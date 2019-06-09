using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnlinePlayersName : MonoBehaviour {
	public GamePlay gamePlayComp;

	public Text myPlayerNameText;
	public Text enemyPlayerNameText;
	public Text myPlayerNameText2;
	public Text enemyPlayerNameText2;
	public string myPlayerName;
	public string enemyPlayerName;

	public bool namesLoaded = false;

	// Use this for initialization
	void Start () {
		myPlayerNameText.text = "";
		enemyPlayerNameText.text = "";
		if (myPlayerNameText2 != null) {
			myPlayerNameText2.text = "";
			enemyPlayerNameText2.text = "";
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (gamePlayComp != null) {
			if (!namesLoaded && gamePlayComp.onlineMode == true) {
				foreach (PhotonPlayer player in PhotonNetwork.playerList) {
					//Debug.Log ("found player = " + player.NickName);
					if (player.IsLocal) {
						if (!myPlayerName.Equals (player.NickName)) {
							myPlayerName = player.NickName;
							myPlayerNameText.text = myPlayerName;
							if (myPlayerNameText2 != null) {
								myPlayerNameText2.text = myPlayerName;
							}
						}
					} else {
						if (!enemyPlayerName.Equals (player.NickName)) {
							enemyPlayerName = player.NickName;
							enemyPlayerNameText.text = enemyPlayerName;
							if (enemyPlayerNameText2 != null) {
								enemyPlayerNameText2.text = enemyPlayerName;
							}
						}
					}
				}
				//namesLoaded = true;
			}
		}
	}
}
