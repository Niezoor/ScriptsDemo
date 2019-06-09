using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardsCommanderBuff : MonoBehaviour {
	private Pawn pawn;
	public bool Registered = false;
	private int friendlyPawnHealthBuff = 3;
	private int guardsCommanderAttackBuff = 1;

	public Pawn BuffedPawn = null;
	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {

	}

	private void ShowParticleEffect(int boardFieldId) {
		Debug.Log ("ShowParticleEffect: " + boardFieldId);
		if (pawn.PawnEffectParticle != null) {
			GameObject particleObject = (GameObject)Instantiate (pawn.PawnEffectParticle);
			Debug.Log ("ShowParticleEffect initialized: " + boardFieldId);
			particleObject.transform.SetParent(pawn.gamePlayComp.Board [boardFieldId].Pawn.transform);
			Quaternion newRot =  Quaternion.identity;
			particleObject.transform.localPosition = new Vector3(0,0,0);
			newRot = Quaternion.Euler (0, 0, 0);
			particleObject.transform.localRotation = newRot;
			particleObject.transform.localScale = new Vector3(1, 1, 1);
		}
	}

	public void BuffGuardsCommander(int unused) {
		Debug.Log ("buff Guards Commander");
		pawn.BuffAttack(BuffedPawn, guardsCommanderAttackBuff);
	}

	private void BuffPawn(int pawnToBuffBoardId) {
		if (pawnToBuffBoardId >= 0) {
			GameObject pawnGOb = pawn.gamePlayComp.Board [pawnToBuffBoardId].Pawn;
			if (pawnGOb != null) {
				Pawn pawnTobuff = pawnGOb.GetComponent<Pawn> ();
				pawnTobuff.BuffHealth(pawn, friendlyPawnHealthBuff);
				ShowParticleEffect (pawnToBuffBoardId);
				pawnTobuff.RegisterOnGetDamageCallback (BuffGuardsCommander);
				BuffedPawn = pawnTobuff;
			}
		}
	}

	private void BuffPawnCallback(int boardId) {
		int handIdx = pawn.CardType == CardsBase.CardTypesEnum.Effect ? pawn.handIndex : -1;//if its a pawn effect, do not play from hand
		BuffPawn (boardId);
		pawn.gamePlayComp.PlayEffectOnBoard (pawn.Name, pawn.pawnBoardID, boardId, handIdx);
	}

	public void ChoosePawnToBuff(int PawnBoardFieldId) {
		GamePlay.TargetSettings settings =
			(GamePlay.TargetSettings.WithFriendlyPawn |
			GamePlay.TargetSettings.WithPawnsLock |
			GamePlay.TargetSettings.MustBeCalled);
		if (pawn.gamePlayComp.SetTargetsOnBoard (settings, pawn.gameObject, BuffPawnCallback)) {
			pawn.gamePlayComp.ShowTargetNotification (pawn.Desc);
		}
	}

	private IEnumerator TriggerBuffTask(int pawnID, int pawnToBuffBoardId) {
		GameObject GuardsCommanderPawn = null;
		int timeout = 30;
		while(GuardsCommanderPawn == null) {
			timeout--;
			yield return new WaitForSeconds (0.1f);
			Pawn GCPawn = pawn.gamePlayComp.GetBoardPawnByID(pawnID);
			if (GCPawn != null) {
				GuardsCommanderPawn = GCPawn.gameObject;
			}
			if (timeout < 0) {
				break;
			}
		}
		if (GuardsCommanderPawn != null) {
			if (pawnToBuffBoardId >= 0) {
				GameObject pawnGOb = pawn.gamePlayComp.Board [pawnToBuffBoardId].Pawn;
				if (pawnGOb != null) {
					Pawn pawnTobuff = pawnGOb.GetComponent<Pawn> ();
					pawnTobuff.BuffHealth (pawn, friendlyPawnHealthBuff);
					ShowParticleEffect (pawnToBuffBoardId);
					if (GuardsCommanderPawn != null) {
						Debug.Log ("successfully register GuardsCommander callback for enemy");
						pawnTobuff.RegisterOnGetDamageCallback (GuardsCommanderPawn.GetComponent<GuardsCommanderBuff> ().BuffGuardsCommander);
						GuardsCommanderPawn.GetComponent<GuardsCommanderBuff> ().BuffedPawn = pawnTobuff;
					} else {
						Debug.LogError ("cannot get GuardsCommander pawn"); 
					}
				} else {
					Debug.LogError ("cannot get guards commander pawn to pawn"); 
				}
			} else {
				Debug.LogError ("bad guards commander pawn to pawn idx"); 
			}
		} else {
			Debug.LogError ("cannot find GuardsCommander pawn"); 
		}
	}

	private void TriggerBuffNoAnim(int pawnID, int pawnToBuffBoardId) {
		Pawn GCPawn = pawn.gamePlayComp.GetBoardPawnByID(pawnID);
		GameObject GuardsCommanderPawn = null;
		if (GCPawn != null) {
			GuardsCommanderPawn = GCPawn.gameObject;
		}
		if (GuardsCommanderPawn != null) {
			if (pawnToBuffBoardId >= 0) {
				GameObject pawnGOb = pawn.gamePlayComp.Board [pawnToBuffBoardId].Pawn;
				if (pawnGOb != null) {
					Pawn pawnTobuff = pawnGOb.GetComponent<Pawn> ();
					pawnTobuff.BuffHealth (pawn, friendlyPawnHealthBuff);
					if (GuardsCommanderPawn != null) {
						Debug.Log ("successfully register GuardsCommander callback for enemy");
						pawnTobuff.RegisterOnGetDamageCallback (GuardsCommanderPawn.GetComponent<GuardsCommanderBuff> ().BuffGuardsCommander);
						GuardsCommanderPawn.GetComponent<GuardsCommanderBuff> ().BuffedPawn = pawnTobuff;
					} else {
						Debug.LogError ("cannot get GuardsCommander pawn"); 
					}
				} else {
					Debug.LogError ("cannot get guards commander pawn to pawn"); 
				}
			} else {
				Debug.LogError ("bad guards commander pawn to pawn idx"); 
			}
		} else {
			Debug.LogError ("cannot find GuardsCommander pawn"); 
		}
	}

	private void TriggerBuff(int pawnID, int pawnToBuffBoardId) {
		if (pawn.gamePlayComp.skipAnimations) {
			TriggerBuffNoAnim (pawnID, pawnToBuffBoardId);
		} else {
			StartCoroutine (TriggerBuffTask (pawnID, pawnToBuffBoardId));
		}
	}

	void Awake() {
		if (!Registered) {
			pawn = GetComponent<Pawn> ();
			pawn.RegisterPlayConfirmCallback(ChoosePawnToBuff);
			pawn.TriggerEffectCallback = TriggerBuff;
			if (pawn.PawnEffectParameters.Length >= 1) {
				friendlyPawnHealthBuff = pawn.PawnEffectParameters [0];
			} else {
				Debug.LogWarning ("Pawn effect parameter 0 is not set");
			}
			if (pawn.PawnEffectParameters.Length >= 2) {
				guardsCommanderAttackBuff = pawn.PawnEffectParameters [1];
			} else {
				Debug.LogWarning ("Pawn effect parameter 1 is not set");
			}
			Registered = true;
		}
	}
}
