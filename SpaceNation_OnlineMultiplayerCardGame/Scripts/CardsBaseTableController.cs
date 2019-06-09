using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardsBaseTableController : MonoBehaviour {
	public GameObject CardPanel;
	public Transform CardsBaseTable;
	public Transform CardsBasePositionRoot;
	public Transform CardsInDeckTable;
	public Transform CardsInDeckPositionRoot;
	public Transform CardsCollectionTable;
	public CardsBase CardsBaseComponent;
	public HeroesBase HeroesBaseComponent;
	public DeckTableController DeckTableControllerComponent;
	public GridLayoutGroup CardsInDeckLayoutGroup;
	public Canvas CardPOPUPCanvas;
	public Transform CardPOPUPPosition;
	public Vector3 InTableCardScale = new Vector3 (16, 16, 16);
	public Vector3 InCollectionCardScale = new Vector3 (14, 14, 14);
	public Vector3 CardPOPUPScale = new Vector3 (50, 50, 50);

	public Toggle ShowAllToggle;
	public Dropdown RarityFilterDropDown;
	public Dropdown RoleFilterDropDown;

	public List<string> RarityFilterList = new List<string>() {"Wszystkie rzadkości", "Srebrne", "Złote", "Diamentowe"};
	public List<string> RolesFilterList = new List<string>() {"Wszystkie role", "Ofensywne", "Defensywne", "Wspierające"};

	private int CurrentRarityFilter = 0;
	private int CurrentRoleFilter = 0;

	public struct cardDescStruct {
		public string CardName;
		public GameObject Card;
		public int CardsNumber;
	}

	public List<cardDescStruct> PanelsInDeckCards = new List<cardDescStruct> ();
	public List<GameObject> PanelsInDeckList = new List<GameObject>();
	public List<GameObject> PanelsInCardsList = new List<GameObject>();
	private GameObject CardInHighlight;

	private bool CardsInDeckTableOut = false;
	private bool CardsInDeckTableMax = false;
	private bool framesToWait = true;
	public bool collectionMode = false;
	public bool VisibilityCheckEnabled = false;
	public bool SetCardAsButtonChild = false;
	private bool visibleCheckEnabled = false;
	private Hero CurrentHero;

	public GameObject NewCardNotifyPrefab;

	public bool DeckBuildV2 = true;
	public GameObject DeckPanelV2Prefab;
	public Transform DeckBuildV2Table;
	public Vector3 DeckBuildV2CardScale = new Vector3 (16, 16, 16);
	public List <string> CardsInDeckNames = new List<string> ();

	public bool PoolLoading = false;
	public bool PoolUnload = false;
	private List <Coroutine> LoadTasks = new List<Coroutine> ();

	void Awake () {
	}

	// Use this for initialization
	void Start () {
		if (RarityFilterDropDown != null) {
			//RarityFilterDropDown.AddOptions (RarityFilterList);
			RarityFilterDropDown.onValueChanged.RemoveAllListeners();
			RarityFilterDropDown.onValueChanged.AddListener (UpdateRarityFilter);
		}
		if (RoleFilterDropDown != null) {
			//RoleFilterDropDown.AddOptions (RolesFilterList);
			RoleFilterDropDown.onValueChanged.RemoveAllListeners();
			RoleFilterDropDown.onValueChanged.AddListener (UpdateRoleFilter);
		}
		CurrentRarityFilter = 0;
		CurrentRoleFilter = 0;
		visibleCheckEnabled = true;
	}

	private void StopAllTasks() {
		Debug.Log ("Stop all load threads " + LoadTasks.Count);
		foreach (Coroutine cor in LoadTasks) {
			if (cor != null) {
				StopCoroutine (cor);
			}
		}
		LoadTasks.Clear ();
		PoolLoading = false;
	}

	public void RefreshCardsTable() {
		visibleCheckEnabled = false;
		StopAllTasks ();
		Debug.Log ("Refresh cards table");
		if (collectionMode) {
			for (int i = CardsInDeckPositionRoot.childCount - 1; i >= 0; --i) {
				GameObject.Destroy (CardsInDeckPositionRoot.GetChild (i).gameObject);
			}
			CardsInDeckPositionRoot.DetachChildren ();

			for (int i = CardsCollectionTable.childCount - 1; i >= 0; --i) {
				GameObject.Destroy (CardsCollectionTable.GetChild (i).gameObject);
			}
			CardsCollectionTable.DetachChildren();

			for (int i = CardsBasePositionRoot.childCount - 1; i >= 0; --i) {
				GameObject.Destroy (CardsBasePositionRoot.GetChild (i).gameObject);
			}
			CardsBasePositionRoot.DetachChildren ();
			PanelsInCardsList.Clear ();
			LoadAllCards();
		} else {
			for (int i = CardsBaseTable.childCount - 1; i >= 0; --i) {
				GameObject.Destroy (CardsBaseTable.GetChild (i).gameObject);
			}
			CardsBaseTable.DetachChildren ();

			for (int i = CardsBasePositionRoot.childCount - 1; i >= 0; --i) {
				GameObject.Destroy (CardsBasePositionRoot.GetChild (i).gameObject);
			}
			CardsBasePositionRoot.DetachChildren ();

			for (int i = CardsCollectionTable.childCount - 1; i >= 0; --i) {
				GameObject.Destroy (CardsCollectionTable.GetChild (i).gameObject);
			}
			CardsCollectionTable.DetachChildren();
			PanelsInCardsList.Clear ();
			LoadCardPool(CurrentHero);
			LoadCardPool (null);
			//PanelsInDeckList.Clear ();
		}
		visibleCheckEnabled = true;
	}

	public void UpdateRarityFilter(int filter) {
		CurrentRarityFilter = filter;
		RefreshCardsTable ();
	}

	public void UpdateRoleFilter(int filter) {
		CurrentRoleFilter = filter;
		RefreshCardsTable ();
	}

	// Update is called once per frame
	void Update () {
		if (framesToWait) {//need to wait one frame to put panels in grid (but why? idk O.o)
			framesToWait = false;
		} else {
			if (VisibilityCheckEnabled && visibleCheckEnabled) {
				CheckVisiblePanelsInList (PanelsInDeckList, 3, CardsInDeckPositionRoot);
				CheckVisiblePanelsInList (PanelsInCardsList, 1, CardsBasePositionRoot);
			}
		}
	}

	public void ShowCardHighlight(string CardName) {
		if (CardInHighlight != null) {
			Destroy (CardInHighlight);
		}
		CardInHighlight = CardsBaseComponent.SpawnCardByName (CardName);
		if (CardInHighlight) {
			CardPOPUPCanvas.enabled = true;
			CardInHighlight.transform.SetParent (CardPOPUPPosition);
			CardInHighlight.transform.localScale = CardPOPUPScale;
			CardInHighlight.transform.localPosition = new Vector3 (0, 0, 0);
			CardInHighlight.GetComponent<CardInteraction> ().SetCardOrder (20);
		}
	}

	public void HideCardHighlight() {
		CardPOPUPCanvas.enabled = false;
		Destroy (CardInHighlight);
	}

	private void CheckVisiblePanelsInList(List<GameObject> PanelsList, int CardsLayerOrder, Transform toSetParent) {
		foreach (GameObject panelOb in PanelsList) {
			CardPanel panel = panelOb.GetComponent<CardPanel> ();
			if (IsPanelVisible (panelOb)) {
				/*if (panel.Card == null) {
					GameObject card = CardsBaseComponent.SpawnCardByName (panel.PanelCardName);
					if (card) {
						card.GetComponent<CardInteraction> ().SetCardOrder (CardsLayerOrder);
						card.transform.SetParent (toSetParent);
						if (collectionMode) {
							card.transform.localScale = InCollectionCardScale;
						} else {
							card.transform.localScale = InTableCardScale;
						}
						card.transform.position = panelOb.transform.position;
						panel.AddCard (card);
						panel.KeepCardPosition = true;
					} else {
						Debug.LogError ("Cannot spawn card named: " + panel.PanelCardName);
					}
				}*/
				panel.Card.SetActive(true);
			} else {
				panel.Card.SetActive(false);
				/*if (panel.Card != null) {
					Debug.Log ("Hide card named: " + panel.PanelCardName);
					panel.HideCard ();
				}*/
			}
		}
	}

	private bool IsPanelVisible(GameObject panelObject) {
		Vector3[] v = new Vector3[4];
		panelObject.GetComponent<RectTransform>().GetWorldCorners (v);
		int sWidth = Screen.width + 400;
		Rect screenRect = new Rect (-200, 0, sWidth, Screen.height);
		int visibleCorners = 0;
		Vector3 tempScreenSpaceCorner;

		for (int i = 0; i < v.Length; i++) {
			tempScreenSpaceCorner = Camera.main.WorldToScreenPoint(v[i]); // Transform world space position of corner to screen space
			if (screenRect.Contains(tempScreenSpaceCorner)) // If the corner is inside the screen
			{
				visibleCorners++;
			}
		}
		if (visibleCorners > 0) {
			return true;
		} else {
			return false;
		}
	}

	public void CardsInDeckTableUp(Animator anim) {
		if (CardsInDeckTableOut == false) {
			CardsInDeckTableOut = true;
			anim.SetBool ("TableOut", CardsInDeckTableOut);
		} else if (CardsInDeckTableMax == false) {
			CardsInDeckTableMax = true;
			anim.SetBool ("TableMax", CardsInDeckTableMax);
			SetCardsAnimSpeedForSec (15);
			CardsInDeckLayoutGroup.constraintCount = 3;
		}
	}

	public void CardsInDeckTableDown(Animator anim) {
		if (CardsInDeckTableMax == true) {
			CardsInDeckTableMax = false;
			anim.SetBool ("TableMax", CardsInDeckTableMax);
			SetCardsAnimSpeedForSec (15);
			CardsInDeckLayoutGroup.constraintCount = 1;
		} else if (CardsInDeckTableOut == true) {
			CardsInDeckTableOut = false;
			anim.SetBool ("TableOut", CardsInDeckTableOut);
		}
	}

	private void SetCardsAnimSpeedForSec(float speed) {
		foreach (GameObject findedPanelOb in PanelsInDeckList) {
			findedPanelOb.GetComponent<CardPanel> ().SetCardsAnimSpeedForSec(speed);
		}
	}

	public void CardClicked(CardPanel panel) {
		if (panel != null) {
			if (panel.NewNotifyObject != null) {
				Destroy (panel.NewNotifyObject);
				CardsBaseComponent.CardChecked (panel.PanelCardName);
			}
		}
	}

	public void RemoveCard(string cardName) {
		Debug.Log ("Remove card from deck:" + cardName);

		DeckTableControllerComponent.RemoveCardFromDeck (CardsBaseComponent.GetIdByName(cardName).ToString());
		CardsInDeckNames.Remove (cardName);
		foreach (GameObject findedPanelOb in PanelsInDeckList) {
			if (findedPanelOb != null) {
				CardPanel findedPanel = findedPanelOb.GetComponent<CardPanel> ();
				if (findedPanel != null) {
					if (findedPanel.PanelCardName == cardName) {
						findedPanel.CardsNumber--;
						findedPanel.RefreshCardsNumber ();
						if (findedPanel.CardsNumber < 1) {
							findedPanel.DestroyPanel ();
							PanelsInDeckList.Remove (findedPanelOb);
							break;
						}
					}
				} else {
					CardViewPanel panel = findedPanelOb.GetComponent<CardViewPanel> ();
					if (panel != null) {
						if (panel.PanelPawn.GetComponent<Pawn> ().Name == cardName) {
							PanelsInDeckList.Remove (findedPanelOb);
							//Destroy (findedPanelOb);
							break;
						}
					}
				}
			}
		}
	}

	private void SortList() {
		List<string> cardNames = new List<string> ();

		framesToWait = true;
		foreach (GameObject findedPanelOb in PanelsInDeckList) {
			cardDescStruct newCard = new cardDescStruct();
			CardPanel findedPanel = findedPanelOb.GetComponent<CardPanel> ();
			newCard.CardName = findedPanel.PanelCardName;
			if (findedPanel.Card != null) {
				newCard.Card = findedPanel.Card;
			}
			newCard.CardsNumber = findedPanel.CardsNumber;
			PanelsInDeckCards.Add (newCard);
			cardNames.Add (findedPanel.PanelCardName);
			Destroy (findedPanel.PrevCard);
			findedPanel.DestroyPanelOnly ();
		}
		PanelsInDeckList.Clear ();
		cardNames = CardsBaseComponent.SortCardsList (cardNames);
		foreach (string cardName in cardNames) {
			GameObject panel = Instantiate (CardPanel);
			CardPanel panelComp = panel.GetComponent<CardPanel> ();
			panel.transform.SetParent (CardsInDeckTable, false);
			panelComp.PanelInDeck = true;
			foreach (cardDescStruct cardStOb in PanelsInDeckCards) {
				if (string.Compare (cardStOb.CardName, cardName) == 0) {
					panelComp.KeepCardPosition = false;
					panelComp.CardPosReached = false;
					if (cardStOb.Card != null) {
						panelComp.AddCard (cardStOb.Card);
					}
					panelComp.PanelCardName = cardStOb.CardName;
					panelComp.CardsNumber = cardStOb.CardsNumber;
					panelComp.RefreshCardsNumber ();
					break;
				}
			}
			PanelsInDeckList.Add (panel);
		}
		PanelsInDeckCards.Clear ();
	}

	private GameObject FindSpawnedCard(List<GameObject> spawnedPawns, string pawnToFind) {
		foreach (GameObject pawn in spawnedPawns) {
			if (pawn != null) {
				if ((pawn.GetComponent<Pawn>().Name.Equals(pawnToFind)) ||
					(pawn.GetComponent<Pawn>().CardID.ToString().Equals(pawnToFind))) {
					spawnedPawns.Remove (pawn);
					return pawn;
				}
			}
		}
		return null;
	}

	public bool CheckCardQuantity(string CardName) {
		bool rv = true;
		int cardcount = 0;
		CardsBase.CardDescriptionClass desc = CardsBaseComponent.FindCardDescByName (CardName);
		if (desc != null) {
			foreach (string cardn in CardsInDeckNames) {
				if (cardn.Equals (desc.Name) || cardn.Equals (desc.CardID)) {
					cardcount++;
				}
			}

			if (cardcount >= desc.MaxQuantity) {
				rv = false;
			}
		}

		return rv;
	}

	private void AddCardToTable (string cardName, Vector3 startCardGlobalPosition, bool noAnim, List<GameObject> spawnedPawns = null) {
		GameObject card = null;
		GameObject pawn = null;
		if (DeckBuildV2 && spawnedPawns != null) {
			pawn = FindSpawnedCard (spawnedPawns, cardName);
		}
		if (pawn == null) {
			card = CardsBaseComponent.SpawnCardByName (cardName);
		}

		if (DeckBuildV2) {
			bool availble = true;
			if (card) {
				//availble = CheckCardQuantity (cardName);
				pawn = card.transform.Find ("Pawn").gameObject;
				pawn.transform.SetParent (CardsInDeckPositionRoot);
				Destroy (card);
			}
			if (availble) {
				CardsInDeckNames.Add (pawn.GetComponent<Pawn>().Name);
				CardsInDeckNames = CardsBaseComponent.SortCardsList (CardsInDeckNames);
				GameObject panel = Instantiate (DeckPanelV2Prefab);
				if (panel != null) {
					panel.transform.SetParent (DeckBuildV2Table.transform, false);
					panel.transform.SetSiblingIndex (CardsInDeckNames.IndexOf (pawn.GetComponent<Pawn>().Name));
					PanelsInDeckList.Add (panel);
					if (pawn != null) {
						CardViewPanel panelView = panel.GetComponent<CardViewPanel> ();
						pawn.GetComponent<KeepParentRenderLayer> ().KeepingActive = false;
						pawn.gameObject.GetComponent<SpriteRenderer> ().sortingOrder = 55;
						pawn.transform.SetParent (CardsInDeckPositionRoot);
						pawn.transform.position = startCardGlobalPosition;
						panelView.SetPawn (pawn, null);
						panelView.TableWidth = DeckBuildV2Table.GetComponent<RectTransform> ().rect.width;
						panelView.MovePawn (true);
						panelView.CardsControllerComp = GetComponent<CardsBaseTableController> ();
						pawn.transform.localScale = InTableCardScale;
					}
				}
			} else {
				DeckTableControllerComponent.RemoveCardFromDeck (cardName);
			}
				
		} else {
			GameObject panel;
			bool panelFinded = false;
			CardPanel panelComp;

			if (card) {
				card.transform.SetParent (CardsInDeckPositionRoot);
				card.transform.localScale = InTableCardScale;
				card.transform.position = startCardGlobalPosition;
				card.GetComponent<CardInteraction> ().SetCardOrder (3);
				foreach (GameObject findedPanelOb in PanelsInDeckList) {
					CardPanel findedPanel = findedPanelOb.GetComponent<CardPanel> ();
					if (findedPanel.PanelCardName == cardName){
						CardInteraction CardInter = card.GetComponent<CardInteraction> ();
						bool availble = true;
						if ((CardInter.CardRarity == CardInteraction.CardRarityEnum.common) &&
							(findedPanel.CardsNumber>=3))
						{
							availble = false;
						} else if ((CardInter.CardRarity == CardInteraction.CardRarityEnum.gold) &&
							(findedPanel.CardsNumber>=2))
						{
							availble = false;
						} else if ((CardInter.CardRarity == CardInteraction.CardRarityEnum.diamond) &&
							(findedPanel.CardsNumber>=1))
						{
							availble = false;
						}
						if (availble) {
							findedPanel.CardsNumber++;
							findedPanel.RefreshCardsNumber ();
							panelComp = findedPanel;
							//panelComp.KeepCardPosition = false;
							if (noAnim) {
								Destroy (card);
							}
							panelComp.AddCard (card);
							//panelComp.SmoothMoveCard ();
							//panelComp.KeepCardPosition = true;
						} else {
							DeckTableControllerComponent.RemoveCardFromDeck (cardName);
							Destroy (card);
						}
						panelFinded = true;
					}
				}
				if (panelFinded == false) {
					panel = Instantiate (CardPanel);
					panelComp = panel.GetComponent<CardPanel> ();
					panel.transform.SetParent (CardsInDeckTable, false);
					panelComp.PanelInDeck = true;
					//panelComp.KeepCardPosition = false;
					panelComp.PanelCardName = cardName;
					panelComp.AddCard (card);
					//panelComp.SmoothMoveCard ();
					panelComp.RefreshCardsNumber ();
					//panelComp.KeepCardPosition = true;
					PanelsInDeckList.Add (panel);
					SortList ();
				}
			}
		}
	}

	public void AddCardToDeck(string cardName, Vector3 startCardGlobalPosition) {
		if (CheckCardQuantity (cardName)) {
			if (DeckTableControllerComponent.AddCardToDeck (CardsBaseComponent.GetIdByName (cardName).ToString ())) {
				AddCardToTable (cardName, startCardGlobalPosition, false);
			}
		}
	}

	public void UnLoadAllCards() {
		visibleCheckEnabled = false;
		StopAllTasks ();
		for (int i = CardsBaseTable.childCount - 1; i >= 0; --i) {
			GameObject.Destroy(CardsBaseTable.GetChild(i).gameObject);
		}
		CardsBaseTable.DetachChildren();
		for (int i = CardsBasePositionRoot.childCount - 1; i >= 0; --i) {
			GameObject.Destroy(CardsBasePositionRoot.GetChild(i).gameObject);
		}
		CardsBasePositionRoot.DetachChildren();

		for (int i = CardsInDeckTable.childCount - 1; i >= 0; --i) {
			GameObject.Destroy(CardsInDeckTable.GetChild(i).gameObject);
		}
		CardsInDeckTable.DetachChildren();
		for (int i = CardsInDeckPositionRoot.childCount - 1; i >= 0; --i) {
			GameObject.Destroy(CardsInDeckPositionRoot.GetChild(i).gameObject);
		}
		CardsInDeckPositionRoot.DetachChildren();

		for (int i = CardsCollectionTable.childCount - 1; i >= 0; --i) {
			GameObject.Destroy(CardsCollectionTable.GetChild(i).gameObject);
		}
		CardsCollectionTable.DetachChildren();

		foreach (GameObject gob in PanelsInDeckList) {
			if (gob != null) {
				Destroy (gob);
			}
		}

		PanelsInDeckList.Clear ();
		PanelsInCardsList.Clear ();
		CardsInDeckNames.Clear ();
		collectionMode = false;
		visibleCheckEnabled = true;
		PoolUnload = true;
	}

	private IEnumerator LoadCardPoolTask(Hero hero) {
		PoolUnload = false;
		while (PoolLoading) {
			yield return new WaitForSeconds (0.01f);
		}
		PoolLoading = true;
		List<CardsBase.CardDescriptionClass> cardsList;
		Color panelColor = Color.white;
		if (hero != null) {
			cardsList = CardsBaseComponent.GetCardsList (hero.CardsPool);
			panelColor = hero.HeroColor;
			CurrentHero = hero;
			Debug.Log (" Load Card pool for: " + hero.Name + " pool:" + hero.CardsPool);
		} else {
			cardsList = CardsBaseComponent.GetCardsList (CardsBase.SelectedHeroCards.Uniwersal);
			Debug.Log (" Load Card uniwersal pool");
		}
		foreach (CardsBase.CardDescriptionClass cardToSpawn in cardsList) {
			if (PoolUnload) {
				break;
			}
			bool FilterOut = false;
		
			//public List<string> RarityFilterList = new List<string>() {"Wszystkie", "Srebrne", "Złote", "Diamentowe"};
			//public List<string> RolesFilterList = new List<string>() {"Wszystkie role", "Ofensywne", "Defensywne", "Wspierające"};

			if ((CurrentRarityFilter == 1) && (cardToSpawn.Rarity != CardInteraction.CardRarityEnum.common))  {
				FilterOut = true;
			} else if ((CurrentRarityFilter == 2) && (cardToSpawn.Rarity != CardInteraction.CardRarityEnum.gold))  {
				FilterOut = true;
			} else if ((CurrentRarityFilter == 3) && (cardToSpawn.Rarity != CardInteraction.CardRarityEnum.diamond))  {
				FilterOut = true;
			}
			if ((CurrentRoleFilter == 1) && (cardToSpawn.Role != CardInteraction.CardRoleEnum.offence))  {
				FilterOut = true;
			} else if ((CurrentRoleFilter == 2) && (cardToSpawn.Role != CardInteraction.CardRoleEnum.defence))  {
				FilterOut = true;
			} else if ((CurrentRoleFilter == 3) && (cardToSpawn.Role != CardInteraction.CardRoleEnum.support))  {
				FilterOut = true;
			}

			if (!FilterOut) {
				GameObject card = CardsBaseComponent.SpawnCard (cardToSpawn, true);
				if (card) {
					CardInteraction CardInter = card.GetComponent<CardInteraction> ();
					GameObject panel = Instantiate (CardPanel);
					CardPanel panelComp = panel.GetComponent<CardPanel> ();

					if (!cardToSpawn.CardUnlocked) {
						if (ShowAllToggle.isOn) {
							card.GetComponent<SpriteRenderer> ().color = new Color (1f, 1f, 1f, 0.5f);
						} else {
							FilterOut = true;
						}
					}

					if (SetCardAsButtonChild) {
						card.transform.SetParent (panel.transform);
					} else {
						card.transform.SetParent (CardsBasePositionRoot);
					}
					card.transform.position = new Vector3 (-10000, 0, 0);//just keep card outside of camera
					card.GetComponent<CardInteraction> ().SetCardOrder (1);
					if (collectionMode) {
						panel.transform.SetParent (CardsCollectionTable, false);
						card.transform.localScale = InCollectionCardScale;
						panelComp.BuildDeckMode = false;
					} else {
						panel.transform.SetParent (CardsBaseTable, false);
						//card.transform.localScale = InTableCardScale;
						card.transform.localScale = InCollectionCardScale;
						panelComp.BuildDeckMode = true;
					}
					if (cardToSpawn.NewlyUnlocked) {
						panelComp.NewNotifyObject = Instantiate (NewCardNotifyPrefab, panel.transform);
					}

					panelComp.PanelInDeck = false;
					panelComp.PanelButton.image.color = panelColor;
					panelComp.AddCard (card);
					panelComp.KeepCardPosition = true;
					panelComp.CardsNumber = cardToSpawn.Quantity;
					panelComp.RefreshCardsNumber ();
					card.transform.position = panel.transform.position;
					PanelsInCardsList.Add (panel);
					yield return new WaitForSeconds (0.01f);
				}
			}
			//Debug.Log ("set card from current pos: " + card.transform.position + " to new pos: " + panel.transform.position);
		}
		PoolLoading = false;
		yield return null;
	}

	public void LoadCardPool(Hero hero) {
		LoadTasks.Add(StartCoroutine (LoadCardPoolTask (hero)));
	}

	public void LoadDeck(string[] deckCardsNames) {
		foreach (string name in deckCardsNames) {
			Debug.Log (" load card: " + name);
			if (name.Length > 0) {
				AddCardToTable (name, new Vector3 (0, 0, 0), true);
			}
		}
	}

	public void LoadDeck(string[] deckCardsNames, List<GameObject> spawnedPawns) {
		foreach (string name in deckCardsNames) {
			Debug.Log (" load card: " + name);
			if (name.Length > 0) {
				AddCardToTable (name, new Vector3 (0, 0, 0), true, spawnedPawns);
			}
		}
	}

	private void LoadAllCards() {
		foreach (GameObject HeroOb in HeroesBaseComponent.HeroesBaseList) {
			Hero currentHero = HeroOb.GetComponent<Hero> ();
			LoadCardPool (currentHero);
		}
		LoadCardPool (null);
	}

	public void LoadCollection() {
		collectionMode = true;
		LoadAllCards ();
	}
}
