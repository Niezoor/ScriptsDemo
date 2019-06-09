using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageHeroOnGetDamage : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;

	private int DamageValue = 1;

	private void DamageEnemyHero(int pawnBoardId) {
		if (pawn.Friendly == true) {
			pawn.gamePlayComp.enemyHero.GetComponent<Pawn> ().TakeDamage (DamageValue);
		} else {
			pawn.gamePlayComp.myHero.GetComponent<Pawn> ().TakeDamage (DamageValue);
		}
	}

	// Update is called once per frame
	void Awake () {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.RegisterOnGetDamageCallback(DamageEnemyHero);

			if (pawn.PawnEffectParameters.Length >= 1) {
				DamageValue = pawn.PawnEffectParameters [0];
			} else {
				Debug.LogWarning ("Pawn effect parameter 0 is not set");
			}
			Registered = true;
		}
	}
}
