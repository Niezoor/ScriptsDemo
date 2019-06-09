using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PlayFab.ClientModels;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;

public class MainMenu : MonoBehaviour {
	public static string GameVersion = "Beta v1.4.0";
	[Header("Menu canvases")]
	public Canvas BackGroundCanvas;
	public Canvas MainMenuCanvas;
	public Canvas WaitForGameCanvas;
	public Canvas GameModeSelectCanvas;
	public Canvas DeckSelectMenuCanvas;
	public Canvas DeckChooseMenuCanvas;
	public Canvas HeroSelectCanvas;
	public Canvas DeckBuildCanvas;
	public Canvas DeckBuildCardsSlotCanvas;
	public Canvas DeckBuildCardsNumberCanvas;
	public Canvas CollectionCanvas;
	public Canvas CannectingCanvas;
	public Canvas DisconnectedCanvas;
	public Canvas FriendLobbyCanvas;
	public Canvas PlayerHUD;
	public Canvas WaitForDataScreen;
	public Canvas CornerMenuCanvas;
	public Canvas CardsFilterCanvas;
	[HideInInspector]
	public Canvas SettingsCanvas;

	[Header("Game main components")]
	public MainMenuPOPUP POPUPWindow;
	private GameObject LootMenuObject;

	public List<Canvas> CurrentCanvas = new List<Canvas>();
	public MyNetworkManager MyNetManager;
	public LocalPlayer PlayerComponent;
	public CardsBaseTableController CardsTableControllerComponent;
	public HeroesBase HeroesBaseComponent;
	public DeckBase DeckBaseComponent;
	public Text VersionText;
	public FriendLobby FriendLobbyComponent;
	public Text challangerFriendNameText;
	public Button PlayButton;
	public GameObject LootMenuOb;
	public GameObject ShopMenuPrefab;
	public Sprite CreditCurrencyRewardImage;

	[Header("Prefabs")]
	public GameObject CardBasePrefab;
	private GameObject CardBase;
	public GameObject SettingsCanvasPrefab;
	private GameObject SettingsCanvasObject;
	public GameObject NetManagerPrefab;
	public GameObject CornerMenuPrefab;

	[Header("variables")]
	public bool DeckChooseMode = false;
	public bool ChooseGameMode = false;
	public bool ConnectingScreen = false;
	private bool waitForLeftRoom = false;

	[Header("notifications")]
	public GameObject NewCardsNotify;
	public Text NewCardsNotifyText;
	public GameObject NewRewardsNotify;
	public Text NewRewardsNotifyText;

	[Header("items")]
	public List<ItemInstance> CardsPacks = new List<ItemInstance>();
	public List<ItemInstance> AllItems = new List<ItemInstance>();
	public int CreditCurrency;

	void Awake() {
		GameObject netOb = GameObject.Find ("NetworkManager");
		if (netOb == null) {
			netOb = Instantiate (NetManagerPrefab);
		}
		netOb.name = "NetworkManager";
		SetupPlayButtonNormal ();
		MyNetManager = netOb.GetComponent<MyNetworkManager> ();
		if (GameObject.Find ("CardsBase") == null) {
			MyNetManager.CardBase = Instantiate (CardBasePrefab);
			MyNetManager.CardBase.name = "CardsBase";
		} else {
			MyNetManager.CardBase = GameObject.Find ("CardsBase");
		}
		CardBase = MyNetManager.CardBase;
		DeckBaseComponent.CardsBaseComponent = CardBase.GetComponent<CardsBase> ();
		CardsTableControllerComponent.DeckTableControllerComponent.DeckViewControllerComponent.CardBaseComponent = DeckBaseComponent.CardsBaseComponent;
		CardsTableControllerComponent.CardsBaseComponent = DeckBaseComponent.CardsBaseComponent;
		MyNetManager.HeroBase = HeroesBaseComponent.gameObject;
		VersionText.text = GameVersion;
		MyNetManager.gameVersionText = VersionText;
		MyNetManager.MainMenuComponent = GetComponent<MainMenu> ();
		MyNetManager.FriendLobbyComponent = FriendLobbyComponent;
		if (MyNetManager.CornerMenuObject == null) {
			MyNetManager.CornerMenuObject = Instantiate (CornerMenuPrefab, Camera.main.transform);
		}
		CornerMenuCanvas = MyNetManager.CornerMenuObject.GetComponent<Canvas> ();
		CornerMenuCanvas.worldCamera = Camera.main;
		CornerMenuCanvas.planeDistance = 10;
	}

	void Start() {
		WaitForGameCanvas.enabled = false;
		DisconnectedCanvas.enabled = false;
		GameModeSelectCanvas.enabled = false;
		DeckSelectMenuCanvas.enabled = false;
		DeckChooseMenuCanvas.enabled = false;
		HeroSelectCanvas.enabled = false;
		DeckBuildCanvas.enabled = false;
		DeckBuildCardsSlotCanvas.enabled = false;
		DeckBuildCardsNumberCanvas.enabled = false;
		CollectionCanvas.enabled = false;
		FriendLobbyCanvas.enabled = false;
		PlayerHUD.enabled = false;
		MainMenuCanvas.enabled = false;
		CornerMenuCanvas.enabled = false;
		ShowWaitIndicator ();
		if (SettingsCanvasPrefab != null) { 
			SettingsCanvasObject = Instantiate (SettingsCanvasPrefab, Camera.main.transform);
			SettingsCanvas = SettingsCanvasObject.GetComponent<Canvas> ();
			SettingsCanvas.enabled = false;
		}
		if (PhotonNetwork.connectedAndReady) {
			Debug.Log ("Welcome in main menu");
			gotoMainMenu ();
		} else {
			Debug.Log ("Wait for game connection");
			gotoConnectingScreen ();
		}
	}

	public void HideCurrentCanvas() {
		ChooseGameMode = false;
		DeckChooseMode = false;
		DeckSelectMenuCanvas.GetComponent<DeckTableController> ().EraseList ();
		foreach (Canvas canv in CurrentCanvas) {
			if (canv == CollectionCanvas) {
				CardsTableControllerComponent.UnLoadAllCards();
			} else if (canv == DeckSelectMenuCanvas) {
				DeckSelectMenuCanvas.GetComponent<DeckTableController> ().EraseList ();
			} else if (canv == DeckBuildCanvas) {
				CardsTableControllerComponent.UnLoadAllCards();
				//CardsTableControllerComponent.DeckTableControllerComponent.DeckBaseComponent.LoadDecksList ();
				DeckBuildCardsSlotCanvas.enabled = false;
				DeckBuildCardsNumberCanvas.enabled = false;
			} else if (canv == FriendLobbyCanvas) {
				if (MyNetManager != null) {
					MyNetManager.LeaveFriendLobby ();
				}
			}
			canv.enabled = false;
		}
		if (MyNetManager != null && MyNetManager.ChatHandler != null) {
			MyNetManager.ChatHandler.HideChat ();
			MyNetManager.ChatHandler.LocalFriendListComponent.HideFriendList ();
		}
		CurrentCanvas.Clear ();
		ConnectingScreen = false;
	}

	public void LoadUserInventory() {
		PlayFabClientAPI.GetUserInventory (new GetUserInventoryRequest (),
			OnLoadUserInventory, error => Debug.LogError (error.GenerateErrorReport ()));
	}

	private void OnLoadUserInventory(GetUserInventoryResult result) {
		LoadLoot (result);
		CreditCurrency = result.VirtualCurrency ["CR"];
	}

	private void LoadLoot(GetUserInventoryResult result) {
		//urodziny CardsPacks.Clear ();
		AllItems = result.Inventory;
		foreach (ItemInstance item in result.Inventory) {
			Debug.Log ("New inventory item:" + item.DisplayName + " class:" + item.ItemClass + " uses:" + item.RemainingUses);
			if (item.ItemClass != null) {
				if (item.ItemClass.Equals("CardsPack")) {
					CardsPacks.Add (item);
					//item.Annotation = item.RemainingUses.GetValueOrDefault (1).ToString();
					ShowNewRewardsNotify (true, item.RemainingUses.GetValueOrDefault (1).ToString());
					if (LootMenuObject != null) {
						LootMenuObject.GetComponent<LootMenu> ().SetMainMenuComponent (this.GetComponent<MainMenu> ());
					}
				} else {
					DeckBaseComponent.CardsBaseComponent.LoadUserCard (item);
				}
			}
		}
	}

	private void CheckRewards() {
		PlayFabClientAPI.ExecuteCloudScript (new ExecuteCloudScriptRequest () {
			FunctionName = "CheckRewards", // Arbitrary function name (must exist in your uploaded cloud.js file)
			FunctionParameter = null, // The parameter provided to your function
			GeneratePlayStreamEvent = true, // Optional - Shows this event in PlayStream
		}, OnCloudCheckRewards, error => Debug.LogError (error.GenerateErrorReport ()));
	}

	private void OnCloudCheckRewards(ExecuteCloudScriptResult result) {
		Dictionary<String, UserDataRecord> Rewards;
		NewItemNotify NewItemNotifyComponent;
		string curr = "";
		int amount = 0;
		UserDataRecord data;

		Debug.Log("Rewards checked" + result.FunctionResult + " error:" + result.Error
			+ " log:" + result.Logs);

		Rewards = PlayFab.Json.JsonWrapper.DeserializeObject<Dictionary<String, UserDataRecord>>(PlayFab.Json.JsonWrapper.SerializeObject(result.FunctionResult));

		GameObject obj = GameObject.Find ("NewItemNotify-Canvas");
		if (obj != null) {
			NewItemNotifyComponent = obj.GetComponent<NewItemNotify> ();
		} else {
			Debug.LogWarning ("Cannot find item notify canvas");
			return;
		}

		GameObject gob = Instantiate (ShopMenuPrefab, Camera.main.transform);
		ShopMenu ShopMenuComponent = gob.GetComponent<ShopMenu> ();

		if (Rewards.TryGetValue ("rewardItems", out data)) {
			if (data != null && data.Value != null && data.Value.Length > 0) {
				ShopMenu.MyShopItemClass item = ShopMenuComponent.FindItem (data.Value);
				NewItemNotifyComponent.ShowNotify (item.Picture, null, item.DisplayName);
			}
		}
		if (Rewards.TryGetValue ("rewardCurr", out data)) {
			if (data != null && data.Value != null && data.Value.Length > 0) {
				curr = data.Value;
			}
		}
		if (Rewards.TryGetValue ("rewardCurrAmount", out data)) {
			if (data != null && data.Value != null && data.Value.Length > 0) {
				amount = Int32.Parse(data.Value);
			}
		}
		if (amount > 0) {
			if (curr.Equals ("CR") && amount > 0) {
				NewItemNotifyComponent.ShowNotify (
					CreditCurrencyRewardImage, null, amount + " Kredytów"
				);
			}
		}
		Destroy (gob);
	}

	public void gotoLootMenu() {
		HideCurrentCanvas ();
		PlayerHUD.enabled = false;
		LootMenuObject = Instantiate (LootMenuOb, Camera.main.transform);
		LootMenuObject.GetComponent<LootMenu> ().SetMainMenuComponent (this.GetComponent<MainMenu> ());
	}

	public void LoadDeckData() {
		ShowWaitIndicator ();
		LoadUserInventory ();
		CardsTableControllerComponent.DeckTableControllerComponent.DeckBaseComponent.LoadDecksList ();
	}

	private void logOutAndExit() {
		PlayerComponent.Logout ();
		MyNetManager.DisConnect ();
		Application.Quit();
	}

	public void ExitGame() {
		POPUPWindow.SetupDialogPOPUPWindow ("Twoje dane logowania zostaną zapamiętane, jeśli się nie wylogujesz",
			"WYJDŹ", "WYLOGUJ", Application.Quit, logOutAndExit);
	}

	public void gotoFriendLobbyMenu(string friendName) {
		HideCurrentCanvas ();
		DeckChooseMode = true;
		PlayerHUD.enabled = true;
		challangerFriendNameText.text = friendName;
		FriendLobbyCanvas.enabled = true;
		DeckSelectMenuCanvas.enabled = true;
		DeckSelectMenuCanvas.GetComponent<DeckTableController> ().PopulateListToSelect ();
		DeckSelectMenuCanvas.GetComponent<DeckTableController> ().ActiveList (PlayerComponent.localPlayerData.deck);
		CurrentCanvas.Add(FriendLobbyCanvas);
		CurrentCanvas.Add(DeckSelectMenuCanvas);
	}

	public void exitFriendLobbyMenu() {
		HideCurrentCanvas ();
		gotoMainMenu ();
	}

	public void gotoLocalGameMode() {
		SceneManager.LoadScene("Board_Scene");
	}

	public void gotoOnlineMatchMakingMode() {
		SceneManager.LoadScene("Board_Scene");
	}
		
	public void SetupPlayButtonReconnect() {
		PlayButton.onClick.RemoveAllListeners ();
		//PlayButton.onClick.AddListener (MyNetManager.RejoinGame);
		//auto-reconnect is setup now
		//PlayButton.GetComponentInChildren<Text>().text = "WRÓĆ DO GRY";
	}

	public void SetupPlayButtonNormal() {
		PlayButton.onClick.RemoveAllListeners ();
		PlayButton.onClick.AddListener (gotoGameModeMenu);
		//PlayButton.GetComponentInChildren<Text>().text = "GRAJ";
	}

	// main menu
	public void gotoMainMenu() {
		HideCurrentCanvas ();
		DeckChooseMode = false;//hide
		CannectingCanvas.enabled = false;
		MainMenuCanvas.enabled = true;
		CornerMenuCanvas.enabled = true;
		PlayerHUD.enabled = true;
		CurrentCanvas.Add(MainMenuCanvas);
		CurrentCanvas.Add(CornerMenuCanvas);
		if (CardsPacks != null && CardsPacks.Count > 0) {
			ShowNewRewardsNotify (true, CardsPacks [0].RemainingUses.GetValueOrDefault(1).ToString());
		} else {
			ShowNewRewardsNotify (false, "");
		}
		if (DeckBaseComponent.CardsBaseComponent.IsUnchecked ()) {
			ShowNewCardsNotify (true);
		} else {
			ShowNewCardsNotify (false);
		}
	}

	public void gotoConnectingScreen() {
		HideCurrentCanvas ();
		CannectingCanvas.enabled = true;
		ConnectingScreen = true;
		PlayerHUD.enabled = false;
		CurrentCanvas.Add(CannectingCanvas);
	}

	public void gotoDisconnectedScreen() {
		HideCurrentCanvas ();
		if (LootMenuObject != null) {
			Destroy (LootMenuObject);
		}
		DisconnectedCanvas.enabled = true;
		PlayerHUD.enabled = false;
		CurrentCanvas.Add(DisconnectedCanvas);
	}

	public void gotoMainLoadingScreen() {
		SceneManager.LoadScene("LoadingScene");
	}

	public void gotoLoginScene() {
		MyNetManager.DisConnect ();
		SceneManager.LoadScene("LoginScene");
	}

	public void exitHelpMenu() {
		gotoMainMenu ();
	}

	public void gotoGameModeMenu() {
		HideCurrentCanvas ();
		ChooseGameMode = true;
		DeckChooseMode = false;
		PlayerHUD.enabled = false;
		GameModeSelectCanvas.enabled = true;
		CurrentCanvas.Add(GameModeSelectCanvas);
		DeckSelectMenuCanvas.GetComponent<DeckTableController> ().ShowDeckToPlay (PlayerComponent.localPlayerData.deck);
	}

	public void SetGameModeQuickPlay() {
		MyNetManager.SetGameModeRandom ();
		gotoWaitForGameScreen ();
	}

	public void SetGameModeTraining() {
		MyNetManager.SetGameModeLocal ();
		gotoWaitForGameScreen ();
	}

	public void gotoWaitForGameScreen() {
		MyNetManager.StartGame ();
		exitDeckSelect ();
		HideCurrentCanvas ();
		WaitForGameCanvas.enabled = true;
		CurrentCanvas.Add(WaitForGameCanvas);
	}

	private IEnumerator WaitForConectToMaster() {
		while (ClientState.ConnectedToMaster != PhotonNetwork.connectionStateDetailed) {
			yield return new WaitForSeconds (0.05f);
		}
		gotoGameModeMenu ();
		waitForLeftRoom = false;
	}

	void OnLeftRoom() {
		if (waitForLeftRoom) {
			//StartCoroutine (WaitForConectToMaster());
		}
	}

	public void exitWaitForGameScreen() {
		Destroy(GameObject.Find ("NetworkManager").GetComponent<MyNetworkManager> ().DeckToPlay);
		//if (PhotonNetwork.inRoom) {
			waitForLeftRoom = true;
			GameObject.Find ("NetworkManager").GetComponent<MyNetworkManager> ().LeaveMyGame ();
		//} else {
			gotoGameModeMenu ();
		//}
	}

	public void gotoDeckChooseMenu() {
		HideCurrentCanvas ();
		DeckChooseMode = true;
		ChooseGameMode = false;
		DeckChooseMenuCanvas.enabled = true;
		DeckSelectMenuCanvas.enabled = true;
		DeckSelectMenuCanvas.GetComponent<DeckTableController> ().PopulateList ();
		DeckSelectMenuCanvas.GetComponent<DeckTableController> ().ActiveList (PlayerComponent.localPlayerData.deck);
		CurrentCanvas.Add (DeckChooseMenuCanvas);
		CurrentCanvas.Add (DeckSelectMenuCanvas);
	}

	public void gotoSettingsMenu() {
		HideCurrentCanvas ();
		SettingsCanvas.enabled = true;
		CurrentCanvas.Add (SettingsCanvas);
	}

	public void exitSettingsMenu() {
		gotoMainMenu ();
	}

	public void gotoCollectionMenu() {
		WaitForDataScreen.enabled = true;
		StartCoroutine (LoadCollection ());
	}

	private IEnumerator LoadCollection() {
		HideCurrentCanvas ();
		CollectionCanvas.enabled = true;
		PlayerHUD.enabled = false;
		CardsFilterCanvas.enabled = true;
		CurrentCanvas.Add(CardsFilterCanvas);
		CurrentCanvas.Add(CollectionCanvas);
		yield return new WaitForSeconds (0.01f);
		CardsTableControllerComponent.LoadCollection ();
		WaitForDataScreen.enabled = false;
		yield return null;
	}

	public void exitCollectionMenu() {
		CardsTableControllerComponent.UnLoadAllCards();
		HideCurrentCanvas ();
		gotoMainMenu ();
	}

	public void gotoDeckSelect() {
		HideCurrentCanvas ();
		PlayerHUD.enabled = false;
		DeckSelectMenuCanvas.enabled = true;
		DeckSelectMenuCanvas.GetComponent<DeckTableController> ().PopulateList ();
		if (DeckChooseMode) {
			DeckSelectMenuCanvas.GetComponent<DeckTableController> ().ActiveList (PlayerComponent.localPlayerData.deck);
			CurrentCanvas.Add(DeckChooseMenuCanvas);
			DeckChooseMenuCanvas.enabled = true;
		} else {
			CurrentCanvas.Add(DeckSelectMenuCanvas);
		}
	}

	public void SaveDeck() {
		//string deck_name = GameObject.Find ("DeckNameInputField").GetComponent<InputField> ().text;
		DeckSelectMenuCanvas.GetComponent<DeckTableController> ().SaveNewDeck ();
		//GameObject.Find ("CardsBase").GetComponent<CardsBase> ().UnloadCards ();
		CardsTableControllerComponent.UnLoadAllCards();
		DeckBuildCardsSlotCanvas.enabled = false;
		DeckBuildCardsNumberCanvas.enabled = false;
		gotoDeckSelect ();
	}

	public void exitDeckSelect() {
		DeckSelectMenuCanvas.GetComponent<DeckTableController> ().EraseList ();
		DeckSelectMenuCanvas.enabled = false;
		DeckChooseMenuCanvas.enabled = false;
		gotoMainMenu ();
	}

	public void gotoDeckBuild() {
		HideCurrentCanvas ();
		PlayerHUD.enabled = false;
		DeckBuildCanvas.enabled = true;
		DeckBuildCardsSlotCanvas.enabled = true;
		DeckBuildCardsNumberCanvas.enabled = true;
		WaitForDataScreen.enabled = true;
		CardsFilterCanvas.enabled = true;
		CurrentCanvas.Add(CardsFilterCanvas);
		//Hero hero = GameObject.Find ("DeckTable").GetComponent<DeckCardsScroll> ().EditedDeck.GetComponent<Deck>().Hero.GetComponent<Hero>();
		StartCoroutine( LoadDeckBuild());
		//Destroy (HeroesBaseComponent.SelectedHero);
		//GameObject.Find ("CardsBase").GetComponent<CardsBase> ().LoadCards (hero.CardsPool);
		//GameObject.Find ("CardsBase").GetComponent<CardsBase> ().LoadCards (CardsBase.SelectedHeroCards.Uniwersal);
		CurrentCanvas.Add(DeckBuildCanvas);
	}

	private IEnumerator LoadDeckBuild() {
		Hero hero = HeroesBaseComponent.SelectedHero.GetComponent<Hero>();
		CardsTableControllerComponent.LoadCardPool (hero);
		CardsTableControllerComponent.LoadCardPool (null);
		WaitForDataScreen.enabled = false;
		yield return null;
	}

	public void gotoHeroSelect() {
		HideCurrentCanvas ();
		PlayerHUD.enabled = false;
		HeroSelectCanvas.enabled  = true;
		if (DeckChooseMode) {
			DeckSelectMenuCanvas.enabled = false;
		}
		//GameObject.Find ("HeroesBase").GetComponent<HeroesBase> ().PopulateList ();
		CurrentCanvas.Add(HeroSelectCanvas);
	}

	public void exitHeroSelect() {
		//GameObject.Find ("HeroesBase").GetComponent<HeroesBase> ().EraseList ();
		gotoDeckSelect ();
	}

	public void exitDeckBuild() {
		POPUPWindow.SetupDialogPOPUPWindow (
			"Zmiany nie będą zapisane\n Czy na pewno chcesz wyjść?",
			exitDeckBuildConfirm,
			null);
	}

	public void exitDeckBuildConfirm() {
		CardsTableControllerComponent.UnLoadAllCards();
		DeckSelectMenuCanvas.GetComponent<DeckTableController> ().EraseList ();
		CardsTableControllerComponent.DeckTableControllerComponent.DeckBaseComponent.LoadDecksList ();
		DeckBuildCardsSlotCanvas.enabled = false;
		DeckBuildCardsNumberCanvas.enabled = false;
		gotoDeckSelect ();
	}

	#region SHOW_NOTIFIES
	public void ShowNewCardsNotify(bool Show) {
		NewCardsNotify.SetActive (Show);
	}

	public void ShowNewRewardsNotify(bool Show, string count) {
		if (Show) {
			NewRewardsNotifyText.text = "x" + count;
		}
		NewRewardsNotify.SetActive (Show);
	}
	#endregion
		
	#region WAIT_INDICATOR
	public void ShowWaitIndicator() {
		WaitForDataScreen.enabled = true;
	}

	public void HideWaitIndicator() {
		WaitForDataScreen.enabled = false;
	}
	#endregion
}
