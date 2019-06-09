using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DeckTableController : MonoBehaviour {
	public bool CreateButtonFirst;
	public bool CirlcleList = false;
	public DeckBase   DeckBaseComponent;
	public HeroesBase heroesBaseComponent;
	public CardsBaseTableController CardsTableController;
	public DeckViewController DeckViewControllerComponent;
	public GameObject newDeckButton;
	public GameObject DeckPanel;
	public Transform  DeckTable;
	public Transform  DecksPosRoot;
	public GameObject DeckPrefab;
	public GameObject SeparatorPrefab;
	public GameObject NetworkingObject;
	public MainMenuPOPUP POPUPWindow;
	public MainMenu MainMenuComponent;
	public InputField DeckNameInput;
	public Text CardsInDeckStatusText;
	public string CurrentHeroName;
	public int toSaveDeckIndex;
	public DeckBase.DeckFromDeckBase DeckToSave;
	public Button PlayButton;
	public Transform CircleParentTransform;
	public CardDeckInterface CirecleInterface;
	public ScrollRectEnsureVisible ScrollRectEnsureVisibleComponent;
	public float DeckPanelScale = 1f;

	public Canvas DeckCreationAnimCanvas;
	public GameObject DeckCreationAnimPlace;
	private GameObject PanelInDeckAnimation;
	private GameObject PanelBackInDeckAnimation;

	public Transform SelectedDeckPanel;

	public GameObject[] DecksInScene = new GameObject[DeckBase.DecksNumberMax*2];

	private int toDeleteDeckIndex;
	//private GameObject NewDeck;
	private int cardInDeckNumber;

	private bool DeckListLoaded = false;

	// Use this for initialization
	void Start () {
		//PopulateList ();
		if (NetworkingObject == null && GameObject.Find ("NetworkManager") != null) {
			NetworkingObject = GameObject.Find ("NetworkManager");
		}
		//DeckBaseComponent.LoadDecksList ();
	}
	
	// Update is called once per frame
	void Update () {

	}

	public GameObject SpawnDeck(string DeckName, string HeroName, string[] CardNames, int DeckRenderOrder = 3) {
		GameObject deck = (GameObject)Instantiate (DeckPrefab);
		Deck deckComponent = deck.GetComponent<Deck> ();
		Debug.LogWarning (" SpawnDeck " + DeckRenderOrder);
		deckComponent.DeckRenderOrder = DeckRenderOrder;
		deckComponent.DeckName = DeckName;
		deckComponent.SetHero(heroesBaseComponent.GetHeroByName(HeroName));
		deckComponent.CardNames = CardNames;
		deckComponent.RefreshCardsNumber ();
		deckComponent.SpawnDeck ();

		return deck;
	}

	private GameObject SpawnDeckPanel(int deckIndex, DeckBase.DeckFromDeckBase DeckInfo, Transform table, bool Editable = true, int DeckRenderOrder = 3) {
		int i = deckIndex;
		if ((DeckInfo.DeckName.Equals ("empty"))) {
			MainMenuComponent.PlayerComponent.localPlayerData.deck = 0;
			MainMenuComponent.PlayerComponent.SavePlayerData ();
			i = 0;
		}
		if (!(DeckInfo.DeckName.Equals ("empty"))) {
			GameObject deck = SpawnDeck (
				DeckInfo.DeckName,
				DeckInfo.HeroName,
				DeckInfo.CardsNames,
				DeckRenderOrder
			);
			GameObject panel = (GameObject)Instantiate (DeckPanel);
			DeckPanel panelComponent = panel.GetComponent<DeckPanel> ();
			Deck deckComponent = deck.GetComponent<Deck> ();

			if (!Editable) {
				panelComponent.EditBTN.interactable = false;
				panelComponent.DelBTN.interactable = false;
			}

			NetworkingObject.GetComponent<MyNetworkManager> ().SetDeckToPlay (deck);

			DecksInScene [i] = deck;
			deckComponent.Hero.GetComponent<SpriteRenderer> ().sortingOrder = 30;
			//DecksInScene [i].transform.SetParent (DecksPosRoot.transform, false);
			panel.transform.SetParent (table, false);
			DecksInScene [i].transform.localScale = new Vector3 (DeckPanelScale, DeckPanelScale, DeckPanelScale);
			panel.transform.localScale = new Vector3 (DeckPanelScale, DeckPanelScale, DeckPanelScale);

			DecksInScene [i].transform.SetParent (panel.transform.GetChild(0).transform);
			DecksInScene [i].transform.localPosition = panelComponent.Deck.transform.localPosition;
			DecksInScene [i].transform.localRotation = panelComponent.Deck.transform.localRotation;
			DecksInScene [i].transform.localScale = panelComponent.Deck.transform.localScale;
	
			if (panelComponent.Deck.gameObject != null) {
				Destroy (panelComponent.Deck.gameObject);
			}
			panelComponent.Hero = deckComponent.Hero;
			panelComponent.Deck = DecksInScene [i];
			panelComponent.DeckName.text = deckComponent.DeckName;
			panelComponent.PanelActive = true;
			panelComponent.index = i;
			if (deckComponent.cardsInDeck != Deck.deckSize) {
				panelComponent.UncompleteDeck ();
			}
			Debug.Log ("Show deck panel nr " + i);
			return panel;
		} else {
			MainMenuComponent.gotoDeckSelect ();
		}
		return null;
	}

	private void AddDeckPanel(int deckIndex, DeckBase.DeckFromDeckBase DeckInfo, bool Editable = true) {
		GameObject panel = null;
		if ((PanelBackInDeckAnimation != null) &&
			(deckIndex == toSaveDeckIndex))
		{
			Debug.LogWarning ("New panel found " + toSaveDeckIndex);
			panel = PanelBackInDeckAnimation;
			if (CirlcleList) {
				PanelBackInDeckAnimation.transform.SetParent(CircleParentTransform);
				CirecleInterface._cards.Add (PanelBackInDeckAnimation.GetComponent<Card> ());
			} else {
				PanelBackInDeckAnimation.transform.SetParent(DeckTable);
				PanelBackInDeckAnimation.transform.localPosition = new Vector3 (0, 0, 0);
				PanelBackInDeckAnimation.transform.localScale = new Vector3 (1, 1, 1);
			}
		} else {
			if (CirlcleList) {
				panel = SpawnDeckPanel (deckIndex, DeckInfo, CircleParentTransform, Editable);
				CirecleInterface._cards.Add (panel.GetComponent<Card> ());
			} else {
				panel = SpawnDeckPanel (deckIndex, DeckInfo, DeckTable, Editable);
			}
		}
	}

	private IEnumerator PopulateListTask(bool withCreateButton) {
		int deckCount = 0;
		DeckListLoaded = false;
		while (DeckBaseComponent.DataLoaded == false) {
			yield return new WaitForSeconds (0.1f);
		}
		if (ScrollRectEnsureVisibleComponent != null) {
			ScrollRectEnsureVisibleComponent.GetComponent<ScrollRect> ().movementType = ScrollRect.MovementType.Elastic;
		}
		if (CreateButtonFirst && withCreateButton) {
			GameObject btn = (GameObject)Instantiate (newDeckButton);
			if (CirlcleList) {
				btn.transform.SetParent (CircleParentTransform, false);
				CirecleInterface._cards.Add (btn.GetComponent<Card> ());
			} else {
				btn.transform.SetParent (DeckTable, false);
			}
			btn.transform.localScale = new Vector3 (DeckPanelScale, DeckPanelScale, DeckPanelScale);
			btn.GetComponent<Button> ().onClick.AddListener (CreateNewDeck);
		}
		/*if (DeckBaseComponent.DecksNumber == 0) {
			Debug.Log ("no deck found");
			yield return null;
		}*/
		for (int i = 0; i < DeckBase.DecksNumberMax; i++) {
			if (!(DeckBaseComponent.MyDecks [i].DeckName.Equals("empty"))) {
				AddDeckPanel (deckCount, DeckBaseComponent.MyDecks [i]);
				deckCount++;
			} else {
				break;
			}
		}
		if (DeckBaseComponent.DecksNumber > 0) {
			if (SeparatorPrefab != null) {
				GameObject btn = (GameObject)Instantiate (SeparatorPrefab);
				if (CirlcleList) {
					btn.transform.SetParent (CircleParentTransform, false);
					CirecleInterface._cards.Add (btn.GetComponent<Card> ());
				} else {
					btn.transform.SetParent (DeckTable, false);
				}
			}
		}
		for (int i = 0; i < DeckBaseComponent.PremadeDecks.Length; i++) {
			AddDeckPanel (deckCount, DeckBaseComponent.PremadeDecks[i], false);
			deckCount++;
		}
		if (!CreateButtonFirst && withCreateButton) {
			GameObject btn = (GameObject)Instantiate (newDeckButton);
			if (CirlcleList) {
				btn.transform.SetParent (CircleParentTransform, false);
				CirecleInterface._cards.Add (btn.GetComponent<Card> ());
			} else {
				btn.transform.SetParent (DeckTable, false);
			}
			btn.transform.localScale = new Vector3 (DeckPanelScale, DeckPanelScale, DeckPanelScale);
			btn.GetComponent<Button> ().onClick.AddListener (CreateNewDeck);
			Debug.Log ("Show new deck panel");
		}
		if (CirlcleList) {
			CirecleInterface.StartDecksAnim ();
		}
		DeckListLoaded = true;
		yield return null;
	}

	public void PopulateList() {
		EraseList ();
		StartCoroutine (PopulateListTask (true));
	}

	public void PopulateListToSelect() {
		StartCoroutine (PopulateListTask (false));
	}

	public void ActiveList(int idx = 0) {
		PlayButton.interactable = false;
		foreach (Transform childTransform in DeckTable) {
			DeckPanel panelComponent = childTransform.GetComponent<DeckPanel> ();
			if (panelComponent != null && panelComponent.DeckAvailable) {
				if (panelComponent.GetDeckIndex () == idx) {
					Debug.Log ("Activate deck panel " + idx);
					PlayButton.interactable = true;
					panelComponent.SelectDeck ();
					break;
				}
			}
		}
	}

	public void ShowDeckToPlay(int idx) {
		if (idx < DeckBaseComponent.DecksNumber) {
			SpawnDeckPanel (idx, DeckBaseComponent.MyDecks [idx], SelectedDeckPanel);
		} else {
			idx -= DeckBaseComponent.DecksNumber;
			if (idx >= DeckBaseComponent.PremadeDecks.Length) {
				SpawnDeckPanel (idx, DeckBaseComponent.PremadeDecks [idx], SelectedDeckPanel, false);
			} else {
				SpawnDeckPanel (0, DeckBaseComponent.PremadeDecks [0], SelectedDeckPanel, false);
			}
		}
	}

	public void ChooseDeck(int idx) {
		if (MainMenuComponent.DeckChooseMode) {
			foreach (Transform childTransform in DeckTable) {
				DeckPanel panelComponent = childTransform.GetComponent<DeckPanel> ();
				if (panelComponent != null && panelComponent.DeckAvailable) {
					if (panelComponent.GetDeckIndex () != idx) {
						panelComponent.DeSelectDeck ();
					} else {
						NetworkingObject.GetComponent<MyNetworkManager> ().SetDeckToPlay (panelComponent.Deck);
						MainMenuComponent.PlayerComponent.localPlayerData.deck = idx;
						MainMenuComponent.PlayerComponent.SavePlayerData ();
						if (ScrollRectEnsureVisibleComponent != null) {
							ScrollRectEnsureVisibleComponent.GetComponent<ScrollRect> ().movementType = ScrollRect.MovementType.Unrestricted;
							ScrollRectEnsureVisibleComponent.CenterOnItem (childTransform.GetComponent<RectTransform> ());
						}
					}
				}
			}
		} else if (MainMenuComponent.ChooseGameMode) {
			MainMenuComponent.gotoDeckChooseMenu ();
		} else {
			LoadDeck (idx);
		}
	}

	public void EraseList() {
		DeckListLoaded = false;
		for (int i = DeckTable.childCount - 1; i >= 0; --i) {
			GameObject.Destroy (DeckTable.GetChild (i).gameObject);
		}
		DeckTable.DetachChildren();
		foreach (Transform childTransform in DecksPosRoot) Destroy(childTransform.gameObject);
		if (SelectedDeckPanel.childCount > 0) {
			Destroy(SelectedDeckPanel.GetChild (0).gameObject);
		}
		CirecleInterface.StopDeckAnim ();
	}

	private void CreateNewDeck() {
		/* new */
		DeckToSave = DeckBaseComponent.AddEmptyDeck ();
		toSaveDeckIndex = DeckBaseComponent.DecksNumber;
		if (DeckToSave != null) {
			for (int i = 0; i < Deck.deckSize; i++) {
				DeckToSave.CardsNames [i] = "";
			}
			cardInDeckNumber = 0;
			RefreshCardsNumber ();
			DeckNameInput.text = "Nowa talia";
			EraseList();
			GameObject.Find ("MainMenu-Canvas").GetComponent<MainMenu>().gotoHeroSelect ();
		}
	}

	public bool AddCardToDeck(string CardName) {
		if (cardInDeckNumber < Deck.deckSize) {
			//Debug.Log ("Add card[" + cardInDeckNumber + "]: " + CardName + " to deck [" + toSaveDeckIndex + "]");
			DeckToSave.CardsNames [cardInDeckNumber] = CardName;
			cardInDeckNumber++;
			RefreshCardsNumber ();
			return true;
		}
		return false;
	}

	public void RemoveCardFromDeck(string CardName) {
		bool finded = false;
		for (int i = 0; i < cardInDeckNumber; i++) {
			if (CardName == DeckToSave.CardsNames [i]) {
				finded = true;
			}
			if (finded) {
				if (i == (cardInDeckNumber-1)) {
					DeckToSave.CardsNames [i] = "";
				} else {
					DeckToSave.CardsNames [i] =
						DeckToSave.CardsNames [i+1];
				}
			}
		}
		if (finded) {
			cardInDeckNumber--;
		} else {
			Debug.LogWarning ("Cannot find card to remove:" + CardName);
		}
		RefreshCardsNumber ();
	}

	public void RefreshCardsNumber() {
		cardInDeckNumber = 0;
		foreach (string name in DeckToSave.CardsNames) {
			if (name != "")
				cardInDeckNumber++;
		}
		CardsInDeckStatusText.text = cardInDeckNumber + "/" + Deck.deckSize;
	}

	public void DeleteDeck(int deckIdx) {
		toDeleteDeckIndex = deckIdx;
		POPUPWindow.SetupDialogPOPUPWindow (
			"Czy chcesz usunąć talie?",
			ConfirmDeleteDeck,
			CancelDeleteDeck);
	}

	public void CancelDeleteDeck()
	{
		POPUPWindow.HidePOPUP ();
	}

	public void ConfirmDeleteDeck()
	{
		POPUPWindow.HidePOPUP ();
		//Debug.Log ("delete deck index:" + toDeleteDeckIndex);
		DeckBaseComponent.DeleteDeck (toDeleteDeckIndex);
		EraseList ();
		PopulateList ();
		MainMenuComponent.gotoDeckSelect ();
	}

	private void ShowDeckCreationAnim() {
		if (DeckCreationAnimCanvas) {
			DeckCreationAnimCanvas.enabled = true;
			PanelBackInDeckAnimation = SpawnDeckPanel (toSaveDeckIndex, DeckBaseComponent.MyDecks [toSaveDeckIndex], DeckCreationAnimPlace.transform, true, 5);
			if (PanelBackInDeckAnimation != null) {
				PanelInDeckAnimation = PanelBackInDeckAnimation.transform.GetChild (0).gameObject;
				if (PanelInDeckAnimation != null) {
					PanelInDeckAnimation.transform.SetParent (DeckCreationAnimPlace.transform, false);
					PanelInDeckAnimation.GetComponent<Animation> ().Play ();
				}
			} else {
				Debug.LogWarning ("Cannot create deck panel");
				DeckCreationAnimCanvas.enabled = false;
			}
		} else {
			Debug.LogWarning ("DeckCreationAnimCanvas is not set");
		}
	}

	private IEnumerator WaitForDeckListReady() {
		while (DeckListLoaded == false) {
			yield return new WaitForSeconds (0.1f);
		}
		if (PanelInDeckAnimation != null && PanelBackInDeckAnimation != null) {
			Debug.Log ("Saving and animation deck end");
			PanelInDeckAnimation.transform.SetParent (PanelBackInDeckAnimation.transform, true);
			PanelInDeckAnimation.GetComponent<SmothTransform> ().SmothTransformTo (new Vector3 (0, 0, 0), 10);
			PanelInDeckAnimation.transform.localScale  = new Vector3 (1, 1, 1);
		}
		DeckCreationAnimCanvas.enabled = false;
	}

	public void EndDeckCreationAnim() {
		Debug.Log ("Animation deck end");
		StartCoroutine (WaitForDeckListReady ());
	}

	public void SaveNewDeck()
	{
		//ShowDeckCreationAnim ();
		Debug.Log ("Save deck numer " + DeckToSave + " named:" + DeckNameInput.text);
		DeckToSave.DeckName = DeckNameInput.text;//DeckComponent.DeckName;
		DeckToSave.HeroName = heroesBaseComponent.SelectedHeroName;
		DeckBaseComponent.SaveDecksList ();
	}

	public void LoadDeck(int deckIndex) {
		MainMenuComponent.WaitForDataScreen.enabled = true;
		StartCoroutine (LoadDeckViewTask (deckIndex));
	}

	private IEnumerator LoadDeckViewTask(int deckIndex) {
		yield return new WaitForSeconds (0.01f);
		DeckPanel panel = GetPanelByDeckIndex (deckIndex);
		if (panel != null) {
			DeckViewControllerComponent.StartPlace = panel.transform;
		}
		if (deckIndex < DeckBaseComponent.DecksNumber) {
			DeckViewControllerComponent.EditButton.interactable = true;
			DeckViewControllerComponent.DeckName.interactable = true;
			DeckViewControllerComponent.LoadDeckView (DeckBaseComponent.MyDecks [deckIndex].CardsNames,
				DeckBaseComponent.MyDecks [deckIndex].HeroName, DeckBaseComponent.MyDecks [deckIndex].DeckName, deckIndex);
		} else {
			deckIndex -= DeckBaseComponent.DecksNumber;
			DeckViewControllerComponent.EditButton.interactable = false;
			DeckViewControllerComponent.DeckName.interactable = false;
			DeckViewControllerComponent.LoadDeckView (DeckBaseComponent.PremadeDecks [deckIndex].CardsNames,
				DeckBaseComponent.PremadeDecks [deckIndex].HeroName, DeckBaseComponent.PremadeDecks [deckIndex].DeckName, deckIndex);
		}
		yield return null;
	}

	public void LoadDeckToEdit(int deckIndex, List<GameObject> spawnedPawns) {
		MainMenuComponent.WaitForDataScreen.enabled = true;
		StartCoroutine (LoadDeckTask (deckIndex, spawnedPawns));
	}

	private IEnumerator LoadDeckTask(int deckIndex, List<GameObject> spawnedPawns) {
		yield return new WaitForSeconds (0.01f);
		toSaveDeckIndex = deckIndex;
		DeckToSave = DeckBaseComponent.MyDecks [deckIndex];
		CardsTableController.LoadDeck (DeckBaseComponent.MyDecks [deckIndex].CardsNames, spawnedPawns);
		DeckNameInput.text = DeckBaseComponent.MyDecks [deckIndex].DeckName;
		if (heroesBaseComponent.SelectedHero != null) {
			Destroy(heroesBaseComponent.SelectedHero.gameObject);
		}
		heroesBaseComponent.SelectedHero = heroesBaseComponent.GetHeroByName (DeckBaseComponent.MyDecks [deckIndex].HeroName);
		heroesBaseComponent.SelectedHeroName = DeckBaseComponent.MyDecks [deckIndex].HeroName;
		heroesBaseComponent.SelectedHero.transform.localPosition = new Vector3 (-1000, -1000, -1000);//hide it in scene
		RefreshCardsNumber ();
		MainMenuComponent.exitDeckSelect ();
		MainMenuComponent.gotoDeckBuild ();
		yield return null;
	}

	private DeckPanel GetPanelByDeckIndex(int index) {
		foreach (Transform childTransform in DeckTable) {
			DeckPanel rv = childTransform.GetComponent<DeckPanel> ();
			if (rv != null && rv.index == index) {
				return rv;
			}
		}
		return null;
	}

	public void SetDeckName(int deckIndex, string newName) {
		if (DeckBaseComponent.MyDecks [deckIndex] != null) {
			if (!DeckBaseComponent.MyDecks [deckIndex].DeckName.Equals (newName)) {
				DeckBaseComponent.MyDecks [deckIndex].DeckName = newName;
				DeckBaseComponent.SaveDecksList ();
				PopulateList ();
			}
		}
	}
}
