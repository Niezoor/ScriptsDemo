using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleAttackBuff : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;
	//local mode: ok
	//online mode: unsupported

	private void ShowParticleEffect(int boardFieldId) {
		if (pawn.PawnEffectParticle != null) {
			GameObject particleObject = (GameObject)Instantiate (pawn.PawnEffectParticle);
			particleObject.transform.SetParent (pawn.gamePlayComp.transform, true);
			Vector3 ParticlePos = pawn.gamePlayComp.Board [boardFieldId].BoardPiece.transform.localPosition;
			ParticlePos.z = -0.25f;
			Quaternion newRot =  Quaternion.identity;
			particleObject.transform.localPosition = ParticlePos;
			newRot = Quaternion.Euler (0, 0, 0);
			particleObject.transform.localRotation = newRot;
			particleObject.transform.localScale = new Vector3(1, 1, 1);
		}
	}


	private void BuffPawn(int pawnToBuffBoardId) {
		if (pawnToBuffBoardId >= 0) {
			GameObject pawnGOb = pawn.gamePlayComp.Board [pawnToBuffBoardId].Pawn;
			if (pawnGOb != null) {
				Pawn pawnTobuff = pawnGOb.GetComponent<Pawn> ();
				int buffAttackValue = pawnTobuff.Attack * 2;
				pawnTobuff.BuffAttack(pawn, buffAttackValue);
				ShowParticleEffect (pawnToBuffBoardId);
			}
		}
	}

	private void BuffPawnCallback(int boardId) {
		int handIdx = pawn.CardType == CardsBase.CardTypesEnum.Effect ? pawn.handIndex : -1;//if its a pawn effect, do not play from hand
		BuffPawn (boardId);
		pawn.gamePlayComp.PlayEffectOnBoard (pawn.Name, pawn.boardPosisionIndex, boardId, handIdx);
	}

	private void ChoosePawnToBuff(int PawnBoardFieldId) {
		GamePlay.TargetSettings settings =
			(GamePlay.TargetSettings.WithFriendlyPawn |
				GamePlay.TargetSettings.WithPawnsLock |
				GamePlay.TargetSettings.MustBeCalled);
		if (pawn.gamePlayComp.SetTargetsOnBoard (settings, pawn.gameObject, BuffPawnCallback)) {
			pawn.gamePlayComp.ShowTargetNotification (pawn.Desc);
		}
	}

	private void TriggerBuff(int startBoardPosId, int pawnToBuffBoardId) {
		BuffPawn (pawnToBuffBoardId);
	}

	private int OnPlaySetup (int unused) {
		return pawn.CardType == CardsBase.CardTypesEnum.Pawn ? 0 : 1;//1: Do not hide the card
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
			pawn.OnPlayCallback = OnPlaySetup;
			pawn.RegisterPlayConfirmCallback(ChoosePawnToBuff);
			pawn.TriggerEffectCallback = TriggerBuff;
			Registered = true;
		}
	}
}
