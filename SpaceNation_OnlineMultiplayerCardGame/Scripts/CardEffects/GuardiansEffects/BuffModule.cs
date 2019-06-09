using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffModule : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;
	public int buffAttackValue = 1;
	public int buffHealthValue = 1;

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
	}

	private void ModuleBuff(int pawnBoardId, int attackBuff, int healthBuff) {
		GameObject pawnOb = pawn.gamePlayComp.Board [pawnBoardId].Pawn;
		if (pawnOb != null) {
			Pawn pawnComp = pawnOb.GetComponent<Pawn> ();
			pawnComp.OnDetachItemCallback = ModuleDetach;
			if (buffAttackValue > 0)
				pawnComp.BuffAttack (pawn, buffAttackValue);
			if (buffHealthValue > 0)
				pawnComp.BuffHealth (pawn, buffHealthValue);
		}
	}

	private void ModuleDetach(int ownerBoardId) {
		ModuleBuff (ownerBoardId, -buffAttackValue, -buffHealthValue);
	}

	private void ModuleAplied(int ownerBoardId) {
		ModuleBuff (ownerBoardId, buffAttackValue, buffHealthValue);
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.OnApplyItemCallback = ModuleAplied;
			if (pawn.PawnEffectParameters.Length >= 1) {
				buffAttackValue = pawn.PawnEffectParameters [0];
			} else {
				Debug.LogWarning ("Pawn effect parameter 0 is not set");
			}
			if (pawn.PawnEffectParameters.Length >= 2) {
				buffHealthValue = pawn.PawnEffectParameters [1];
			} else {
				Debug.LogWarning ("Pawn effect parameter 0 is not set");
			}
			Registered = true;
		}
	}
}
