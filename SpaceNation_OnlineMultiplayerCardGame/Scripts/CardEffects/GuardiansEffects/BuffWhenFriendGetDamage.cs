using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffWhenFriendGetDamage : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;
	public int buffAttackValue = 1;
	public int buffHealthValue = 1;
	private List<GameObject> RegisteredPawns = new List<GameObject> ();

	private bool Enabled = false;

	// Use this for initialization
	void Start () {

	}

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

	private void buffThisPawn(int boardId) {
		if (pawn) {
			ShowParticleEffect (boardId);
			if (buffAttackValue > 0)
				pawn.BuffAttack (pawn, buffAttackValue);
			if (buffHealthValue > 0)
				pawn.BuffHealth (pawn, buffHealthValue);
		}
	}

	// Update is called once per frame
	void Update () {
		//EffectUpdate (0);
	}

	private void EffectUpdate (int u) {
		/*bool Enabled = false;
		for (int i = 0; i < GamePlay.IndexMAX; i++) {
			if ((pawn != null) &&
				(pawn.gamePlayComp != null) &&
				(pawn.gamePlayComp.Board [i].Pawn != null) &&
				(pawn.gamePlayComp.Board [i].Pawn == pawn.gameObject))
			{
				Enabled = true;//is on board
				break;
			}
		}*/
		if (Enabled) {
			for (int i = 0; i < GamePlay.IndexMAX; i++) {
				GameObject pawnOb = pawn.gamePlayComp.Board [i].Pawn;
				if (pawnOb != null) {
					Pawn pawnComp = pawnOb.GetComponent<Pawn> ();
					if ((pawn.Friendly && pawnComp.Friendly) ||
					    (!pawn.Friendly && !pawnComp.Friendly)) {
						if (!RegisteredPawns.Contains (pawnOb) &&
						    (pawnOb != pawn.gameObject)) {
							RegisteredPawns.Add (pawnOb);
							pawnComp.RegisterOnGetDamageCallback (buffThisPawn);
						}
					}
				}
			}
		}
	}

	private void EnableEffect(int u) {
		Enabled = true;
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.RegisterOnBoardUpdateCallback (EffectUpdate);
			pawn.RegisterLocalPlayConfirmCallback (EnableEffect);
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
