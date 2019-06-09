using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poisoner : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;

	private void PoisonPawn(int pawnBoardId) {
		Debug.Log ("FirePawn Callback called with:" + pawnBoardId);
		GameObject pawnOb = pawn.gamePlayComp.Board [pawnBoardId].Pawn;
		if (pawnOb != null) {
			pawnOb.GetComponent<Pawn> ().Poisoned = true;
		}
	}


	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.RegisterOnAttackCallback (PoisonPawn);

			Registered = true;
		}
	}
}
