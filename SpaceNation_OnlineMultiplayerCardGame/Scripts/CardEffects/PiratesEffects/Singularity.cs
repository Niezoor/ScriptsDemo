using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singularity : MonoBehaviour {
	private Pawn pawn;
	private GameObject attackMark;
	private int lastBoardIndex;
	public bool Registered = false;
	public int Damage;
	//local mode: ok
	//online mode: ok

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

	void OnDestroy() {
		Debug.Log ("black hole Destroyed!!! ");
	}

	public void MovePawn(Pawn pawnToMove, int boardPosIndex) {
		Debug.Log ("Move pawn to black hole on board id " + boardPosIndex);
		Vector3 newPawnPos = new Vector3 (0, 0, 0);
		pawn.gamePlayComp.RemovePawnFromPosisionImpl (pawnToMove.boardPosisionIndex);
		pawn.gamePlayComp.PutPawnOnPosisionImpl (pawnToMove.gameObject, boardPosIndex);
		//pawn.gamePlayComp.ConfirmPawnOnPosision (pawnToMove.gameObject, boardPosIndex, false);
		pawnToMove.boardPosisionIndex = boardPosIndex;
		pawnToMove.boardPosisionIndexPrev = boardPosIndex;
		pawnToMove.boardSavedPosisionIndexPrev = boardPosIndex;
		newPawnPos = pawn.gamePlayComp.Board [boardPosIndex].BoardPiece.transform.localPosition;
		newPawnPos.z = Pawn.PawnPosDown;
		pawnToMove.gameObject.GetComponent<SmothTransform>().SmothTransformTo (newPawnPos, 10f);
	}

	public void DoBlackHoleSingularity(int friendly, int boardFieldId) {
		int index = boardFieldId;
		int startIndex = boardFieldId;
		int prevIdx = startIndex;
		int directionIdx = 0;

		Debug.Log ("Black hole singularity on board id " + boardFieldId);

		ShowParticleEffect (boardFieldId);

		do {
			bool goToNextDir = false;
			int newIndex;

			Debug.Log (" Blackhole looking for pawns " + boardFieldId);

			if (directionIdx == 0) {
				newIndex = pawn.gamePlayComp.GetBoardIndexUp (index);
			} else if (directionIdx == 1) {
				newIndex = pawn.gamePlayComp.GetBoardIndexDown (index);
			} else if (directionIdx == 2) {
				newIndex = pawn.gamePlayComp.GetBoardIndexUpLeft (index);
			} else if (directionIdx == 3) {
				newIndex = pawn.gamePlayComp.GetBoardIndexUpRight (index);
			} else if (directionIdx == 4) {
				newIndex = pawn.gamePlayComp.GetBoardIndexDownLeft (index);
			} else if (directionIdx == 5) {
				newIndex = pawn.gamePlayComp.GetBoardIndexDownRight (index);
			} else {
				Debug.Log (" Blackhole exit loop" + prevIdx);
				break;
			}

			Debug.Log (" Blackhole looking for pawns find in  " + newIndex);

			if (newIndex != -1) {
				GameObject pawnGOb = pawn.gamePlayComp.Board [newIndex].Pawn;
				if ((pawnGOb != null) &&
					(pawnGOb != pawn.gamePlayComp.myHero) &&
					(pawnGOb != pawn.gamePlayComp.enemyHero)) {
					Debug.Log (" Blackhole looking for pawns found someone  " + newIndex);
					Pawn targetPawn = pawnGOb.GetComponent<Pawn> ();
					Debug.Log (" Blackhole looking for pawns found someone target name " + targetPawn.Name);
					if ((pawn.Friendly && !targetPawn.Friendly) ||
						(!pawn.Friendly && targetPawn.Friendly))
					{
						int localIdx = newIndex;
						int localNextIdx = 0;
						prevIdx = newIndex;

						Debug.Log (" Blackhole looking for pawns found process pawn " + targetPawn.Name);

						while (true) {
							if (directionIdx == 0) {
								localNextIdx = pawn.gamePlayComp.GetBoardIndexDown (localIdx);
							} else if (directionIdx == 1) {
								localNextIdx = pawn.gamePlayComp.GetBoardIndexUp (localIdx);
							} else if (directionIdx == 2) {
								localNextIdx = pawn.gamePlayComp.GetBoardIndexDownRight (localIdx);
							} else if (directionIdx == 3) {
								localNextIdx = pawn.gamePlayComp.GetBoardIndexDownLeft (localIdx);
							} else if (directionIdx == 4) {
								localNextIdx = pawn.gamePlayComp.GetBoardIndexUpRight (localIdx);
							} else if (directionIdx == 5) {
								localNextIdx = pawn.gamePlayComp.GetBoardIndexUpLeft (localIdx);
							} else {
								break;
							}
							if (pawn.gamePlayComp.IsFreePosision(localNextIdx)) {
								prevIdx = localNextIdx;
							} else {
								Debug.Log (" Blackhole looking for pawns found break 1" + localNextIdx);
								break;
							}
							if (localNextIdx == startIndex) {
								Debug.Log (" Blackhole looking for pawns found break 2" + localNextIdx);
								break;
							}
							localIdx = localNextIdx;
						}
						if (pawn.gamePlayComp.IsFreePosision(prevIdx)) {
							Debug.Log (" call : Move pawn to black hole on board id " + prevIdx);
							MovePawn(targetPawn, prevIdx);
						} else {
							Debug.Log (" Blackhole looking for pawns found cannot move to" + prevIdx);
						}
					} else {
						goToNextDir = true;
					}
				}
			} else {
				goToNextDir = true;
			}

			if (goToNextDir) {
				directionIdx++;
				index = startIndex;
				prevIdx = index;
			} else {
				prevIdx = index;
				index = newIndex;
			}
		} while (true);
	}

	public void DoBlackHoleSingularityV2(int friendly, int boardFieldId) {
		List <int> PosList = pawn.gamePlayComp.GetBoardIndexesAround (boardFieldId, GamePlay.BoardRangeMax);
		List <int> FreePosList = new List<int> ();
		List <Pawn> PawnsList = new List<Pawn> ();
		Debug.Log ("Black hole singularity on board id " + boardFieldId);
		ShowParticleEffect (boardFieldId);

		if (PosList.Count > 0) {
			foreach (int idxPos in PosList) {
				if (pawn.gamePlayComp.IsFreePosision (idxPos)) {
					Debug.Log (" free pos added on " + idxPos);
					FreePosList.Add (idxPos);
				} else {
					GameObject pawnGOb = pawn.gamePlayComp.Board [idxPos].Pawn;
					if ((pawnGOb != null) &&
					   (pawnGOb != pawn.gamePlayComp.myHero) &&
					   (pawnGOb != pawn.gamePlayComp.enemyHero)) {
						Pawn nextPawn = pawnGOb.GetComponent<Pawn> ();
						Debug.Log (" pawn detected on " + idxPos);
						if ((pawn.Friendly && !nextPawn.Friendly) ||
						   (!pawn.Friendly && nextPawn.Friendly)) {
							Debug.Log (" pawn added on " + idxPos);
							PawnsList.Add (nextPawn);
						}
					}
				}
			}
			if (PawnsList.Count > 0) {
				int count = 0;
				foreach (int posToMove in FreePosList) {
					Debug.Log (" call : Move pawn to black hole on board id " + posToMove);
					Pawn targetPawn = PawnsList[count];
					count++;
					MovePawn(targetPawn, posToMove);
					pawn.gamePlayComp.DoDamageOnBoard (pawn, posToMove, Damage);
					if (count >= PawnsList.Count) {
						break;
					}
				}
			} else {
				Debug.LogWarning ("No pawns for action");
			}
		} else {
			Debug.LogWarning ("No free space on board");
		}
	}

	private int OnEffectPlay(int boardFieldId) {
		int returnValue = -1;
		int startPosId;
		int pawnPosId = pawn.gamePlayComp.GetClosestOverMouseAnyIndexPosition (pawn, false, false, false, false);

		//Debug.Log ("deal damage board id " + pawnPosId);
		if (pawnPosId >= 0) {
			if (pawn.CardType == CardsBase.CardTypesEnum.Effect) {
				startPosId = pawn.gamePlayComp.myHero.GetComponent<Pawn> ().boardPosisionIndex;
			} else {
				startPosId = pawn.boardPosisionIndex;
			}
			//pawn.gamePlayComp.ShowMarkForEnemy (pawnPosId, pawn.handIndex);
			pawn.gamePlayComp.CreateAttackBeam (startPosId, pawnPosId);
			if (attackMark == null) {
				attackMark = pawn.gamePlayComp.CreateAttackMark (pawnPosId, 0);
				lastBoardIndex = pawnPosId;
			}
			if (lastBoardIndex != pawnPosId) {
				Destroy (attackMark);
			}
			returnValue = 0;
		}
		return returnValue;
	}

	private void OnEffectConfirm(int boardFieldId) {
		int handIdx;
		if (pawn.CardType == CardsBase.CardTypesEnum.Effect) {
			handIdx = pawn.handIndex;
		} else {
			handIdx = -1;
		}
		DoBlackHoleSingularityV2 (0, lastBoardIndex);
		pawn.gamePlayComp.PlayEffectOnBoard (pawn.Name, -1, lastBoardIndex, handIdx);
		pawn.gamePlayComp.DestroyMarkBeam ();
		Destroy (attackMark);
	}

	private void OnDealDamageCancel(int boardFieldId) {
		pawn.gamePlayComp.DestroyMarkBeam ();
		Destroy (attackMark);
	}

	private void TriggerEffect(int startFieldId, int endFieldId) {
		pawn.DetachPawn ();
		DoBlackHoleSingularityV2(startFieldId, endFieldId);
		Debug.Log ("triggerr done " + endFieldId);
		Destroy (pawn.gameObject);
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.OnPlayCallback = OnEffectPlay;
			pawn.RegisterPlayConfirmCallback(OnEffectConfirm);
			pawn.OnPlayCancelCallback = OnDealDamageCancel;
			pawn.TriggerEffectCallback = TriggerEffect;

			if (pawn.PawnEffectParameters.Length >= 1) {
				Damage = pawn.PawnEffectParameters [0];
			} else {
				Debug.LogWarning ("Pawn effect parameter 0 is not set");
			}

			Registered = true;
		}
	}
}
