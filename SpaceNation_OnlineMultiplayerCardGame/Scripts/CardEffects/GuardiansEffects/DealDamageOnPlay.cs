using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParticlePlayground;

public class DealDamageOnPlay : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;
	public int DmgValue = 1;
	public bool tutorialMode = false;
	Pawn enemyPawnToAttack = null;

	private void ShotEvent(PlaygroundEventParticle EvParticle) {
		if (EvParticle != null) {
			Debug.Log ("Event triggered " + EvParticle);
		}
		enemyPawnToAttack.TakeDamageFromProjectile (DmgValue);
		enemyPawnToAttack.RecevingDamageDone ();
	}

	private void ShowParticleEffect(int ownerid, int boardId) {
		if (pawn.PawnEffectParticle != null) {
			enemyPawnToAttack.PrepareToReceiveDamage (0);
			Vector3 ParticlePos = pawn.gamePlayComp.Board [ownerid].BoardPiece.transform.localPosition;
			Quaternion ParticleRot = Quaternion.identity;
			ParticlePos.z = -0.5f;
			ParticleRot.eulerAngles = new Vector3 (0, 0, 0);
			GameObject ParticleOb = Instantiate (pawn.PawnEffectParticle, pawn.gamePlayComp.transform);
			GameObject Target = ParticleOb.transform.GetChild (0).gameObject;
			Target.transform.SetParent (enemyPawnToAttack.transform);
			Target.transform.localPosition = new Vector3 (0, 0, 0);
			ParticleOb.transform.localRotation = ParticleRot;
			ParticleOb.transform.localPosition = ParticlePos;
			PlaygroundParticlesC Particle = ParticleOb.GetComponent<PlaygroundParticlesC> ();
			PlaygroundC.GetEvent (0, Particle).particleEvent += ShotEvent;
			Particle.emit = true;
		} else {
			Debug.LogError ("no effect particle found");
		}
	}

	private void DealDamagImpl(int startid, int boardId) {
		Debug.Log ("ShowParticleEffect: " + startid + "end: " + boardId);
		GameObject pawnOb = pawn.gamePlayComp.Board [boardId].Pawn;
		if (pawnOb != null) {
			enemyPawnToAttack = pawnOb.GetComponent<Pawn> ();
			enemyPawnToAttack.TakeDamage (DmgValue);
			if (!pawn.gamePlayComp.skipAnimations) {
				ShowParticleEffect (startid, boardId);
			} else {
				enemyPawnToAttack.RefreshHealth (enemyPawnToAttack.Health);
			}
		} else {
			Debug.LogError ("cannot find target pawn:" + boardId);
		}
		
	}

	private void DealDamag(int boardId) {
		int handIdx = pawn.CardType == CardsBase.CardTypesEnum.Effect ? pawn.handIndex : -1;//if its a pawn effect, do not play from hand
		DealDamagImpl (pawn.boardPosisionIndex, boardId);
		pawn.gamePlayComp.PlayEffectOnBoard (pawn.Name, pawn.boardPosisionIndex, boardId, handIdx);
	}

	private void DealDamageCallback(int PawnBoardFieldId) {
		GamePlay.TargetSettings settings =
			(	GamePlay.TargetSettings.WithEnemyPawn |
				GamePlay.TargetSettings.WithEnemyHero |
			 	GamePlay.TargetSettings.WithPawnsLock |
				GamePlay.TargetSettings.MustBeCalled);
		if (tutorialMode) {
			settings =
				(	GamePlay.TargetSettings.WithEnemyPawn |
					GamePlay.TargetSettings.WithPawnsLock |
					GamePlay.TargetSettings.MustBeCalled);
		}
		if (pawn.gamePlayComp.SetTargetsOnBoard (settings, pawn.gameObject, DealDamag)) {
			pawn.gamePlayComp.ShowTargetNotification (pawn.Desc);
		}
	}

	private void TriggerEffect(int startFieldId, int endFieldId) {
		DealDamagImpl (startFieldId, endFieldId);
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.RegisterPlayConfirmCallback(DealDamageCallback);
			pawn.TriggerEffectCallback = TriggerEffect;
			if (pawn.PawnEffectParameters.Length >= 1) {
				DmgValue = pawn.PawnEffectParameters [0];
			} else {
				Debug.LogWarning ("Pawn effect parameter 0 is not set");
			}
			Registered = true;
		}
	}
}
