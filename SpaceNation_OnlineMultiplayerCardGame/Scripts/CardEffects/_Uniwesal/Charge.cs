using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Charge : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;

	// Use this for initialization
	void Start () {

	}

	private void playCallback(int targetBoardId) {
		pawn.Charge = true;
		pawn.SetAttackTarget ();
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.RegisterPlayConfirmCallback (playCallback);
			pawn.Charge = true;

			Registered = true;
		}
	}
}
