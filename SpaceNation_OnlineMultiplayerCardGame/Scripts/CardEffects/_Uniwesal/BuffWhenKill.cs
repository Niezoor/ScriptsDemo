using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffWhenKill : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;
	public int buffAttackValue = 1;
	public int buffHealthValue = 1;

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

	private void OnkillCallback(int boardId) {
		ShowParticleEffect (boardId);
		if (buffAttackValue > 0)
			pawn.BuffAttack (pawn, buffAttackValue);
		if (buffHealthValue > 0)
			pawn.BuffHealth (pawn, buffHealthValue);
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.RegisterOnKillCallback (OnkillCallback);
			if (pawn.PawnEffectParameters.Length >= 1) {
				buffAttackValue = pawn.PawnEffectParameters [0];
			} else {
				Debug.LogWarning ("Pawn effect parameter 0 is not set");
			}
			if (pawn.PawnEffectParameters.Length >= 2) {
				buffHealthValue = pawn.PawnEffectParameters [1];
			} else {
				Debug.LogWarning ("Pawn effect parameter 0 is not set");
			}
			Registered = true;
		}
	}
}
