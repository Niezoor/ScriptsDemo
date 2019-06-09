using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordMaster : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;

	// Use this for initialization
	void Start () {

	}

	private void attackCallback(int targetBoardId) {
		int nextBoardIndex = -1;

		for (int i = 0; i < 6; i++) {
			if (i == 0)
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUp (pawn.boardPosisionIndex);
			else if (i == 1)
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexDown (pawn.boardPosisionIndex);
			else if (i == 2)
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUpLeft (pawn.boardPosisionIndex);
			else if (i == 3)
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUpRight (pawn.boardPosisionIndex);
			else if (i == 4)
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexDownLeft (pawn.boardPosisionIndex);
			else
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexDownRight (pawn.boardPosisionIndex);

			if ((nextBoardIndex != -1) && (targetBoardId != nextBoardIndex)) {
				GameObject pawnOb = pawn.gamePlayComp.Board [nextBoardIndex].Pawn;
				if (pawnOb != null) {
					if ((pawn.Friendly && !pawnOb.GetComponent<Pawn> ().Friendly) ||
					    (!pawn.Friendly && pawnOb.GetComponent<Pawn> ().Friendly)) {
						pawn.gamePlayComp.DoDamageOnBoard (pawn, nextBoardIndex, pawn.Attack);
					}
				}
			}
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
