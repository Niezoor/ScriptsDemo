using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Granade : MonoBehaviour {
	private int hit_Damage = 4;
	private int next_Damage = 3;
	private Pawn pawn;
	private GameObject attackMark;
	private List<GameObject> attackMarks = new List<GameObject> ();
	private int lastBoardIndex;
	public bool Registered = false;
	//local mode: ok
	//online mode: ok

	private IEnumerator ShowParticleEffect(int boardFieldId) {
		if (pawn.PawnEffectParticle) {
			GameObject particleObject = (GameObject)Instantiate (pawn.PawnEffectParticle, pawn.gamePlayComp.transform);
			Vector3 ParticlePos = pawn.gamePlayComp.Board [boardFieldId].BoardPiece.transform.localPosition;
			particleObject.transform.localPosition = ParticlePos;
		}

		yield return null;
	}

	private void CreateGranadeAttackMarks(int boardFieldId) {
		int nextBoardIndex;

		attackMark = pawn.gamePlayComp.CreateAttackMark (lastBoardIndex, hit_Damage);

		for (int i = 0; i < 6; i++) {
			if (i == 0)
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUp (lastBoardIndex);
			else if (i == 1)
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexDown (lastBoardIndex);
			else if (i == 2)
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUpLeft (lastBoardIndex);
			else if (i == 3)
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUpRight (lastBoardIndex);
			else if (i == 4)
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexDownLeft (lastBoardIndex);
			else
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexDownRight (lastBoardIndex);

			if (nextBoardIndex != -1) {
				attackMarks.Add (pawn.gamePlayComp.CreateAttackMark (nextBoardIndex, next_Damage));
			}
		}
	}

	private void DestroyGranadeAttackMarks() {
		Destroy (attackMark);
		foreach (GameObject GOb in attackMarks) {
			if (GOb != null)
				Destroy (GOb);
		}
		attackMarks.Clear ();
	}

	private void TakeGranadeDamage(int boardFieldId) {
		int nextBoardIndex;
		if (!pawn.gamePlayComp.skipAnimations) {
			StartCoroutine (ShowParticleEffect (boardFieldId));
		}
		pawn.gamePlayComp.DoDamageOnBoard (pawn ,boardFieldId, hit_Damage);
		for (int i = 0; i < 6; i++) {
			if (i == 0)
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUp (boardFieldId);
			else if (i == 1)
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexDown (boardFieldId);
			else if (i == 2)
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUpLeft (boardFieldId);
			else if (i == 3)
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUpRight (boardFieldId);
			else if (i == 4)
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexDownLeft (boardFieldId);
			else
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexDownRight (boardFieldId);

			if (nextBoardIndex != -1) {
				pawn.gamePlayComp.DoDamageOnBoard (pawn ,nextBoardIndex, next_Damage);
			}
		}
		Destroy (pawn.gameObject, 3);
	}

	private int OnGranadePlay(int boardFieldId) {
		int startPosId = pawn.gamePlayComp.myHero.GetComponent<Pawn> ().boardPosisionIndex;
		int boardId = pawn.gamePlayComp.GetClosestOverMouseAnyIndexPosition (pawn, false, false, false, false);
		pawn.gamePlayComp.CreateAttackBeam (startPosId, boardId);
		//pawn.gamePlayComp.ShowMarkForEnemy (boardId, pawn.handIndex);
		//Debug.Log ("Granate board id " + boardId);
		if (attackMark == null) {
			//Debug.Log ("Granate  create attack mark " + boardId);
			CreateGranadeAttackMarks(boardId);
			lastBoardIndex = boardId;
		}
		if (lastBoardIndex != boardId) {
			DestroyGranadeAttackMarks ();
		}
		lastBoardIndex = boardId;
		return 0;
	}

	private void OnGranadePlayConfirm(int boardFieldId) {
		pawn.DetachPawn ();//to wait for processes end
		TakeGranadeDamage(lastBoardIndex);
		pawn.gamePlayComp.DestroyMarkBeam ();
		DestroyGranadeAttackMarks ();
		pawn.gamePlayComp.PlayEffectOnBoard (pawn.Name, 0, lastBoardIndex, pawn.handIndex);
	}

	private void OnGranadePlayCancel(int boardFieldId) {
		pawn.gamePlayComp.DestroyMarkBeam ();
		DestroyGranadeAttackMarks ();
	}

	private void TriggerEffect(int startFieldId, int endFieldId) {
		TakeGranadeDamage(endFieldId);
	}

	// Use this for initialization
	void Start () {

	}

	private List<int> AIMove(int boardId) {
		GamePlay.TargetSettings settings = (GamePlay.TargetSettings.WithFriendlyHero | GamePlay.TargetSettings.WithFriendlyPawn);
		if (pawn.gamePlayComp.SetTargetsOnBoard (settings, pawn.gameObject, null)) {
			return pawn.gamePlayComp.TargetsList;
		}
		return null;
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.AITriggerEffectCallback = AIMove;
			pawn.OnPlayCallback = OnGranadePlay;
			pawn.RegisterPlayConfirmCallback(OnGranadePlayConfirm);
			pawn.OnPlayCancelCallback = OnGranadePlayCancel;
			pawn.TriggerEffectCallback = TriggerEffect;

			if (pawn.PawnEffectParameters.Length >= 1) {
				hit_Damage = pawn.PawnEffectParameters [0];
			} else {
				Debug.LogWarning ("Pawn effect parameter 0 is not set");
			}
			if (pawn.PawnEffectParameters.Length >= 2) {
				next_Damage = pawn.PawnEffectParameters [1];
			} else {
				Debug.LogWarning ("Pawn effect parameter 1 is not set");
			}
			Registered = true;
		}
	}
}
