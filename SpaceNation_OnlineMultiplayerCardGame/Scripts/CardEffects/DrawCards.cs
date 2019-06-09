using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawCards : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;
	private int lastBoardIndex;
	private int cardNumber = 1;
	//local mode: -
	//online mode: -

	private void AddCardToHand(int cardsNumer) {
		for (int i = 0; i < cardsNumer; i++) {
			pawn.gamePlayComp.Draw ();
		}
	}

	private int AddCardToHandPlay(int boardFieldId) {
		int rv = pawn.CardType == CardsBase.CardTypesEnum.Effect ? 1 : 0;
		return rv;
	}

	private void AddCardToHandConfirm(int boardFieldId) {
		AddCardToHand (cardNumber);
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.OnPlayCallback = AddCardToHandPlay;
			pawn.RegisterPlayConfirmCallback(AddCardToHandConfirm);

			if (pawn.PawnEffectParameters.Length >= 1) {
				cardNumber = pawn.PawnEffectParameters [0];
			} else {
				Debug.LogWarning ("Pawn effect parameter 0 is not set");
			}
			Registered = true;
		}
	}
}
