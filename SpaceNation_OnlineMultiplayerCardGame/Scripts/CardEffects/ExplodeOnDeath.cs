using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeOnDeath : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;

	private int DamageValue = 1;

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

	private void ExplodeOnDeathTrigger(int boardFieldId) {
		int nextBoardIndex;

		ShowParticleEffect (boardFieldId);

		for (int i = 0; i < 6; i++) {
			if (i == 0)
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUp (boardFieldId);
			else if (i == 1)
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexDown (boardFieldId);
			else if (i == 2)
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUpLeft (boardFieldId);
			else if (i == 3)
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUpRight (boardFieldId);
			else if (i == 4)
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexDownLeft (boardFieldId);
			else
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexDownRight (boardFieldId);

			if (nextBoardIndex != -1) {
				pawn.gamePlayComp.DoDamageOnBoard (pawn ,nextBoardIndex, DamageValue);
			}
		}
	}

	// Update is called once per frame
	void Awake () {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.RegisterDeathCallback(ExplodeOnDeathTrigger);

			if (pawn.PawnEffectParameters.Length >= 1) {
				DamageValue = pawn.PawnEffectParameters [0];
			} else {
				Debug.LogWarning ("Pawn effect parameter 0 is not set");
			}
			Registered = true;
		}
	}
}
