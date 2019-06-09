using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmoredGuardBuff : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;

	private int HealthLeftTrigger = 2;
	private int AttackBuff = 3;

	private bool buffed = false;
	// Use this for initialization
	void Start () {
		
	}

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

	private void BuffGuard () {
		if (!buffed) {
			for (int i = 0; i < Pawn.PawnConfigNumber; i++) {
				if (pawn.PawnConfiguration [i].block) {
					pawn.PawnConfiguration [i].block = false;
				}
			}
			pawn.ApplyConfig ();
			pawn.BuffAttack (pawn, AttackBuff);
			ShowParticleEffect (pawn.boardPosisionIndex);
			buffed = true;
		}
	}
	
	// Update is called once per frame
	void EffectUpdate (int u) {
		if ((pawn != null) && (pawn.Health <= HealthLeftTrigger)) {
			BuffGuard ();
		}
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.RegisterOnBoardUpdateCallback (EffectUpdate);
			if (pawn.PawnEffectParameters.Length >= 1) {
				HealthLeftTrigger = pawn.PawnEffectParameters [0];
			} else {
				Debug.LogWarning ("Pawn effect parameter 0 is not set");
			}
			if (pawn.PawnEffectParameters.Length >= 2) {
				AttackBuff = pawn.PawnEffectParameters [1];
			} else {
				Debug.LogWarning ("Pawn effect parameter 0 is not set");
			}
			Registered = true;
		}
	}
}
