using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab.ClientModels;
using PlayFab;
using System;

public class ShopMenu : MonoBehaviour {
	private Canvas MenuCanvas;

	public Button BackButton;
	public Text CurrencyText;
	public Transform ShopTable;
	public GameObject ShopPanelPrefab;
	private MainMenu MainMenuComponent;
	private NewItemNotify NewItemNotifyComponent;

	//public List<ItemInstance> ShopItems = new List<ItemInstance>();
	//private ItemInstance ItemToBuy = null;

	[System.Serializable]
	public class MyShopItemClass {
		public string ItemID;
		public string DisplayName;
		public Sprite Picture;
		public string Currency;
		public uint Price;
	}

	public List<MyShopItemClass> MyShopItems = new List<MyShopItemClass>();
	private MyShopItemClass ItemToBuy = null;

	// Use this for initialization
	void Start () {
		MenuCanvas = this.GetComponent<Canvas> ();
		MenuCanvas.worldCamera = Camera.main;
		BackButton.onClick.AddListener (BackButtonClick);
		LoadItems ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetMainMenu(MainMenu comp) {
		MainMenuComponent = comp;
		SetCurrency (MainMenuComponent.CreditCurrency);
	}

	public void HideShopMenu() {
		MenuCanvas.enabled = false;
	}

	private void LoadItems() {
		foreach (MyShopItemClass item in MyShopItems) {
			GameObject panel = Instantiate (ShopPanelPrefab, ShopTable);
			panel.GetComponent<ShopPanel> ().SetupPanel (
				item.DisplayName,
				item.ItemID,
				item.Picture,
				item.Price,
				item.Currency,
				TryBuyItem
			);
		}
		RefreshItemList ();
	}

	private void RefreshItemList() {
		if (MainMenuComponent != null) {
			foreach (Transform child in ShopTable.transform) {
				ShopPanel panel = child.GetComponent<ShopPanel> ();
				if (panel != null) {
					if (panel.ItemPrice > MainMenuComponent.CreditCurrency) {
						panel.PanelButton.interactable = false;
					}
				}
			}
		}
	}

	private void TryBuyItem(string ItemID, uint ItemPrice, string ItemCurrency) {
		if (MainMenuComponent != null) {
			string message = "Czy chcesz dokonać zakupu?";
			ItemToBuy = FindItem (ItemID);
			if (ItemToBuy != null) {
				MainMenuComponent.POPUPWindow.SetupDialogPOPUPWindow (
					message, ConfirmBuy, null
				);
			}
		}
	}

	private void ConfirmBuy() {
		if ((ItemToBuy != null) && (MainMenuComponent != null)) {
			Debug.Log ("Request buy :" + ItemToBuy.ItemID);
			PurchaseItemRequest request = new PurchaseItemRequest ();
			request.ItemId = ItemToBuy.ItemID;
			request.Price = (int)ItemToBuy.Price;
			request.VirtualCurrency = ItemToBuy.Currency;
			PlayFabClientAPI.PurchaseItem (request, PurchaseItemResult, PurchaseItemFailed);
			MainMenuComponent.ShowWaitIndicator ();
		}
	}

	private NewItemNotify GetNewIntemNotifyComponent() {
		if (NewItemNotifyComponent == null) {
			GameObject obj = GameObject.Find ("NewItemNotify-Canvas");
			if (obj != null) {
				NewItemNotifyComponent = obj.GetComponent<NewItemNotify> ();
			} else {
				Debug.LogWarning ("Cannot find item notify canvas");
			}
		}
		return NewItemNotifyComponent;
	}

	private void PurchaseItemResult(PurchaseItemResult result) {
		Debug.Log ("Item bought");
		if (GetNewIntemNotifyComponent () != null) {
			NewItemNotifyComponent.ShowNotify (ItemToBuy.Picture, null, ItemToBuy.DisplayName);
		}
		if (MainMenuComponent != null) {
			SetCurrency (MainMenuComponent.CreditCurrency - (int)ItemToBuy.Price);
			MainMenuComponent.LoadUserInventory ();
			MainMenuComponent.HideWaitIndicator ();
			RefreshItemList ();
		} else {
			ExitShopMenu ();
		}
	}

	private void PurchaseItemFailed(PlayFabError obj) {
		Debug.LogWarning ("Cannot open card pack");
		Debug.LogError (obj.GenerateErrorReport ());
		MainMenuComponent.HideWaitIndicator ();
		RefreshItemList ();
		//ExitShopMenu ();
	}

	public MyShopItemClass FindItem(string id) {
		foreach (MyShopItemClass item in MyShopItems) {
			if (item.ItemID.Equals (id)) {
				return item;
			}
		}
		return null;
	}

	private void SetCurrency(int cash) {
		if (CurrencyText != null) {
			CurrencyText.text = cash.ToString ();
		}
	}

	private void BackButtonClick() {
		ExitShopMenu ();
	}

	private void ExitShopMenu() {
		Destroy (this.gameObject);
	}
}
