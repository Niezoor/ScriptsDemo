using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyPawn : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;
	private GameObject attackMark;
	private GameObject attackMarkBeam;
	private int lastBoardIndex;
	//local mode: -
	//online mode: -

	private void DestroyChoosenPawn(int pawnBoardId) {
		GameObject PawnOb = pawn.gamePlayComp.Board [pawnBoardId].Pawn;
		if (PawnOb != null) {
			PawnOb.GetComponent<Pawn> ().PawnDie ();
		}
	}

	private int DestroyPawnPlay(int boardFieldId) {
		int returnValue = -1;
		int pawnPosId = pawn.gamePlayComp.GetClosestOverMouseAnyIndexPosition (pawn, true, false, true, true);
		int startPosId = pawn.gamePlayComp.myHero.GetComponent<Pawn> ().boardPosisionIndex;
		if (pawnPosId >= 0) {
			pawn.gamePlayComp.ShowMarkForEnemy (pawnPosId, pawn.handIndex);
			pawn.gamePlayComp.CreateAttackBeam (startPosId, pawnPosId);
			if (attackMark == null) {
				attackMark = pawn.gamePlayComp.CreateAttackMark (pawnPosId, 0);
				lastBoardIndex = pawnPosId;
			}
			if (lastBoardIndex != pawnPosId) {
				Destroy (attackMark);
			}
			returnValue = 0;
		}
		return returnValue;
	}

	private void DestroyPawnPlayConfirm(int boardFieldId) {
		int handIdx;
		int startPosIdx;
		if (pawn.CardType == CardsBase.CardTypesEnum.Effect) {
			handIdx = pawn.handIndex;
			startPosIdx = pawn.gamePlayComp.myHero.GetComponent<Pawn> ().boardPosisionIndex;
		} else {
			handIdx = -1;
			startPosIdx = pawn.boardPosisionIndex;
		}
		DestroyChoosenPawn (lastBoardIndex);
		pawn.gamePlayComp.PlayEffectOnBoard (pawn.Name, startPosIdx, lastBoardIndex, handIdx);
		pawn.gamePlayComp.DestroyMarkBeam ();
		Destroy (attackMark);
	}

	private void DestroyPawnPlayCancel(int boardFieldId) {
		pawn.gamePlayComp.DestroyMarkBeam ();
		Destroy (attackMark);
	}

	private void TriggerEffect(int startFieldId, int endFieldId) {
		DestroyChoosenPawn(endFieldId);
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.OnPlayCallback = DestroyPawnPlay;
			pawn.RegisterPlayConfirmCallback(DestroyPawnPlayConfirm);
			pawn.OnPlayCancelCallback = DestroyPawnPlayCancel;
			pawn.TriggerEffectCallback = TriggerEffect;
			Registered = true;
		}
	}
}
