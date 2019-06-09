using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextBuff : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;

	private int buffAttackValue = 1;
	private int buffHealthValue = 1;
	public bool EnableEffect = false;

	private List<Pawn> RegisteredPawns = new List<Pawn> ();
	private List<Pawn> RegisteredPawnsCurrent = new List<Pawn> ();

	void Update () {
		if (EnableEffect) {
			BuffNearPawn (pawn.boardPosisionIndex);
		}
	}

	void EffectUpdate(int unused) {
		if (EnableEffect) {
			BuffNearPawn (pawn.boardPosisionIndex);
		}
	}

	private void BuffNearPawn(int CenterBoardId) {
		int nextBoardIndex = -1;

		if (CenterBoardId != -1) {
			for (int i = 0; i < 6; i++) {
				if (i == 0)
					nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUp (CenterBoardId);
				else if (i == 1)
					nextBoardIndex = pawn.gamePlayComp.GetBoardIndexDown (CenterBoardId);
				else if (i == 2)
					nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUpLeft (CenterBoardId);
				else if (i == 3)
					nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUpRight (CenterBoardId);
				else if (i == 4)
					nextBoardIndex = pawn.gamePlayComp.GetBoardIndexDownLeft (CenterBoardId);
				else
					nextBoardIndex = pawn.gamePlayComp.GetBoardIndexDownRight (CenterBoardId);

				if (nextBoardIndex != -1) {
					GameObject pawnOb = pawn.gamePlayComp.Board [nextBoardIndex].Pawn;
					if ((pawnOb != null) && (pawnOb.GetComponent<Pawn> () != pawn)) {
						if ((pawn.Friendly) && (pawnOb.GetComponent<Pawn> ().Friendly)) {
							BuffPawn (pawnOb.GetComponent<Pawn> ());
						} else if ((!pawn.Friendly) && (!pawnOb.GetComponent<Pawn> ().Friendly)) {
							BuffPawn (pawnOb.GetComponent<Pawn> ());
						}
					}
				}
			}
		}
		UnBuffPawns ();
	}

	private void UnBuffPawns() {
		List<Pawn> toRemove = new List<Pawn> ();
		foreach (Pawn curPawn in RegisteredPawns) {
			if (!RegisteredPawnsCurrent.Contains(curPawn)) {
				if (curPawn != null) {
					curPawn.BuffHealth (pawn, -buffHealthValue);
					curPawn.BuffAttack (pawn, -buffAttackValue);
				}
				toRemove.Add (curPawn);
			}
		}
		foreach (Pawn curPawn in toRemove) {
			RegisteredPawns.Remove (curPawn);
		}
		toRemove.Clear ();
		RegisteredPawnsCurrent.Clear ();
	}

	private void BuffPawn(Pawn pawnToBuff) {
		RegisteredPawnsCurrent.Add (pawnToBuff);
		if (!RegisteredPawns.Contains(pawnToBuff)) {
			pawnToBuff.BuffHealth (pawn, buffHealthValue);
			pawnToBuff.BuffAttack (pawn, buffAttackValue);
			RegisteredPawns.Add (pawnToBuff);
		}
	}

	private void UnBuffAllPawns(int pawnId) {
		EnableEffect = false;
		foreach (Pawn curPawn in RegisteredPawns) {
			if (curPawn != null) {
				curPawn.BuffHealth (pawn, -buffHealthValue);
				curPawn.BuffAttack (pawn, -buffAttackValue);
			}
		}
		RegisteredPawns.Clear ();
		RegisteredPawnsCurrent.Clear ();
	}

	private void PlayConfirmCallback (int PawnBoardId) {
		EnableEffect = true;
		//pawn.gamePlayComp.PlayEffectOnBoard (pawn.Name, pawn.boardPosisionIndex, -1, -1);
	}

	private void TriggerEffect(int pawnBoardFieldId, int unused) {
		if (pawn.gamePlayComp.Board [pawnBoardFieldId].Pawn != null) {
			pawn.gamePlayComp.Board [pawnBoardFieldId].Pawn.GetComponent<NextBuff> ().EnableEffect = true;
		} else {
			Debug.LogError ("Cannot find pawn to trigger effect on position:" + pawnBoardFieldId);
		}
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.RegisterLocalPlayConfirmCallback (PlayConfirmCallback);
			pawn.RegisterDeathCallback (UnBuffAllPawns);
			pawn.RegisterOnBoardUpdateCallback (EffectUpdate);
			pawn.TriggerEffectCallback = TriggerEffect;
			if (pawn.PawnEffectParameters.Length >= 1) {
				buffAttackValue = pawn.PawnEffectParameters [0];
			} else {
				Debug.LogWarning ("Pawn effect parameter 0 is not set");
			}
			if (pawn.PawnEffectParameters.Length >= 2) {
				buffHealthValue = pawn.PawnEffectParameters [1];
			} else {
				Debug.LogWarning ("Pawn effect parameter 1 is not set");
			}
			Registered = true;
		}
	}
}
