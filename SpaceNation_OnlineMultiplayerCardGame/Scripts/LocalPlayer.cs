using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;

public class LocalPlayer : MonoBehaviour {
	public MyNetworkManager myNetManager;

	public bool DataLoaded = false;
	public bool DataSaveing = false;

	public string PlayerDisplayName;

	public PlayerData localPlayerData;
	public LocalPlayerData PlayerDataComponent;
	private MyPlayerStatsInfo PlayerInfo;

	[System.Serializable]
	public class GameAvatar
	{
		public Sprite image;
		public string desc;
	};

	public List<GameAvatar> Avatars = new List<GameAvatar>();

	public const string TutorialFlag = "TutorialDone";
	public const string PrebuildDecksFlag = "PremadeDecksLoaded";
	public static int ExpPerLevel;

	public static LocalPlayer Instace { get; private set;}

	private void Awake() {
		if (Instace == null) {
			Instace = this;
			DontDestroyOnLoad (gameObject);
		} else {
			Destroy (gameObject);
		}
	}

	// Use this for initialization
	void Start () {
		FindComponents ();
		PlayFabClientAPI.GetTitleData( new GetTitleDataRequest(), LoadTitleData,
			error => Debug.LogError(error.GenerateErrorReport()));
		// call by MyPlayerStatsInfo //GetPlayerProfile();
	}
	
	// Update is called once per frame
	void Update () {

	}

	private void FindComponents() {
		if (PlayerDataComponent == null && GameObject.Find ("LocalPlayerData") != null) {
			PlayerDataComponent = GameObject.Find ("LocalPlayerData").GetComponent<LocalPlayerData> ();
		}
		if (PlayerInfo == null && GameObject.Find ("PlayerInfo") != null) {
			PlayerInfo = GameObject.Find ("PlayerInfo").GetComponent<MyPlayerStatsInfo> ();
		}
		if (myNetManager == null && GameObject.Find ("NetworkManager") != null) {
			myNetManager = GameObject.Find ("NetworkManager").GetComponent<MyNetworkManager> ();
		}
	}

	private void LoadTitleData(GetTitleDataResult result) {
		if (result != null) {
			ExpPerLevel = Int32.Parse(result.Data ["ExpPerLevel"]);
		}
	}

	public void Logout() {
		localPlayerData.logout = true;
		SavePlayerData ();
	}

	public void SavePlayerData() {
		if (PlayerDataComponent != null) {
			PlayerDataComponent.SaveData (localPlayerData);
		}
	}

	public void GetPlayerProfile() {
		PlayFabClientAPI.GetPlayerProfile( new GetPlayerProfileRequest() {
			PlayFabId = ZPlayerPrefs.GetString ("playfabId"),
			ProfileConstraints = new PlayerProfileViewConstraints() {
				ShowDisplayName = true,
				ShowStatistics = true,
				ShowAvatarUrl = true
			}
		}, LoadPlayerData,
		error => Debug.LogError(error.GenerateErrorReport()));
	}

	public void SavePlayerFlag(string flag, int value) {
		UpdateUserDataRequest req = new UpdateUserDataRequest ();
		req.Data = new Dictionary<string, string> ();
		req.Data.Add (flag, value.ToString());

		DataSaveing = true;
		PlayFabClientAPI.UpdateUserData (req, flagSaved, OnPlayFabError);
	}

	private void flagSaved(UpdateUserDataResult result) {
		DataSaveing = false;
		Debug.Log ("user flag saved");
	}

	public static void ParsePlayfabStats(List<StatisticModel> statistic, PlayerData data) {
		foreach (StatisticModel stat in statistic) {
			if (stat.Name.Equals ("Level")) {
				data.level = stat.Value;
			} else if (stat.Name.Equals ("exp")) {
				data.exp = stat.Value;
			} else if (stat.Name.Equals ("Rang")) {
				data.rank = stat.Value;
			} else if (stat.Name.Equals ("qp_wins")) {
				data.qp_wins = stat.Value;
			} else if (stat.Name.Equals ("qp_losses")) {
				data.qp_losses = stat.Value;
			} else if (stat.Name.Equals ("qp_games_guard")) {
				data.qp_games_guard = stat.Value;
			} else if (stat.Name.Equals ("qp_games_pirate")) {
				data.qp_games_pirate = stat.Value;
			} else if (stat.Name.Equals ("Wins")) {
				data.wins = stat.Value;
			} else if (stat.Name.Equals ("Looses")) {
				data.looses = stat.Value;
			} else if (stat.Name.Equals ("tr_wins")) {
				data.tr_wins = stat.Value;
			} else if (stat.Name.Equals ("tr_losses")) {
				data.tr_losses = stat.Value;
			} else if (stat.Name.Equals ("tr_games_guard")) {
				data.tr_games_guard = stat.Value;
			} else if (stat.Name.Equals ("tr_games_pirate")) {
				data.tr_games_pirate = stat.Value;
			} else if (stat.Name.Equals ("rk_games_guard")) {
				data.rk_games_guard = stat.Value;
			} else if (stat.Name.Equals ("rk_games_pirate")) {
				data.rk_games_pirate = stat.Value;
			}
		}
	}

	public void LoadPlayerData(GetPlayerProfileResult result) {
		FindComponents ();
		if (PlayerDataComponent != null) {
			PlayerDataComponent.LoadData (localPlayerData);
		}
		if (result != null) {
			Debug.Log ("Get profile: " + result.PlayerProfile.DisplayName);
			PlayerDisplayName = result.PlayerProfile.DisplayName;
			localPlayerData.dispName = PlayerDisplayName;
			if (result.PlayerProfile.AvatarUrl != null && result.PlayerProfile.AvatarUrl.Length > 0) {
				SetAvatar (result.PlayerProfile.AvatarUrl);
			} else {
				int rand_av = UnityEngine.Random.Range (0, 5);
				SetAvatar (rand_av.ToString());
				PlayFabClientAPI.UpdateAvatarUrl (new UpdateAvatarUrlRequest(){
					ImageUrl = rand_av.ToString()
				},null, error => Debug.LogError(error.GenerateErrorReport()));
			}

			if (result.PlayerProfile.Statistics != null) {
				ParsePlayfabStats (result.PlayerProfile.Statistics, localPlayerData);
			} else {
				Debug.LogWarning ("Cannot get player statistics");
			}
		}
		if (PlayerInfo != null) {
			if (PlayerInfo.PlayerNameText != null) {
				PlayerInfo.PlayerNameText.text = localPlayerData.dispName;
			}
			if (PlayerInfo.PlayerLVLText != null) {
				PlayerInfo.PlayerLVLText.text = localPlayerData.level.ToString ();
			}
		}
		if (myNetManager != null) {
			myNetManager.SetPlayerName (PlayerDisplayName);
		}
		DataLoaded = true;
		SavePlayerData ();
	}

	public void SetAvatar(Image imageComponent, string av_name) {
		GameAvatar av = null;
		if (Avatars.Count > 0) {
			av = Avatars.Find (x => x.image.name.Equals (av_name));
			if (av == null) {
				av = Avatars [0];
			}
		}
		if (av != null) {
			imageComponent.sprite = av.image;
		}
	}

	public void SetAvatar(string av_name) {
		if (PlayerInfo != null) {
			SetAvatar (PlayerInfo.AvatarImage, av_name);
			PlayerInfo.GetComponent<Canvas> ().enabled = true;
		}
		localPlayerData.avatar = av_name;
	}

	#region SAVE_STATS
	private void addStat(List <StatisticUpdate> statList, string name, int value) {
		StatisticUpdate item = new StatisticUpdate ();
		item.StatisticName = name;
		item.Value = value;
		statList.Add (item);
	}

	public void StorePlayerStats(bool updateLVL, bool updateQP, bool updateRank, bool updateTR) {
		if (DataLoaded) {
			List <StatisticUpdate> stats = new List<StatisticUpdate>();
			UpdatePlayerStatisticsRequest request = new UpdatePlayerStatisticsRequest();
			DataSaveing = true;

			if (updateLVL) {
				addStat (stats, "Level", localPlayerData.level);
				addStat (stats, "exp", localPlayerData.exp);
			}
			if (updateQP) {
				addStat (stats, "qp_wins", localPlayerData.qp_wins);
				addStat (stats, "qp_losses", localPlayerData.qp_losses);
			}
			if (updateRank) {
				addStat (stats, "Rang", localPlayerData.rank);
				addStat (stats, "Wins", localPlayerData.wins);
				addStat (stats, "Looses", localPlayerData.looses);
			}
			if (updateTR) {
				addStat (stats, "tr_wins", localPlayerData.tr_wins);
				addStat (stats, "tr_losses", localPlayerData.tr_losses);
			}

			request.Statistics = stats;
			PlayFabClientAPI.UpdatePlayerStatistics (request, StatsUpdated, OnPlayFabError);
		}
	}

	private void StatsUpdated(UpdatePlayerStatisticsResult result) {
		DataSaveing = false;
		Debug.Log ("Stats updated");
	}
	#endregion

	private void OnPlayFabError(PlayFabError obj) {
		DataSaveing = false;
		Debug.LogError ("Stats cannot be updated");
	}
}
