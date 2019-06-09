using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HandCanvas : MonoBehaviour {
	public Button HandCanvasButton;
	public GameObject HandPanel;
	public Transform Table;
	public float CardInHandScale = 40;

	public TextMeshProUGUI ActionPoints;
	public TextMeshProUGUI TurnTime;

	public Canvas handCanvas { get { return GetComponent<Canvas> (); } }
	public List<GameObject> Panels = new List<GameObject>();
	public Hand hand;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (handCanvas.enabled) {
			ActionPoints.SetText (hand.GameplayComp.Mana.ToString ());
			if (hand.GameplayComp.TurnRemainningTime >= 0) {
				TurnTime.SetText (hand.GameplayComp.TurnRemainningTime.ToString ());
			}
		}
	}

	public void AddCard(GameObject card) {
		Debug.Log ("Add card to hand canvas:" + card);
		if (card != null) {
			GameObject panel = Instantiate (HandPanel, Table);
			CardPanel cardPanel = panel.GetComponent<CardPanel> ();
			Panels.Add (panel);
			card.transform.localRotation = Quaternion.identity;
			card.transform.SetParent (panel.transform, false);
			card.GetComponent<SmothTransform> ().StopAll();
			cardPanel.AddCard (card);
			card.GetComponent<CardInteraction>().SetCardOrder (50);
			card.GetComponent<CardInteraction> ().CardPanelComp = cardPanel;
			card.GetComponent<SmothTransform> ().SmoothScaleTo (new Vector3 (CardInHandScale, CardInHandScale, CardInHandScale), 15);
		}
	}

	private IEnumerator EnterCardsAnimation(GameObject[] HandCards) {
		int i = 0;

		foreach (GameObject card in HandCards) {
			if (card != null) {
				card.transform.localRotation = Quaternion.identity;
				card.GetComponent<SmothTransform> ().StopAll();
				card.GetComponent<CardInteraction>().SetCardOrder (50);
				if (Panels.Count >= i) {
					if (Panels [i].GetComponent<CardPanel> ().Card == null) {
						Panels [i].GetComponent<CardPanel> ().AddCard (card);
						card.GetComponent<SmothTransform> ().SmoothScaleTo (new Vector3 (CardInHandScale, CardInHandScale, CardInHandScale), 15);
					}
					i++;
				}
				yield return new WaitForSeconds (0.05f);
			}
		}
	}

	public void ShowHandCanvas(GameObject[] HandCards) {
		Debug.Log ("Show hand canvas");
		bool useAnim = true;

		hand.GameplayComp.DisableAllPawnOnBoard ();
		hand.GameplayComp.EndTurnButton.interactable = false;
		handCanvas.enabled = true;
		foreach (GameObject card in HandCards) {
			if (card != null) {
				GameObject panel = Instantiate (HandPanel, Table);
				Panels.Add (panel);
				card.GetComponent<CardInteraction> ().CardPanelComp = panel.GetComponent<CardPanel> ();
				card.transform.SetParent (panel.transform, false);
				if (!useAnim) {
					panel.GetComponent<CardPanel> ().AddCard (card);
					card.GetComponent<SmothTransform> ().SmoothScaleTo (new Vector3 (CardInHandScale, CardInHandScale, CardInHandScale), 15);
					card.GetComponent<CardInteraction> ().PlayMoveSound();
				}
			}
		}
		if (useAnim) {
			StartCoroutine (EnterCardsAnimation (HandCards));
		}
	}

	public void HideHandCanvas() {
		Debug.Log ("Show hand canvas");
		hand.GameplayComp.EnableOtherPawnOnBoard ();
		hand.GameplayComp.EndTurnButton.interactable = true;
		hand.SortCardsInHandImpl ();
		hand.Hided = true;
		foreach (GameObject panel in Panels) {
			if (panel.GetComponent<CardPanel> ().Card != null) {
				panel.GetComponent<CardPanel> ().Card.transform.SetParent (hand.transform, false);
			}
			Destroy (panel);
		}
		foreach (GameObject card in hand.HandCards) {
			if (card != null) {
				card.GetComponent<CardInteraction> ().EnableCard ();
			}
		}
		if (hand.HandCards.Length > 0) {
			if (hand.HandCards [0] != null) {
				if (hand.HandCards [0].GetComponent<CardInteraction> () != null) {
					hand.HandCards [0].GetComponent<CardInteraction> ().PlayMoveSound ();
				}
			}
		}
		Panels.Clear ();
		handCanvas.enabled = false;
	}
}
