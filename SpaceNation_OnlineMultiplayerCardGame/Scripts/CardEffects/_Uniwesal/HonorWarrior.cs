using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HonorWarrior : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;

	// Use this for initialization
	void Start () {

	}

	private void attackCallback(int targetBoardId) {
		GameObject pawnOb = pawn.gamePlayComp.Board [targetBoardId].Pawn;
		if (pawnOb != null) {
			pawnOb.GetComponent<Pawn> ().DetachWeapon ();
		}
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.RegisterOnAttackCallback (attackCallback);

			Registered = true;
		}
	}
}
