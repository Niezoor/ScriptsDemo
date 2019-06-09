using UnityEngine;
using System.Collections;

public class Hand : MonoBehaviour {

	public float handCircleRadius = 15;
	public float handDegMax = 18;
	public float handDegMin = 15;
	public const int cardsNumberMax = 10;

	public float GlobalYPosNormal = -19.2f;
	public float GlobalYPosStartHand = -16f;
	public float GlobalYPosHide = -17f;
	public float GlobalXPosHide = -3.5f;
	public float GlobalScaleHide = 0.8f;
	public bool StartHand;
	public bool HandShowHideModeEnabled = false;
	public bool HandShowHideModeAutoEnabled = false;
	public bool Hided = false;
	public bool HideDelayedCancel = false;
	public float CardInHandScale = 0.18f;

	public GameObject[] HandCards = new GameObject[cardsNumberMax];
	private int cardsNumber = 0;
	public int focusCard = -1;
	public bool LittleHandHideEnable = false;

	public bool UseCardFocus = false;
	public bool UseHandCanvas = false;
	public HandCanvas handCanvas;
	public GamePlay GameplayComp;
	public bool CardCanBePlayed = false;

	void Awake () {
		StartHand = false;
		if (UseHandCanvas) {
			this.transform.localPosition = new Vector3 (GlobalXPosHide, GlobalYPosHide, transform.localPosition.z);
			this.transform.localScale = new Vector3 (GlobalScaleHide, GlobalScaleHide, GlobalScaleHide);
			Hided = true;
		}
	}

	void Update () {
		if (HandShowHideModeEnabled && HandShowHideModeAutoEnabled) {
			if (Input.GetMouseButtonDown (0) && !StartHand) {
				if (!Hided) {
					StartCoroutine (HideHandDelayed ());
				}
			}
		}
	}

	public void SetupStartHand() {
		Vector3 tmpPos = transform.position;
		tmpPos.y = GlobalYPosStartHand;
		transform.position = tmpPos;
		StartHand = true;
	}

	public void SetupNormalHand() {
		if (!UseHandCanvas) {
			Vector3 newPos = transform.position;
			newPos.y = GlobalYPosNormal;
			StartHand = false;
			GetComponent<SmothTransform> ().SmothTransformTo (newPos, 10);
			RefreshCardInHand ();
			for (int i = 0; i < cardsNumber; i++) {
				HandCards [i].GetComponent<CardInteraction> ().StartDraw = false;
			}
		}
	}

	private IEnumerator HideHandDelayed() {
		if (!UseHandCanvas) {
			Debug.Log ("Hide hand delayed");
			yield return new WaitForSeconds (0.1f);
			if (!HideDelayedCancel) {
				Hided = true;
				Vector3 newPos = new Vector3 (GlobalXPosHide, GlobalYPosHide, transform.localPosition.z);
				Vector3 newScale = new Vector3 (GlobalScaleHide, GlobalScaleHide, GlobalScaleHide);
				Debug.Log ("Hide hand delayed wait over");
				StartCoroutine (CardAnimation (newPos, newScale, null));
			}
			HideDelayedCancel = false;
		}
	}

	private IEnumerator EnableOtherCardsDelayed() {
		yield return new WaitForSeconds (0.5f);
		EnableOtherCards ();
	}

	private IEnumerator CardAnimation(Vector3 newPos, Vector3 newScale, GameObject UnblockedCard) {
		if (UnblockedCard != null) {
			DisableOtherCards (UnblockedCard);
		} else {
			DisableOtherCards (null);
		}
		SmothTransform SMT = GetComponent<SmothTransform> ();
		SMT.smoothTransformPosRunning = false;
		SMT.smoothTransformScaleRunning = false;
		SMT.SmothTransformTo (newPos, 10);
		SMT.SmoothScaleTo (newScale, 10);
		yield return new WaitForSeconds (0.5f);
		if (UnblockedCard == null) {
			StartCoroutine (EnableOtherCardsDelayed());
		}
	}

	public void HideHand(GameObject card) {
		if (!Hided && !StartHand) {
			Hided = true;
			Debug.Log ("Hide hand with card:" + card);
			if (UseHandCanvas) {
				handCanvas.HideHandCanvas ();
				if (card == null) {
					StartCoroutine (EnableOtherCardsDelayed ());
				}
			} else {
				Vector3 newPos = new Vector3 (GlobalXPosHide, GlobalYPosHide, transform.localPosition.z);
				Vector3 newScale = new Vector3 (GlobalScaleHide, GlobalScaleHide, GlobalScaleHide);
				StartCoroutine (CardAnimation (newPos, newScale, card));
			}
		}
	}

	public void HideHandSimple(GameObject card) {
		if (LittleHandHideEnable) {
			if (!StartHand) {
				Hided = true;
				Debug.Log ("Hide hand simple with card:" + card);
				if (UseHandCanvas) {
					handCanvas.HideHandCanvas ();
					if (card == null) {
						StartCoroutine (EnableOtherCardsDelayed ());
					}
				} else {
					Vector3 newPos = new Vector3 (0, -20, transform.localPosition.z);
					Vector3 newScale = new Vector3 (1, 1, 1);
					StartCoroutine (CardAnimation (newPos, newScale, card));
				}
			}
		}
	}

	public void HideHand() {
		HideHand (null);
	}

	public void ShowHand() {
		HideDelayedCancel = true;
		if (HandShowHideModeEnabled && Hided) {
			Hided = false;
			Debug.Log ("Show hand");
			if (UseHandCanvas) {
				handCanvas.ShowHandCanvas (HandCards);
				StartCoroutine (EnableOtherCardsDelayed ());
			} else {
				Vector3 newPos = new Vector3 (0, GlobalYPosNormal, transform.localPosition.z);
				Vector3 newScale = new Vector3 (1, 1, 1);
				StartCoroutine (CardAnimation (newPos, newScale, null));
			}
		}
	}

	public void RefreshCardInHand() {
		bool anyCanBePlayed = false;
		if (!StartHand) {
			for (int i = 0; i < cardsNumber; i++) {
				anyCanBePlayed |= HandCards [i].GetComponent<CardInteraction> ().SignalCanBePlayed ();
			}
		}
		CardCanBePlayed = anyCanBePlayed;
	}

	public void RefreshTouchOfCardsInHand() {
		Debug.Log ("Refresh Touch Of Cards In Hand");
		for (int i = 0; i < cardsNumber; i++) {
			HandCards [i].GetComponent<CardInteraction> ().TouchOff ();
		}
	}

	public void DisableTouchEnterOfCardsInHand() {
		Debug.Log ("Disable Touch Of Cards In Hand");
		for (int i = 0; i < cardsNumber; i++) {
			HandCards [i].GetComponent<CardInteraction> ().isDisableTouchEnter = true;
		}
	}

	public void EnableTouchEnterOfCardsInHand() {
		Debug.Log ("Enable Touch Of Cards In Hand");
		for (int i = 0; i < cardsNumber; i++) {
			HandCards [i].GetComponent<CardInteraction> ().isDisableTouchEnter = false;
		}
	}

	public void SortCardsInHandImpl() {
		float rotationScale = (handDegMax - handDegMin) / cardsNumberMax;
		float halfRot = (rotationScale * cardsNumber) + handDegMin;
		float rotStep;

		if (focusCard == -1) {
			rotStep = (halfRot * 2) / (cardsNumber - 1);
		} else {
			rotStep = (halfRot * 2) / (cardsNumber - 1);
		}
		int cardsNumberHalf = cardsNumber / 2;

		//Debug.Log ("cards nr:" + cardsNumber + " half rot:" + halfRot + " rot step:" + rotStep);

		Quaternion newCardRot = Quaternion.identity;

		for (int i = 0; i < cardsNumber; i++) {
			Vector3 newCardPos = new Vector3(0,0,(0.01f*-i));
			int factory;

			if (focusCard != -1) {
				if (focusCard == i) {
					factory = cardsNumberHalf - i;
				} else if (focusCard > i) {
					factory = cardsNumberHalf - i + 1;
				} else {
					factory = cardsNumberHalf - i - 1;
				}
			} else {
				factory = cardsNumberHalf - i;
			}

			if (cardsNumber % 2 == 0) {
				float degRot;
				float radRot;
				if (cardsNumber == 2) {
					if (i == 0)
						degRot = 3;//should not be hardcoded
					else
						degRot = -3;
				} else {
					if (focusCard == -1) {
						degRot = rotStep * factory - (rotStep / 2);
					} else {
						if (focusCard == i) {
							degRot = rotStep * factory - (rotStep / 2);
						} else if (focusCard > i) {
							degRot = rotStep * factory - (rotStep / 2);
						} else {
							degRot = rotStep * factory - (rotStep / 2);
						}
					}
				}
				radRot = degRot * Mathf.Deg2Rad;
				float s = Mathf.Sin (radRot);
				float c = Mathf.Cos	(radRot);

				newCardPos.y = handCircleRadius;

				float xnew = newCardPos.x * c - newCardPos.y * s;
				float ynew = newCardPos.x * s + newCardPos.y * c;

				newCardPos.x = xnew;
				newCardPos.y = ynew;

				newCardRot.eulerAngles = new Vector3 (0, 0, degRot);
				//Debug.Log ("New card rot:" + newCardRot.z);
			} else {
				if (cardsNumber == 1) {
					newCardPos.y = handCircleRadius;
				} else {
					float degRot = rotStep * factory;
					float radRot = degRot * Mathf.Deg2Rad;

					float s = Mathf.Sin (radRot);
					float c = Mathf.Cos	(radRot);

					newCardPos.y = handCircleRadius;

					float xnew = newCardPos.x * c - newCardPos.y * s;
					float ynew = newCardPos.x * s + newCardPos.y * c;

					newCardPos.x = xnew;
					newCardPos.y = ynew;

					newCardRot.eulerAngles = new Vector3 (0, 0, degRot);
					//Debug.Log ("New card rot:" + newCardRot.z);
				}
			}
			if (HandCards [i] != null) {
				HandCards [i].transform.SetParent (this.transform);
				HandCards [i].GetComponent<SmothTransform> ().SmothTransformTo (newCardPos, newCardRot, 10);
				HandCards [i].GetComponent<SmothTransform> ().SmoothScaleTo(new Vector3(CardInHandScale, CardInHandScale, CardInHandScale), 5);
				HandCards [i].GetComponent<CardInteraction> ().SetCardOrder (i + 40);
				HandCards [i].GetComponent<CardInteraction> ().SetCardHandIndex (i);
			} else {
				RemoveCardFromHandWithDestroy (i);
			}
			//cardInter.SetCardOrder(i)
			//if (fast != true) {
				//yield return new WaitForSeconds (0.05f);
			//}
		}
	}

	public void SortCardsInHand() {
		if (UseHandCanvas && handCanvas != null) {
			if (Hided) {
				SortCardsInHandImpl ();
			}
		} else {
			SortCardsInHandImpl ();
		}
	}

	public bool AddCardToHand(GameObject card) {
		if (card != null) {
			if (cardsNumber < cardsNumberMax) {
				card.transform.SetParent (this.transform);
				if (!StartHand) {
					//card.GetComponent<CardInteraction> ().StartDraw = false;
				}
				if (UseHandCanvas && !Hided) {
					handCanvas.AddCard (card);
				}
				HandCards [cardsNumber] = card;
				//card.GetComponent<SmothTransform> ().SmoothScaleTo(new Vector3(CardInHandScale, CardInHandScale, CardInHandScale), 5);
				cardsNumber++;
				SortCardsInHand ();
			} else {
				return false;
			}
		}
		return true;
	}

	public void RemoveCardFromHand(GameObject card) {
		bool found = false;

		for (int i = 0; i < cardsNumber; i++) {
			if (HandCards [i] == card) {
				found = true;
			}
			if (found) {
				if (((i + 1) < cardsNumberMax)) {
					HandCards [i] = HandCards [i + 1];
				}
			}
		}
		if (found) {
			cardsNumber--;
		}
		SortCardsInHand ();
	}

	public void RemoveCardFromHandWithDestroy(string name) {
		for (int i = 0; i < cardsNumber; i++) {
			if (HandCards [i].GetComponent<CardInteraction>().CardName.Equals(name)) {
				RemoveCardFromHandWithDestroy (i);
				break;
			}
		}
		SortCardsInHand ();
	}

	public void RemoveCardFromHandWithDestroy(int index) {
		Debug.Log ("Remove card number: " + index);
		if (index >= 0) {
			if (HandCards [index] != null) {
				Destroy (HandCards [index]);
				cardsNumber--;
			}
			for (int i = index; i < cardsNumber; i++) {
				if ((i + 1) < cardsNumberMax)
					HandCards [i] = HandCards [i + 1];
			}
		}
		SortCardsInHand ();
	}

	public int GetHandIndexByPawn(GameObject pawn) {
		for (int i = 0; i < cardsNumber; i++) {
			if (HandCards [i].transform.Find ("Pawn").gameObject == pawn) {
				return i;
			}
		}
		return cardsNumber;
	}

	public int GetHandIndexByPawn(string name) {
		for (int i = 0; i < cardsNumber; i++) {
			if (HandCards [i].GetComponent<CardInteraction>().CardName.Equals(name)) {
				return i;
			}
		}
		return 0;
	}

	public void DisableOtherCards(GameObject Card) {
		for (int i = 0; i < cardsNumber; i++) {
			if ((Card == null) || (HandCards [i] != Card)) {
				HandCards [i].GetComponent<CardInteraction> ().DisableCard();
			}
		}
	}

	public void EnableOtherCards() {
		for (int i = 0; i < cardsNumber; i++) {
			if (HandCards [i] != null) {
				HandCards [i].GetComponent<CardInteraction> ().EnableCard ();
			}
		}
	}

	public void SetCardFocus(int cardHandIndex) {
		if (UseCardFocus) {
			focusCard = cardHandIndex;
			SortCardsInHand ();
		}
	}
}
