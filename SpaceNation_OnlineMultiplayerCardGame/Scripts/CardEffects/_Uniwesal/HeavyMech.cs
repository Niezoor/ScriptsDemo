using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyMech : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;

	private int currHealth = -1;

	// Update is called once per frame
	void Update () {
		EffectUpdate (0);
	}

	private void EffectUpdate (int u) {
		if (pawn != null) {
			if (currHealth != pawn.Health) {
				pawn.SetAttack (pawn.Health);
				currHealth = pawn.Health;
			}
			if (currHealth != pawn.Attack) {
				pawn.SetHealth (pawn.Attack);
				currHealth = pawn.Attack;
			}
		}
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.RegisterOnBoardUpdateCallback (EffectUpdate);
			if (pawn.PawnEffectParameters.Length >= 1) {
				currHealth = pawn.PawnEffectParameters [0];
			} else {
				Debug.LogWarning ("Pawn effect parameter 0 is not set");
			}

			Registered = true;
		}
	}
}
