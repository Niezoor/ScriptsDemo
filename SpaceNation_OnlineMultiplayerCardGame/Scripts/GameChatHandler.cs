using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using ExitGames.Client.Photon.Chat;
using UnityEngine.UI;

public class GameChatHandler : MonoBehaviour, IChatClientListener {
	public Canvas ChatCanvas;
	public GameObject ChatMessagePrefab;
	public Transform ChatMessagesTable;
	public string GlobalChannelName = "SpaceNationGlobalChannel";
	public ChatClient chatClient;
	public FriendListHandler LocalFriendListComponent;
	public ScrollRect scrollRect;
	public InputField ChatInputField;
	public Dropdown ChannelsSwitchMenu;
	public List<string> GameChannels = new List<string> () {"Czat globalny"};
	public bool WaitForSub = true;
	public int UnreadedMessages = 0;
	public Text NotifyText;
	public MessageNotify MessageNotifyComponent;
	public MainMenuPOPUP POPUPWindowComponent;

	public const int offline = ChatUserStatus.Offline;
	public const int online  = ChatUserStatus.Online;
	public const int playing = ChatUserStatus.Playing;

	private int ClientCurrentStatus = offline;

	[System.Serializable]
	public class RealChannelName
	{
		public string userName;
		public string channelName;
		public List<string> chatHistory = new List<string> ();
		public int unreadMessages = 0;
	}
	public List<RealChannelName> RealChannelsNames = new List<RealChannelName> ();
	private string messageToSend = "";
	private string localPlayerId = "";
	public string challanger = "";
	public string challangerId = "";
	public MyNetworkManager NetworkManager;

	private bool nowConnecting = false;
	private bool chatDisconnected = false;
	// Use this for initialization
	void Start () {
		ChatCanvas.enabled = false;
		RealChannelName rn = new RealChannelName ();
		rn.userName = "Czat globalny";
		rn.channelName = GameChannels[0];
		RealChannelsNames.Add (rn);
		RefreshChannelsList ();

		if (NetworkManager == null && GameObject.Find ("NetworkManager") != null) {
			NetworkManager = GameObject.Find ("NetworkManager").GetComponent<MyNetworkManager> ();
		}

		if (MessageNotifyComponent == null && GameObject.Find ("MessageNotify-Canvas") != null) {
			MessageNotifyComponent = GameObject.Find ("MessageNotify-Canvas").GetComponent<MessageNotify> ();
		}

		if (POPUPWindowComponent == null && GameObject.Find ("POPUP-Canvas") != null) {
			POPUPWindowComponent = GameObject.Find ("POPUP-Canvas").GetComponent<MainMenuPOPUP> ();
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (!chatDisconnected) {
			if (chatClient != null) {
				chatClient.Service ();
				if (PhotonNetwork.connected && !nowConnecting && this.chatClient.State != ChatState.ConnectedToFrontEnd) {
					NetworkManager.ConnectChat ();
				}
			} else if (PhotonNetwork.connected && !nowConnecting) {
				NetworkManager.ConnectChat ();
			}
		}
	}

	public void GameChatConnect (string userName) {
		bool tryConnect = false;
		if (this.chatClient == null) {
			tryConnect = true;
		} else {
			if (this.chatClient.State != ChatState.ConnectedToFrontEnd &&
			    this.chatClient.State != ChatState.ConnectingToFrontEnd &&
				this.chatClient.State != ChatState.ConnectedToNameServer &&
				this.chatClient.State != ChatState.ConnectingToNameServer) {
				tryConnect = true;
			}
			Debug.Log ("GameChatConnect state: " + this.chatClient.State);
		}
		if (tryConnect)
		{
			nowConnecting = true;
			chatDisconnected = false;
			this.chatClient = new ChatClient (this);
			ExitGames.Client.Photon.Chat.AuthenticationValues authVals =
				new ExitGames.Client.Photon.Chat.AuthenticationValues (userName);
			authVals.AuthType = ExitGames.Client.Photon.Chat.CustomAuthenticationType.Custom;
			localPlayerId = ZPlayerPrefs.GetString ("playfabId");
			authVals.AddAuthParameter ("username", localPlayerId);
			authVals.AddAuthParameter ("token", ZPlayerPrefs.GetString ("token"));

			this.chatClient.AuthValues = authVals;
			this.chatClient.ChatRegion = "EU";
			Debug.Log ("Connect chat user: " + userName + ", id: " + localPlayerId + ", token: " + ZPlayerPrefs.GetString ("token"));
			this.chatClient.Connect (PhotonNetwork.PhotonServerSettings.ChatAppID, MainMenu.GameVersion, authVals);
		} else {
			Debug.LogWarning ("Chat already connected");
		}
	}

	public void GameChatDisconnect() {
		chatDisconnected = true;
		if (chatClient != null) {
			chatClient.Disconnect ();
		}
	}

	public void ShowChat() {
		Debug.Log ("show chat");
		ChatCanvas.enabled = true;
		ShowChatMessages ();
	}

	public void HideChat() {
		ChatCanvas.enabled = false;
		ClearChatWindow ();
	}

	public void StartPrivateChat(string user) {
		if (!GameChannels.Contains (user)) {
			GameChannels.Add (user);
			RealChannelName rn = new RealChannelName ();
			rn.userName = user;
			rn.channelName = "unknown";
			RealChannelsNames.Add (rn);
			RefreshChannelsList ();
		}
		if (!ChatCanvas.enabled) {
			ChannelsSwitchMenu.value = GameChannels.IndexOf (user);
			ChannelsSwitchMenu.RefreshShownValue ();
			SwitchChannel (ChannelsSwitchMenu.value);
		}
	}

	public void SetClientStatus(int status) {
		if (this.chatClient.State == ChatState.ConnectedToFrontEnd) {
			ClientCurrentStatus = status;
			chatClient.SetOnlineStatus (status);
		}
	}
		
	#region FriendChallange
	public void ShowChallangeNotify(string friendName) {
		challanger = friendName;
		if ((challangerId.Length > 0) && (ClientCurrentStatus == online)) {
			if (POPUPWindowComponent == null && GameObject.Find ("POPUP-Canvas") != null) {
				POPUPWindowComponent = GameObject.Find ("POPUP-Canvas").GetComponent<MainMenuPOPUP> ();
			}
			if (POPUPWindowComponent != null) {
				POPUPWindowComponent.SetupDialogPOPUPWindow (
					"Gracz <color=#00ffffff>'" + challanger + "'</color> zaprasza Cię do gry",
					"Przyjmij", "Odrzuć",
					ConfirmChallange, CancelChallange, CancelChallange);
			}
		}
	}

	public void SendChallangeRequest(string userID) {
		string msg = "req_chall[_command]" + PhotonNetwork.playerName;
		challanger = LocalFriendListComponent.GetNameByID (userID);
		chatClient.SendPrivateMessage (userID, msg);
	}

	private void ConfirmChallange() {
		string roomName = localPlayerId + challanger + PhotonNetwork.time.ToString ();
		roomName = roomName.Replace (".", string.Empty);
		string msg = "accept_chall[_command]" + roomName;
		NetworkManager.JoinToFiendsLobby (roomName, challanger);
		chatClient.SendPrivateMessage (challangerId, msg);
		challanger = "";
		challangerId = "";
	}

	private void CancelChallange() {
		string msg = "reject_chall[_command]" + localPlayerId;
		chatClient.SendPrivateMessage (challangerId, msg);
		challanger = "";
		challangerId = "";
	}
	#endregion

	#region PUNChatLogic
	public void DebugReturn(ExitGames.Client.Photon.DebugLevel level, string message) {
		Debug.Log ("Debug from chat[" + level + "]:" + message);
	}

	public void OnConnected() {
		chatClient.Subscribe (new string[] {
			GlobalChannelName
		}
		);
		SetClientStatus(online);
		nowConnecting = false;
		RefreshFriendList ();
	}

	public void OnDisconnected() {
		Debug.LogWarning ("Disconnected from chat");
		chatDisconnected = true;
	}

	public void OnChatStateChange(ChatState state) {
		Debug.Log ("Chat state changed:" + state);
		if (state == ChatState.Disconnected) {
			chatDisconnected = true;
			chatClient.Disconnect ();
		}
	}

	public void OnSubscribed(string[] channels, bool[] results) {
		foreach (string dbg_channel in channels) {
			Debug.Log ("Subscribed channel:" + dbg_channel);
		}
		WaitForSub = false;
		ClearChatWindow ();
		ShowChatMessages ();
	}

	public void OnUnsubscribed(string[] channels) {
		foreach (string dbg_channel in channels) {
			Debug.Log ("Unsubsribed channel:" + dbg_channel);
		}
	}

	public void OnGetMessages(string channelName, string[] senders, object[] messages) {
		int msgCount = messages.Length;
		for (int i = 0; i < msgCount; i++) {
			string sender = senders [i];
			string msg = (string)messages [i];
			Debug.Log ("new message:ch" + channelName + ":" + sender + ": " + msg);
			//string senderName = ""; meybe I will use that later
			//string[] msgs =  msg.Split (new string[] {"[_nickname]", "[_target]"}, StringSplitOptions.RemoveEmptyEntries);
			//if (msgs.Length >= 1) {
			//	senderName = msgs [1];
			//}
			if (ChannelsSwitchMenu.value == 0) {
				if (ChatCanvas.enabled) {
					PrintChatMessage (sender, msg);
				}
			}
		}
	}

	public void OnPrivateMessage(string sender, object message, string channelName) {
		Debug.Log ("Receive private message["+ channelName + "]:" + sender + ": " + message);
		string msg = (string)message;
		if (msg.Contains("[_command]")) {
			if (!sender.Equals (localPlayerId)) {
				string command = "";
				string param = "";
				string[] msgs = msg.Split (new string[] { "[_command]" }, StringSplitOptions.RemoveEmptyEntries);
				if (msgs.Length >= 1) {
					command = msgs [0];
				}
				if (msgs.Length >= 2) {
					param = msgs [1];
					if (command.Equals ("req_chall")) {
						//show notify and check that challanger is your friend
						challangerId = sender;
						if (LocalFriendListComponent.GetIDByName (param).Length == 0) {
							LocalFriendListComponent.AddFriendRequest (param);
							//CancelChallange ();
						} else {
							ShowChallangeNotify (param);
						}
					}
					if (command.Equals ("accept_chall")) {
						if (param.Length > 0) {
							NetworkManager.JoinToFiendsLobby (param, challanger);
						}
					}
					if (command.Equals ("reject_chall")) {
						LocalFriendListComponent.ChallageCancel (param);
					}
				}
			}
		} else {
			string senderName = "";
			string targetName = "";
			string[] msgs =  msg.Split (new string[] {"[_nickname]", "[_target]"}, StringSplitOptions.RemoveEmptyEntries);
			if (msgs.Length >= 1) {
				senderName = msgs [1];
			}
			if (msgs.Length >= 2) {
				targetName = msgs [2];
			}
			RealChannelName rn = null;
			if (!senderName.Equals (PhotonNetwork.playerName)) {
				StartPrivateChat (senderName);
				rn = GetRealChannelByUser (senderName);
				if (GameChannels [ChannelsSwitchMenu.value].Equals (senderName)) {
					PrintChatMessage (sender, msg);
				}
				if (LocalFriendListComponent.GetIDByName (senderName).Length == 0) {
					LocalFriendListComponent.AddFriendRequest (senderName);
				}
			} else {
				rn = GetRealChannelByUser (targetName);
				if (GameChannels [ChannelsSwitchMenu.value].Equals (targetName)) {
					PrintChatMessage (sender, msg);
				}
			}
			if (!ChatCanvas.enabled) {
				UnreadedMessages++;
				rn.unreadMessages++;
				ChannelsSwitchMenu.value = GameChannels.IndexOf (senderName);
				RefreshNotifyCounter ();
			}
			if (rn != null) {
				rn.chatHistory.Add (msg);
			}
		}
	}

	public void OnStatusUpdate(string user, int status, bool gotMessage, object message) {
		Debug.Log ("User: " + user + " update status: " + status);
		if (status == online) {
			LocalFriendListComponent.SetFriendOnlineStatus(user);
		} else if (status == ChatUserStatus.Offline) {
			LocalFriendListComponent.SetFriendOfflineStatus(user);
		} else if (status == playing) {
			LocalFriendListComponent.SetFriendInGameStatus(user);
		}
	}
	#endregion

	/*private void AddSubscription() {
		WaitForSub = true;
		List<string> chList = new List<string> ();
		foreach (RealChannelName RN in RealChannelsNames) {
			chList.Add (RN.channelName);
		}
		chatClient.Subscribe (chList.ToArray());
	}*/

	private void RefreshNotifyCounter() {
		if (UnreadedMessages > 0) {
			NotifyText.text = UnreadedMessages.ToString ();
		} else {
			NotifyText.text = "";
			UnreadedMessages = 0;
		}
	}

	public void RefreshFriendList() {
		Debug.Log ("Refresh chat friends");
		if (this.chatClient == null) {
			NetworkManager.ConnectChat ();
		}
		if (this.chatClient != null) {
			if (this.chatClient.State == ChatState.ConnectedToFrontEnd) {
				List<string> chatList = new List<string> ();
				foreach (var friend in LocalFriendListComponent.SpaceNationFriendList) {
					chatList.Add (friend.playfabId);
					Debug.Log ("Add chat friends:" + friend.playfabId);
				}
				this.chatClient.AddFriends (chatList.ToArray ());
			}
		}
	}

	public void SwitchChannel(int channelIndex) {
		Debug.Log ("Switch to channel index:" + channelIndex + ": " + GameChannels[channelIndex]);
		ShowChatMessages ();
	}

	public void SetMessageText(string message) {
		messageToSend = message;
	}

	public void SendMessage() {
		if (messageToSend.Length > 0) {
			messageToSend = messageToSend + "[_nickname]" + PhotonNetwork.playerName;
			if (ChannelsSwitchMenu.value == 0) {
				chatClient.PublishMessage (GlobalChannelName, messageToSend);
			} else {
				string target = LocalFriendListComponent.GetIDByName (GameChannels [ChannelsSwitchMenu.value]);
				messageToSend = messageToSend + "[_target]" + GameChannels [ChannelsSwitchMenu.value];
				Debug.Log ("Sending private message to:" + target);
				chatClient.SendPrivateMessage (target, messageToSend);
			}
			messageToSend = "";
			ChatInputField.text = "";
			#if UNITY_EDITOR || UNITY_STANDALONE
			ChatInputField.Select();
			ChatInputField.ActivateInputField();
			#endif
		}
	}

	public void PrintChatMessage(string sender, string message) {
		GameObject ChatTextOb = Instantiate (ChatMessagePrefab);
		string[] msgs =  message.Split (new string[] {"[_nickname]", "[_target]"}, StringSplitOptions.RemoveEmptyEntries);
		string senderName = "";
		if (msgs.Length > 1) {
			senderName = msgs [1] + ": ";
		}
		ChatTextOb.GetComponent<Text> ().text = "<color=#00ffffff>" + senderName + "</color>" + msgs[0];
		if (!ChatCanvas.enabled) {
			if (MessageNotifyComponent == null && GameObject.Find ("MessageNotify-Canvas") != null) {
				MessageNotifyComponent = GameObject.Find ("MessageNotify-Canvas").GetComponent<MessageNotify> ();
			}
			if (MessageNotifyComponent != null) {
				MessageNotifyComponent.ShowNotifyMessage (senderName + msgs [0]);
				if (MessageNotifyComponent.Btn != null) {
					MessageNotifyComponent.Btn.onClick.RemoveAllListeners ();
					MessageNotifyComponent.Btn.onClick.AddListener (MessageNotifyComponent.HideNotify);
					if (sender.Length > 0) {
						MessageNotifyComponent.Btn.onClick.AddListener (ShowChat);
					}
				}
			}
		}
		Canvas.ForceUpdateCanvases();
		ChatTextOb.transform.SetParent (ChatMessagesTable, false);
		scrollRect.verticalScrollbar.value=0f;
		ChatMessagesTable.GetComponent<VerticalLayoutGroup> ().padding.top = 18;
		Canvas.ForceUpdateCanvases();
		scrollRect.verticalNormalizedPosition = 0f;;
		Canvas.ForceUpdateCanvases();
	}

	private void ShowChatMessages() {
		if (!WaitForSub && ChatCanvas.enabled) {
			Debug.Log ("ShowChatMessages");
			ChatChannel ch = null;
			ClearChatWindow ();
			if (ChannelsSwitchMenu.value == 0) {
				Debug.Log (" get cache in private channels ");
				ch = this.chatClient.PublicChannels [GlobalChannelName];
				if (ch != null) {
					for (int i = 0; i < ch.MessageCount; i++) {
						string sender = ch.Senders [i];
						string msg = (string)ch.Messages [i];
						Debug.Log ("Get message from cache - channel:" +
							GameChannels [ChannelsSwitchMenu.value] + ", sender:" + sender + ", msg:" + msg);
						PrintChatMessage (sender, msg);
					}
				}
			} else {
				RealChannelName rn = GetRealChannelByUser (GameChannels [ChannelsSwitchMenu.value]);
				if (rn != null) {
					foreach (string nextMessage in rn.chatHistory) {
						PrintChatMessage ("chat_history", nextMessage);
					}
					UnreadedMessages -= rn.unreadMessages;
					rn.unreadMessages = 0;
					RefreshNotifyCounter ();
				} else {
					Debug.LogError ("Cannot get history for private chat with: " + GameChannels [ChannelsSwitchMenu.value]);
				}
			}
			foreach (var key in this.chatClient.PrivateChannels.Keys) {
				Debug.Log (" key in private channels: " + key);
			}
			foreach (var key in this.chatClient.PublicChannels.Keys) {
				Debug.Log (" key in public channels: " + key);
			}
		}
	}

	private string GetChannelNameByUser(string name) {
		string rv = "";
		foreach (RealChannelName RN in RealChannelsNames) {
			if (RN.userName.Equals (name)) {
				rv = RN.channelName;
				break;
			}
		}
		return rv;
	}

	private RealChannelName GetRealChannelByUser(string name) {
		RealChannelName rv = null;
		foreach (RealChannelName RN in RealChannelsNames) {
			if (RN.userName.Equals (name)) {
				rv = RN;
				break;
			}
		}
		return rv;
	}

	private void ClearChatWindow() {
		foreach (Transform childTransform in ChatMessagesTable)
			Destroy (childTransform.gameObject);
	}

	private void RefreshChannelsList() {
		ChannelsSwitchMenu.ClearOptions ();
		ChannelsSwitchMenu.AddOptions (GameChannels);
		ChannelsSwitchMenu.RefreshShownValue ();
	}

	void OnApplicationQuit() {
		Debug.Log ("Disconnect chat on app quit");
		// probably hangs the app
		if (chatClient != null) {
			chatClient.Disconnect ();
		}
	}
}
