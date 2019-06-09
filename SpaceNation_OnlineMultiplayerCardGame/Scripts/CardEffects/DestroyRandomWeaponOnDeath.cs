using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyRandomWeaponOnDeath : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;

	private void DestroyRandomEnemyWeapon(int pawnBoardId) {
		Debug.Log (" Destroy enemy weapon");
		for (int i = 0; i < GamePlay.IndexMAX; i++) {
			GameObject pawnOb = pawn.gamePlayComp.Board [i].Pawn;
			if (pawnOb != null) {
				Pawn pawntoCheck = pawnOb.GetComponent<Pawn> ();
				if (pawn.WeaponName.Length > 0) {
					Debug.Log (" find enemy weapon :" + pawn.boardPosisionIndex);
					if (pawn.Friendly == true) {
						if (pawntoCheck.Friendly == false) {
							pawntoCheck.DetachWeapon ();
						}
					} else {
						if (pawntoCheck.Friendly == true) {
							pawntoCheck.DetachWeapon ();
						}
					}
				}
			}
		}
	}

	// Update is called once per frame
	void Awake () {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.RegisterDeathCallback(DestroyRandomEnemyWeapon);

			Registered = true;
		}
	}
}
