using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InviolableWarrior : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;
	private int CurrentBoardPos = -1;

	private bool pawnBuffed = false;
	private bool Enabled = false;

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

	void Update () {
		EffectUpdate (0);
	}

	private void EffectUpdate (int u) {
		if (Enabled) {
			if (CurrentBoardPos == pawn.boardPosisionIndex) {
				buffWarrior ();
			} else {
				unbuffWarrior (0);
			}
		}
	}

	private void buffWarrior() {
		if (!pawnBuffed) {
			ShowParticleEffect (pawn.boardPosisionIndex);
			for (int i = 0; i < Pawn.PawnConfigNumber; i++) {
				pawn.PawnConfiguration [i].block = true;
			}
			pawn.ApplyConfig ();
			pawnBuffed = true;
		}
	}

	public void unbuffWarrior(int unused) {
		if (pawnBuffed) {
			for (int i = 0; i < Pawn.PawnConfigNumber; i++) {
				pawn.PawnConfiguration [i].block = false;
			}
			pawn.ApplyConfig ();
			pawnBuffed = false;
		}
	}

	private void newTurnCallback(int pawnBoardId) {
		Enabled = true;
		CurrentBoardPos = pawn.boardPosisionIndex;
		buffWarrior ();
	}

	private void TriggerBuff(int buffState/*1-buff,0-debuff*/, int pawnToBuffBoardId) {
		if (buffState == 0) {
			if (pawnToBuffBoardId >= 0) {
				if (pawn.gamePlayComp.Board [pawnToBuffBoardId].Pawn != null) {
					GameObject PawnOnBoard = pawn.gamePlayComp.Board [pawnToBuffBoardId].Pawn;
					PawnOnBoard.GetComponent<InviolableWarrior>().unbuffWarrior (pawnToBuffBoardId);
				}
			}
		}
	}

	// Update is called once per frame
	void Awake () {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.RegisterOnNewTurnCallback(newTurnCallback);
			pawn.RegisterOnBoardUpdateCallback (EffectUpdate);
			//pawn.RegisterOnMoveCallback (unbuffWarrior);
			pawn.TriggerEffectCallback = TriggerBuff;

			Registered = true;
		}
	}
}
