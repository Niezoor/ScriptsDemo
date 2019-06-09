using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckViewController : MonoBehaviour {
	public Canvas DeckViewCanvas;
	public Button EditButton;
	[HideInInspector]
	public CardsBase CardBaseComponent;
	public HeroesBase HeroesBaseComponent;
	public DeckTableController DeckTableContrComponent;
	public RectTransform CardTable;
	public Transform HeroPlace;
	public Transform StartPlace;
	public Transform PawnPositionRoot;
	public float heroScale;
	public InputField DeckName;
	public GameObject PanelPrefab;
	public MainMenu MainMenuComponent;

	public float AnimationInterval = 0.1f;
	public int AnimationSpeed = 20;

	public float PawnScale = 15;
	public bool StartAtPositionLeft;

	public Color SilverColor;
	public Color GoldColor;
	public Color DiamondColor;

	public List<GameObject> PanelsList = new List<GameObject> ();

	public float TableWidth = 0;

	private int DeckIndex;
	//private Coroutine LoadTask = null;
	private bool StopLoadTask = false;
	// Use this for initialization
	void Start () {
		DeckViewCanvas.enabled = false;
		DeckName.onEndEdit.RemoveAllListeners ();
		DeckName.onEndEdit.AddListener(ChangeDeckName);
		DeckName.DeactivateInputField ();
		EditButton.onClick.RemoveAllListeners ();
		EditButton.onClick.AddListener (EditDeck);
	}
	
	// Update is called once per frame
	void Update () {
		TableWidth = CardTable.rect.width;//tmp
	}

	public void PawnClicked(string name) {
		DeckTableContrComponent.CardsTableController.ShowCardHighlight (name);
	}

	private void ChangeDeckName(string name) {
		if (DeckViewCanvas.enabled) {
			Debug.Log ("Set deck name:" + name);
			DeckTableContrComponent.SetDeckName (DeckIndex, name);
		}
	}

	private IEnumerator LoadList(List<string> cardNames) {
		int count = 0;
		int cellxsize = (int)CardTable.GetComponent<GridLayoutGroup> ().cellSize.x;
		foreach (string name in cardNames) {
			Debug.Log (" load card: " + name);
			if (!StopLoadTask) {
				if (name.Length > 0) {
					//yield return new WaitForSeconds (0.1f);
					//AddCardToViewTable (name);
					GameObject panel = Instantiate (PanelPrefab);
					if (panel != null) {
						int inrow = (int)TableWidth / cellxsize;

						if (inrow >= 8) {
							int rest = cardNames.Count - count;
							int last = cardNames.Count % inrow;

							if (!StartAtPositionLeft) {
								rest--;
							}
							if (last > 0) {
								if (rest <= last) {
									GameObject pairpanel = Instantiate (PanelPrefab);
									pairpanel.transform.SetParent (CardTable.transform, false);
									PanelsList.Add (pairpanel);
								}
								if (count == 5) {
									CardTable.GetComponent<GridLayoutGroup> ().padding.top = (int)CardTable.GetComponent<GridLayoutGroup> ().cellSize.y / 2;
								}
							}
						}

						panel.transform.SetParent (CardTable.transform, false);
						PanelsList.Add (panel);
						count++;
						if (CardBaseComponent != null) {
							GameObject card = CardBaseComponent.SpawnCardByName (name);
							if (card != null) {
								Transform pawnTransform = card.transform.Find ("Pawn");
								if (pawnTransform != null) {
									pawnTransform.gameObject.GetComponent<KeepParentRenderLayer> ().KeepingActive = false;
									pawnTransform.gameObject.GetComponent<SpriteRenderer> ().sortingOrder = 55;
									Color color = pawnTransform.GetComponent<SpriteRenderer> ().color;
									color.a = 1;
									pawnTransform.GetComponent<SpriteRenderer> ().color = color;
									if (card.GetComponent<CardInteraction> ().CardRarity == CardInteraction.CardRarityEnum.common) {
										color = SilverColor;
									} else if (card.GetComponent<CardInteraction> ().CardRarity == CardInteraction.CardRarityEnum.gold) {
										color = GoldColor;
									} else if (card.GetComponent<CardInteraction> ().CardRarity == CardInteraction.CardRarityEnum.diamond) {
										color = DiamondColor;
									}
									pawnTransform.GetComponent<Pawn> ().SetBorder (color);
									pawnTransform.SetParent (this.transform);
									if (HeroPlace != null) {
										pawnTransform.SetParent (HeroPlace, false);
									}
									pawnTransform.localPosition = new Vector3 (0, 0, 0);
									if (StartPlace != null) {
										pawnTransform.position = StartPlace.position;
									}
									pawnTransform.rotation = CardTable.rotation;
									panel.GetComponent<CardViewPanel> ().SetPawn (pawnTransform.gameObject, GetComponent<DeckViewController> ());
									panel.GetComponent<CardViewPanel> ().TableWidth = TableWidth;
									yield return new WaitForSeconds (AnimationInterval);
									panel.GetComponent<CardViewPanel> ().MovePawn ();
									pawnTransform.localScale = new Vector3 (PawnScale, PawnScale, PawnScale);
								}
								Destroy (card);
							}
						}
					}
				}
			}
		}
		yield return null;
	}

	public void LoadDeckView(string[] deckCardsNames, string heroName, string deckName, int deckIndex) {
		MainMenuComponent.WaitForDataScreen.enabled = false;
		DeckIndex = deckIndex;
		TableWidth = CardTable.rect.width;
		List<string> cardNames = CardBaseComponent.SortCardsList (new List<string>(deckCardsNames));
		if (HeroPlace != null) {
			GameObject hero = HeroesBaseComponent.GetHeroByName (heroName);
			hero.transform.SetParent (HeroPlace, false);
			hero.transform.localPosition = new Vector3 (0, 0, 0);
			hero.transform.localScale = new Vector3 (heroScale, heroScale, heroScale);
			hero.GetComponent<SpriteRenderer> ().sortingOrder = 55;
			PanelsList.Add (hero);
		}
		DeckName.text = deckName;
		DeckViewCanvas.enabled = true;
		StopLoadTask = false;
		StartCoroutine(LoadList (cardNames));
	}

	public void DisableDeckView() {
		StopLoadTask = true;
		foreach (GameObject panel in PanelsList) {
			Destroy (panel);
		}
		PanelsList.Clear ();
		DeckViewCanvas.enabled = false;
	}

	public void EditDeck() {
		List<GameObject> PawnsList = new List<GameObject> ();
		foreach (GameObject panel in PanelsList) {
			if (panel != null) {
				if (panel.GetComponent<CardViewPanel> () != null) {
					GameObject pawn = panel.GetComponent<CardViewPanel> ().PanelPawn;
					if (pawn != null) {
						PawnsList.Add (pawn);
					}
					panel.GetComponent<CardViewPanel> ().PanelPawn = null;
				}
			}
		}
		DisableDeckView ();
		DeckTableContrComponent.LoadDeckToEdit (DeckIndex, PawnsList);
	}
}
