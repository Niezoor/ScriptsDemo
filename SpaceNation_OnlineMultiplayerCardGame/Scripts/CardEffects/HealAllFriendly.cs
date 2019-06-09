using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealAllFriendly : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;
	private int lastBoardIndex;
	private int HealValue;
	//local mode: -
	//online mode: -

	private void HealAllFriendlyPawns(bool friendly) {
		for (int i = 0; i < GamePlay.IndexMAX; i++) {
			GameObject pawnOb = pawn.gamePlayComp.Board [i].Pawn;
			if (pawnOb != null) {
				Pawn TargetPawn = pawnOb.GetComponent<Pawn> ();
				if (friendly) {
					if (TargetPawn.Friendly) {
						TargetPawn.Heal (pawn.pawnBoardID, HealValue);
					}
				} else {
					if (!TargetPawn.Friendly) {
						TargetPawn.Heal (pawn.pawnBoardID, HealValue);
					}
				}
			}
		}
	}

	private int HealAllPawnsPlay(int boardFieldId) {
		int rv = pawn.CardType == CardsBase.CardTypesEnum.Effect ? 1 : 0;
		return rv;
	}

	private void HealAllPawnsConfirm(int boardFieldId) {
		int handIdx = pawn.CardType == CardsBase.CardTypesEnum.Effect ? pawn.handIndex : -1;
		HealAllFriendlyPawns (pawn.Friendly);
		pawn.gamePlayComp.PlayEffectOnBoard (pawn.Name, -1/*unfriendly for enemy*/, 0, handIdx);
	}

	private void TriggerEffect(int friendly, int unused) {
		HealAllFriendlyPawns (pawn.Friendly);
	}

	private List<int> AIMove(int boardId) {
		List<int> rv = new List<int> ();
		rv.Add (-1);
		return rv;
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.AITriggerEffectCallback = AIMove;
			pawn.OnPlayCallback = HealAllPawnsPlay;
			pawn.RegisterPlayConfirmCallback(HealAllPawnsConfirm);
			pawn.TriggerEffectCallback = TriggerEffect;

			if (pawn.PawnEffectParameters.Length >= 1) {
				HealValue = pawn.PawnEffectParameters [0];
			} else {
				Debug.LogWarning ("Pawn effect parameter 0 is not set");
			}
			Registered = true;
		}
	}
}
