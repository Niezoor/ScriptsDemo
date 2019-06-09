using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleAttack : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;

	public int AttackInThisTurn = 0;

	private void NewTurnCallback (int attackedPawnBoardId) {
		AttackInThisTurn = 0;
	}

	private void AttackCallback (int attackedPawnBoardId) {
		Debug.Log ("Double attack callback " + AttackInThisTurn);
		AttackInThisTurn++;
		if (AttackInThisTurn >= 2) {
			pawn.AttackAlready = true;
		} else {
			pawn.AttackAlready = false;
		}
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.RegisterOnAttackCallback (AttackCallback);
			pawn.RegisterOnNewTurnCallback (NewTurnCallback);
			Registered = true;
		}
	}
}
