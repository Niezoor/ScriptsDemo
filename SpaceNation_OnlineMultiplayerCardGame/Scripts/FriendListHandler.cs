using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;

public class FriendListHandler : MonoBehaviour {
	public Canvas FriendListCanvas;
	public Transform PanelsTable;
	public GameObject FriendPanelPrefab;
	public GameObject FriendOptionsPanelPrefab;

	public GameChatHandler ChatHandler;
	public bool RefreshingFriendList = true;
	public bool listFilled = false;
	public int onlineFriends = 0;
	public InputField friendToFindInputField;
	public Text NotifyText;
	public System.DateTime CurrentTime = new System.DateTime();

	private List<FriendPanel> FriendPanelsList = new List<FriendPanel> ();

	[System.Serializable]
	public class SpaceNationFriend
	{
		public string name = "";
		public string playfabId = "";
		public string avatar;
		public FriendPanel panel;
		public System.DateTime lastLogin;
	};

	public List<SpaceNationFriend> SpaceNationFriendList = new List<SpaceNationFriend> ();
	private string friendToFindName = "";
	private string friendToRemove = "";
	private string friendIDToRemove = "";

	public GameObject PlayerProfilePrefab;
	private PlayerProfile PlayerProfileComponent;

	// Use this for initialization
	void Start () {
		FriendListCanvas.enabled = false;
		InvokeRepeating("UpdateTime", 0, 600);
		GetFriendListFromServer ();
	}

	// Update is called once per frame
	void Update () {
	}

	public void SetFriendOnlineStatus(string FriendID) {
		FriendPanel panel = GetPanelById (FriendID);
		Debug.Log ("player: " + FriendID + " is Online");
		if (panel != null) {
			panel.SetStatusOnline ();
		}
		RefreshNotifyCounter ();
	}

	public void SetFriendOfflineStatus(string FriendID) {
		FriendPanel panel = GetPanelById (FriendID);
		Debug.Log ("player: " + FriendID + " is Offline");
		if (panel != null) {
			panel.SetStatusOffline ();
		}
		RefreshNotifyCounter ();
	}

	public void SetFriendInGameStatus(string FriendID) {
		FriendPanel panel = GetPanelById (FriendID);
		Debug.Log ("player: " + FriendID + " is Offline");
		if (panel != null) {
			panel.SetStatusInGame ();
		}
		RefreshNotifyCounter ();
	}

	public void HideFriendList() {
		FriendListCanvas.enabled = false;
	}

	public void ShowFriendList() {
		FriendListCanvas.enabled = true;
		FillFriendList ();
	}

	private void RefreshNotifyCounter() {
		onlineFriends = 0;
		foreach (SpaceNationFriend SNF in SpaceNationFriendList) {
			if (SNF.panel != null) {
				if (SNF.panel.localFriendStatus != FriendPanel.LocalFriendStatuses.OFFLINE) {
					onlineFriends++;
				}
			}
		}
		if (onlineFriends > 0) {
			NotifyText.text = onlineFriends.ToString ();
		} else {
			NotifyText.text = "";
			onlineFriends = 0;
		}
	}

	public void HideAllPanelsOptions() {
		foreach (FriendPanel panel in FriendPanelsList) {
			panel.HideOptionsPanel ();
		}
	}

	#region ShowFriendProfile
	public void GetPlayerStats(string playerPlayfabId) {
		if (playerPlayfabId != null) {
			GetLeaderboardAroundPlayerRequest request = new GetLeaderboardAroundPlayerRequest ();
			request.MaxResultsCount = 1;
			request.PlayFabId = playerPlayfabId;
			request.ProfileConstraints = new PlayerProfileViewConstraints () {
				ShowStatistics = true,
				ShowDisplayName = true,
				ShowAvatarUrl = true
			};
			request.StatisticName = "Wins";
			PlayFabClientAPI.GetLeaderboardAroundPlayer(request, GetPlayerStatsResult, OnPlayFabError);
			HideFriendList ();
			PlayerProfileComponent = Instantiate (PlayerProfilePrefab, Camera.main.transform).GetComponent<PlayerProfile> ();
			PlayerProfileComponent.GetComponent<Canvas> ().worldCamera = Camera.main;
		}
	}

	private void GetPlayerStatsResult(GetLeaderboardAroundPlayerResult result) {
		if (result.Leaderboard.Count > 1) {
			Debug.LogError ("too much profiles returned");
		}
		if (PlayerProfileComponent != null) {
			PlayerProfileModel Prof = result.Leaderboard [0].Profile;
			PlayerData data = new PlayerData ();
			data.dispName = Prof.DisplayName;
			data.avatar = Prof.AvatarUrl;
			LocalPlayer.ParsePlayfabStats (Prof.Statistics, data);
			PlayerProfileComponent.LoadPlayerProfile (data);
		}
	}
	#endregion

	#region RemoveFriend
	public void RemoveLocalFriend(string friendName) {
		friendToRemove = friendName;
		if (ChatHandler.POPUPWindowComponent == null && GameObject.Find ("POPUP-Canvas") != null) {
			ChatHandler.POPUPWindowComponent = GameObject.Find ("POPUP-Canvas").GetComponent<MainMenuPOPUP> ();
		}
		if (ChatHandler.POPUPWindowComponent != null) {
			ChatHandler.POPUPWindowComponent.SetupDialogPOPUPWindow (
				"Czy chcesz usunąć gracza <color=#00ffffff>'" + friendToRemove + "'</color> z twojej listy znajomych?",
				RemoveLocalFriendConfirm, RemoveLocalFriendCancel, RemoveLocalFriendCancel);
		}
	}

	private void RemoveLocalFriendCancel() {
		GetPanelById (GetIDByName (friendToRemove)).RemovePending = false;
		HideAllPanelsOptions ();
	}

	private void RemoveLocalFriendConfirm() {
		foreach (SpaceNationFriend SNfriend in SpaceNationFriendList) {
			if (SNfriend.name.Equals (friendToRemove)) {
				RemoveFriendRequest request = new RemoveFriendRequest ();
				request.FriendPlayFabId = SNfriend.playfabId;
				friendIDToRemove = SNfriend.playfabId;
				PlayFabClientAPI.RemoveFriend (request, OnRemoveFriendSuccess, OnPlayFabError);
				break;
			}
		}
	}

	private void OnRemoveFriendSuccess(RemoveFriendResult result) {
		Debug.Log ("Remove friend success");
		FriendPanel panel = GetPanelById (friendIDToRemove);
		if (panel != null) {
			panel.RemovePanel ();
		}
		if (friendIDToRemove.Length > 0) {
			ChatHandler.chatClient.RemoveFriends (new string[] { friendIDToRemove });
		}
	}

	#endregion

	#region AddFriend
	public void SetFriendToFindName(string name) {
		friendToFindName = name;
	}

	public void AddFriendRequest(string name) {
		friendToFindName = name;
		if (ChatHandler.POPUPWindowComponent == null && GameObject.Find ("POPUP-Canvas") != null) {
			ChatHandler.POPUPWindowComponent = GameObject.Find ("POPUP-Canvas").GetComponent<MainMenuPOPUP> ();
		}
		if (ChatHandler.POPUPWindowComponent != null) {
			ChatHandler.POPUPWindowComponent.SetupDialogPOPUPWindow (
				"Czy chcesz dodać <color=#00ffffff>'" + name + "'</color> do znajomych,\naby umożliwić interakcje z tym graczem?",
				"Dodaj", "Odrzuć",
				AddFriend, AddFriendCancel, AddFriendCancel);
		}
	}

	public void AddFriend() {
		if (friendToFindName.Length > 0) {
			AddFriendRequest request = new AddFriendRequest ();
			request.FriendTitleDisplayName = friendToFindName;
			PlayFabClientAPI.AddFriend (request, OnFriendAdded, OnPlayFabError);
			friendToFindInputField.text = "";
		}
	}

	public void AddFriend(string name) {
		SetFriendToFindName (name);
		AddFriend ();
	}

	private void AddFriendCancel() {
		friendToFindName = "";
		friendToFindInputField.text = "";
	}

	private void OnFriendAdded(AddFriendResult result) {
		if (result.Created) {
			Debug.Log ("Friend Sucessfully added");
		}
		ChatHandler.ShowChallangeNotify (friendToFindName);
		RefreshFriendList ();
	}
	#endregion

	#region FriendList
	public void GetFriendListFromServer() {
		GetFriendsListRequest request = new GetFriendsListRequest ();
		request.ProfileConstraints = new PlayerProfileViewConstraints () {
			ShowLastLogin = true,
			ShowDisplayName = true,
			ShowAvatarUrl = true
		};
		PlayFabClientAPI.GetFriendsList (request, onFriendListUpdate, OnPlayFabError);
	}

	private void onFriendListUpdate(GetFriendsListResult result) {
		ClearFrienList ();
		SpaceNationFriendList.Clear ();
		foreach (PlayFab.ClientModels.FriendInfo Finfo in result.Friends) {
			SpaceNationFriend SNfriend = new SpaceNationFriend ();
			//Debug.Log ("friend disp name " + Finfo.TitleDisplayName);
			//SNfriend.name = Finfo.TitleDisplayName;
			SNfriend.name = Finfo.Profile.DisplayName;
			SNfriend.playfabId = Finfo.FriendPlayFabId;
			SNfriend.lastLogin = Finfo.Profile.LastLogin.Value;
			SNfriend.avatar = Finfo.Profile.AvatarUrl;
			SpaceNationFriendList.Add (SNfriend);
		}
		FillFriendList ();
		ChatHandler.RefreshFriendList ();
	}

	private void RefreshFriendList() {
		GetFriendListFromServer ();
	}

	private void FillFriendList() {
		if (!listFilled) {
			//LocalPlayer player = ChatHandler.NetworkManager.player;
			ClearFrienList ();
			foreach (SpaceNationFriend SNfriend in SpaceNationFriendList) {
				GameObject panel = Instantiate (FriendPanelPrefab);
				FriendPanel panelComp = panel.GetComponent<FriendPanel> ();
				panel.transform.SetParent (PanelsTable, false);
				FriendPanelsList.Add (panelComp);
				panelComp.FriendListHandlerComponent = GetComponent<FriendListHandler> ();
				panelComp.FriendNameText.text = SNfriend.name;
				panelComp.lastLogin = SNfriend.lastLogin;
				SNfriend.panel = panelComp;
				if (LocalPlayer.Instace != null) {
					LocalPlayer.Instace.SetAvatar (panelComp.AvatarImage, SNfriend.avatar);
				}
			}
			listFilled = true;
		}
	}

	private void ClearFrienList() {
		if (listFilled) {
			foreach (Transform childTransform in PanelsTable)
				Destroy (childTransform.gameObject);
			FriendPanelsList.Clear ();
			listFilled = false;
		}
	}
	#endregion

	#region UpdateTime
	private void UpdateTime() {
		GetTimeRequest request = new GetTimeRequest ();
		PlayFabClientAPI.GetTime (request, OnTimeUpdate, OnPlayFabError);
	}

	private void OnTimeUpdate(GetTimeResult result) {
		CurrentTime = result.Time;
		foreach (SpaceNationFriend SNfriend in SpaceNationFriendList) {
			if (SNfriend.panel != null) {
				if (SNfriend.panel.localFriendStatus == FriendPanel.LocalFriendStatuses.OFFLINE) {
					SNfriend.panel.UpdateOfflineTime ();
				}
			}
		}
	}

	#endregion

	public void StartChatWithFriend(string FriendName) {
		ChatHandler.StartPrivateChat (FriendName);
		HideFriendList ();
		ChatHandler.ShowChat ();
	}

	public void ChallangeFriend(string FriendName) {
		ChatHandler.SendChallangeRequest (GetIDByName (FriendName));
		//HideFriendList ();
	}

	public void ChallageCancel(string FriendID) {
		if (GetPanelById (FriendID) != null) {
			GetPanelById (FriendID).OptionsPanelComponent.ChallangeButton.interactable = true;
		}
	}

	private FriendPanel GetPanelById(string id) {
		FriendPanel rv = null;
		foreach (SpaceNationFriend SNF in SpaceNationFriendList) {
			if (SNF.playfabId.Equals (id)) {
				rv = SNF.panel;
				break;
			}
		}
		return rv;
	}

	public string GetNameByID(string id) {
		foreach (SpaceNationFriend SNF in SpaceNationFriendList) {
			if (SNF.playfabId.Equals (id)) {
				return SNF.name;
			}
		}
		return "";
	}

	public string GetIDByName(string name) {
		foreach (SpaceNationFriend SNF in SpaceNationFriendList) {
			if (SNF.name.Equals (name)) {
				return SNF.playfabId;
			}
		}
		return "";
	}

	void OnPlayFabError(PlayFabError error)
	{
		Debug.Log ("Got an error: " + error.ErrorMessage);
		if (PlayerProfileComponent != null) {
			PlayerProfileComponent.ClosePlayerProfile ();
		}
	}
}
