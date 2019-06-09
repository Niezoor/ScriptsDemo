using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mobilization : MonoBehaviour {
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

	private IEnumerator MobilizationEffectTask() {
		pawn.DetachPawn ();//to wait for processes end
		for (int i = 0; i < GamePlay.IndexMAX; i++) {
			GameObject pawnOb = pawn.gamePlayComp.Board [i].Pawn;
			Debug.Log ("pawn check:"+i);
			if (pawnOb != null) {
				Pawn TargetPawn = pawnOb.GetComponent<Pawn> ();
				if (TargetPawn.Friendly && pawnOb != pawn.gamePlayComp.myHero) {
					if (TargetPawn.Health < TargetPawn.MaxHealth) {
						Debug.Log ("pawn found:"+i);
						pawn.gamePlayComp.Draw ();
						ShowParticleEffect (i);
						yield return new WaitForSeconds (0.8f);
					}
				}
			}
		}
		Debug.Log ("search end");
		Destroy (pawn.gameObject);
		yield return null;
	}

	private void MobilizationEffect(int boardId) {
		StartCoroutine (MobilizationEffectTask ());
		int handIdx = pawn.CardType == CardsBase.CardTypesEnum.Effect ? pawn.handIndex : -1;
		pawn.gamePlayComp.PlayEffectOnBoard (pawn.Name, 0, 0, handIdx);
	}

	private int OnPlaySetup (int unused) {
		return pawn.CardType == CardsBase.CardTypesEnum.Pawn ? 0 : 1;//1: Do not hide the card
	}

	//private void TriggerBuff(int pawnStartBoardId, int pawnEndBoardId) {
	//	Debug.Log ("Nothing to do on trigger");
	//}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.OnPlayCallback = OnPlaySetup;
			pawn.RegisterPlayConfirmCallback(MobilizationEffect);
			//pawn.TriggerEffectCallback = TriggerBuff;
			Registered = true;
		}
	}
}
