using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using PlayFab;
using PlayFab.ClientModels;

public class MyNetworkManager : MonoBehaviour {

	//private GUIStyle guiStyle = new GUIStyle();// Just for screen debugs
	public enum gameModeEnum {quickPlay, training, withFriend, tutorial, ranked};
	public gameModeEnum GameMode;
	public string GameplayScene = "Board_Scene";
	public string MainMenuScene = "Main_Menu";
	public string ExitScene = "LoadingScene";
	public GameObject DeckToPlay;
	public GameObject CardBase;
	public GameObject HeroBase;
	public MainMenu MainMenuComponent;
	//public LocalPlayer player;

	public int minimumPlayers;
	public int PlayerTtl = 120;
	public int EmptyRoomTtl = 120;
	public bool AutoRejoin = true;
	public Text gameVersionText;
	public Canvas SceneloadCanvas;

	public FriendLobby FriendLobbyComponent;
	public GameChatHandler ChatHandler;
	public UIHelpScreen HelpScreenComponent;
	public GameObject CornerMenuObject;

	public bool RefreshFriends = false;
	public bool Joining = false;
	public bool ReJoining = false;
	public bool Leaving = false;
	public bool Disconnecting = false;
	public bool Disconnected = false;

	public int onlinePlayers = 0;
	public int searchingGamePlayers = 0;
	private string _challangerName = "";
	private string _challangerRoomName = "";
	private bool localPlayerReady = false;
	private bool otherPlayerReady = false;

	public string chatIdCache = "";

	public bool checkOnlineState = false;
	public delegate void NetManagerEvent ();

	public List <string> InternalFriendList = new List<string>();

	public string PrevRoomName = "";
	public static string RoomNameProperty = "CurrentRoom";
	public static string GameModeProperty = "GameMode";
	//public string[] playerList = new string[10];

	// Use this for initialization
	void Start () {
		if (ChatHandler == null && GameObject.Find ("Chat-Canvas") != null) {
			ChatHandler = GameObject.Find ("Chat-Canvas").GetComponent<GameChatHandler> ();
		}
		if (FriendLobbyComponent == null && GameObject.Find ("FriendLobby-Canvas") != null) {
			FriendLobbyComponent = GameObject.Find ("FriendLobby-Canvas").GetComponent<FriendLobby> ();
		}
		if (HelpScreenComponent == null && GameObject.Find ("HelpScreen-Canvas") != null) {
			HelpScreenComponent = GameObject.Find ("HelpScreen-Canvas").GetComponent<UIHelpScreen> ();
		}
		FindMainMenuObject ();
		FindPlayerDataObject ();
		if (!PhotonNetwork.connected) {
			Debug.Log ("Networking start: connect");
			Connect ();
		}
		SceneManager.sceneLoaded += OnLevelFinishedLoading;
		if (MainMenuComponent != null) {
			SceneloadCanvas = MainMenuComponent.BackGroundCanvas;
			MainMenuComponent.gotoMainMenu ();
			MainMenuComponent.LoadDeckData ();
		}
	}
	
	// Update is called once per frame
	void Update () {
		onlinePlayers = PhotonNetwork.countOfPlayers;
		searchingGamePlayers = PhotonNetwork.countOfPlayersOnMaster;
		checkOnlineState = PhotonNetwork.connectedAndReady;
		//if (!Disconnecting && !PhotonNetwork.connected && !PhotonNetwork.connecting) {
		//	Connect ();
		//}
	}

	private IEnumerator ReconnectingTask() {
		while (!PhotonNetwork.connectedAndReady) {
			yield return new WaitForSeconds (5f);
			if (!Disconnecting && !PhotonNetwork.connected && !PhotonNetwork.connecting) {
				Connect ();
			}
		}
	}

	private IEnumerator WaitForConnectAndCall(NetManagerEvent NetEvent) {
		Debug.LogWarning ("Need to reconect to take action");
		while (!PhotonNetwork.connectedAndReady) {
			yield return new WaitForSeconds (0.05f);
		}
		yield return new WaitForSeconds (2f);
		while (!PhotonNetwork.connectedAndReady) {
			yield return new WaitForSeconds (0.05f);
		}//double check
		if (NetEvent != null) {
			NetEvent ();
		}
	}

	private void FindPlayerDataObject() {
		/*if ((player == null) &&
			(GameObject.Find ("Player") != null)) {
			player = GameObject.Find ("Player").GetComponent<LocalPlayer> ();
		}*/
	}

	private void FindMainMenuObject() {
		if ((MainMenuComponent == null) &&
			(GameObject.Find ("MainMenu-Canvas") != null)) {
			MainMenuComponent = GameObject.Find ("MainMenu-Canvas").GetComponent<MainMenu> ();
		}
	}

	#region PHOTON_FLOW
	public void OnDisconnectedFromPhoton() {
		Debug.LogWarning ("Client lost connection to photon");
		Disconnected = true;
		if (SceneManager.GetActiveScene ().name == GameplayScene) {
			if (GameMode != gameModeEnum.training) {
				//SceneManager.LoadScene (MainMenuScene);
				GotoMainMenuScene();
				//ReConnect();
			}
		} else {
			if ((MainMenuComponent == null) &&
				(GameObject.Find ("MainMenu-Canvas") != null)) {
				MainMenuComponent = GameObject.Find ("MainMenu-Canvas").GetComponent<MainMenu> ();
			}
			if (MainMenuComponent != null) {
				MainMenuComponent.gotoDisconnectedScreen ();
			}
		}
		StartCoroutine (ReconnectingTask());
	}

	public void Connect() {
		FindPlayerDataObject ();
		if (LocalPlayer.Instace != null) {
			if (LocalPlayer.Instace.DataLoaded == false) {
				LocalPlayer.Instace.LoadPlayerData (null);
				chatIdCache = LocalPlayer.Instace.localPlayerData.dispName;
			}
		}
		Debug.Log ("Connect game version:" + gameVersionText.text);
		PhotonNetwork.ConnectUsingSettings (gameVersionText.text);
		ConnectChat ();
		Disconnecting = false;
	}

	public void ConnectChat() {
		/*if ((player == null) &&
			(GameObject.Find ("Player") != null)) {
			player = GameObject.Find ("Player").GetComponent<LocalPlayer> ();
		}*/
		if (LocalPlayer.Instace != null) {
			if (LocalPlayer.Instace.DataLoaded == false) {
				LocalPlayer.Instace.LoadPlayerData (null);
			}
		}
		if (ChatHandler == null && GameObject.Find ("Chat-Canvas") != null) {
			ChatHandler = GameObject.Find ("Chat-Canvas").GetComponent<GameChatHandler> ();
		}
		if (ChatHandler != null) {
			if (LocalPlayer.Instace.localPlayerData.dispName != null) {
				if ((chatIdCache == null || chatIdCache.Length == 0) && LocalPlayer.Instace.localPlayerData.dispName.Length != 0) {
					chatIdCache = LocalPlayer.Instace.localPlayerData.dispName;
				}
			}
			if (chatIdCache != null) {
				if (chatIdCache.Length != 0) {
					ChatHandler.GameChatConnect (chatIdCache);
					Debug.Log ("connect to chat with data: " + LocalPlayer.Instace.localPlayerData.dispName + " cache: " + chatIdCache);
				}
			}
		}
	}

	public void DisConnect() {
		Debug.Log ("Disconnect game version:" + gameVersionText.text);
		Disconnecting = true;
		PhotonNetwork.Disconnect();
		if (ChatHandler != null) {
			ChatHandler.GameChatDisconnect ();
		}
	}

	public void ReConnect() {
		Debug.Log ("Try to reconnect and rejoin");
		ReJoining = true;
		LoadGameplayScene ();
		PhotonNetwork.ReconnectAndRejoin();
		//Disconnecting = false;
	}

	public void OnJoinedLobby()
	{
		Debug.Log("JoinRandom");
		MainMenuComponent.gotoMainMenu ();
		//UpdateFriendsList();
	}

	public void OnConnectedToMaster()
	{
		Debug.Log ("Client connected to photon");
		// when AutoJoinLobby is off, this method gets called when PUN finished the connection (instead of OnJoinedLobby())
		if ((MainMenuComponent == null) &&
			(GameObject.Find ("MainMenu-Canvas") != null)) {
			MainMenuComponent = GameObject.Find ("MainMenu-Canvas").GetComponent<MainMenu> ();
			//if (MainMenuComponent.ConnectingScreen) {
				
			//}
		}
		if (Disconnected && MainMenuComponent != null) {
			Disconnected = false;
			MainMenuComponent.gotoMainMenu ();
			MainMenuComponent.LoadDeckData ();
		}
		Disconnecting = false;
		RefreshFriends = true;
	}

	public void OnPhotonPlayerDisconnected() {
		Debug.Log ("Player disconnected");

		GameObject gameplay = GameObject.Find ("Gameplay");
		if (SceneManager.GetActiveScene ().name.Equals(GameplayScene)) {
			//if (!gameplay.GetComponent<GamePlay> ().UsePhotonEventsIsteadOfRPC) {
				// with rpc is not posible to reconnect
				gameplay.GetComponent<GamePlay> ().WinGame (0);
			//}
		} else {
			if (GameMode == gameModeEnum.withFriend) {
				LeaveFriendLobby ();
			}
			if (!SceneManager.GetActiveScene ().name.Equals(MainMenuScene)) {
				GotoMainMenuScene ();
			}
			MainMenuComponent.gotoMainMenu ();
		}
	}
	#endregion

	public void StartGame() {
		if (DeckToPlay != null) {
			DeckToPlay.transform.SetParent (this.transform);
		}
		ReJoining = false;
		if (GameMode == gameModeEnum.quickPlay) {
			FindRandomGameRoom ();
		} else if (GameMode == gameModeEnum.training || GameMode == gameModeEnum.tutorial) {
			LoadGameplayScene ();
		}
	}

	public void SetGameModeRandom() {
		GameMode = gameModeEnum.quickPlay;
	}

	public void SetGameModeLocal() {
		GameMode = gameModeEnum.training;
	}

	public void SetGameModeWithFriend() {
		GameMode = gameModeEnum.withFriend;
	}

	public void SetGameModeTutorial() {
		if (!SceneManager.GetActiveScene ().name.Equals (GameplayScene)) {
			GameMode = gameModeEnum.tutorial;
		}
	}

	public void SetGameModeRanked() {
		GameMode = gameModeEnum.ranked;
	}

	/* pre alpha match making */
	#region MATCHMAKING
	private void FindRandomGameRoom() {
		if (LeaveMyGame ()) {
			StartCoroutine(WaitForConnectAndCall (FindRandomGameRoom));
			return;
		}
		string gameMode = GameMode.ToString ();
		var roomExpectedProps = new ExitGames.Client.Photon.Hashtable() { { "gm", gameMode } };
		RefreshFriends = false;
		ReJoining = false;
		Joining = true;
		PhotonNetwork.JoinRandomRoom(roomExpectedProps, 0);
	}

	public void OnPhotonRandomJoinFailed()
	{
		Debug.Log ("OnPhotonRandomJoinFailed");
		ReJoining = false;
		RoomOptions roomOpts = new RoomOptions();
		roomOpts.MaxPlayers = 2;
		roomOpts.PlayerTtl = PlayerTtl;//myPlayerTTL * 1000;//sec to milisec
		roomOpts.EmptyRoomTtl = EmptyRoomTtl;//1 minute
		roomOpts.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "gm", GameMode.ToString () } };
		string[] props = { "gm" };
		roomOpts.CustomRoomPropertiesForLobby = props;
		Debug.Log ("Create room: " + LocalPlayer.Instace + " gameMode: " + GameMode.ToString ());
		PhotonNetwork.CreateRoom(null, roomOpts, null);
	}

	public void OnPhotonJoinRoomFailed()
	{
		Debug.Log("OnPhotonJoinRoomFailed");
		RefreshFriends = true;
		Joining = false;
		ReJoining = false;
		PrevRoomName = "";
		SaveRoomName ("");
		if (SceneManager.GetActiveScene ().name.Equals (GameplayScene)) {
			GotoMainMenuScene ();
		} else {
			OnRejoinFailed();
		}
	}

	public void OnJoinedRoom() {
		Debug.Log("OnJoinedRoom:" + PhotonNetwork.room.Name);
		RefreshFriends = true;
		Joining = false;
		if (PhotonNetwork.playerList.Length >= minimumPlayers) {
			PhotonNetwork.room.IsVisible = false;
			SaveRoomName (PhotonNetwork.room.Name);
			if (GameMode == gameModeEnum.withFriend && !ReJoining) {
				LoadLobbyMenu ();
			} else {
				LoadGameplayScene ();
			}
		} else if (Leaving) {
			Leaving = false;
			LeaveMyGame ();
		}
	}
	
	public void OnPhotonPlayerConnected(PhotonPlayer player) {
		Debug.Log ("OnPhotonPlayerConnected: " + player);
		if (!SceneManager.GetActiveScene().name.Equals(GameplayScene)) {
			if (PhotonNetwork.playerList.Length >= minimumPlayers) {
				PhotonNetwork.room.IsVisible = false;
				SaveRoomName (PhotonNetwork.room.Name);
				if (GameMode == gameModeEnum.withFriend) {
					LoadLobbyMenu ();
				} else {
					LoadGameplayScene ();
				}
			}
		}
	}
	#endregion


	#region WITH_FRIEND
	public void JoinToFiendsLobby(string roomName, string challangerName) {
		SetGameModeWithFriend ();
		Debug.Log ("Create private game room named: " + roomName + ", with friend: " + challangerName);
		_challangerName = challangerName;
		_challangerRoomName = roomName;
		JoinToFiendsLobbyRoom();
	}

	private void JoinToFiendsLobbyRoom() {
		if (LeaveMyGame ()) {
			StartCoroutine(WaitForConnectAndCall (JoinToFiendsLobbyRoom));
			return;
		}
		Debug.Log ("Join to private room");
		RoomOptions roomOpts = new RoomOptions ();
		roomOpts.IsVisible = false;
		roomOpts.MaxPlayers = 2;
		roomOpts.PlayerTtl = PlayerTtl;
		roomOpts.EmptyRoomTtl = EmptyRoomTtl;
		PhotonNetwork.JoinOrCreateRoom (_challangerRoomName, roomOpts, null);
		SaveRoomName (_challangerRoomName);
	}

	private void LoadLobbyMenu() {
		localPlayerReady = false;
		otherPlayerReady = false;
		if (ChatHandler != null) {
			ChatHandler.LocalFriendListComponent.HideFriendList ();
		}
		if (MainMenuComponent != null) {
			MainMenuComponent.gotoFriendLobbyMenu (_challangerName);
		}
	}

	private bool checkPlayerReady() {
		return (localPlayerReady && otherPlayerReady) ? true : false;
	}

	public void SignalFriendReady() {
		localPlayerReady = !localPlayerReady;
		FriendLobbyComponent.SetPlayerReady (localPlayerReady);
		FriendLobbyComponent.gameObject.GetComponent<PhotonView> ().RPC ("ReceiveSignalReady", PhotonTargets.Others, localPlayerReady);
		if (checkPlayerReady ()) {
			LoadGameplayScene ();
		}
	}

	public void ReceiveSignalReady(bool ready) {
		otherPlayerReady = ready;
		FriendLobbyComponent.SetFriendReady (otherPlayerReady);
		if (checkPlayerReady ()) {
			LoadGameplayScene ();
		}
	}

	public void LeaveFriendLobby() {
		LeaveMyGame ();
		/*if (MainMenuComponent != null) {
			MainMenuComponent.gotoMainMenu();
		}*/
	}
	#endregion

	#region PLAYFAB_SAVE_AND_LOAD
	public void SaveRoomName(string roomName) {
		UpdateUserDataRequest request = new UpdateUserDataRequest ();
		request.Data = new Dictionary<string, string> ();
		if (roomName.Equals("")) {
			List<string> RmKey = new List<string> ();
			RmKey.Add (RoomNameProperty);
			RmKey.Add (GamePlayCore.CurrentDeckProperty);
			RmKey.Add (GamePlayCore.CurrentDeckNameProperty);
			RmKey.Add (GamePlayCore.CurrentDeckHeroProperty);
			request.KeysToRemove = RmKey;
		} else {
			request.Data.Add (RoomNameProperty, roomName);
			request.Data.Add (GameModeProperty, GameMode.ToString());
			Debug.Log ("save game mode:" + GameMode.ToString());
		}

		PlayFabClientAPI.UpdateUserData (request, RoomSaved, error => Debug.LogError(error.GenerateErrorReport()));
	}

	private void RoomSaved(UpdateUserDataResult result) {
		Debug.Log ("Current gameplay room name saved");
	}

	public void RejoinGame() {
		if (!PhotonNetwork.connectedAndReady) {
			StartCoroutine(WaitForConnectAndCall (RejoinGame));
			return;
		}
		if (PrevRoomName != null &&
		    PrevRoomName.Length > 0 &&
		    !PrevRoomName.Equals ("")) {
			RefreshFriends = false;
			Joining = true;
			ReJoining = true;//Unset when game will be synced
			MainMenuComponent.ShowWaitIndicator ();
			PhotonNetwork.ReJoinRoom (PrevRoomName);
		} else {
			OnRejoinFailed();
		}
	}

	public void OnRejoinFailed() {
		MainMenuComponent.gotoMainMenu ();
		MainMenuComponent.SetupPlayButtonNormal ();
		MainMenuComponent.HideWaitIndicator ();
		ChatHandler.PrintChatMessage ("", "Poprzednia gra zakończyła się");
	}
	#endregion

	public void GotoMainMenuScene() {
		CornerMenuObject.transform.parent = null;
		ChatHandler.LocalFriendListComponent.HideFriendList();
		ChatHandler.HideChat();
		HelpScreenComponent.DisableHelpMenu();
		CornerMenuObject.GetComponent<Canvas> ().enabled = false;
		DontDestroyOnLoad (CornerMenuObject);

		SceneManager.LoadScene(ExitScene);
		if (PhotonNetwork.inRoom) {
			PhotonNetwork.LeaveRoom ();
		}
	}

	public bool LeaveMyGame() {
		bool rv = false;
		if (Joining) {
			Leaving = true;
			return true;
		}
		if (PhotonNetwork.connectionStateDetailed == ClientState.PeerCreated) {
			DisConnect ();
			rv = true;
		}
		if (PhotonNetwork.inRoom) {
			Debug.Log ("Leaving room");
			PhotonNetwork.room.IsVisible = false;
			PhotonNetwork.LeaveRoom ();
			rv = true;
		}
		ChatHandler.RefreshFriendList();
		return rv;
	}

	public void OnLeftRoom() {
		ChatHandler.SetClientStatus (GameChatHandler.online);
	}

	public void SetPlayerName(string name) {
		PhotonNetwork.playerName = name;
	}

	public void OnUpdatedFriendsList() {
		Debug.Log("Updated: " + PhotonNetwork.Friends.Count.ToString());
	}

	public void OnUpdateFriendList() {
		Debug.Log("Update: " + PhotonNetwork.Friends.Count.ToString());
	}

	public void HideAllMenus() {
		ChatHandler.LocalFriendListComponent.HideFriendList();
		ChatHandler.HideChat();
		HelpScreenComponent.DisableHelpMenu();
		CornerMenuObject.GetComponent<Canvas> ().enabled = false;
	}

	public void LoadGameplayScene() {
		ChatHandler.SetClientStatus (GameChatHandler.playing);
		DontDestroyOnLoad (this.gameObject);
		if (MainMenuComponent != null) {
			SceneloadCanvas = MainMenuComponent.BackGroundCanvas;
		}
		if (DeckToPlay != null) {
			DeckToPlay.transform.SetParent (this.transform.root);
			DeckToPlay.transform.localScale = new Vector3 (0, 0, 0);
			DontDestroyOnLoad (DeckToPlay.gameObject);
			DeckToPlay.transform.localScale = new Vector3 (1, 1, 1);
		}
		if (SceneloadCanvas != null) {
			SceneloadCanvas.transform.SetParent (transform.parent);
			SceneloadCanvas.enabled = true;
			DontDestroyOnLoad (SceneloadCanvas.gameObject);
		}
		DontDestroyOnLoad (CardBase.gameObject);
		DontDestroyOnLoad (HeroBase.gameObject);

		HideAllMenus ();
		CornerMenuObject.transform.parent = null;//SetParent (transform.root);
		DontDestroyOnLoad (CornerMenuObject);

		PhotonNetwork.LoadLevel (GameplayScene);
	}

	void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode) {
		if (scene.name.Equals (GameplayScene)) {
			if (CornerMenuObject != null) {
				
				CornerMenuObject.transform.SetParent (Camera.main.transform);//GameObject.Find ("CornerMenuCanvas").transform);
				CornerMenuObject.GetComponent<Canvas>().worldCamera = Camera.main;
				CornerMenuObject.GetComponent<Canvas>().enabled = true;
				/*Canvas.ForceUpdateCanvases();
				RectTransform rect = CornerMenuObject.GetComponent<RectTransform> ();
				rect.localScale = new Vector3(1,1,1);
				Vector3 pos = rect.localPosition;
				pos.z = 0;
				rect.localPosition = pos;
				Canvas.ForceUpdateCanvases();*/
			}
		} else if (scene.name.Equals (MainMenuScene)) {
			if (CornerMenuObject != null) {
				if (ChatHandler != null) {
					ChatHandler.SetClientStatus (GameChatHandler.online);
				}
				CornerMenuObject.transform.SetParent (Camera.main.transform);//GameObject.Find ("MainMenu-Canvas").transform, false);
				CornerMenuObject.GetComponent<Canvas>().worldCamera = Camera.main;
				CornerMenuObject.GetComponent<Canvas>().enabled = true;
				/*Canvas.ForceUpdateCanvases();
				RectTransform rect = CornerMenuObject.GetComponent<RectTransform> ();
				rect.localScale = new Vector3(1,1,1);
				Vector3 pos = rect.localPosition;
				pos.z = 0;
				rect.localPosition = pos;
				Canvas.ForceUpdateCanvases();*/
			}
			FindMainMenuObject ();
			if (MainMenuComponent) {
				MainMenuComponent.LoadDeckData ();
			} else {
				Debug.LogError ("Cannot find mainmenu component");
			}
		}
	}

	public void HideSceneLoadScreen() {
		if (SceneloadCanvas != null) {
			Destroy (SceneloadCanvas.gameObject);
		}
	}

	public void SetDeckToPlay(GameObject deck) {
		DeckToPlay = deck;
	}

	public void UnSetObjectsToDestroy() {
		if (HeroBase != null) {
			HeroBase.transform.SetParent (GameObject.Find ("Gameplay").transform);
		}
		if (CardBase != null) {
			CardBase.transform.SetParent (GameObject.Find ("Gameplay").transform);
		}
		//this.transform.SetParent (GameObject.Find ("Gameplay").transform);
	}

	/*public void OnGUI() {
		guiStyle.fontSize = 10;
		guiStyle.normal.textColor = Color.red;
		//GUILayout.Label (PhotonNetwork.connectionStateDetailed.ToString (), guiStyle);
		GUI.TextField(new Rect(Screen.width-100, Screen.height-10, 10, 10), PhotonNetwork.connectionStateDetailed.ToString (), guiStyle);
	}*/
}
