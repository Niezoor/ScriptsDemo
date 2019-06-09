using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffHeroAttack : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;
	public int buffAttackValue = 1;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
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

	public void HeroAttackBuff() {
		GameObject gameplay = GameObject.Find("Gameplay");
		if (gameplay) {
			Pawn hero = null;
			if (pawn.Friendly) {
				hero = gameplay.GetComponent<GamePlay>().myHero.GetComponent<Pawn> ();
			} else {
				hero = gameplay.GetComponent<GamePlay>().enemyHero.GetComponent<Pawn> ();
			}
			hero.BuffAttack (pawn, buffAttackValue);
			if (hero.boardPosisionIndex >= 0) {
				ShowParticleEffect (hero.boardPosisionIndex);
			}
		}
	}

	private void onPlayCallb(int boardID) {
		HeroAttackBuff ();
		int handIdx = pawn.CardType == CardsBase.CardTypesEnum.Effect ? pawn.handIndex : -1;//if its a pawn effect, do not play from hand
		pawn.gamePlayComp.PlayEffectOnBoard (pawn.Name, pawn.boardPosisionIndex, boardID, handIdx);
	}

	private void TrigerEfectCB(int startID, int endID) {
		//HeroAttackBuff ();//localy
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.RegisterLocalPlayConfirmCallback(onPlayCallb);
			pawn.TriggerEffectCallback = TrigerEfectCB;
			if (pawn.PawnEffectParameters.Length >= 1) {
				buffAttackValue = pawn.PawnEffectParameters [0];
			} else {
				Debug.LogWarning ("Pawn effect parameter 0 is not set");
			}
			Registered = true;
		}
	}
}
