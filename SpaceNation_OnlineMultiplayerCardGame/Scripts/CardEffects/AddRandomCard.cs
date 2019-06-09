using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddRandomCard : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;
	private int lastBoardIndex;
	private int cardNumber = 1;
	//local mode: -
	//online mode: -

	private void AddCardToHandTaskNoAnim(int cardsNumber) {
		List<string> cardsList = pawn.gamePlayComp.CardsComp.CardsToList ();
		Debug.Log ("Draw random cards:" + cardsNumber);
		for (int i = 0; i < cardsNumber; i++) {
			string randomCard = cardsList [Random.Range (0, cardsList.Count)];
			Debug.Log ("Draw random card named:" + randomCard);
			pawn.gamePlayComp.Draw (randomCard);
		}
		Debug.Log ("Draw end");
		Destroy (pawn.gameObject);
	}

	private IEnumerator AddCardToHandTask(int cardsNumber) {
		List<string> cardsList = pawn.gamePlayComp.CardsComp.CardsToList ();
		Debug.Log ("Draw random cards:" + cardsNumber);
		for (int i = 0; i < cardsNumber; i++) {
			string randomCard = cardsList [Random.Range (0, cardsList.Count)];
			Debug.Log ("Draw random card named:" + randomCard);
			pawn.gamePlayComp.Draw (randomCard);
			yield return new WaitForSeconds (1f);
		}
		Debug.Log ("Draw end");
		Destroy (pawn.gameObject);
		yield return null;
	}

	private void AddCardToHand(int cardsNumer) {
		pawn.DetachPawn ();
		pawn.transform.localPosition = new Vector3(0,0,-1000);
		if (pawn.gamePlayComp.skipAnimations) {
			AddCardToHandTaskNoAnim (cardNumber);
		} else {
			StartCoroutine (AddCardToHandTask (cardsNumer));
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
