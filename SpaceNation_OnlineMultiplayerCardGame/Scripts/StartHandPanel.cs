using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartHandPanel : MonoBehaviour {
	public Button ChangeButton;
	public GameObject Card;
	public StartHand StartHandComponent;
	public int CardDeckNumber = -1;
	public int CardHandNumber = -1;

	// Use this for initialization
	void Start () {
		ChangeButton.onClick.AddListener (ChangeCard);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void ChangeCard() {
		if (StartHandComponent != null && Card != null) {
			StartHandComponent.GamePlayComponent.DrawNextStartCard (Card, CardDeckNumber, CardHandNumber);
			Card = null;//forget this card
			Destroy(ChangeButton.gameObject);
		}
	}
}
