using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Deck : MonoBehaviour {
	public static int deckSize = 30;
	public string DeckName;
	public string[] CardNames = new string[deckSize];
	public GameObject Hero;
	public GameObject CardRevers;

	public int cardsInDeck = 0;
	public bool RefercesSpawned = false;
	public bool GetLastReversFromList = true;

	public Vector3 HeroPosition = new Vector3 (0 , 0.004194769f, 0.0145f);
	public Vector3 HeroScale = new Vector3 (0.0009115753f, 0.0005031814f, 0.003705502f);
	public Vector3 CardScale = new Vector3 (14, 14, 14);
	public int DeckRenderOrder = 3;

	public List<GameObject> CardsReversInDeck = new List<GameObject> ();

	void Awake () {
		RefreshCardsNumber ();
		//SpawnDeck ();
	}

	void Start() {
		//RefreshCardsNumber ();
		//SpawnDeck ();
	}

	private void DestroyRevers() {
		foreach (GameObject revers in CardsReversInDeck) {
			Destroy (revers);
		}
		CardsReversInDeck.Clear ();
		RefercesSpawned = true;
	}

	public void SpawnReverses(int number, bool erase = true) {
		if (CardRevers != null) {
			if (erase) {
				DestroyRevers ();
			}
			for (int i = 0; i < number; i++) {
				GameObject card = Instantiate (CardRevers, this.transform);
				card.transform.localPosition = new Vector3 (0, 0, (0.6f * CardsReversInDeck.Count));
				card.transform.localScale = CardScale;
				card.GetComponent<CardInteraction> ().SetCardOrder (DeckRenderOrder);//(deckSize+1) - i);
				CardsReversInDeck.Add (card);
			}
			RefercesSpawned = true;
		} else {
			Debug.LogError ("Please assign deck revers prefab");
		}
	}

	public GameObject GetRevers() {
		GameObject rv = null;
		if (RefercesSpawned && CardsReversInDeck.Count > 0) {
			CardsReversInDeck.RemoveAll(GameObject => GameObject == null);
			if (GetLastReversFromList) {
				rv = CardsReversInDeck[0];
			} else {
				rv = CardsReversInDeck [CardsReversInDeck.Count - 1];
			}
			CardsReversInDeck.Remove (rv);
		}
		return rv;
	}

	public void SpawnDeck () {
		if (CardRevers != null) {
			SpawnReverses (deckSize);//(cardsInDeck);
		}
	}

	public GameObject SetHero(GameObject hero, bool keepPos = false) {
		Debug.Log ("Set hero, old:" + Hero + " new:" + hero);
		SpriteRenderer renderer = null;
		if (hero) {
			if (Hero) {
				Destroy (Hero);
			}
			if (!keepPos) {
				hero.transform.SetParent (this.transform);
				hero.transform.localPosition = HeroPosition;
				hero.transform.localScale = HeroScale;
			}
			Hero = hero;
		}
		renderer = Hero.GetComponent<SpriteRenderer> ();
		if (renderer != null) {
			Debug.LogWarning (" set hero " + DeckRenderOrder);
			Hero.GetComponent<SpriteRenderer> ().sortingOrder = (DeckRenderOrder * 10) + 1;
		}
		return Hero;
	}

	public bool AddCardToDeck(string CardName) {
		if (cardsInDeck < deckSize) {
			CardNames [cardsInDeck] = CardName;
			cardsInDeck++;
			return true;
		}
		return false;
	}

	public void RemoveCardFromDeck(string CardName) {
		bool finded = false;
		RefreshCardsNumber ();
		for (int i = 0; i < cardsInDeck; i++) {
			if (CardName == CardNames [i]) {
				finded = true;
			}
			if (finded) {
				if (i == (cardsInDeck-1)) {
					CardNames [i] = "";
				} else {
					CardNames [i] = CardNames [i+1];
				}
			}
		}
		if (finded) {
			cardsInDeck--;
		}
	}

	public void ShuffleDeck() {
		Debug.Log ("ShuffleDeck");
		System.Random random = new System.Random();

		for( int i = 0; i < cardsInDeck; i ++ ) {
			int j = random.Next( i, cardsInDeck );
			string temporary = CardNames[ i ];
			CardNames[ i ] = CardNames[ j ];
			CardNames[ j ] = temporary;
		}
	}

	public string SwapCard(int CardNumber) {
		System.Random random = new System.Random();
		int randCard = random.Next(0, cardsInDeck+1);
		if (randCard != cardsInDeck) {
			string tmp = CardNames [randCard];
			CardNames [randCard] = CardNames [CardNumber];
			CardNames [CardNumber] = tmp;
		}
		Debug.Log ("Swap card number:" + CardNumber +" and" + randCard + " new:" + CardNames [CardNumber]);
		return CardNames [CardNumber];
	}

	public GameObject GetNextCard(CardsBase allCards) {
		cardsInDeck--;
		if (cardsInDeck < 0) {
			return null;
		} else {
			return allCards.SpawnCardByName (CardNames [cardsInDeck]);
		}
	}

	public void RefreshCardsNumber() {
		cardsInDeck = 0;
		foreach (string name in CardNames) {
			if (name != "") {
				cardsInDeck++;
			}
		}
	}
}
