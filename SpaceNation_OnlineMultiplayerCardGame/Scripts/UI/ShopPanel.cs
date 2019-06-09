using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : MonoBehaviour {
	public Button PanelButton;
	public Text ItemNameText;
	public Text ItemPriceText;
	public Image ItemPicture;

	public string ItemID;
	public uint ItemPrice;
	public string ItemCurrency;

	public delegate void ShopPanelCallback (string ItemID, uint ItemPrice, string ItemCurrency);
	private ShopPanelCallback PanelCallback = null;

	// Use this for initialization
	void Start () {
		PanelButton.onClick.AddListener (PanelClicked);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void PanelClicked() {
		if (PanelCallback != null) {
			PanelCallback (ItemID, ItemPrice, ItemCurrency);
		}
	}

	public void SetupPanel(string itemName, string itemId, Sprite pic, uint price, string currency, ShopPanelCallback callback) {
		ItemNameText.text = itemName;
		ItemID = itemId;
		ItemPicture.sprite = pic;
		ItemPrice = price;
		ItemPriceText.text = price.ToString () + " $";
		ItemCurrency = currency;
		PanelCallback = callback;
	}
}
