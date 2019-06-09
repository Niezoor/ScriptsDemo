using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardPanel : MonoBehaviour {
	public Button PanelButton;
	public Image PanelImage;
	public GameObject Card;
	public string PanelCardName;
	public Text CardsNumberText;
	public int CardsNumber = 1;
	public bool PanelInDeck = false;
	public bool KeepCardPosition = false;
	public bool BuildDeckMode = true;
	public float CardPositionTrackingSpeed = 15;
	public float CardPositionTrackingSpeedHard = 1000;

	private CardsBaseTableController CardsControllerComp;

	public GameObject PrevCard;
	public GameObject NewNotifyObject;
	public bool CardPosReached = false;
	//public List<GameObject> CardList = new List<GameObject> ();

	// Use this for initialization
	void Start () {
		//RefreshCardsNumber (1);
		if (GameObject.Find ("DeckBuild-Canvas") != null) {
			CardsControllerComp = GameObject.Find ("DeckBuild-Canvas").GetComponent<CardsBaseTableController> ();
			PanelButton.onClick.AddListener (OnClick);
		}
		this.GetComponent<ButtonLongPress> ().onLongPress.AddListener (OnLongPress);
	}
	
	// Update is called once per frame
	void LateUpdate () {
		if (KeepCardPosition) {
			RefreshCardPosition ();
		}
		if (PrevCard != null && Card != null) {
			if (CardPosReached){
				Debug.Log ("Destroy PrevCard:" + PrevCard);
				Destroy (PrevCard);
			}
		}
	}

	public void OnLongPress() {
		Debug.Log (" Panel long pressed ");
		if (CardsControllerComp != null) {
			CardsControllerComp.ShowCardHighlight (PanelCardName);
		}
		Card.GetComponent<CardInteraction> ().longPressDetected = true;
	}

	public void OnClick() {
		if (CardsControllerComp != null) {
			CardsControllerComp.CardClicked(this.GetComponent<CardPanel>());
		}
		if (Card != null && BuildDeckMode) {
			if (PanelInDeck) {
				CardsControllerComp.RemoveCard (Card.GetComponent<CardInteraction> ().CardName);
			} else {
				CardsControllerComp.AddCardToDeck (Card.GetComponent<CardInteraction> ().CardName, this.transform.position);
			}
		} else {
			CardsControllerComp.ShowCardHighlight (PanelCardName);
		}
	}

	private IEnumerator SetCardsAnimSpeedForSecTask(float newSpeed) {
		float prevSpeed = CardPositionTrackingSpeedHard;
		CardPositionTrackingSpeedHard = newSpeed;
		yield return new WaitForSeconds (0.5f);
		CardPositionTrackingSpeedHard = prevSpeed;
	}

	public void SetCardsAnimSpeedForSec(float newSpeed) {
		StartCoroutine (SetCardsAnimSpeedForSecTask (newSpeed));
	}

	private IEnumerator RefreshCardPosTask() {
		yield return new WaitForSeconds (0.05f);
		if (Card != null) {
			Card.GetComponent<SmothTransform> ().SmoothGlobalTransformTo (this.transform.position, CardPositionTrackingSpeed);
			Card.GetComponent<SmothTransform> ().SmothTransformTo (Quaternion.identity, CardPositionTrackingSpeed);
		}
		int maxDelay = 0;
		while (Card != null && Card.GetComponent<SmothTransform> ().smoothTransformGlobalPosRunning == true) {
			Card.GetComponent<SmothTransform> ().SmoothGlobalTransformTo (this.transform.position, CardPositionTrackingSpeed);
			yield return new WaitForSeconds (0.05f);
			if (maxDelay >= 10) {
				break;
			}
			maxDelay++;
		}
		if (Card != null) {
			KeepCardPosition = true;
		}
	}

	public void AddCard(GameObject card) {
		KeepCardPosition = false;
		PanelCardName = card.GetComponent<CardInteraction> ().CardName;
		if (Card != null) {
			if (PrevCard != null) {
				Debug.Log ("Destroy PrevCard:" + PrevCard);
				Destroy (PrevCard);
			}
			card.GetComponent<SmothTransform> ().SmoothGlobalTransformTo (this.transform.position, CardPositionTrackingSpeed);
			card.GetComponent<SmothTransform> ().SmothTransformTo (Quaternion.identity, CardPositionTrackingSpeed);
			PrevCard = card;
		} else {
			Card = card;
		}
		CardPosReached = false;
		StartCoroutine (RefreshCardPosTask ());
	}

	public void RefreshCardsNumber() {
		if (CardsNumberText != null) {
			if (CardsNumber < 2) {
				CardsNumberText.text = "";
			} else {
				CardsNumberText.text = "x" + CardsNumber;
			}
		}
	}

	public void SmoothMoveCard() {
		KeepCardPosition = false;
		StartCoroutine (RefreshCardPosTask ());
	}

	public void RefreshCardPosition () {
		if (Card != null) {
			if (Card.GetComponent<SmothTransform> ().smoothTransformGlobalPosRunning == false) {
				CardPosReached = true;
				//KeepCardPosition = true;
			}
			Card.GetComponent<SmothTransform> ().smoothTransformPosRunning = false;
			Card.GetComponent<SmothTransform> ().smoothTransformGlobalPosRunning = false;
			//Card.GetComponent<SmothTransform> ().SmoothGlobalTransformTo (this.transform.position, CardPositionTrackingSpeedHard);
			Card.gameObject.transform.position = this.transform.position;
			Card.gameObject.transform.localRotation = Quaternion.identity;
		}
	}

	public void DestroyPanelOnly() {
		Debug.Log ("DestroyPanelOnly:" + Card);
		Destroy (this.gameObject);
	}

	public void DestroyPanel() {
		Debug.Log ("DestroyPanel:" + Card);
		Destroy(Card);
		if (PrevCard != null) {
			Destroy (PrevCard);
		}
		Destroy (this.gameObject);
	}

	public void HideCard() {
		PanelCardName = Card.GetComponent<CardInteraction> ().CardName;
		if (KeepCardPosition) {
			Debug.Log ("DestroyPanel:" + Card);
			Destroy (Card);
			if (PrevCard != null) {
				Destroy (PrevCard);
			}
		}
	}
}
