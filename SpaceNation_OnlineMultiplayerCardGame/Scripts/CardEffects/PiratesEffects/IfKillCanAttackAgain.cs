using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IfKillCanAttackAgain : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;

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

	private void KillCallback (int KilledPawnBoardId) {
		Debug.Log("Call on kill callback - Killed Pawn BoardId " + KilledPawnBoardId);
		pawn.AttackAlready = false;
		ShowParticleEffect (pawn.boardPosisionIndex);
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.RegisterOnKillCallback(KillCallback);
			Registered = true;
		}
	}
}
