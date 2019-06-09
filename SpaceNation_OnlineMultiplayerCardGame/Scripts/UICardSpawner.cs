using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICardSpawner : MonoBehaviour {
	[System.Serializable]
	public class CardSpawnPositionsClass
	{
		public string CardNameToSpawn;
		public Transform CardToSpawnPos;
		public float CardScale = 10;
		public int CardOrder = 1;
		public bool PawnOnly = false;
		public int cardRotationIndex = 0;
		[HideInInspector]
		public GameObject CardObject;
	}

	public List<CardSpawnPositionsClass> CardSpawnPositions = new List<CardSpawnPositionsClass> ();

	private Canvas myCanvas { get { return GetComponent<Canvas> (); } }
	public CardsBase CardsComp;
	private bool SpawnEnabled = false;
	// Use this for initialization
	void Start () {
		if (GameObject.Find ("CardsBaseRecovery(Clone)") != null) {
			CardsComp = GameObject.Find ("CardsBaseRecovery(Clone)").GetComponent<CardsBase> ();
		} else if (GameObject.Find ("CardsBaseRecovery") != null) {
			CardsComp = GameObject.Find ("CardsBaseRecovery").GetComponent<CardsBase> ();
		} else if (GameObject.Find ("CardsBase") != null) {
			CardsComp = GameObject.Find ("CardsBase").GetComponent<CardsBase> ();
		} else {
			CardsComp = Instantiate (CardsComp);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (myCanvas.enabled && !SpawnEnabled) {
			SpawnCards ();
		} else if (!myCanvas.enabled && SpawnEnabled) {
			DestroySpawns ();
		}
	}

	public void SpawnCards() {
		SpawnEnabled = true;
		if (CardsComp == null && GameObject.Find ("CardsBase") != null) {
			CardsComp = GameObject.Find ("CardsBase").GetComponent<CardsBase> ();
		}
		foreach (CardSpawnPositionsClass cardToSpawn in CardSpawnPositions) {
			if (CardsComp != null) {
				GameObject GOb = CardsComp.SpawnCardByName (cardToSpawn.CardNameToSpawn);
				int order = cardToSpawn.CardOrder + 120;
				if (GOb != null) {
					GOb.GetComponent<CardInteraction> ().SetCardOrder (order);
					if (cardToSpawn.PawnOnly) {
						Transform pawnTransform = GOb.transform.Find ("Pawn");
						if (pawnTransform != null) {
							pawnTransform.transform.SetParent (cardToSpawn.CardToSpawnPos);
							pawnTransform.gameObject.GetComponent<KeepParentRenderLayer> ().KeepingActive = false;
							pawnTransform.gameObject.GetComponent<SpriteRenderer> ().sortingOrder = order*10;
							if (cardToSpawn.cardRotationIndex != 0) {
								pawnTransform.GetComponent<Pawn> ().SetPawnRotation (cardToSpawn.cardRotationIndex);
							}
							Color color = pawnTransform.GetComponent<SpriteRenderer> ().color;
							color.a = 1;
							pawnTransform.GetComponent<SpriteRenderer> ().color = color;
							Destroy (GOb);
							GOb = pawnTransform.gameObject;
						}
					}
					cardToSpawn.CardObject = GOb;
					GOb.transform.SetParent (cardToSpawn.CardToSpawnPos);
					GOb.transform.localPosition = new Vector3 (0, 0, 0);
					GOb.transform.localScale = new Vector3 (cardToSpawn.CardScale, cardToSpawn.CardScale, cardToSpawn.CardScale);
				}
			}
		}
	}

	public void DestroySpawns() {
		SpawnEnabled = false;
		foreach (CardSpawnPositionsClass cardToSpawn in CardSpawnPositions) {
			if (cardToSpawn.CardObject != null) {
				cardToSpawn.CardObject.transform.SetParent (transform.root);
				Destroy (cardToSpawn.CardObject);
			}
		}
	}
}
