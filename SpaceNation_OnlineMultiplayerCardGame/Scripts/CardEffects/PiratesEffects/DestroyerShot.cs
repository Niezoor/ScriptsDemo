using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyerShot : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;

	private void AttackCallback (int attackedPawnBoardId) {
		int nextBoardIndex = attackedPawnBoardId;
		GamePlay.attackDirections attackDir = pawn.gamePlayComp.GetAttackDirection (pawn.boardPosisionIndex, attackedPawnBoardId);

		do {
			if (attackDir == GamePlay.attackDirections.Up) {
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUp (nextBoardIndex);
			} else if (attackDir == GamePlay.attackDirections.UpLeft) {
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUpLeft (nextBoardIndex);
			} else if (attackDir == GamePlay.attackDirections.UpRight) {
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUpRight (nextBoardIndex);
			} else if (attackDir == GamePlay.attackDirections.Down) {
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexDown (nextBoardIndex);
			} else if (attackDir == GamePlay.attackDirections.DownRight) {
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexDownRight (nextBoardIndex);
			} else if (attackDir == GamePlay.attackDirections.DownLeft) {
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexDownLeft (nextBoardIndex);
			} else {
				Debug.LogError("uknown attack direction");
				break;
			}
			if (nextBoardIndex != -1) {
				Debug.Log(" destryer attack " + nextBoardIndex);
				pawn.gamePlayComp.DoDamageOnBoard (pawn ,nextBoardIndex, pawn.Attack);
			}
		} while (nextBoardIndex != -1);
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.IgnorsShields = true;
			pawn.RegisterOnAttackCallback (AttackCallback);
			Registered = true;
		}
	}
}
