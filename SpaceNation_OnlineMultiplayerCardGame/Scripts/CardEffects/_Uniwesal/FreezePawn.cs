using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezePawn : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;

	private void FrezzePawnImpl(int pawnBoardId) {
		Debug.Log ("FirePawn Callback called with:" + pawnBoardId);
		GameObject pawnOb = pawn.gamePlayComp.Board [pawnBoardId].Pawn;
		if (pawnOb != null) {
			pawnOb.GetComponent<Pawn> ().Freeze ();
		}
	}


	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.RegisterOnAttackCallback (FrezzePawnImpl);

			Registered = true;
		}
	}
}
