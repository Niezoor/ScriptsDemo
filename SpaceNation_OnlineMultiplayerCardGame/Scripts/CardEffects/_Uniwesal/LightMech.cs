using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightMech : MonoBehaviour {
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

	private void onDieCallback(int boardFieldId) {
		Debug.Log ("Light mech effect on boardposID:" + boardFieldId);
		if (pawn.Friendly) {
			List<GameObject> cardList = new List<GameObject> ();
			Hand handComp = pawn.gamePlayComp.HandComp;
			if (handComp != null) {
				foreach (GameObject cardOb in handComp.HandCards) {
					if (cardOb != null) {
						Pawn cardPawn = cardOb.GetComponent<CardInteraction> ().pawnComponent;
						if (cardPawn != null) {
							if (cardPawn.CardType == CardsBase.CardTypesEnum.Pawn) {
								cardList.Add (cardOb);
							}
						} else {
							Debug.LogError ("cannot find pawn component");
						}
					}
				}
			} else {
				Debug.LogError ("cannot find hand component");
			}
			if (cardList.Count > 0) {
				GameObject[] randomCardarray = cardList.ToArray ();
				GameObject randomCardOb = randomCardarray [Random.Range (0, cardList.Count - 1)];
				Transform pawnTransform = randomCardOb.transform.Find ("Pawn");
				pawnTransform.SetParent (pawn.gamePlayComp.transform, false);
				pawnTransform.gameObject.GetComponent<KeepParentRenderLayer> ().KeepingActive = false;
				pawnTransform.GetComponent<KeepParentRenderLayer> ().KeepingActive = false;
				Pawn pawnComp = pawnTransform.gameObject.GetComponent<Pawn> ();
				pawnComp.Friendly = true;
				pawnComp.MovePawnToStartBoardPos (boardFieldId);
				pawnComp.GetComponent<PolygonCollider2D> ().enabled = true;

				Vector3 newPawnPos = new Vector3 (0, 0, 0);
				newPawnPos = pawn.gamePlayComp.Board [boardFieldId].BoardPiece.transform.localPosition;
				newPawnPos.z = Pawn.PawnPosDown;
				pawnComp.GetComponent<SmothTransform> ().SmothTransformTo (newPawnPos, 10f);

				pawn.gamePlayComp.SetCardBoardID (pawnComp);

				pawn.gamePlayComp.PutPawnOnPosision (pawnTransform.gameObject, boardFieldId, true);
				pawn.gamePlayComp.PutPawnOnPosisionImpl (pawnTransform.gameObject, boardFieldId);
				pawn.gamePlayComp.ConfirmPawnOnPosision (pawnTransform.gameObject, boardFieldId, false);

				pawnComp.boardPosisionIndex = boardFieldId;
				pawnComp.boardPosisionIndexPrev = boardFieldId;
				pawnComp.boardSavedPosisionIndexPrev = boardFieldId;
				pawnComp.isFirstPlay = false;

				//pawnComp.ResetState ();

				pawn.gamePlayComp.HandComp.RemoveCardFromHandWithDestroy (pawnComp.handIndex);
				ShowParticleEffect (boardFieldId);

				Debug.LogWarning ("light mech put card:" + pawnComp.Name);
			} else {
				Debug.LogWarning ("No pawns find in hand");
			}
		}
	}

	// Update is called once per frame
	void Awake () {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.RegisterDeathCallback(onDieCallback);
			//pawn.DeathBloked = true;

			Registered = true;
		}
	}
}
