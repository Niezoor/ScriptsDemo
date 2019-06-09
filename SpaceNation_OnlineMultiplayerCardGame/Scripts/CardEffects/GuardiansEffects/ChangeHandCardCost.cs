using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeHandCardCost : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;
	public int costChange = -1;
	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {

	}

	public void ChangeHandCardCostImpl(int PawnBoardFieldId) {
		if (pawn.gamePlayComp.HandComp != null) {
			foreach (GameObject cardOb in pawn.gamePlayComp.HandComp.HandCards) {
				if (cardOb != null) {
					CardInteraction card = cardOb.GetComponent<CardInteraction> ();
					if (card != null)
						card.SetCardCost (card.CardCost + costChange);
				}
			}
		}
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.RegisterPlayConfirmCallback(ChangeHandCardCostImpl);
			if (pawn.PawnEffectParameters.Length >= 1) {
				costChange = pawn.PawnEffectParameters [0];
			} else {
				Debug.LogWarning ("Pawn effect parameter 0 is not set");
			}
			Registered = true;
		}
	}
}
