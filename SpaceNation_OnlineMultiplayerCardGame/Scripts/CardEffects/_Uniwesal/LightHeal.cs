using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightHeal : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;

	private int HealValue = 1;

	private void ShowParticleEffect(int unused) {
		if (pawn.PawnEffectParticle != null) {
			GameObject particleObject = (GameObject)Instantiate (pawn.PawnEffectParticle);
			particleObject.transform.SetParent (pawn.transform, true);
			Vector3 ParticlePos = new Vector3 (0, 0, -0.5f);//pawn.gamePlayComp.Board [boardFieldId].BoardPiece.transform.localPosition;
			Quaternion newRot =  Quaternion.identity;
			particleObject.transform.localPosition = ParticlePos;
			newRot = Quaternion.Euler (0, 0, 0);
			particleObject.transform.localRotation = newRot;
			particleObject.transform.localScale = new Vector3(1, 1, 1);
		}
	}

	private void HealNearPawn(int CenterBoardId) {
		int nextBoardIndex = -1;

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
					if ((pawn.Friendly) && (pawnOb.GetComponent<Pawn> ().Friendly) ||
						(!pawn.Friendly) && (!pawnOb.GetComponent<Pawn> ().Friendly)) {
						pawnOb.GetComponent<Pawn> ().Heal(pawn.pawnBoardID, HealValue);
						//ShowParticleEffect (nextBoardIndex);
					}
				}
			}
		}
	}

	private void newTurnCallback(int CenterBoardId) {
		Debug.Log ("New turn callback for Light Heal:" + CenterBoardId);
		HealNearPawn (CenterBoardId);
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.RegisterOnNewTurnCallback (newTurnCallback);
			pawn.RegisterLocalPlayConfirmCallback (ShowParticleEffect);
			if (pawn.PawnEffectParameters.Length >= 1) {
				HealValue = pawn.PawnEffectParameters [0];
			} else {
				Debug.LogWarning ("Pawn effect parameter 0 is not set");
			}
			Registered = true;
		}
	}
}
