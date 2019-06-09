using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannotMoveOnLowHealth : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;

	private int HealthLeftTrigger = 2;

	void Update () {
		if ((pawn != null) && (pawn.Health <= HealthLeftTrigger)) {
			pawn.SetAttackOnlyMode ();
		}
	}

	private void EffectUpdate (int u) {
		if ((pawn != null) && (pawn.Health <= HealthLeftTrigger)) {
			pawn.SetAttackOnlyMode ();
		}
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.RegisterOnBoardUpdateCallback (EffectUpdate);
			if (pawn.PawnEffectParameters.Length >= 1) {
				HealthLeftTrigger = pawn.PawnEffectParameters [0];
			} else {
				Debug.LogWarning ("Pawn effect parameter 0 is not set");
			}
			Registered = true;
		}
	}
}
