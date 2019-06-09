using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezePawnWeapon : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;

	private void FreezePawn(int pawnBoardId) {
		Debug.Log ("FireWeapon Callback called with:" + pawnBoardId);
		GameObject pawnOb = pawn.gamePlayComp.Board [pawnBoardId].Pawn;
		if (pawnOb != null) {
			pawnOb.GetComponent<Pawn> ().Freeze ();
		}
	}

	private void WeaponeDetach(int ownerBoardId) {
		GameObject pawnOb = pawn.gamePlayComp.Board [ownerBoardId].Pawn;
		if (pawnOb != null) {
			Debug.Log ("Register weapon onAttackCalb");
			pawnOb.GetComponent<Pawn> ().RemoveOnAttackCallback (FreezePawn);
		}
		Destroy (this.gameObject);
	}

	private void ModuleAplied(int ownerBoardId) {
		GameObject pawnOb = pawn.gamePlayComp.Board [ownerBoardId].Pawn;
		if (pawnOb != null) {
			Debug.Log ("Register weapon onAttackCalb");
			pawnOb.GetComponent<Pawn> ().RegisterOnAttackCallback (FreezePawn);
			pawnOb.GetComponent<Pawn> ().OnDetachItemCallback = WeaponeDetach;
		}
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.OnApplyItemCallback = ModuleAplied;
			Registered = true;
		}
	}
}
