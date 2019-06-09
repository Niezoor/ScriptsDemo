using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeckCardsScroll : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {
	public bool onDropZone = false;
	public GameObject EditedDeck;
	public float inDeckScale = 10;
	public GameObject cardsNumberText;
	public GameObject CardHighlightPlace;
	public Canvas CardHighlightCanvas;

	public float onPositionUpY = -357;
	public float onPositionDownY = -509;

	private Deck DeckComp;
	//private Transform dropScroll;
	public bool mouseDown = false;
	private bool setuped = true;
	private bool posDown = true;
	private Vector3 MousePosStart;
	//private GameObject CardInPlace;


	void Start() {
		if (EditedDeck != null) {
			DeckComp = EditedDeck.GetComponent<Deck> ();
		}
		/*if (GameObject.Find ("DeckCardsTable") != null) {
			dropScroll = GameObject.Find ("DeckCardsTable").GetComponent<Transform> ();
		}*/
	}

	// Update is called once per frame
	void Update () {
		if (mouseDown) {
			float deltaY = (MousePosStart.y - Input.mousePosition.y)*2;
			Vector3 newPos = transform.localPosition;
			/*Debug.Log("Position: " + transform.localPosition.y + 
				"mouse start: " + MousePosStart.y +
				"mouse now: " + Input.mousePosition.y +
				"delta: " + deltaY);*/
			if (posDown) {
				if (deltaY < 0) {
					if ((onPositionDownY - deltaY) < onPositionUpY) {
						newPos.y = onPositionDownY - deltaY;
					} else {
						newPos.y = onPositionUpY;
					}
				} else if (deltaY > 0) {
					if ((onPositionDownY - deltaY) > onPositionDownY) {
						newPos.y = onPositionDownY - deltaY;
					} else {
						newPos.y = onPositionDownY;
					}
				}
			} else {
				if (deltaY < 0) {
					if ((onPositionUpY - deltaY) < onPositionUpY) {
						newPos.y = onPositionUpY - deltaY;
					} else {
						newPos.y = onPositionUpY;
					}
				} else if (deltaY > 0) {
					if ((onPositionUpY - deltaY) > onPositionDownY) {
						newPos.y = onPositionUpY - deltaY;
					} else {
						newPos.y = onPositionDownY;
					}
				}
			}
			//MousePosStart.y = Input.mousePosition.y;
			transform.localPosition = newPos;
			setuped = false;
		} else {
			if (setuped == false) {
				Vector3 newPos = transform.localPosition;
				if (newPos.y > ((onPositionUpY + onPositionDownY) / 2)-10) {
					newPos.y = onPositionUpY;
					posDown = false;
				} else {
					newPos.y = onPositionDownY;
					posDown = true;
				}
				GetComponent<SmothTransform> ().SmothTransformTo (newPos, 20);
				//transform.localPosition = newPos;
				setuped = true;
			}
		}
	}

	public void OnPointerDown(PointerEventData eventData) {
		MousePosStart = Input.mousePosition;
		mouseDown = true;
	}

	public void OnPointerUp(PointerEventData eventData){
		//Debug.Log ("On pointer up");
		mouseDown = false;
	}

	public void OnPointerEnter(PointerEventData eventData) {
		//Debug.Log ("On pointer enter");
		onDropZone = true;
	}

	public void OnPointerExit(PointerEventData eventData) {
		//Debug.Log ("On pointer enter");
		onDropZone = false;
	}

	public void SetNewDeck(GameObject deck) {
		Debug.Log ("Set deck to edid");
		EditedDeck = deck;
		if (EditedDeck != null) {
			DeckComp = EditedDeck.GetComponent<Deck> ();
			cardsNumberText.GetComponent<Text> ().text = DeckComp.cardsInDeck + "/" + Deck.deckSize;
		}
	}

	/*public void ShowCardInHighlight(GameObject card) {
		CardHighlightCanvas.enabled = true;
		CardInPlace = card;
		card.GetComponent<CardInteraction> ().SetCardOrder(300);
		card.GetComponent<CardInteraction> ().deck_build_mode_enabled = false;
		card.transform.SetParent (CardHighlightPlace.transform);
		card.transform.localPosition = new Vector3 (0,0,0);
		card.transform.localScale = new Vector3 (30, 30, 30);
	}*/

	public void QuitHighlight(){
		//Destroy (CardInPlace);
		CardHighlightCanvas.enabled = false;
	}

	/*public void Build_RemoveCardToDeck (GameObject card) {
		if (EditedDeck != null) {
			if (DeckComp == null) {
				DeckComp = EditedDeck.GetComponent<Deck> ();
			}
			DeckComp.RemoveCardFromDeck(card.GetComponent<CardInteraction>().CardName);
			cardsNumberText.GetComponent<Text> ().text = DeckComp.cardsInDeck + "/" + Deck.deckSize;
		}
	}

	public bool Build_AddCardToDeck (GameObject card) {
		bool res;

		onDropZone = false;

		if (EditedDeck != null) {
			if (DeckComp == null) {
				DeckComp = EditedDeck.GetComponent<Deck> ();
			}
		} else {
			return false;
		}

		res = DeckComp.AddCardToDeck (card.GetComponent<CardInteraction>().CardName);
		if (res) {
			AddCardToDropZone (card);
		} else {
			Debug.Log ("cannot add");
		}
		return res;
	}

	public void AddCardToDropZone(GameObject card) {
		Debug.Log ("AddCardToDropZone Add card and sort");
		Destroy (card);
		for (int i = dropScroll.childCount - 1; i >= 0; --i) {
			GameObject.Destroy(dropScroll.GetChild(i).gameObject);
		}
		cardsNumberText.GetComponent<Text> ().text = DeckComp.cardsInDeck + "/" + Deck.deckSize;
		GameObject.Find ("CardsBase").GetComponent<CardsBase>().LoadDeck(DeckComp.CardNames);//To sort cards
		//GameObject.Find ("CardsBase").GetComponent<CardsBase>().SortCardInDeck();
	}*/
}
