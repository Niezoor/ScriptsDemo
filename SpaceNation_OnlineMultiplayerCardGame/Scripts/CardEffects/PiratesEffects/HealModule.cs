using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealModule : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;
	private int healValue = 1;
	private bool ModuleEnabled = false;
	private Pawn HealedPawn = null;

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
	}

	private void ModuleHeal(int pawnBoardId) {
		if ((ModuleEnabled) && (HealedPawn != null)) {
			HealedPawn.Heal (pawn.pawnBoardID, healValue);
		}
	}

	private void ModuleDetach(int ownerBoardId) {
		ModuleEnabled = false;
	}

	private void ModuleAplied(int ownerBoardId) {
		ModuleEnabled = true;
		GameObject pawnOb = pawn.gamePlayComp.Board [ownerBoardId].Pawn;
		if (pawnOb != null) {
			Pawn pawnComp = pawnOb.GetComponent<Pawn> ();
			pawnComp.OnDetachItemCallback = ModuleDetach;
			pawnComp.RegisterOnAttackCallback (ModuleHeal);
			HealedPawn = pawnComp;
		}
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.OnApplyItemCallback = ModuleAplied;
			if (pawn.PawnEffectParameters.Length >= 1) {
				healValue = pawn.PawnEffectParameters [0];
			} else {
				Debug.LogWarning ("Pawn effect parameter 0 is not set");
			}
			Registered = true;
		}
	}
}
