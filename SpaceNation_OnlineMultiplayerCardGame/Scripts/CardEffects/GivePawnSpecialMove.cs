using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GivePawnSpecialMove : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;
	private GameObject attackMark;
	private int lastBoardIndex;
	private int pawnToBuffBoardId;
	//local mode: ok
	//online mode: unsupported

	private void GiveSpecialMoveToChoosedPawn(int pawnToBuffId) {
		if (pawnToBuffId >= 0) {
			GameObject pawnGOb = pawn.gamePlayComp.Board [pawnToBuffId].Pawn;
			if (pawnGOb != null) {
				Pawn pawnTobHeal = pawnGOb.GetComponent<Pawn> ();
				pawnTobHeal.SpecialMovement = true;
				pawnTobHeal.ApplyConfig ();
			}
		}
	}

	private int OnGiveSpecialMovePlay(int PawnBoardFieldId) {
		int startBoardIdx = pawn.CardType == CardsBase.CardTypesEnum.Effect ? pawn.gamePlayComp.myHero.GetComponent<Pawn> ().boardPosisionIndex : PawnBoardFieldId;

		pawnToBuffBoardId = pawn.gamePlayComp.GetClosestOverMouseAnyIndexPosition(pawn, true, true, false, false);//with friendly pawn
		Debug.Log ("find pawn to buff:" + pawnToBuffBoardId + "start pawn:" + startBoardIdx);
		if (pawnToBuffBoardId >= 0) {
			pawn.gamePlayComp.CreateMarkBeamFullControll (startBoardIdx, pawnToBuffBoardId);
			if (attackMark == null) {
				attackMark = pawn.gamePlayComp.CreateAttackMark (pawnToBuffBoardId, 0);
				lastBoardIndex = pawnToBuffBoardId;
			}
			if (lastBoardIndex != pawnToBuffBoardId) {
				Destroy (attackMark);
			}
		}

		return 0;
	}

	private void OnGiveSpecialMoveConfirm(int PawnBoardFieldId) {
		Debug.Log ("ChoosePawnToBuff:" + pawnToBuffBoardId + "start pawn:" + PawnBoardFieldId);
		int handIdx = pawn.CardType == CardsBase.CardTypesEnum.Effect ? pawn.handIndex : -1;//if its a pawn effect, do not play from hand
		Destroy (attackMark);
		pawn.gamePlayComp.DestroyMarkBeamFullControll ();
		GiveSpecialMoveToChoosedPawn (pawnToBuffBoardId);
		pawn.gamePlayComp.PlayEffectOnBoard (pawn.Name, pawn.boardPosisionIndex, pawnToBuffBoardId, handIdx);
	}

	private void OnGiveSpecialMoveCancel(int PawnBoardFieldId) {
		Debug.Log ("CancelPawnToBuff:" + pawnToBuffBoardId + "start pawn:" + PawnBoardFieldId);
		Destroy (attackMark);
		pawn.gamePlayComp.DestroyMarkBeamFullControll ();
	}

	private void TriggeriveSpecialMove(int startBoardPosId, int endpawnToBuffBoardId) {
		GiveSpecialMoveToChoosedPawn (endpawnToBuffBoardId);
	}

	void Start () {
	}

	private List<int> AIMove(int boardId) {
		GamePlay.TargetSettings settings = GamePlay.TargetSettings.WithEnemyPawn;
		if (pawn.gamePlayComp.SetTargetsOnBoard (settings, pawn.gameObject, null)) {
			return pawn.gamePlayComp.TargetsList;
		}
		return null;
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.AITriggerEffectCallback = AIMove;
			pawn.OnPlayCallback = OnGiveSpecialMovePlay;
			pawn.RegisterPlayConfirmCallback(OnGiveSpecialMoveConfirm);
			pawn.OnPlayCancelCallback = OnGiveSpecialMoveCancel;
			pawn.TriggerEffectCallback = TriggeriveSpecialMove;

			Registered = true;
		}
	}
}
