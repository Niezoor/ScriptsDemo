using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendPanel : MonoBehaviour {
	public Text FriendNameText;
	public Text FriendStatusText;
	public Image AvatarImage;
	public FriendListHandler FriendListHandlerComponent;

	public Color OnlineColor;
	public Color OfflineColor;

	public System.DateTime lastLogin;

	[System.Serializable]
	public enum LocalFriendStatuses
	{
		ONLINE,
		OFFLINE,
		INGAME,
		NONE,
	};

	public LocalFriendStatuses localFriendStatus;
	public LocalFriendStatuses __test_StatusToSet = LocalFriendStatuses.NONE;
	private GameObject optionPanel;
	public FriendOptionsPanel OptionsPanelComponent;
	public bool RemovePending = false;
	// Use this for initialization
	void Start () {
		localFriendStatus = LocalFriendStatuses.NONE;
		SetStatusOffline ();
	}
	
	// Update is called once per frame
	void Update () {
		if (__test_StatusToSet == LocalFriendStatuses.ONLINE) {
			SetStatusOnline ();
		}
		if (__test_StatusToSet == LocalFriendStatuses.OFFLINE) {
			SetStatusOffline ();
		}
		if (__test_StatusToSet == LocalFriendStatuses.INGAME) {
			SetStatusInGame ();
		}
		__test_StatusToSet = LocalFriendStatuses.NONE;
	}

	public void SetStatusOnline() {
		if (localFriendStatus != LocalFriendStatuses.ONLINE) {
			Debug.Log ("Friend: " + FriendNameText.text + " status ONLINE");
			localFriendStatus = LocalFriendStatuses.ONLINE;
			FriendNameText.color = OnlineColor;
			FriendStatusText.text = "";
			RefreshOptionsPanel ();
		}
	}

	public void SetStatusInGame() {
		if (localFriendStatus != LocalFriendStatuses.INGAME) {
			Debug.Log ("Friend: " + FriendNameText.text + " status INGAME");
			localFriendStatus = LocalFriendStatuses.INGAME;
			FriendNameText.color = OnlineColor;
			FriendStatusText.text = "w grze";
			RefreshOptionsPanel ();
		}
	}

	public void SetStatusOffline() {
		if (localFriendStatus != LocalFriendStatuses.OFFLINE) {
			Debug.Log ("Friend: " + FriendNameText.text + " status OFFLINE");
			FriendNameText.color = OfflineColor;
			localFriendStatus = LocalFriendStatuses.OFFLINE;
			UpdateOfflineTime ();
			RefreshOptionsPanel ();
		}
	}

	public void UpdateOfflineTime() {
		if (FriendListHandlerComponent != null) {
			string lastLoginText = "Ostatnie zalogowanie\n";
			System.TimeSpan diffRes = FriendListHandlerComponent.CurrentTime - lastLogin;
			if (diffRes.Days > 0) {
				if (diffRes.Days == 1) {
					lastLoginText += " dzien temu";
				} else {
					lastLoginText += diffRes.Days + " dni temu";
				}
			} else if (diffRes.Hours > 0) {
				if (diffRes.Hours == 1) {
					lastLoginText += " godzine temu";
				} else {
					lastLoginText += diffRes.Hours + " godzin temu";
				}
			} else if (diffRes.Minutes > 0) {
				if (diffRes.Minutes == 1) {
					lastLoginText += " minute temu";
				} else {
					lastLoginText += diffRes.Minutes + " minut temu";
				}
			}
			FriendStatusText.text = lastLoginText;
		}
	}

	public void ShowOptionsPanel() {
		if (optionPanel == null) {
			FriendListHandlerComponent.HideAllPanelsOptions ();
			optionPanel = Instantiate (FriendListHandlerComponent.FriendOptionsPanelPrefab);
			OptionsPanelComponent = optionPanel.GetComponent<FriendOptionsPanel> ();
			RefreshOptionsPanel ();
			optionPanel.transform.SetParent (FriendListHandlerComponent.PanelsTable, false);
			optionPanel.transform.SetSiblingIndex (this.transform.GetSiblingIndex () + 1);
			OptionsPanelComponent.ChatButton.onClick.AddListener (ChatWithFriend);
			OptionsPanelComponent.ChallangeButton.onClick.AddListener (ChallengeFriend);
			OptionsPanelComponent.StatsButton.onClick.AddListener (ShowFriendProfile);
			OptionsPanelComponent.RemoveButton.onClick.AddListener (RemoveFriend);
			if (RemovePending) {
				OptionsPanelComponent.RemoveButton.interactable = false;
			}
		} else {
			HideOptionsPanel ();
		}
	}

	private void RefreshOptionsPanel() {
		if (OptionsPanelComponent != null) {
			if (localFriendStatus == LocalFriendStatuses.ONLINE) {
				OptionsPanelComponent.ChatButton.interactable = true;
				OptionsPanelComponent.ChallangeButton.interactable = true;
			} else if (localFriendStatus == LocalFriendStatuses.INGAME) {
				OptionsPanelComponent.ChatButton.interactable = true;
				OptionsPanelComponent.ChallangeButton.interactable = false;
			} else {
				OptionsPanelComponent.ChatButton.interactable = false;
				OptionsPanelComponent.ChallangeButton.interactable = false;
			}
		}
	}

	private void ChatWithFriend() {
		Debug.Log ("Start chat with friend:" + FriendNameText.text);
		FriendListHandlerComponent.StartChatWithFriend (FriendNameText.text);
	}

	private void ChallengeFriend() {
		Debug.Log ("Challange friend:" + FriendNameText.text);
		FriendListHandlerComponent.ChallangeFriend (FriendNameText.text);
		OptionsPanelComponent.ChallangeButton.interactable = false;
	}

	private void ShowFriendProfile() {
		FriendListHandlerComponent.GetPlayerStats (FriendListHandlerComponent.GetIDByName (FriendNameText.text));
	}

	private void RemoveFriend() {
		Debug.Log ("Remove friend:" + FriendNameText.text);
		RemovePending = true;
		OptionsPanelComponent.RemoveButton.interactable = false;
		FriendListHandlerComponent.RemoveLocalFriend (FriendNameText.text);
	}

	public void RemovePanel() {
		HideOptionsPanel ();
		Destroy (this.gameObject);
	}

	public void HideOptionsPanel() {
		if (optionPanel != null) {
			Destroy (optionPanel);
		}
	}
}
