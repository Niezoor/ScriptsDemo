using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealMax : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;
	private GameObject attackMark;
	private int lastBoardIndex;
	private int pawnToBuffBoardId;
	//local mode: ok
	//online mode: unsupported

	private void HealMaxChoosedPawn(int pawnToHealBoardId) {
		if (pawnToHealBoardId >= 0) {
			GameObject pawnGOb = pawn.gamePlayComp.Board [pawnToHealBoardId].Pawn;
			if (pawnGOb != null) {
				Pawn pawnTobHeal = pawnGOb.GetComponent<Pawn> ();
				pawnTobHeal.Heal (pawn.pawnBoardID, pawnTobHeal.MaxHealth);
			}
		}
	}

	private int OnHealMaxPlay(int PawnBoardFieldId) {
		int startBoardIdx = pawn.CardType == CardsBase.CardTypesEnum.Effect ? pawn.gamePlayComp.myHero.GetComponent<Pawn> ().boardPosisionIndex : PawnBoardFieldId;

		pawnToBuffBoardId = pawn.gamePlayComp.GetClosestOverMouseAnyIndexPosition(pawn, true, true, false, false);//with friendly pawn
		Debug.Log ("find pawn to buff:" + pawnToBuffBoardId + "start pawn:" + PawnBoardFieldId);
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

	private void OnHealMaxPlayConfirm(int PawnBoardFieldId) {
		Debug.Log ("OnHealMaxPlayConfirm:" + pawnToBuffBoardId + "start pawn:" + PawnBoardFieldId);
		int handIdx = pawn.CardType == CardsBase.CardTypesEnum.Effect ? pawn.handIndex : -1;//if its a pawn effect, do not play from hand
		Destroy (attackMark);
		pawn.gamePlayComp.DestroyMarkBeamFullControll ();
		HealMaxChoosedPawn (pawnToBuffBoardId);
		pawn.gamePlayComp.PlayEffectOnBoard (pawn.Name, pawn.boardPosisionIndex, pawnToBuffBoardId, handIdx);
	}

	private void OnHealMaxPlayCancel(int PawnBoardFieldId) {
		Debug.Log ("CancelPawnToBuff:" + pawnToBuffBoardId + "start pawn:" + PawnBoardFieldId);
		Destroy (attackMark);
		pawn.gamePlayComp.DestroyMarkBeamFullControll ();
	}

	private void TriggerHealMax(int startBoardPosId, int pawnToBuffBoardId) {
		HealMaxChoosedPawn (pawnToBuffBoardId);
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
			pawn.OnPlayCallback = OnHealMaxPlay;
			pawn.RegisterPlayConfirmCallback(OnHealMaxPlayConfirm);
			pawn.OnPlayCancelCallback = OnHealMaxPlayCancel;
			pawn.TriggerEffectCallback = TriggerHealMax;

			Registered = true;
		}
	}
}
