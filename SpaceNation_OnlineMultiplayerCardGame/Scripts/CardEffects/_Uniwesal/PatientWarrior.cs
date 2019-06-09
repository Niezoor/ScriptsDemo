using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatientWarrior : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;

	private bool FirstGetCost = true;
	public int ThisCardCost = 0;
	public int DeathPawns = 0;
	public CardInteraction CardInterComp = null;

	private void OnSomeDiedCallback (int deathPawnBoardId) {
		if (pawn != null) {
			if (pawn.Friendly) {
				GameObject pawnOb = pawn.gamePlayComp.Board [deathPawnBoardId].Pawn;
				if (pawnOb != null) {
					if (pawnOb.GetComponent<Pawn> ().Friendly) {
						DeathPawns++;
						if (CardInterComp != null) {
							CardInterComp.SetCardCost ((ThisCardCost - DeathPawns >= 0) ? ThisCardCost - DeathPawns : 0);
						}
					}
				}
			}
		}
	}

	private void NewTurnCallback (int idx) {
		if (FirstGetCost) {
			if (pawn != null) {
				if (pawn.transform.parent != null) {
					CardInterComp = pawn.transform.parent.gameObject.GetComponent<CardInteraction> ();
					if (CardInterComp != null) {
						ThisCardCost = CardInterComp.CardCost;
					}
					FirstGetCost = false;
				}
			}
		} else {
			if (CardInterComp != null) {
				CardInterComp.SetCardCost (ThisCardCost);
			}
		}
		DeathPawns = 0;
	}

	void Start () {
		if (GameObject.Find ("Gameplay") != null) {
			GamePlay gamePlayComp = GameObject.Find ("Gameplay").GetComponent<GamePlay> ();
			if (gamePlayComp) {
				gamePlayComp.myHero.GetComponent<Pawn> ().RegisterOnSomeOneDiedCallback (OnSomeDiedCallback);
				gamePlayComp.myHero.GetComponent<Pawn> ().RegisterOnNewTurnCallback (NewTurnCallback);
			}
		}
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			//little hacky way, lets register to our hero becouse he must be alive and be on board

			Registered = true;
		}
	}
}
