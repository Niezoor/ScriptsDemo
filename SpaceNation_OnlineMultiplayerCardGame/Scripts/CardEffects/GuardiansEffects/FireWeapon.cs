using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireWeapon : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;
	private int FireForTurns = 1;

	private void FirePawn(int pawnBoardId) {
		Debug.Log ("FireWeapon Callback called with:" + pawnBoardId);
		GameObject pawnOb = pawn.gamePlayComp.Board [pawnBoardId].Pawn;
		if (pawnOb != null) {
			pawnOb.GetComponent<Pawn> ().SetOnFire(FireForTurns);
		}
	}

	private void WeaponeDetach(int ownerBoardId) {
		GameObject pawnOb = pawn.gamePlayComp.Board [ownerBoardId].Pawn;
		if (pawnOb != null) {
			Debug.Log ("Register weapon onAttackCalb");
			pawnOb.GetComponent<Pawn> ().RemoveOnAttackCallback (FirePawn);
		}
		Destroy (this.gameObject);
	}

	private void ModuleAplied(int ownerBoardId) {
		GameObject pawnOb = pawn.gamePlayComp.Board [ownerBoardId].Pawn;
		if (pawnOb != null) {
			Debug.Log ("Register weapon onAttackCalb");
			pawnOb.GetComponent<Pawn> ().RegisterOnAttackCallback (FirePawn);
			pawnOb.GetComponent<Pawn> ().OnDetachItemCallback = WeaponeDetach;
		}
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.OnApplyItemCallback = ModuleAplied;
			if (pawn.PawnEffectParameters.Length >= 1) {
				FireForTurns = pawn.PawnEffectParameters [0];
			} else {
				Debug.LogWarning ("Pawn effect parameter 0 is not set");
			}
			Registered = true;
		}
	}
}
