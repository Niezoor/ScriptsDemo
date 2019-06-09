using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab.ClientModels;
using PlayFab;
using PlayFab.Json;
using System;
using TMPro;

public class LootMenu : MonoBehaviour {
	private Animator AnimationController;
	private Canvas MenuCanvas;
	public Animator PackAnimationController;
	public Button ControllButton;
	public Text ButtonText;
	public Button BackButton;
	public Button ShopButton;
	public GameObject ShopMenuPrefab;
	public float CardReversEndScale;
	public float CardEndScale;
	public int MovingSpeed;
	public GameObject Package;
	public GameObject CardRevers;
	public GameObject CardPanel;
	public Transform CardsTable;
	public Text PacksCount;
	public Text CurrencyCountText;
	public float UpPos;
	public float DownPos;
	[Header("VFX")]
	public GameObject GoldCardRay;
	public GameObject DiamondCardRay;
	public GameObject GoldCardFlipFX;
	public GameObject DiamondCardFlipFX;
	public GameObject DuplicateNotify;
	public ParticleSystem PackParticleEffect;
	[Header("AudioFX")]
	public AudioSource PackAudioSource;
	public AudioClip UrodzinyGosi;
	public AudioClip PackStartOpen;
	public AudioClip PackOpen;
	public AudioClip FlipCardNormal;
	public AudioClip FlipCardGold;
	public AudioClip FlipCardDiamond;
	public AudioClip CashSound;

	[Header("Lists")]
	public List<GameObject> CardsPanels = new List<GameObject> ();

	public List<GameObject> Cards = new List<GameObject> ();

	private MainMenu MainMenuComponent;
	public bool OpenProceed = false;
	public bool OpenFinished = false;
	private bool GettingCards = false;
	private bool PackLoaded = false;

	private int PacksUses = 0;
	// Use this for initialization
	void Start () {
		AnimationController = this.GetComponent<Animator> ();
		MenuCanvas = this.GetComponent<Canvas> ();
		ControllButton.onClick.AddListener (MainButtonClick);
		BackButton.onClick.AddListener (BackButtonClick);
		ShopButton.onClick.AddListener (OpenShopMenu);

		MenuCanvas.worldCamera = Camera.main;
	}
	
	// Update is called once per frame
	void Update () {
		if (MainMenuComponent != null) {
			if (!CurrencyCountText.text.Equals (MainMenuComponent.CreditCurrency.ToString ())) {
				CurrencyCountText.text = MainMenuComponent.CreditCurrency.ToString ();
			}
		}
	}

	public void SetMainMenuComponent(MainMenu comp) {
		MainMenuComponent = comp;
		ShowPackCount ();
	}

	private void ShowNewPackToOpen() {
		if (!PackLoaded) {
			PackAnimationController.SetTrigger ("SetupNew");
			PackLoaded = true;
		}
	}

	private void SetPacksCount(string count) {
		if (PacksCount != null) {
			if (count.Equals ("0")) {
				PacksCount.text = "BRAK";
			} else {
				PacksCount.text = count;
			}
		}
	}

	private void ShowPackCount() {
		if (MainMenuComponent != null) {
			if (MainMenuComponent.CardsPacks.Count > 0) {
				if (MainMenuComponent.CardsPacks [0].ItemClass.Equals ("urodzinowa")) {
					SetPacksCount ("Prezent dla Żuczka");
				} else {
					SetPacksCount ("" + MainMenuComponent.CardsPacks [0].RemainingUses + "\n" +
					MainMenuComponent.CardsPacks [0].Annotation);
				}
				ShowNewPackToOpen ();
				PacksUses = MainMenuComponent.CardsPacks [0].RemainingUses.HasValue ?
					MainMenuComponent.CardsPacks [0].RemainingUses.Value : 0;
			} else {
				SetPacksCount ("0");
				PacksUses = 0;
			}
		}
	}

	public void PackDown() {
		if (MainMenuComponent.CardsPacks.Count > 0) {
			if (!OpenProceed) {
				PackAnimationController.SetTrigger ("StartOpen");
				PackAudioSource.PlayOneShot(PackStartOpen);
			}
		}
	}

	public void PackRelease() {
		if (MainMenuComponent.CardsPacks.Count > 0) {
			if (!OpenProceed) {
				OpenCardPack (MainMenuComponent.CardsPacks[0].ItemClass);
				PackAnimationController.SetTrigger ("WaitForOpen");
			}
		}
	}

	private void MainButtonClick() {
		Debug.Log ("MainButtonClick clicked");
		if (OpenFinished) {
			GetItems ();
			//urodziny
			//MainMenuComponent.POPUPWindow.SetupDialogPOPUPWindow (
			//	"Zajrzyj do sypialni", "co tam jest?", "OMG?!?!", GetItems, GetItems);
		}
		if (MainMenuComponent.CardsPacks.Count > 0) {
			if (!OpenProceed) {
				OpenCardPack (MainMenuComponent.CardsPacks[0].ItemClass);
				PackAnimationController.SetTrigger ("StartOpen");
				PackAnimationController.SetTrigger ("WaitForOpen");
				PackAudioSource.PlayOneShot(PackStartOpen);
			}
		}
	}

	private void BackButtonClick() {
		BackToMainMenu ();
	}

	private void RemoveItem(ItemInstance item) {
		if (item.RemainingUses > 1) {
			item.RemainingUses--;
		} else {
			MainMenuComponent.CardsPacks.Remove (item);
		}
		ShowPackCount ();
	}

	private void OpenCardPack(string packClass) {
		Debug.Log ("open pack:" + packClass);
		AnimationController.SetTrigger ("StartOpen");
		OpenProceed = true;
		if (packClass.Equals("urodzinowa")) {
			OpenGosiaGiftPack ();
		} else {
			OpenNormalCardPack ();
		}
	}

	private void OpenGosiaGiftPack() {
		List<ItemInstance> items = new List<ItemInstance> ();
		ItemInstance item1 = new ItemInstance ();
		ItemInstance item2 = new ItemInstance ();
		ItemInstance item3 = new ItemInstance ();
		ItemInstance item4 = new ItemInstance ();
		ItemInstance item5 = new ItemInstance ();
	
		item3.ItemId = "8000";
		item3.RemainingUses = 1;
		item2.ItemId = "8001";
		item2.RemainingUses = 1;
		item1.ItemId = "8002";
		item1.RemainingUses = 1;
		item5.ItemId = "8003";
		item5.RemainingUses = 1;
		item4.ItemId = "8004";
		item4.RemainingUses = 1;
		items.Add (item1);
		items.Add (item2);
		items.Add (item3);
		items.Add (item4);
		items.Add (item5);
		GrantPackItems (items);
	}

	private void OpenNormalCardPack() {
		ItemInstance pack = MainMenuComponent.CardsPacks [0];
		Debug.Log ("Request open normal card pack:" + pack.RemainingUses.Value);

		/*UnlockContainerInstanceRequest request = new UnlockContainerInstanceRequest();
		request.CatalogVersion = pack.CatalogVersion;
		request.ContainerItemInstanceId = pack.ItemInstanceId;
		PlayFabClientAPI.UnlockContainerInstance( request, OpenNormalCardPackSuccess, OpenPackageFail);
		*/

		PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
		{
				FunctionName = "OpenCardPack",
				FunctionParameter = new { containerItemInstanceId = pack.ItemInstanceId },
				GeneratePlayStreamEvent = true,
			}, OpenPackInCloud, OpenPackageFail);
	}

	private IEnumerator AddCardFromPack(GameObject card, GameObject panel, int i, CardsBase.CardDescriptionClass cardDesc) {
		SmothTransform ST = card.GetComponent<SmothTransform> ();
		yield return new WaitForSeconds (0.5f);
		yield return new WaitForSeconds (0.1f*i);
		card.transform.SetParent (panel.transform, true);
		ST.SmothTransformTo (new Vector3 (0, 0, 0), Quaternion.identity, MovingSpeed);
		ST.SmoothScaleTo (new Vector3 (CardReversEndScale, CardReversEndScale, CardReversEndScale), MovingSpeed);
		if (cardDesc != null) {
			if (cardDesc.Rarity == CardInteraction.CardRarityEnum.gold) {
				Instantiate (GoldCardRay, card.transform);
			} else if (cardDesc.Rarity == CardInteraction.CardRarityEnum.diamond) {
				Instantiate (DiamondCardRay, card.transform);
			}
		}
	}

	private void OpenPackInCloud(ExecuteCloudScriptResult result) {
		List<ItemInstance> items = PlayFab.Json.JsonWrapper.DeserializeObject<List<ItemInstance>>(PlayFab.Json.JsonWrapper.SerializeObject(result.FunctionResult));
		GrantPackItems (items);
	}

	private void OpenNormalCardPackSuccess(UnlockContainerItemResult result) {
		GrantPackItems (result.GrantedItems);
	}

	private void GrantPackItems(List<ItemInstance> GrantedItems) {
		int i = 0;
		Debug.Log ("Result open normal card pack:" + GrantedItems.Count);
		ButtonText.text = "ZGARNIJ";
		PackParticleEffect.Emit (1);
		RemoveItem (MainMenuComponent.CardsPacks[0]);
		foreach (ItemInstance item in GrantedItems) {
			int id = -1;
			Int32.TryParse (item.ItemId, out id);
			int quantity = item.RemainingUses.GetValueOrDefault (1);
			CardsBase.CardDescriptionClass cardDesc = MainMenuComponent.DeckBaseComponent.CardsBaseComponent.FindCardDescByID(id);
			Debug.Log ("unlock card :" + item.DisplayName + " x" + item.RemainingUses.GetValueOrDefault (1) + " class:" + item.ItemClass);

			int maxCount = 3;
			int givenMoney = 5;

			if (cardDesc.Rarity == CardInteraction.CardRarityEnum.common) {
				maxCount = 3;
				givenMoney = 5;
			} else if (cardDesc.Rarity == CardInteraction.CardRarityEnum.gold) {
				maxCount = 2;
				givenMoney = 10;
			} else if (cardDesc.Rarity == CardInteraction.CardRarityEnum.diamond) {
				maxCount = 1;
				givenMoney = 20;
			}

			int cardNumber = cardDesc.Quantity;
			quantity -= cardDesc.Quantity;

			while(quantity > 0) {
				if (MainMenuComponent != null) {
					cardDesc = MainMenuComponent.DeckBaseComponent.CardsBaseComponent.UnlockCard (id);
				}
				GameObject card = Instantiate (CardRevers);
				card.transform.position = Package.transform.position;
				card.transform.rotation = Package.transform.rotation;
				card.transform.localScale = new Vector3 (0.8f, 0.8f, 0.8f);
				if (card != null) {
					GameObject panel = Instantiate (CardPanel, CardsTable);
					LootPanel lootPanel = panel.GetComponent<LootPanel> ();
					if ((quantity + cardNumber) > maxCount) {
						lootPanel.Duplicate = true;
						lootPanel.RewardForDuplicate = givenMoney;
					}
					lootPanel.Rarity = cardDesc.Rarity;
					card.GetComponent<CardInteraction> ().SetCardOrder (2);
					StartCoroutine( AddCardFromPack (card, panel, i, cardDesc));
					lootPanel.RegisterListener (FlipCard, Int32.Parse (item.ItemId), i);

					Cards.Add (card);
					CardsPanels.Add (panel);
					i++;
				}
				quantity--;
			}
		}
		PackAnimationController.SetTrigger ("EndOpen");
		PackAudioSource.PlayOneShot(PackOpen);
		PackLoaded = false;
		OpenFinished = true;
	}

	private IEnumerator FlipCardTask(GameObject revers, int cardID, int PanelID) {
		AudioClip AtoPlay = null;
		LootPanel Panel = CardsPanels [PanelID].GetComponent<LootPanel> ();

		GameObject card = MainMenuComponent.DeckBaseComponent.CardsBaseComponent.SpawnCardByID (cardID);
		card.transform.SetParent (revers.transform);
		card.transform.localPosition = new Vector3 (0, 0, 0);
		card.transform.localRotation = Quaternion.Euler(0,180,0);
		card.transform.localScale = new Vector3 (CardEndScale, CardEndScale, CardEndScale);
		//card.GetComponent<CardInteraction> ().SetObjectVisible(false);
		card.SetActive(false);
		revers.GetComponent<SmothTransform> ().SmothTransformTo(Quaternion.Euler(0,180,0), 5);

		if (Panel.Rarity == CardInteraction.CardRarityEnum.common) {
			AtoPlay = FlipCardNormal;
		} else if (Panel.Rarity == CardInteraction.CardRarityEnum.gold) {
			GameObject fx = Instantiate (GoldCardFlipFX, Panel.transform);
			fx.transform.localPosition = new Vector3 (0, 0, 0);
			fx.transform.localScale = new Vector3 (76, 76, 76);
			AtoPlay = FlipCardGold;
		} else if (Panel.Rarity == CardInteraction.CardRarityEnum.diamond) {
			GameObject fx = Instantiate (DiamondCardFlipFX, Panel.transform);
			fx.transform.localPosition = new Vector3 (0, 0, 0);
			fx.transform.localScale = new Vector3 (76, 76, 76);
			AtoPlay = FlipCardDiamond;
		}
		if (AtoPlay != null) {
			revers.GetComponent<AudioSource> ().PlayOneShot (AtoPlay);
		}

		int timeout = 20;
		while (revers.transform.rotation.eulerAngles.y < 90) {
			if (timeout < 0) {
				break;
			}
			timeout--;
			yield return new WaitForSeconds (0.1f);
		}
		//Debug.Log ("  rot y : " + revers.transform.rotation.eulerAngles.y);
		//card.GetComponent<CardInteraction> ().SetObjectVisible(true);
		card.SetActive(true);
		revers.GetComponent<CardInteraction> ().SetObjectVisible(false);
		if (Panel.Duplicate) {
			string mes = "Nadliczbowa\n+" + CardsPanels [PanelID].GetComponent<LootPanel> ().RewardForDuplicate;
			Instantiate (DuplicateNotify, revers.transform.parent).GetComponent<TextMeshPro> ().SetText (mes);
			MainMenuComponent.CreditCurrency += CardsPanels [PanelID].GetComponent<LootPanel> ().RewardForDuplicate;
			PackAudioSource.PlayOneShot(CashSound);
		}
		if (AllFlipped ()) {
			yield return new WaitForSeconds (0.1f);
			AnimationController.SetTrigger ("EndOpen");
			//urodziny
			//AudioStartAndLoop audioManager = GameObject.Find("AudioManager").GetComponent<AudioStartAndLoop>();
			//audioManager.StopMusic ();
			//audioManager.AudioManager.clip = UrodzinyGosi;
			//audioManager.AudioManager.Play ();
			//audioManager.AudioManager.volume = 1;
			//audioManager.AudioManager.loop = true;
		}
	}

	private bool AllFlipped() {
		bool notAllFlipped = false;
		foreach (GameObject ob in CardsPanels) {
			if (!ob.GetComponent<LootPanel> ().Clicked) {
				notAllFlipped = true;
				break;
			}
		}
		return !notAllFlipped;
	}

	private void FlipCard(int cardID, int panel) {
		if (Cards [panel]) {
			StartCoroutine (FlipCardTask(Cards [panel], cardID, panel));
		}
	}

	private void OpenPackageFail(PlayFabError obj) {
		Debug.LogWarning ("Cannot open card pack");
		Debug.LogError (obj.GenerateErrorReport ());
		OpenProceed = false;
		BackToMainMenu ();
	}

	private IEnumerator FlipAllTask() {
		foreach (GameObject ob in CardsPanels) {
			ob.GetComponent<LootPanel> ().PanelClick ();
			yield return new WaitForSeconds (0.1f);
		}
	}

	private void FlipAll() {
		StartCoroutine (FlipAllTask());
	}

	private IEnumerator GetCardAnim(GameObject card) {
		Vector3 posNow = card.transform.localPosition;
		SmothTransform ST = card.GetComponent<SmothTransform> ();
		Vector3 upPos = new Vector3 (posNow.x, posNow.y + UpPos, posNow.z);
		Vector3 downPos = new Vector3 (posNow.x, posNow.y + DownPos, posNow.z);
		ST.SmothTransformTo (upPos, 10);
		yield return new WaitForSeconds (0.1f);
		ST.SmothTransformTo (downPos, 10);
	}

	private IEnumerator GetItemsTask() {
		if (!AllFlipped()) {
			FlipAll ();
			yield return new WaitForSeconds (1f);
		}
		foreach (GameObject card in Cards) {
			StartCoroutine (GetCardAnim (card));
			yield return new WaitForSeconds (0.1f);
		}
		yield return new WaitForSeconds (1f);
		AnimationController.SetTrigger ("GetCards");
		ButtonText.text = "OTWÓRZ";
		foreach (GameObject ob in CardsPanels) {
			Destroy (ob);
		}
		if (PacksUses > 0) {
			ShowNewPackToOpen ();
		}
		CardsPanels.Clear ();
		Cards.Clear ();
		OpenProceed = false;
		OpenFinished = false;
		GettingCards = false;
	}

	private void GetItems() {
		if (!GettingCards) {
			Debug.Log ("GetItems");
			GettingCards = true;
			StartCoroutine (GetItemsTask ());
		}
	}

	private void BackToMainMenu() {
		if (MainMenuComponent != null) {
			if (OpenProceed && !OpenFinished) {
				return;
			}
			PackLoaded = true;
			if (OpenFinished) {
				GetItems ();
			}
			AnimationController.SetTrigger ("ExitLootMenu");
			PackAnimationController.SetTrigger ("Remove");
			MainMenuComponent.gotoMainMenu ();
			Destroy (this.gameObject, 2);
		}
	}

	private void OpenShopMenu() {
		if (MainMenuComponent != null) {
			if (OpenProceed && !OpenFinished) {
				return;
			}
			Instantiate (ShopMenuPrefab).GetComponent<ShopMenu> ().SetMainMenu (MainMenuComponent);
		}
	}
}
