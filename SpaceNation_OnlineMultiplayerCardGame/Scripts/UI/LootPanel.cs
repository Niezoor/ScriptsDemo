using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LootPanel : MonoBehaviour {
	public int CardID = -1;
	public int PanelID = -1;
	public bool Clicked = false;
	public bool Duplicate = false;
	public int RewardForDuplicate;
	public CardInteraction.CardRarityEnum Rarity;

	public delegate void LootPanelCallback (int CardID, int PanelID);
	public LootPanelCallback PanelCallback = null;
	// Use this for initialization
	void Start () {
		GetComponent<Button> ().onClick.AddListener(PanelClick);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void RegisterListener(LootPanelCallback callback, int cardID, int panelID) {
		CardID = cardID;
		PanelID = panelID;
		PanelCallback = callback;
	}

	public void PanelClick() {
		if (PanelCallback != null) {
			if (!Clicked && CardID != -1) {
				PanelCallback (CardID, PanelID);
				Clicked = true;
			}
		}
	}
}
