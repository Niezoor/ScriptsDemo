using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameplayMenu : MonoBehaviour {
	private Canvas SubMenuCanvas;
	public string ExitScene;
	public GameObject SettingsMenuCanvasPrefab;
	public OnlinePlayersName OnlinePlayersNameComponent;
	public Button AddFriendBtn;
	private GameObject settingMenuObject;
	private bool playerChecked = false;
	public WinScreen WinScreenComponent;
	public Text LeaveGameButtonText;
	// Use this for initialization
	void Start () {
		SubMenuCanvas = GetComponent<Canvas> ();
		SubMenuCanvas.enabled = false;
		if (settingMenuObject == null) {
			settingMenuObject = Instantiate (SettingsMenuCanvasPrefab, Camera.main.transform);
			settingMenuObject.GetComponent<Canvas> ().worldCamera = Camera.main;
			settingMenuObject.GetComponent<Canvas> ().planeDistance = 7;
			settingMenuObject.GetComponent<Canvas> ().enabled = false;
		}
		if (GameObject.Find ("NetworkManager") != null) {
			MyNetworkManager netManager = GameObject.Find ("NetworkManager").GetComponent<MyNetworkManager> ();
			if (netManager.GameMode == MyNetworkManager.gameModeEnum.training ||
				netManager.GameMode == MyNetworkManager.gameModeEnum.tutorial) {
				Destroy (AddFriendBtn.gameObject);
			}
		}
	}

	public void OpenSubmenu() {
		if (WinScreenComponent.GameEnd) {
			LeaveGameButtonText.text = "Wyjdź";
		} else {
			LeaveGameButtonText.text = "Poddaj się";
		}
		SubMenuCanvas.enabled = true;
	}

	public void CloseSubmenu() {
		SubMenuCanvas.enabled = false;
	}

	public void OpenSettingsMenu() {
		settingMenuObject.GetComponent<Canvas> ().enabled = true;
	}

	public void LeaveGame() {
		if (GameObject.Find ("NetworkManager") == null) {
			SceneManager.LoadScene(ExitScene);
		} else {
			GameObject.Find ("NetworkManager").GetComponent<MyNetworkManager> ().GotoMainMenuScene ();
		}
		/*if ((PhotonNetwork.connected) &&
			(GameObject.Find ("NetworkManager").GetComponent<MyNetworkManager> ().gameMode != MyNetworkManager.gameModeEnum.local)) {
			PhotonNetwork.LeaveRoom ();
		} else {
			//Application.LoadLevel("Main_Menu");
			SceneManager.LoadScene(ExitScene);
		}*/
	}

	//void OnLeftRoom() {
		//Application.LoadLevel("Main_Menu");
		//SceneManager.LoadScene(ExitScene);
	//}

	public void AddCurrentPlayerToFriends() {
		if (GameObject.Find ("NetworkManager") != null) {
			MyNetworkManager netManager = GameObject.Find ("NetworkManager").GetComponent<MyNetworkManager> ();
			netManager.ChatHandler.LocalFriendListComponent.AddFriend (OnlinePlayersNameComponent.enemyPlayerName);
			AddFriendBtn.interactable = false;
		}
		CloseSubmenu ();
	}

	private bool CheckPlayer(string name) {
		bool rv = false;
		if (GameObject.Find ("NetworkManager") != null) {
			MyNetworkManager netManager = GameObject.Find ("NetworkManager").GetComponent<MyNetworkManager> ();
			if (netManager.ChatHandler.LocalFriendListComponent.GetIDByName (OnlinePlayersNameComponent.enemyPlayerName).Length > 0) {
				rv = true;
			}
		}
		playerChecked = true;
		return rv;
	}
	
	// Update is called once per frame
	void Update () {
		if (!playerChecked) {
			if (AddFriendBtn != null) {
				if (OnlinePlayersNameComponent != null &&
				    OnlinePlayersNameComponent.enemyPlayerName.Length > 0) {
					if (CheckPlayer (OnlinePlayersNameComponent.enemyPlayerName)) {
						Destroy (AddFriendBtn.gameObject);
					} else {
						AddFriendBtn.interactable = true;
					}
				}
			}
		}
	}
}
