using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SourceCommander : MonoBehaviour {
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

	private void GetDmgCallback(int pawnBoardId) {
		if (pawn.Friendly) {
			pawn.gamePlayComp.Draw ();
			ShowParticleEffect (pawnBoardId);
		} else {
			if (pawn.gamePlayComp.AIEnemy != null) {
				if (pawn.gamePlayComp.NetManager == null ||
					pawn.gamePlayComp.NetManager.GetComponent<MyNetworkManager> ().GameMode == MyNetworkManager.gameModeEnum.training) {
					if (!pawn.gamePlayComp.TutorialMode) {
						pawn.gamePlayComp.AIEnemy.GetComponent<AI>().AICardDraw(false);
					}
				}
			}
		}
	}

	// Update is called once per frame
	void Awake () {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.RegisterOnGetDamageCallback(GetDmgCallback);

			Registered = true;
		}
	}
}
