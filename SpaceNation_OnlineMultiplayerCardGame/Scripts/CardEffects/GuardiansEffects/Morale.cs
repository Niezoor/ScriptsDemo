using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Morale : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;

	public int buffAttackValueForEveryPawn = 1;
	public int buffHealthValueForEveryPawn = 1;
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

	private void BuffPawn(Pawn pawnToBuff, int buffAttackValue, int buffHealthValue) {
		ShowParticleEffect (pawnToBuff.boardPosisionIndex);
		if (pawnToBuff != null) {
			if (buffAttackValue != 0)	
				pawnToBuff.BuffAttack(pawn, buffAttackValue);
			if (buffHealthValue != 0)
				pawnToBuff.BuffHealth(pawn, buffHealthValue);
		}
	}

	private int CountNearPawn(bool Friendly, int CenterBoardId) {
		int nextBoardIndex = -1;
		int count = 0;

		for (int i = 0; i < 6; i++) {
			if (i == 0)
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUp (CenterBoardId);
			else if (i == 1)
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexDown (CenterBoardId);
			else if (i == 2)
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUpLeft (CenterBoardId);
			else if (i == 3)
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUpRight (CenterBoardId);
			else if (i == 4)
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexDownLeft (CenterBoardId);
			else
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexDownRight (CenterBoardId);

			if (nextBoardIndex != -1) {
				GameObject pawnOb = pawn.gamePlayComp.Board [nextBoardIndex].Pawn;
				if (pawnOb != null) {
					if (!pawn.Friendly) {
						if (!pawnOb.GetComponent<Pawn> ().Friendly)
							count++;
					} else {
						if (pawnOb.GetComponent<Pawn> ().Friendly)
							count++;
					}
				}
			}
		}
		return count;
	}

	private void BuffMoralePawns() {
		for (int i = 0; i < GamePlay.IndexMAX; i++) {
			GameObject pawnOb = pawn.gamePlayComp.Board [i].Pawn;
			if (pawnOb != null) {
				Pawn TargetPawn = pawnOb.GetComponent<Pawn> ();
				int x = CountNearPawn (pawn.Friendly, TargetPawn.boardPosisionIndex);
				if (!pawn.Friendly) {
					if (!TargetPawn.Friendly) {
						BuffPawn (TargetPawn, buffAttackValueForEveryPawn * x, buffHealthValueForEveryPawn * x);
					}
				} else {
					if (TargetPawn.Friendly) {
						BuffPawn (TargetPawn, buffAttackValueForEveryPawn * x, buffHealthValueForEveryPawn * x);
					}
				}
			}
		}
	}

	private void MoraleEffectCallback(int boardId) {
		int handIdx = pawn.CardType == CardsBase.CardTypesEnum.Effect ? pawn.handIndex : -1;
		BuffMoralePawns ();
		pawn.gamePlayComp.PlayEffectOnBoard (pawn.Name, -1/*unfriendly for enemy*/, 0, handIdx);
	}

	private void TriggerBuff(int Friendly, int pawnToBuffBoardId) {
		BuffMoralePawns ();
	}

	private int OnPlaySetup (int unused) {
		return pawn.CardType == CardsBase.CardTypesEnum.Pawn ? 0 : 1;//1: Do not hide the card
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.OnPlayCallback = OnPlaySetup;
			pawn.RegisterPlayConfirmCallback(MoraleEffectCallback);
			pawn.TriggerEffectCallback = TriggerBuff;
			if (pawn.PawnEffectParameters.Length >= 1) {
				buffAttackValueForEveryPawn = pawn.PawnEffectParameters [0];
			} else {
				Debug.LogWarning ("Pawn effect parameter 0 is not set");
			}
			if (pawn.PawnEffectParameters.Length >= 2) {
				buffHealthValueForEveryPawn = pawn.PawnEffectParameters [1];
			} else {
				Debug.LogWarning ("Pawn effect parameter 1 is not set");
			}
			Registered = true;
		}
	}
}
