using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffWhenHurt : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;

	public int buffAttackValue = 1;
	public int buffHealthValue = 1;

	private bool pawnBuffed = false;

	public bool EffectEnabled = false;

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

	private void buffPawn() {
		if (!pawnBuffed) {
			pawn.BuffAttack (pawn, buffAttackValue);
			pawn.BuffHealth (pawn, buffHealthValue);
			pawnBuffed = true;
			if (pawn.boardPosisionIndex != -1) {
				ShowParticleEffect (pawn.boardPosisionIndex);
			}
		}
	}

	private void unBuffPawn() {
		if (pawnBuffed) {
			pawn.BuffAttack (pawn, -buffAttackValue);
			pawn.BuffHealth (pawn, -buffHealthValue);
			pawnBuffed = false;
		}
	}

	// Update is called once per frame
	void Update () {
		EffectUpdate (0);
	}

	private void EffectUpdate (int u) {
		if (EffectEnabled) {
			if (pawn != null) {
				if (pawn.MaxHealth > pawn.Health) {
					buffPawn ();
				} else {
					unBuffPawn ();
				}
			}
		}
	}

	private void EnableEffect(int unused) {
		EffectEnabled = true;
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();

			pawn.RegisterLocalPlayConfirmCallback (EnableEffect);
			pawn.RegisterOnBoardUpdateCallback (EffectUpdate);
			if (pawn.PawnEffectParameters.Length >= 1) {
				buffAttackValue = pawn.PawnEffectParameters [0];
			} else {
				Debug.LogWarning ("Pawn effect parameter 0 is not set");
			}
			if (pawn.PawnEffectParameters.Length >= 2) {
				buffHealthValue = pawn.PawnEffectParameters [1];
			} else {
				Debug.LogWarning ("Pawn effect parameter 1 is not set");
			}

			Registered = true;
		}
	}
}
