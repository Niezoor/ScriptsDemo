using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartHand : MonoBehaviour {
	public GameObject PanelPrefab;
	public Transform Table;
	public Canvas HandCanvas;
	public Button KeepHandButton;
	public GamePlay GamePlayComponent;
	public Hand HandComponent;

	public float CardScale;
	public Vector3 CardPosition;

	public List<GameObject> PanelsList = new List<GameObject>();
	// Use this for initialization
	void Awake () {
		int cardNumber = Deck.deckSize - 1;
		for (int i = 0; i < GamePlayComponent.startHandCardsNumber; i++) {
			GameObject panel = Instantiate (PanelPrefab);
			if (panel != null) {
				panel.transform.SetParent (Table, false);
				panel.GetComponent<StartHandPanel> ().CardDeckNumber = cardNumber;
				panel.GetComponent<StartHandPanel> ().CardHandNumber = i;
				PanelsList.Add (panel);
				cardNumber--;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void KeepHand() {
		foreach (GameObject panel in PanelsList) {
			if (panel) {
				StartHandPanel handPanel = panel.GetComponent<StartHandPanel> ();
				if (handPanel.Card != null) {
					handPanel.Card.GetComponent<CardInteraction> ().SetCardInterationsEnable (true);
					HandComponent.AddCardToHand (handPanel.Card);
				}
			}
			Destroy (panel);
		}
		//HandCanvas.enabled = false;
		KeepHandButton.interactable = false;
		GamePlayComponent.KeepStartHand ();
	}

	public void AddCardToStartHand(GameObject card, int CardNumber) {
		Debug.Log("Add card :" + card);
		bool found = false;
		foreach (GameObject panel in PanelsList) {
			if (panel != null) {
				StartHandPanel handPanel = panel.GetComponent<StartHandPanel> ();
				if (handPanel.CardDeckNumber == CardNumber) {
					Debug.Log ("found free panel :" + panel);
					if (handPanel.Card != null) {
						Destroy (handPanel.Card);
					}
					handPanel.Card = card;
					handPanel.StartHandComponent = GetComponent<StartHand> ();
					card.GetComponent<CardInteraction> ().cardHandIndex = PanelsList.IndexOf (panel);
					card.transform.SetParent (panel.transform);
					SmothTransform STF = card.GetComponent<SmothTransform> ();
					STF.SmoothScaleTo (new Vector3 (CardScale, CardScale, CardScale), 10);
					Quaternion rot = Quaternion.identity;
					rot.eulerAngles = new Vector3 (0, 0, 0);
					STF.SmothTransformTo (CardPosition, rot, 10);
					found = true;
					break;
				}
			}
		}
		if (!found) {
			card.GetComponent<CardInteraction> ().SetCardInterationsEnable (true);
			HandComponent.AddCardToHand (card);
		}
	}
}
