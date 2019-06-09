using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerProfile : MonoBehaviour {

	public Canvas ProfileCanvas;
	public Canvas LoadingCanvas;

	public Text PlayerNameText;
	public Text PlayerLevelText;
	public Text PlayerExpText;
	public ProgressBar ExperienceBar;

	public RatePanel QuickGameRatePanel;

	public Color WinsColor;
	public Color LoosesColor;

	public Dropdown GameModesDropDown;
	public Transform UsagePanelTable;
	public GameObject GuardUsagePanel;
	public GameObject PirateUsagePanel;

	[System.Serializable]
	public struct GameModeStat {
		public string Name;
		public MyNetworkManager.gameModeEnum Mode;
	}
	public List<GameModeStat> gameModesList = new List<GameModeStat> ();
	public int CurrentModeID = 0;
	public PlayerData CurrData;

	public bool Loading = true;

	public Button ChangeAvatarBtn;
	public Text ChangeAvatarBtnText;
	public GameObject ChangeAvatarMenuPrefab;
	//public LocalPlayer Player;
	public Image AvatarImage;

	private bool RateSetup = false;
	private int ExpMax;

	void Awake () {
		LoadingCanvas.enabled = true;
		ProfileCanvas.enabled = false;
	}

	// Use this for initialization
	void Start () {
		//SetupStats ();
		//ShowPlayerProfile();
		List<Dropdown.OptionData> opts = new List<Dropdown.OptionData>();
		foreach (GameModeStat mode in gameModesList) {
			Dropdown.OptionData data = new Dropdown.OptionData(mode.Name);
			opts.Add (data);
		}
		GameModesDropDown.AddOptions (opts);
		/*if (Player == null && GameObject.Find ("Player") != null) {
			Player = GameObject.Find ("Player").GetComponent<LocalPlayer> ();
		}*/
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void ShowChangeAvatarMenu() {
		/*if (Player == null && GameObject.Find ("Player") != null) {
			Player = GameObject.Find ("Player").GetComponent<LocalPlayer> ();
		}*/
		if (LocalPlayer.Instace != null) {
			GameObject gob = Instantiate (ChangeAvatarMenuPrefab, Camera.main.transform);
			gob.GetComponent<Canvas> ().worldCamera = Camera.main;
			gob.GetComponent<AvatarMenu> ().SetupTable (LocalPlayer.Instace.Avatars);
			ClosePlayerProfile ();
		}
	}

	public void LoadPlayerProfile(PlayerData data, bool showChangeAvatarMenu = false) {
		if (PlayerNameText != null) {
			PlayerNameText.text = data.dispName;
		}
		if (PlayerExpText != null) {
			PlayerExpText.text = data.exp.ToString ();
		}
		if (PlayerLevelText != null) {
			PlayerLevelText.text = data.level.ToString ();
		}
		if (ExperienceBar != null) {
			ExperienceBar.SetMax (LocalPlayer.ExpPerLevel);
			ExperienceBar.SetProgress (data.exp);
			ExperienceBar.SetAfterText ("/" + LocalPlayer.ExpPerLevel);
		}

		/*if (Player == null && GameObject.Find ("Player") != null) {
			Player = GameObject.Find ("Player").GetComponent<LocalPlayer> ();
		}*/
		if (LocalPlayer.Instace != null) {
			LocalPlayer.Instace.SetAvatar (AvatarImage, data.avatar);
		}

		CurrData = data;

		RefreshStats (data);
		LoadingCanvas.enabled = false;
		ProfileCanvas.enabled = true;

		if (showChangeAvatarMenu) {
			ChangeAvatarBtn.onClick.AddListener (ShowChangeAvatarMenu);
		} else {
			ChangeAvatarBtn.interactable = false;
			ChangeAvatarBtnText.text = "";
		}
	}

	private int GetWins(PlayerData data) {
		MyNetworkManager.gameModeEnum mode = gameModesList [CurrentModeID].Mode;
		if (mode == MyNetworkManager.gameModeEnum.quickPlay) {
			Debug.Log (" get qp_wins");
			return data.qp_wins;
		} else if (mode == MyNetworkManager.gameModeEnum.training) {
			Debug.Log (" get tr_wins");
			return data.tr_wins;
		} else if (mode == MyNetworkManager.gameModeEnum.ranked) {
			Debug.Log (" get wins");
			return data.wins;
		}
		Debug.Log (" get default");
		return data.qp_wins;
	}

	private int GetLosts(PlayerData data) {
		MyNetworkManager.gameModeEnum mode = gameModesList [CurrentModeID].Mode;
		if (mode == MyNetworkManager.gameModeEnum.quickPlay) {
			return data.qp_losses;
		} else if (mode == MyNetworkManager.gameModeEnum.training) {
			return data.tr_losses;
		} else if (mode == MyNetworkManager.gameModeEnum.ranked) {
			return data.looses;
		}
		return data.qp_losses;
	}

	private void SetupStats(PlayerData data) {
		int wins = GetWins (data);
		int lost = GetLosts (data);
		QuickGameRatePanel.SetTitle(gameModesList [CurrentModeID].Name);
		QuickGameRatePanel.AddRateElem ("Wygrane", wins, WinsColor);
		QuickGameRatePanel.AddRateElem ("Przegrane", lost, LoosesColor);
		QuickGameRatePanel.ShowPercentage ();
		RateSetup = true;
	}

	private void SetUsageBar(ProgressBar Bar, int usage, int max) {
		Debug.Log(" setup usage (" + usage + "/" + max  + ")");
		if (Bar != null) {
			Bar.SetMax (max);
			Bar.SetProgress (usage);
		}
	}

	private void SetupUsegeBars(PlayerData data) {
		int pirate = 0, guard = 0, max = 0;
		MyNetworkManager.gameModeEnum mode = gameModesList [CurrentModeID].Mode;
		ProgressBar GuardUsageBar = null;
		ProgressBar PirateUsageBar = null;

		foreach (Transform child in UsagePanelTable) {
			GameObject.Destroy(child.gameObject);
		}

		if (mode == MyNetworkManager.gameModeEnum.quickPlay) {
			guard = data.qp_games_guard;
			pirate = data.qp_games_pirate;
		} else if (mode == MyNetworkManager.gameModeEnum.training) {
			guard = data.tr_games_guard;
			pirate = data.tr_games_pirate;
		} else if (mode == MyNetworkManager.gameModeEnum.ranked) {
			guard = data.rk_games_guard;
			pirate = data.rk_games_pirate;
		}

		if (pirate > guard) {
			max = pirate;
			PirateUsageBar = Instantiate (PirateUsagePanel, UsagePanelTable).GetComponent<HeroUsagePanel> ().UsageBar;
			GuardUsageBar = Instantiate (GuardUsagePanel, UsagePanelTable).GetComponent<HeroUsagePanel> ().UsageBar;
		} else {
			max = guard;
			GuardUsageBar = Instantiate (GuardUsagePanel, UsagePanelTable).GetComponent<HeroUsagePanel> ().UsageBar;
			PirateUsageBar = Instantiate (PirateUsagePanel, UsagePanelTable).GetComponent<HeroUsagePanel> ().UsageBar;
		}

		if (max < 1) {
			max = 1;
		}

		SetUsageBar (GuardUsageBar, guard, max);
		SetUsageBar (PirateUsageBar, pirate, max);
	}

	public void RefreshStats(PlayerData data) {
		if (!RateSetup) {
			SetupStats (data);
		} else {
			int wins = GetWins (data);
			int lost = GetLosts (data);
			QuickGameRatePanel.SetTitle(gameModesList [CurrentModeID].Name);
			QuickGameRatePanel.ModifyRateElem ("Wygrane", wins);
			QuickGameRatePanel.ModifyRateElem ("Przegrane", lost);
			QuickGameRatePanel.ShowPercentage ();
		}
		SetupUsegeBars (data);
		QuickGameRatePanel.PlayAnim ();
	}

	public void ShowPlayerProfile() {
		LoadingCanvas.enabled = true;
	}

	public void ClosePlayerProfile() {
		ProfileCanvas.enabled = false;
		Destroy (this.gameObject);
	}

	public void SwitchGameMode(int modeId) {
		CurrentModeID = modeId;
		RefreshStats (CurrData);
	}
}
