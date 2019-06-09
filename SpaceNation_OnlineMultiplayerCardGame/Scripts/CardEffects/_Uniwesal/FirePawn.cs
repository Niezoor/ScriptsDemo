using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirePawn : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;
	private int FireForTurns = 1;

	private void FirePawnImpl(int pawnBoardId) {
		Debug.Log ("FirePawn Callback called with:" + pawnBoardId);
		GameObject pawnOb = pawn.gamePlayComp.Board [pawnBoardId].Pawn;
		if (pawnOb != null) {
			pawnOb.GetComponent<Pawn> ().SetOnFire(FireForTurns);
		}
	}


	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.RegisterOnAttackCallback (FirePawnImpl);
			if (pawn.PawnEffectParameters.Length >= 1) {
				FireForTurns = pawn.PawnEffectParameters [0];
			} else {
				Debug.LogWarning ("Pawn effect parameter 0 is not set");
			}
			Registered = true;
		}
	}
}
