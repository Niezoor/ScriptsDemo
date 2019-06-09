using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamage : MonoBehaviour {
	private int DamageValue = 1;
	private Pawn pawn;
	private GameObject attackMark;
	private int lastBoardIndex;
	public bool Registered = false;
	//local mode: ok
	//online mode: ok

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
			Destroy (particleObject, 3);
		}
	}

	private void DoDealDamage(int startBoardFieldId, int boardFieldId) {
		ShowParticleEffect (boardFieldId);
		pawn.gamePlayComp.DoDamageOnBoard (pawn ,boardFieldId, DamageValue);
	}

	private int OnDealDamagePlay(int boardFieldId) {
		int returnValue = -1;
		int startPosId;
		int pawnPosId = pawn.gamePlayComp.GetClosestOverMouseAnyIndexPosition (pawn, false, false, false, false);

		//Debug.Log ("deal damage board id " + pawnPosId);
		if (pawnPosId >= 0) {
			if (pawn.CardType == CardsBase.CardTypesEnum.Effect) {
				startPosId = pawn.gamePlayComp.myHero.GetComponent<Pawn> ().boardPosisionIndex;
			} else {
				startPosId = pawn.boardPosisionIndex;
			}
			//pawn.gamePlayComp.ShowMarkForEnemy (pawnPosId, pawn.handIndex);
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

	private void OnDealDamageConfirm(int boardFieldId) {
		int handIdx;
		int startPosIdx;
		if (pawn.CardType == CardsBase.CardTypesEnum.Effect) {
			handIdx = pawn.handIndex;
			startPosIdx = pawn.gamePlayComp.myHero.GetComponent<Pawn> ().boardPosisionIndex;
		} else {
			handIdx = -1;
			startPosIdx = pawn.boardPosisionIndex;
		}
		DoDealDamage (startPosIdx, lastBoardIndex);
		pawn.gamePlayComp.PlayEffectOnBoard (pawn.Name, startPosIdx, lastBoardIndex, handIdx);
		pawn.gamePlayComp.DestroyMarkBeam ();
		Destroy (attackMark);
	}

	private void OnDealDamageCancel(int boardFieldId) {
		pawn.gamePlayComp.DestroyMarkBeam ();
		Destroy (attackMark);
	}

	private void TriggerEffect(int startFieldId, int endFieldId) {
		DoDealDamage(startFieldId, endFieldId);
	}

	private List<int> AIMove(int boardId) {
		GamePlay.TargetSettings settings = (GamePlay.TargetSettings.WithFriendlyPawn | GamePlay.TargetSettings.WithFriendlyHero);
		if (pawn.gamePlayComp.SetTargetsOnBoard (settings, pawn.gameObject, null)) {
			return pawn.gamePlayComp.TargetsList;
		}
		return null;
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.AITriggerEffectCallback = AIMove;
			pawn.OnPlayCallback = OnDealDamagePlay;
			pawn.RegisterPlayConfirmCallback(OnDealDamageConfirm);
			pawn.OnPlayCancelCallback = OnDealDamageCancel;
			pawn.TriggerEffectCallback = TriggerEffect;

			if (pawn.PawnEffectParameters.Length >= 1) {
				DamageValue = pawn.PawnEffectParameters [0];
			} else {
				Debug.LogWarning ("Pawn effect parameter 0 is not set");
			}
			Registered = true;
		}
	}
}
