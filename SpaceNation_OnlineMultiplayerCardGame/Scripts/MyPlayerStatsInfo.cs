using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyPlayerStatsInfo : MonoBehaviour {
	public Text PlayerNameText;
	public Text PlayerLVLText;
	public Image AvatarImage;

	[Header("PlayerProfile v2")]
	public GameObject PlayerProfilePrefab;

	// Use this for initialization
	void Start () {
		if (LocalPlayer.Instace != null) {
			LocalPlayer.Instace.GetPlayerProfile ();
			this.GetComponent<Canvas> ().enabled = false;
		} else {
			Debug.LogWarning ("Cannot find player profile");
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ShowStats() {
		PlayerProfile profileView = Instantiate (PlayerProfilePrefab, Camera.main.transform).GetComponent<PlayerProfile>();
		profileView.GetComponent<Canvas> ().worldCamera = Camera.main;
		profileView.LoadPlayerProfile (LocalPlayer.Instace.localPlayerData, true);
	}
}
