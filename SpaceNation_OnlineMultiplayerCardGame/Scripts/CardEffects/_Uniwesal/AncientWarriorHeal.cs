using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AncientWarriorHeal : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;

	public int HealValue = 1;
	public Pawn TargetPawn = null;

	private GameObject ParticleObject;
	//local mode: ok
	//online mode: unsupported

	public void ShowParticleEffect() {
		if (pawn.PawnEffectParticle != null) {
			if (TargetPawn != null) {
				if (ParticleObject == null) {
					ParticleObject = (GameObject)Instantiate (pawn.PawnEffectParticle);
					ParticleObject.transform.SetParent (pawn.transform, true);
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
		Debug.Log ("AncientWarriorHeal on NewTurnCallback " + boardId);
		if (TargetPawn != null) {
			TargetPawn.Heal (pawn.pawnBoardID, HealValue);
		}
	}

	private void SetTarget(int boardId) {
		Debug.Log ("SuperVisor on SetTarget " + boardId);
		if (boardId >= 0) {
			GameObject pawnGOb = pawn.gamePlayComp.Board [boardId].Pawn;
			if (pawnGOb != null) {
				TargetPawn = pawnGOb.GetComponent<Pawn> ();
				ShowParticleEffect ();
			}
		}
	}

	private void TargetPawnCallback(int boardId) {
		Debug.Log ("AncientWarriorHeal on TargetPawnCallback " + boardId);
		SetTarget (boardId);
		pawn.gamePlayComp.PlayEffectOnBoard (pawn.Name, pawn.boardPosisionIndex, boardId, -1);
		pawn.gamePlayComp.HideTargetNotification ();
		pawn.SetSelect (false);
	}

	private void ChoosePawnToBuff(int PawnBoardFieldId) {
		Debug.Log ("AncientWarriorHeal on ChoosePawnToBuff " + PawnBoardFieldId);
		GamePlay.TargetSettings settings =
			(GamePlay.TargetSettings.WithFriendlyPawn |
				GamePlay.TargetSettings.WithPawnsLock |
				GamePlay.TargetSettings.MustBeCalled);
		if (pawn.gamePlayComp.SetTargetsOnBoard (settings, pawn.gameObject, TargetPawnCallback)) {
			pawn.gamePlayComp.ShowTargetNotification (pawn.Desc);
		}
	}

	private void UnselectCallback(int boardId) {
		Debug.Log ("AncientWarriorHeal on UnselectCallback " + boardId);
		pawn.gamePlayComp.HideTargetNotification ();
	}

	private void AttackRulesOverride(int boardId) {
		Debug.Log ("AncientWarriorHeal on AttackRulesOverride " + boardId);
		if (TargetPawn == null || TargetPawn.Death) {
			GamePlay.TargetSettings settings = GamePlay.TargetSettings.WithFriendlyPawn;
			if (pawn.gamePlayComp.SetTargetsOnBoard (settings, pawn.gameObject, TargetPawnCallback)) {
				pawn.gamePlayComp.ShowTargetNotification (pawn.Desc);
			}
		}
	}

	private void TriggerSupervisorHeal(int startBoardPosId, int pawnToBuffBoardId) {
		Debug.Log ("AncientWarriorHeal on TriggerSupervisorHeal " + pawnToBuffBoardId);
		GameObject pawnGOb = pawn.gamePlayComp.Board [startBoardPosId].Pawn;
		if (pawnGOb != null) {
			AncientWarriorHeal Apawn = pawnGOb.GetComponent<AncientWarriorHeal> ();
			if (Apawn != null) {
				if (pawnToBuffBoardId >= 0) {
					GameObject pawnGObTarget = pawn.gamePlayComp.Board [pawnToBuffBoardId].Pawn;
					if (pawnGObTarget != null) {
						Apawn.TargetPawn = pawnGObTarget.GetComponent<Pawn> ();
						Apawn.ShowParticleEffect ();
					}
				}
			}
		}
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.RegisterPlayConfirmCallback(ChoosePawnToBuff);
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
