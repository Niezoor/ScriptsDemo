using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerfullChampion : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;
	public bool detached = false;

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

	public IEnumerator PowerfullChampionEffectTask(Pawn championPawn, int fiendly) {//0fiendly -1unfriendly
		Debug.Log("pawn triggering start task - detached?:" + detached);
		for (int i = 0; i < GamePlay.IndexMAX; i++) {
			GameObject pawnOb = pawn.gamePlayComp.Board [i].Pawn;
			if (pawnOb != null) {
				Pawn TargetPawn = pawnOb.GetComponent<Pawn> ();
				if ((TargetPawn.Friendly && fiendly == 0) ||
					(!TargetPawn.Friendly && fiendly == -1)) {
					if (TargetPawn != championPawn) {
						championPawn.BuffHealth (pawn, 1);
						ShowParticleEffect (TargetPawn.boardPosisionIndex);
						Debug.Log(" === pawn found ");
						yield return new WaitForSeconds (0.2f);
					}
				}
			}
		}
		Debug.Log("pawn triggering end");
	}

	public void PowerfullChampionEffectTaskNoAnim(Pawn championPawn, int fiendly) {//0fiendly -1unfriendly
		Debug.Log("pawn triggering start task - detached?:" + detached);
		for (int i = 0; i < GamePlay.IndexMAX; i++) {
			GameObject pawnOb = pawn.gamePlayComp.Board [i].Pawn;
			if (pawnOb != null) {
				Pawn TargetPawn = pawnOb.GetComponent<Pawn> ();
				if ((TargetPawn.Friendly && fiendly == 0) ||
					(!TargetPawn.Friendly && fiendly == -1)) {
					if (TargetPawn != championPawn) {
						championPawn.BuffHealth (pawn, 1);
						ShowParticleEffect (TargetPawn.boardPosisionIndex);
						Debug.Log(" === pawn found ");
					}
				}
			}
		}
		Debug.Log("pawn triggering end");
	}

	private void onPlayCallback(int boardId) {
		if (pawn.gamePlayComp.skipAnimations) {
			PowerfullChampionEffectTaskNoAnim (pawn, pawn.Friendly ? 0 : -1);
		} else {
			StartCoroutine (PowerfullChampionEffectTask (pawn, pawn.Friendly ? 0 : -1));
		}
		//int handIdx = pawn.CardType == CardsBase.CardTypesEnum.Effect ? pawn.handIndex : -1;
		//pawn.gamePlayComp.PlayEffectOnBoard (pawn.Name, boardId, 0, handIdx);
	}

	/*public void playEnemyEffect() {
		StartCoroutine (PowerfullChampionEffectTask (pawn, -1));
	}

	private void TriggerBuff(int targetBoardPosId, int notUsed) {
		GameObject pawnOb = pawn.gamePlayComp.Board [targetBoardPosId].Pawn;
		if (pawnOb != null) {
			Debug.Log("pawn triggering start - detach pawn");
			if (pawnOb.GetComponent<PowerfullChampion> () != null) {
				pawnOb.GetComponent<PowerfullChampion> ().playEnemyEffect ();
			}
		}
	}*/

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.RegisterLocalPlayConfirmCallback(onPlayCallback);
			//pawn.TriggerEffectCallback = TriggerBuff;
			Registered = true;
		}
	}
}
