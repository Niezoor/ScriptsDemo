using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealFriendlyFire : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.FriendlyFireEnabled = true;
			Registered = true;
		}
	}
}
