using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupervisorHealer : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;

	private int HealValue = 1;
	public Pawn TargetPawn = null;
	private bool TargetBuffed = false;
	private GameObject ParticleObject;
	//local mode: ok
	//online mode: unsupported

	public void ShowParticleEffect() {
		if (pawn.PawnEffectParticle != null) {
			if (TargetPawn != null) {
				if (ParticleObject == null) {
					ParticleObject = (GameObject)Instantiate (pawn.PawnEffectParticle);
					ParticleObject.transform.SetParent (pawn.transform, false);
					ParticleObject.transform.localPosition = new Vector3 (0, 0, -0.25f);
					ParticleObject.transform.localRotation = Quaternion.identity;
				}
				Transform EndPoint = ParticleObject.GetComponent<ArcRenderer> ().EndPoint;
				EndPoint.SetParent (TargetPawn.transform, false);
				EndPoint.localPosition = new Vector3 (0, 0, -0.25f);
				EndPoint.localRotation = Quaternion.identity;
			}
		}
	}

	private void NewTurnCallback(int boardId) {
		Debug.Log ("SuperVisor on NewTurnCallback " + boardId);
		if (TargetPawn != null) {
			TargetPawn.Heal (pawn.pawnBoardID, HealValue);
			TargetPawn.SpecialMovement = true;
			TargetPawn.ApplyConfig ();
			TargetBuffed = true;
		}
	}

	private void SetTarget(int superVisorID, int boardId) {
		Debug.Log ("SuperVisor on SetTarget " + boardId);
		if (boardId >= 0) {
			if (TargetBuffed) {
				if (TargetPawn != null) {
					TargetPawn.RestoreOriginalPawnMovement ();
				}
			}
			if (boardId >= 0) {
				GameObject pawnGOb = pawn.gamePlayComp.Board [boardId].Pawn;
				if (pawnGOb != null) {
					TargetPawn = pawnGOb.GetComponent<Pawn> ();
					TargetPawn.SpecialMovement = true;
					TargetPawn.ApplyConfig ();
					ShowParticleEffect ();
				}
			}
		}
	}

	private void TargetPawnCallback(int boardId) {
		Debug.Log ("SuperVisor on TargetPawnCallback " + boardId);
		SetTarget (pawn.boardPosisionIndex, boardId);
		pawn.gamePlayComp.PlayEffectOnBoard (pawn.Name, pawn.boardPosisionIndex, boardId, -1);
		pawn.gamePlayComp.HideTargetNotification ();
		pawn.pawnState = Pawn.pawnStates.onboard_idle;
		pawn.SetSelect (false);
	}

	private void ChoosePawnToBuff(int PawnBoardFieldId) {
		Debug.Log ("SuperVisor on ChoosePawnToBuff " + PawnBoardFieldId);
		GamePlay.TargetSettings settings =
			(GamePlay.TargetSettings.WithFriendlyPawn |
				GamePlay.TargetSettings.WithPawnsLock |
				GamePlay.TargetSettings.MustBeCalled);
		if (pawn.gamePlayComp.SetTargetsOnBoard (settings, pawn.gameObject, TargetPawnCallback)) {
			pawn.gamePlayComp.ShowTargetNotification (pawn.Desc);
		} else {
			pawn.pawnState = Pawn.pawnStates.onboard_idle;
		}
	}

	private void UnselectCallback(int boardId) {
		Debug.Log ("SuperVisor on UnselectCallback " + boardId);
		pawn.gamePlayComp.HideTargetNotification ();
	}

	private void AttackRulesOverride(int boardId) {
		Debug.Log ("SuperVisor on AttackRulesOverride " + boardId);
		if (TargetPawn == null || TargetPawn.Death) {
			GamePlay.TargetSettings settings = GamePlay.TargetSettings.WithFriendlyPawn;
			if (pawn.gamePlayComp.SetTargetsOnBoard (settings, pawn.gameObject, TargetPawnCallback)) {
				pawn.gamePlayComp.ShowTargetNotification (pawn.Desc);
			}
		}
	}

	private void TriggerSupervisorHeal(int startBoardPosId, int pawnToBuffBoardId) {
		Debug.Log ("SuperVisor on TriggerSupervisorHeal " + pawnToBuffBoardId);
		GameObject pawnGOb = pawn.gamePlayComp.Board [startBoardPosId].Pawn;
		if (pawnGOb != null) {
			SupervisorHealer SVpawn = pawnGOb.GetComponent<SupervisorHealer> ();
			if (SVpawn) {
				if (pawnToBuffBoardId >= 0) {
					if (SVpawn.TargetBuffed) {
						if (SVpawn.TargetPawn != null) {
							SVpawn.TargetPawn.RestoreOriginalPawnMovement ();
						}
					}
					if (pawnToBuffBoardId >= 0) {
						GameObject pawnGOb1 = pawn.gamePlayComp.Board [pawnToBuffBoardId].Pawn;
						if (pawnGOb1 != null) {
							SVpawn.TargetPawn = pawnGOb1.GetComponent<Pawn> ();
							SVpawn.TargetPawn.SpecialMovement = true;
							SVpawn.TargetPawn.ApplyConfig ();
							SVpawn.ShowParticleEffect ();
						}
					}
				}
			}
		}
	}

	private List<int> AIMove(int boardId) {
		GamePlay.TargetSettings settings = GamePlay.TargetSettings.WithEnemyPawn;
		if (pawn.gamePlayComp.SetTargetsOnBoard (settings, pawn.gameObject, null)) {
			return pawn.gamePlayComp.TargetsList;
		}
		return null;
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.RegisterPlayConfirmCallback(ChoosePawnToBuff);
			pawn.AITriggerEffectCallback = AIMove;
			pawn.TriggerEffectCallback = TriggerSupervisorHeal;
			pawn.AttackRulesOverrideCallback = AttackRulesOverride;
			pawn.RegisterOnNewTurnCallback (NewTurnCallback);
			pawn.OnDeselectCallback = UnselectCallback;
			if (pawn.PawnEffectParameters.Length >= 1) {
				HealValue = pawn.PawnEffectParameters [0];
			} else {
				Debug.LogWarning ("Pawn effect parameter 0 is not set");
			}
			Registered = true;
		}
	}
}
