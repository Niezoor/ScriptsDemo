using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParticlePlayground;

public class VerticalLineAttack : MonoBehaviour {
	private GameObject ExplodeParticleEffect;
	private int DamageValue = 4;
	private Pawn pawn;
	private GameObject attackMark;
	private List<GameObject> attackMarks = new List<GameObject> ();
	private int lastBoardIndex;
	private GamePlay.attackDirections attackDir = GamePlay.attackDirections.Up;
	public bool Registered = false;
	public bool TutorialMode = false;
	public delegate void TutorialModeBadPlay ();
	public TutorialModeBadPlay TutorialModeBadPlayCallback = null;
	public int TutorialModeStartIndex = -1;
	//local mode: ok
	//online mode: ok

	//This script is very ugly, so mych places to improvment
	private void CreateVerticalLineAttackMarks(int boardFieldId) {
		int nextBoardIndex = boardFieldId;

		attackMark = pawn.gamePlayComp.CreateAttackMark (boardFieldId, DamageValue);

		do {
			if ((attackDir == GamePlay.attackDirections.Up) || (attackDir == GamePlay.attackDirections.Down)) {
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUp (nextBoardIndex);
			} else if ((attackDir == GamePlay.attackDirections.UpLeft) || (attackDir == GamePlay.attackDirections.DownRight)) {
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUpLeft (nextBoardIndex);
			} else if ((attackDir == GamePlay.attackDirections.UpRight) || (attackDir == GamePlay.attackDirections.DownLeft)) {
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUpRight (nextBoardIndex);
			} else {
				Debug.LogError("uknown attack direction");
				break;
			}
			if (nextBoardIndex != -1) {
				attackMarks.Add (pawn.gamePlayComp.CreateAttackMark (nextBoardIndex, DamageValue));
			}
		} while (nextBoardIndex != -1);

		nextBoardIndex = boardFieldId;
		do {
			if ((attackDir == GamePlay.attackDirections.Up) || (attackDir == GamePlay.attackDirections.Down)) {
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexDown (nextBoardIndex);
			} else if ((attackDir == GamePlay.attackDirections.UpLeft) || (attackDir == GamePlay.attackDirections.DownRight)) {
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexDownRight (nextBoardIndex);
			} else if ((attackDir == GamePlay.attackDirections.UpRight) || (attackDir == GamePlay.attackDirections.DownLeft)) {
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexDownLeft (nextBoardIndex);
			} else {
				Debug.LogError("uknown attack direction");
				break;
			}
			if (nextBoardIndex != -1) {
				attackMarks.Add (pawn.gamePlayComp.CreateAttackMark (nextBoardIndex, DamageValue));
			}
		} while (nextBoardIndex != -1);
	}

	private void DestroyVerticalLineAttackMarks() {
		Destroy (attackMark);
		foreach (GameObject GOb in attackMarks) {
			if (GOb != null)
				Destroy (GOb);
		}
		attackMarks.Clear ();
	}

	private int GetStartFirePosition(int boardFieldId) {
		int nextBoardIndex = boardFieldId;
		int startBoardIndex = boardFieldId;

		do {
			if ((attackDir == GamePlay.attackDirections.Up) || (attackDir == GamePlay.attackDirections.Down)) {
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexDown (nextBoardIndex);
			} else if ((attackDir == GamePlay.attackDirections.UpLeft) || (attackDir == GamePlay.attackDirections.DownRight)) {
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexDownRight (nextBoardIndex);
			} else if ((attackDir == GamePlay.attackDirections.UpRight) || (attackDir == GamePlay.attackDirections.DownLeft)) {
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexDownLeft (nextBoardIndex);
			} else {
				Debug.LogError("unknown attack direction");
				break;
			}
			if (nextBoardIndex != -1) {
				startBoardIndex = nextBoardIndex;
			}
		} while (nextBoardIndex != -1);

		if (TutorialMode && startBoardIndex != TutorialModeStartIndex) {
			if (TutorialModeBadPlayCallback != null) {
				TutorialModeBadPlayCallback ();
			}
			return -1;
		}

		return startBoardIndex;
	}

	private IEnumerator TakeVerticalLineDamage(int boardFieldId) {
		int nextBoardIndex = boardFieldId;

		do {
			if (nextBoardIndex != -1) {
				Debug.Log(" explode ");
				yield return new WaitForSeconds (0.2f);
				GameObject particleObject = (GameObject)Instantiate(ExplodeParticleEffect, pawn.gamePlayComp.transform);
				Vector3 ParticlePos = pawn.gamePlayComp.Board [nextBoardIndex].BoardPiece.transform.localPosition;
				particleObject.transform.localPosition = ParticlePos;
				pawn.gamePlayComp.DoDamageOnBoard (pawn, nextBoardIndex, DamageValue);
			}
			if ((attackDir == GamePlay.attackDirections.Up) || (attackDir == GamePlay.attackDirections.Down)) {
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUp (nextBoardIndex);
			} else if ((attackDir == GamePlay.attackDirections.UpLeft) || (attackDir == GamePlay.attackDirections.DownRight)) {
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUpLeft (nextBoardIndex);
			} else if ((attackDir == GamePlay.attackDirections.UpRight) || (attackDir == GamePlay.attackDirections.DownLeft)) {
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUpRight (nextBoardIndex);
			} else {
				Debug.LogError("uknown attack direction");
				break;
			}
		} while (nextBoardIndex != -1);
		Destroy (pawn.gameObject);
		yield return null;
	}

	private void TakeVerticalLineDamageNoAnim(int boardFieldId) {
		int nextBoardIndex = boardFieldId;

		do {
			if (nextBoardIndex != -1) {
				Debug.Log(" explode ");
				pawn.gamePlayComp.DoDamageOnBoard (pawn, nextBoardIndex, DamageValue);
			}
			if ((attackDir == GamePlay.attackDirections.Up) || (attackDir == GamePlay.attackDirections.Down)) {
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUp (nextBoardIndex);
			} else if ((attackDir == GamePlay.attackDirections.UpLeft) || (attackDir == GamePlay.attackDirections.DownRight)) {
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUpLeft (nextBoardIndex);
			} else if ((attackDir == GamePlay.attackDirections.UpRight) || (attackDir == GamePlay.attackDirections.DownLeft)) {
				nextBoardIndex = pawn.gamePlayComp.GetBoardIndexUpRight (nextBoardIndex);
			} else {
				Debug.LogError("uknown attack direction");
				break;
			}
		} while (nextBoardIndex != -1);
		Destroy (pawn.gameObject);
	}

	private GamePlay.attackDirections GetAttackDir(int index, int indexPrev) {
		GamePlay.attackDirections retValue;

		if (pawn.gamePlayComp.GetBoardIndexUp (indexPrev) == index) {
			retValue = GamePlay.attackDirections.Up;
		} else if (pawn.gamePlayComp.GetBoardIndexDown (indexPrev) == index) {
			retValue = GamePlay.attackDirections.Down;
		} else if (pawn.gamePlayComp.GetBoardIndexUpLeft (indexPrev) == index) {
			retValue = GamePlay.attackDirections.UpLeft;
		} else if (pawn.gamePlayComp.GetBoardIndexUpRight (indexPrev) == index) {
			retValue = GamePlay.attackDirections.UpRight;
		} else if (pawn.gamePlayComp.GetBoardIndexDownLeft (indexPrev) == index) {
			retValue = GamePlay.attackDirections.DownLeft;
		} else if (pawn.gamePlayComp.GetBoardIndexDownRight (indexPrev) == index) {
			retValue = GamePlay.attackDirections.DownRight;
		} else {
			retValue = GamePlay.attackDirections.Up;
		}
		return retValue;
	}

	private int OnVerticalLinePlay(int boardFieldId) {
		//int startPosId = pawn.gamePlayComp.myHero.GetComponent<Pawn> ().boardPosisionIndex;
		int boardId = pawn.gamePlayComp.GetClosestOverMouseAnyIndexPosition (pawn, false, false, false, false);
		//pawn.gamePlayComp.CreateAttackBeam (startPosId, boardId);
		//pawn.gamePlayComp.ShowMarkForEnemy (boardId, pawn.handIndex);
		//Debug.Log ("Granate board id " + boardId);
		if (attackMark == null) {
			//Debug.Log ("Granate  create attack mark " + boardId);
			CreateVerticalLineAttackMarks(boardId);
			lastBoardIndex = boardId;
		}
		if (lastBoardIndex != boardId) {
			DestroyVerticalLineAttackMarks ();
			if (lastBoardIndex != -1) {
				attackDir = GetAttackDir (boardId, lastBoardIndex);
			}
		}
		lastBoardIndex = boardId;
		return 0;
	}

	private int dirToIndex(GamePlay.attackDirections atDir) {
		int rv = -1;
		if (atDir == GamePlay.attackDirections.Up) {
			rv = 0;
		} else if (atDir == GamePlay.attackDirections.UpLeft) {
			rv = 1;
		} else if (atDir == GamePlay.attackDirections.UpRight) {
			rv = 2;
		} else if (atDir == GamePlay.attackDirections.Down) {
			rv = 3;
		} else if (atDir == GamePlay.attackDirections.DownLeft) {
			rv = 4;
		} else if (atDir == GamePlay.attackDirections.DownRight) {
			rv = 5;
		} else {
			Debug.LogError("uknown attack direction");
		}

		return rv;
	}

	private GamePlay.attackDirections indexToDir(int index) {
		GamePlay.attackDirections rv = GamePlay.attackDirections.Up;
		if (index == 0) {
			rv = GamePlay.attackDirections.Up;
		} else if (index == 1) {
			rv = GamePlay.attackDirections.UpLeft;
		} else if (index == 2) {
			rv = GamePlay.attackDirections.UpRight;
		} else if (index == 3) {
			rv = GamePlay.attackDirections.Down;
		} else if (index == 4) {
			rv = GamePlay.attackDirections.DownLeft;
		} else if (index == 5) {
			rv = GamePlay.attackDirections.DownRight;
		} else {
			Debug.LogError("uknown attack direction");
		}

		return rv;
	}

	private void OnVerticalLinePlayConfirm(int boardFieldId) {
		int startFirePositionIndex = GetStartFirePosition (lastBoardIndex);
		if (startFirePositionIndex != -1) {
			pawn.DetachPawn ();//to wait for processes end
			pawn.gamePlayComp.PlayEffectOnBoard (pawn.Name, dirToIndex (attackDir), startFirePositionIndex, pawn.handIndex);
			StartCoroutine (TakeVerticalLineDamage (startFirePositionIndex));
		}
		pawn.gamePlayComp.DestroyMarkBeam ();
		DestroyVerticalLineAttackMarks ();
	}

	private void OnVerticalLinePlayCancel(int boardFieldId) {
		pawn.gamePlayComp.DestroyMarkBeam ();
		DestroyVerticalLineAttackMarks ();
	}

	private void TriggerEffect(int startFieldId, int endFieldId) {
		attackDir = indexToDir (startFieldId);
		if (pawn.gamePlayComp.skipAnimations) {
			TakeVerticalLineDamageNoAnim (endFieldId);
		} else {
			StartCoroutine (TakeVerticalLineDamage (endFieldId));
		}
	}

	// Use this for initialization
	void Start () {

	}

	private List<int> AIMove(int boardId) {
		GamePlay.TargetSettings settings = (GamePlay.TargetSettings.WithFriendlyPawn | GamePlay.TargetSettings.WithFriendlyHero);
		if (pawn.gamePlayComp.SetTargetsOnBoard (settings, pawn.gameObject, null)) {
			return pawn.gamePlayComp.TargetsList;
		}
		return null;
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.AITriggerEffectCallback = AIMove;
			pawn.OnPlayCallback = OnVerticalLinePlay;
			pawn.RegisterPlayConfirmCallback(OnVerticalLinePlayConfirm);
			pawn.OnPlayCancelCallback = OnVerticalLinePlayCancel;
			pawn.TriggerEffectCallback = TriggerEffect;

			if (pawn.PawnEffectParameters.Length >= 1) {
				DamageValue = pawn.PawnEffectParameters [0];
			} else {
				Debug.LogWarning ("Pawn effect parameter 0 is not set");
			}

			if (pawn.PawnEffectParticle != null) {
				ExplodeParticleEffect = pawn.PawnEffectParticle;
			} else {
				Debug.LogWarning ("Pawn effect particles is not choosen");
			}
			Registered = true;
		}
	}
}
