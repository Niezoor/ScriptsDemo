using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushOnAttack : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;

	private void AttackCallback (int attackedPawnBoardId) {
		int nextBoardIndex = -1;
		GamePlay.attackDirections attackDir = pawn.gamePlayComp.GetAttackDirection (pawn.boardPosisionIndex, attackedPawnBoardId);

		if (attackDir == GamePlay.attackDirections.Up) {
			nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUp (attackedPawnBoardId);
		} else if (attackDir == GamePlay.attackDirections.UpLeft) {
			nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUpLeft (attackedPawnBoardId);
		} else if (attackDir == GamePlay.attackDirections.UpRight) {
			nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUpRight (attackedPawnBoardId);
		} else if (attackDir == GamePlay.attackDirections.Down) {
			nextBoardIndex = pawn.gamePlayComp.GetBoardIndexDown (attackedPawnBoardId);
		} else if (attackDir == GamePlay.attackDirections.DownRight) {
			nextBoardIndex = pawn.gamePlayComp.GetBoardIndexDownRight (attackedPawnBoardId);
		} else if (attackDir == GamePlay.attackDirections.DownLeft) {
			nextBoardIndex = pawn.gamePlayComp.GetBoardIndexDownLeft (attackedPawnBoardId);
		} else {
			Debug.LogError("uknown attack direction");
		}
		if (nextBoardIndex != -1) {
			if (pawn.gamePlayComp.IsFreePosision (nextBoardIndex)) {
				GameObject pawnGOb = pawn.gamePlayComp.Board [attackedPawnBoardId].Pawn;
				if (pawnGOb != null) {
					Pawn pawntomove = pawnGOb.GetComponent<Pawn> ();
					Vector3 newPawnPos = new Vector3 (0, 0, 0);
					pawn.gamePlayComp.RemovePawnFromPosisionImpl (attackedPawnBoardId);
					pawn.gamePlayComp.PutPawnOnPosisionImpl (pawnGOb, nextBoardIndex);
					pawntomove.boardPosisionIndex = nextBoardIndex;
					pawntomove.boardPosisionIndexPrev = nextBoardIndex;
					pawntomove.boardSavedPosisionIndexPrev = nextBoardIndex;
					newPawnPos = pawn.gamePlayComp.Board [nextBoardIndex].BoardPiece.transform.localPosition;
					newPawnPos.z = Pawn.PawnPosDown;
					pawnGOb.GetComponent<SmothTransform>().SmothTransformTo (newPawnPos, 10f);
				}
			}
		}
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.RegisterOnAttackCallback (AttackCallback);
			Registered = true;
		}
	}
}
