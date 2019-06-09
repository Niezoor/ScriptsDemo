using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlankerAttack : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;

	private void TargetPawnCallback(int targerBoardId) {
		pawn.PawnAttackTargetCallback (targerBoardId);
	}

	private void AttackRulesOverride (int KilledPawnBoardId) {
		if ((!pawn.isFirstPlay) &&
			(!pawn.AttackAlready))
		{
			GamePlay.TargetSettings settings =
				(GamePlay.TargetSettings.WithEnemyHero |
				GamePlay.TargetSettings.WithEnemyPawn);
			if (pawn.gamePlayComp.SetTargetsOnBoard (settings, pawn.gameObject, TargetPawnCallback)) {
				pawn.gamePlayComp.ShowTargetNotification (pawn.Desc);
			}
		}
	}

	private void UnselectCallback(int x) {
		pawn.gamePlayComp.HideTargetNotification ();
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.IgnorsShields = true;
			pawn.AttackRulesOverrideCallback = AttackRulesOverride;
			pawn.OnDeselectCallback = UnselectCallback;
			Registered = true;
		}
	}
}
