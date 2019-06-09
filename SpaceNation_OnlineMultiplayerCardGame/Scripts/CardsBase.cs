using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using PlayFab.ClientModels;
using PlayFab;

public class CardsBase : MonoBehaviour {
	public float card_scale = 8f;
	public float inDeckScale = 16f;
	public Transform CardsTable;
	public Transform CardsInDeckTable;
	public GameObject CardPrefab;
	public Sprite SilverCardSprite;
	public Sprite GoldCardSprite;
	public Sprite DiamondCardSprite;

	public bool DataLoaded = false;

	public enum PawnConfigSet {None, Block, Melee, Distance, MeleeAndBlock, DistanceAndBlock};
	public enum CardTypesEnum {Pawn, Effect, Weapon};

	public enum SelectedHeroCards {Hero1, Hero2, Uniwersal};

	[System.Serializable]
	public class CardDescriptionClass {
		public string Name;
		public int CardID;
		public bool CardEnabled = true;
		public bool CardUnlocked = false;
		public bool NewlyUnlocked = false;
		public int Quantity = 0;
		public int MaxQuantity {
			get { 
				if (Rarity == CardInteraction.CardRarityEnum.common) {
					return 3;
				} else if (Rarity == CardInteraction.CardRarityEnum.gold) {
					return 2;
				} else if (Rarity == CardInteraction.CardRarityEnum.diamond) {
					return 1;
				}
				return 3;
			}
		}
		[TextArea]
		public string Description;
		public CardTypesEnum CardMode;
		public CardInteraction.CardRarityEnum Rarity;
		public CardInteraction.CardRoleEnum Role;
		public int Attack;
		public int Health;
		public int Cost;
		public bool SpecialMovement;
		public Sprite Character;
		public Sprite Background;
		public bool ItemChangePawnConfig = false;
		public bool ItemMergePawnConfig = false;
		private const int pawnConfigSize = 6;
		public PawnConfigSet[] PawnConfig = new PawnConfigSet[pawnConfigSize];
		public GameObject BulletParticlePrefab;
		public Pawn.PawnShootingModeEnum ShootingMode = Pawn.PawnShootingModeEnum.singleShot;
		public float ShootInterval = 0.3f;
		public AudioClip DeathSound;
		public string EffectComponent;
		public int[] EffectParameters;
		public GameObject EffectParticles;
	}
	public List<CardDescriptionClass> Hero1CardsBase = new List<CardDescriptionClass> ();
	public List<CardDescriptionClass> Hero2CardsBase = new List<CardDescriptionClass> ();
	public List<CardDescriptionClass> UniwersalCardsBase = new List<CardDescriptionClass> ();

	private static CardsBase Instance;

	void Start () {
		//GenerateCardsID();
	}

	/// <summary>
	/// For test only.! Call once to save genarated and sorted data in prefab.
	/// </summary>
	private void GenerateCardsID() {
		int id = 1001;
		Hero1CardsBase = SortCardsByManaAndName (Hero1CardsBase);
		foreach (CardDescriptionClass CardDescription in Hero1CardsBase) {
			/*if (CardDescription.Rarity == CardInteraction.CardRarityEnum.common) {
				CardDescription.MaxQuantity = 3;
			} else if (CardDescription.Rarity == CardInteraction.CardRarityEnum.gold) {
				CardDescription.MaxQuantity = 2;
			} else if (CardDescription.Rarity == CardInteraction.CardRarityEnum.diamond) {
				CardDescription.MaxQuantity = 1;
			}*/
			CardDescription.CardID = id;
			id++;
		}
		id = 2001;
		Hero2CardsBase = SortCardsByManaAndName (Hero2CardsBase);
		foreach (CardDescriptionClass CardDescription in Hero2CardsBase) {
			/*if (CardDescription.Rarity == CardInteraction.CardRarityEnum.common) {
				CardDescription.MaxQuantity = 3;
			} else if (CardDescription.Rarity == CardInteraction.CardRarityEnum.gold) {
				CardDescription.MaxQuantity = 2;
			} else if (CardDescription.Rarity == CardInteraction.CardRarityEnum.diamond) {
				CardDescription.MaxQuantity = 1;
			}*/
			CardDescription.CardID = id;
			id++;
		}
		id = 0;
		UniwersalCardsBase = SortCardsByManaAndName (UniwersalCardsBase);
		foreach (CardDescriptionClass CardDescription in UniwersalCardsBase) {
			/*if (CardDescription.Rarity == CardInteraction.CardRarityEnum.common) {
				CardDescription.MaxQuantity = 3;
			} else if (CardDescription.Rarity == CardInteraction.CardRarityEnum.gold) {
				CardDescription.MaxQuantity = 2;
			} else if (CardDescription.Rarity == CardInteraction.CardRarityEnum.diamond) {
				CardDescription.MaxQuantity = 1;
			}*/
			CardDescription.CardID = id;
			id++;
		}
	}

	public void LoadUserCard(ItemInstance item) {
		int id = -1;
		Int32.TryParse (item.ItemId, out id);
		CardDescriptionClass cardDesc = FindCardDescByID (id);
		if (cardDesc != null) {
			Debug.Log ("card found:" + cardDesc.CardID);
			cardDesc.CardUnlocked = true;
			cardDesc.Quantity = item.RemainingUses.GetValueOrDefault(1);
		}
		DataLoaded = true;
	}

	public CardDescriptionClass UnlockCard(int cardId) {
		CardDescriptionClass cardDesc = FindCardDescByID (cardId);
		if (cardDesc != null) {
			Debug.Log ("card found:" + cardDesc.CardID + " x" + cardDesc.Quantity);
			if (!cardDesc.CardUnlocked) {
				cardDesc.NewlyUnlocked = true;
				cardDesc.CardUnlocked = true;
				cardDesc.Quantity = 1;
			} else {
				cardDesc.Quantity++;
				if (cardDesc.Quantity > cardDesc.MaxQuantity) {
					cardDesc.Quantity = cardDesc.MaxQuantity;
				} else {
					cardDesc.NewlyUnlocked = true;
				}
			}
			return cardDesc;
		}
		return null;
	}

	public void CardChecked(string cardName) {
		CardDescriptionClass cardDesc = FindCardDescByName (cardName);
		if (cardDesc != null) {
			cardDesc.NewlyUnlocked = false;
		}
	}

	public bool IsUnchecked() {
		foreach (CardDescriptionClass CardDescription in Hero1CardsBase) {
			if (CardDescription.NewlyUnlocked) {
				return true;
			}
		}
		foreach (CardDescriptionClass CardDescription in Hero2CardsBase) {
			if (CardDescription.NewlyUnlocked) {
				return true;
			}
		}
		foreach (CardDescriptionClass CardDescription in UniwersalCardsBase) {
			if (CardDescription.NewlyUnlocked) {
				return true;
			}
		}
		return false;
	}

	public List<string> CardsToList() {
		List<string> rv = new List<string>();
		foreach (CardDescriptionClass CardDescription in Hero1CardsBase) {
			rv.Add (CardDescription.Name);
		}
		foreach (CardDescriptionClass CardDescription in Hero2CardsBase) {
			rv.Add (CardDescription.Name);
		}
		foreach (CardDescriptionClass CardDescription in UniwersalCardsBase) {
			rv.Add (CardDescription.Name);
		}

		return rv;
	}

	/*void Awake(){
		DontDestroyOnLoad (this);

		if (Instance == null) {
			Instance = this;
		} else {
			DestroyObject(gameObject);
		}
	}*/

	public CardDescriptionClass FindCardDescByID(int ID) {
		CardDescriptionClass rv = null;

		foreach (CardDescriptionClass CardDescription in Hero1CardsBase) {
			if (ID == CardDescription.CardID) {
				rv = CardDescription;
			}
		}
		if (rv == null) {
			foreach (CardDescriptionClass CardDescription in Hero2CardsBase) {
				if (ID == CardDescription.CardID) {
					rv = CardDescription;
				}
			}
		}
		if (rv == null) {
			foreach (CardDescriptionClass CardDescription in UniwersalCardsBase) {
				if (ID == CardDescription.CardID) {
					rv = CardDescription;
				}
			}
		}

		if (rv == null) {
			Debug.LogWarning("Cannot find card description with ID: " + ID);
		}

		return rv;
	}

	public CardDescriptionClass FindCardDescByName(string name) {

		if (name == null || name.Length <= 0)
			return null;

		foreach (CardDescriptionClass CardDescription in Hero1CardsBase) {
			if (name.Equals(CardDescription.Name)) {
				return CardDescription;
			}
		}

		foreach (CardDescriptionClass CardDescription in Hero2CardsBase) {
			if (name.Equals(CardDescription.Name)) {
				return  CardDescription;
			}
		}

		foreach (CardDescriptionClass CardDescription in UniwersalCardsBase) {
			if (name.Equals(CardDescription.Name)) {
				return  CardDescription;
			}
		}

			
		int id = -200;
		if (Int32.TryParse (name, out id)) {
			return FindCardDescByID (id);
		}
			
		Debug.LogWarning("Cannot find card description for name: " + name);

		return null;
	}

	public int GetIdByName(string name) {
		CardDescriptionClass desc = FindCardDescByName (name);
		if (desc != null) {
			return desc.CardID;
		} else {
			return -1;
		}
	}

	public GameObject SpawnCardByID(int ID) {
		CardDescriptionClass cardDesc = FindCardDescByID(ID);
		if (cardDesc != null) {
			return SpawnCard (cardDesc, false);
		} else {
			return null;
		}
	}

	public GameObject SpawnCardByName(string name) {
		CardDescriptionClass cardDesc = FindCardDescByName(name);
		if (cardDesc != null) {
			return SpawnCard (cardDesc, false);
		} else {
			return null;
		}
	}

	public GameObject SpawnCard(CardDescriptionClass CardDescription, bool build_mode) {
		GameObject Card;
		CardInteraction CardInteraction;
		GameObject Pawn;
		Pawn PawnComponent;
		SpriteRenderer cardSpriteRenderer;

		if (!CardDescription.CardEnabled) {
			return null;
		}

		Card = (GameObject)Instantiate (CardPrefab, new Vector3(0,0,0), Quaternion.identity);
		Pawn = Card.transform.Find("Pawn").gameObject;
		Pawn.GetComponent<SpriteRenderer> ().sprite = CardDescription.Background;
		Pawn.transform.Find("Character").gameObject.GetComponent<SpriteRenderer> ().sprite = CardDescription.Character;
		cardSpriteRenderer = Card.GetComponent<SpriteRenderer> ();

		PawnComponent = Pawn.GetComponent<Pawn> ();
		PawnComponent.CardID = CardDescription.CardID;
		PawnComponent.Name = CardDescription.Name;
		PawnComponent.Desc = CardDescription.Description;
		PawnComponent.CardType = CardDescription.CardMode;

		PawnComponent.SetAttack (CardDescription.Attack);
		PawnComponent.SetHealth (CardDescription.Health);
		PawnComponent.SetConfig (CardDescription.PawnConfig, CardDescription.SpecialMovement);
		PawnComponent.ApplyConfig ();
		PawnComponent.ItemApplyConfig = CardDescription.ItemChangePawnConfig;
		PawnComponent.ItemMergeConfig = CardDescription.ItemMergePawnConfig;

		if (CardDescription.BulletParticlePrefab != null) {
			PawnComponent.ShotParticlePrefab = CardDescription.BulletParticlePrefab;
		}
		PawnComponent.ShootingMode = CardDescription.ShootingMode;
		PawnComponent.ShootInterval = CardDescription.ShootInterval;

		PawnComponent.PawnEffectParameters = CardDescription.EffectParameters;
		PawnComponent.PawnEffectParticle = CardDescription.EffectParticles;
		if (CardDescription.DeathSound != null) {
			PawnComponent.DeathSound = CardDescription.DeathSound;
		}

		CardInteraction = Card.GetComponent<CardInteraction> ();
		CardInteraction.SetName (CardDescription.Name);
		CardInteraction.SetDescription (CardDescription.Description);
		CardInteraction.SetCardCost (CardDescription.Cost);
		CardInteraction.SetCardOrder(1);
		CardInteraction.CardRole = CardDescription.Role;
		CardInteraction.CardRarity = CardDescription.Rarity;

		if (CardDescription.CardMode == CardTypesEnum.Pawn) {
			CardInteraction.SetTypeDescText ("Postać");
		} else if (CardDescription.CardMode == CardTypesEnum.Weapon) {
			CardInteraction.SetTypeDescText ("Ekwipunek");
		} else if (CardDescription.CardMode == CardTypesEnum.Effect) {
			CardInteraction.SetTypeDescText ("Efekt");
		}

		if (CardDescription.Rarity == CardInteraction.CardRarityEnum.common) {
			cardSpriteRenderer.sprite = SilverCardSprite;
		} else if (CardDescription.Rarity == CardInteraction.CardRarityEnum.gold) {
			cardSpriteRenderer.sprite = GoldCardSprite;
		} else if (CardDescription.Rarity == CardInteraction.CardRarityEnum.diamond) {
			cardSpriteRenderer.sprite = DiamondCardSprite;
		}

		if (!build_mode) {
			if (CardDescription.EffectComponent.Length > 0) {
				Type componentType = Type.GetType (CardDescription.EffectComponent);
				if (componentType != null) {
					Pawn.AddComponent (componentType);
				} else {
					Debug.LogWarning ("You are missing to add component class named: " + CardDescription.EffectComponent);
				}
			}
		}

		Card.SetActive(true);

		return Card;
	}

	/*public List<CardDescriptionClass> GetCardsList() {
		List<CardDescriptionClass> sortedCardsList;

		sortedCardsList = SortCardsByManaAndName (Hero1CardsBase);
		sortedCardsList.AddRange(SortCardsByManaAndName (Hero2CardsBase));
		sortedCardsList.AddRange(SortCardsByManaAndName (UniwersalCardsBase));
		return sortedCardsList;
	}*/

	public List<CardDescriptionClass> GetCardsList(SelectedHeroCards SelectHero) {
		List<CardDescriptionClass> HeroCardsBase;

		if (SelectHero == SelectedHeroCards.Hero1) {
			HeroCardsBase = Hero1CardsBase;
		} else if (SelectHero == SelectedHeroCards.Hero2) {
			HeroCardsBase = Hero2CardsBase;
		} else {
			HeroCardsBase = UniwersalCardsBase;
		}

		HeroCardsBase = SortCardsByManaAndName (HeroCardsBase);
		return HeroCardsBase;
	}

	/*public void LoadCards(SelectedHeroCards SelectHero) {
		List<CardDescriptionClass> HeroCardsBase;

		if (SelectHero == SelectedHeroCards.Hero1) {
			HeroCardsBase = Hero1CardsBase;
		} else if (SelectHero == SelectedHeroCards.Hero2) {
			HeroCardsBase = Hero2CardsBase;
		} else {
			HeroCardsBase = UniwersalCardsBase;
		}

		List<CardDescriptionClass> sortedCardsList = SortCardsByManaAndName (HeroCardsBase);

		foreach (CardDescriptionClass CardDescription in sortedCardsList) {
			SpawnCard (CardDescription, true);
		}
	}*/

	private List<CardDescriptionClass> SortCardsByManaAndName(List<CardDescriptionClass> cardsPool) {
		List<CardDescriptionClass> sortedList = new List<CardDescriptionClass> ();
		for (int i = 0; i <= 10; i++) {
			List<CardDescriptionClass> tmpList = new List<CardDescriptionClass> ();
			foreach (CardDescriptionClass CardDescription in cardsPool) {
				if (CardDescription.Cost == i) {
					tmpList.Add (CardDescription);
				}
			}
			tmpList = SortCardsByName (tmpList);
			sortedList = sortedList.Concat (tmpList).ToList();
		}

		return sortedList;
	}

	private List<CardDescriptionClass> SortCardsByName(List<CardDescriptionClass> cardsList) {
		return cardsList.OrderBy (x => x.Name).ToList();
	}

	public List<string> SortCardsList (List<string> ListToSort) {
		List<string> SortedList = new List<string>();
		List<CardDescriptionClass> deckCardsToSort = new List<CardDescriptionClass> ();

		foreach (string cardName in ListToSort) {
			CardDescriptionClass desc = FindCardDescByName (cardName);
			if (desc != null) {
				deckCardsToSort.Add (desc);
			}
		}
		deckCardsToSort = SortCardsByManaAndName (deckCardsToSort);
		foreach (CardDescriptionClass CardDescription in deckCardsToSort) {
			SortedList.Add (CardDescription.Name);
		}
		return SortedList;
	}

	/*public void UnloadCards() {
		for (int i = CardsTable.childCount - 1; i >= 0; --i) {
			GameObject.Destroy(CardsTable.GetChild(i).gameObject);
		}
		CardsTable.DetachChildren();
		for (int i = CardsInDeckTable.childCount - 1; i >= 0; --i) {
			GameObject.Destroy(CardsInDeckTable.GetChild(i).gameObject);
		}
		CardsInDeckTable.DetachChildren();
	}*/

	/*public void SortCardInDeck() {
		Debug.Log ("SortCardInDeck");
		List<string> deckCardsToSort = new List<string> ();
		for (int i = CardsInDeckTable.childCount - 1; i >= 0; --i) {
			Debug.Log ("SortCardInDeck to list");
			deckCardsToSort.Add(CardsInDeckTable.GetChild(i).gameObject.GetComponent<CardInteraction>().CardName);
		}
		for (int i = CardsInDeckTable.childCount - 1; i >= 0; --i) {
			Debug.Log ("SortCardInDeck remove children");
			GameObject.Destroy(CardsInDeckTable.GetChild(i).gameObject);
		}
		LoadDeck (deckCardsToSort);
	}*/

	/*public void LoadDeck(List<string> cardNames) {
		Debug.Log ("LoadDeck list");
		List<CardDescriptionClass> deckCardsToSort = new List<CardDescriptionClass> ();

		foreach (string cardName in cardNames) {
			CardDescriptionClass desc = FindCardDescByName (cardName);
			if (desc != null) {
				deckCardsToSort.Add (desc);
			}
		}

		deckCardsToSort = SortCardsByManaAndName (deckCardsToSort);
		foreach (CardDescriptionClass CardDescription in deckCardsToSort) {
			GameObject card = SpawnCard (CardDescription, false);
			card.GetComponent<CardInteraction> ().inDeckInteractions = true;
			AddCardToDropZone (card);
		}
	}

	public void LoadDeck(string[] cardNames) {
		Debug.Log ("LoadDeck array");
		List<CardDescriptionClass> deckCardsToSort = new List<CardDescriptionClass> ();

		foreach (string cardName in cardNames) {
			CardDescriptionClass desc = FindCardDescByName (cardName);
			if (desc != null) {
				deckCardsToSort.Add (desc);
			}
		}

		deckCardsToSort = SortCardsByManaAndName (deckCardsToSort);
		foreach (CardDescriptionClass CardDescription in deckCardsToSort) {
			GameObject card = SpawnCard (CardDescription, false);
			card.GetComponent<CardInteraction> ().inDeckInteractions = true;
			AddCardToDropZone (card);
		}
	}*/

	/*private void AddCardToDropZone(GameObject card) {
		card.transform.SetParent (CardsInDeckTable, false);
		card.transform.localScale = new Vector3 (inDeckScale, inDeckScale, inDeckScale);
		card.GetComponent<CardInteraction> ().SetCardOrder (4);
	}*/
}
