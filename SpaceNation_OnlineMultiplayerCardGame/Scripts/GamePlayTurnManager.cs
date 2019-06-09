using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon;
using System;
using TMPro;

public class GamePlayTurnManager : PunBehaviour, IPunTurnManagerCallbacks {

	[System.Serializable]
	public class GameplayPlayerMove
	{
		public GamePlayActionStack.ActionTypeEnum Action;
		public int PawnID;
		public string EffectName;
		public int TargetPawnID;
		public int TargetPosition;
		public int TargetRotation;
		public int HandIndex;
	}

	public int LastTurn = -1;
	public TextMeshPro TurnTimeText;

	private PunTurnManager turnManager;
	private GamePlay gPlay;

	private bool audioPlayed = false;

	// Use this for initialization
	public void StartTurnManager (GamePlay gp) {
		gPlay = gp;
		this.turnManager = this.gameObject.AddComponent<PunTurnManager>();
		this.turnManager.TurnManagerListener = this;
		this.turnManager.TurnDuration = (float)gPlay.TurnDuration;
	}
	
	// Update is called once per frame
	void Update () {
		if (this.turnManager != null && this.turnManager.Turn > 0) {
			int seconds = (int)this.turnManager.RemainingSecondsInTurn;
			if (seconds < gPlay.TurnDurationAlarmStartAt) {
				gPlay.EndTurnButton.ChangeButtonToAlarmMode ();
			}
			gPlay.TurnRemainningTime = seconds;
			TurnTimeText.SetText (seconds.ToString());
			if (seconds == 3) {
				if (audioPlayed == false) {
					gPlay.GameplayAudio.PlayOneShot (gPlay.CountDownAudioClip);
				}
				audioPlayed = true;
			} else {
				audioPlayed = false;
			}
		}
	}

	/// <summary>
	/// Send move by Photon event (Photon turn extention).
	/// Some action does not require lot of params.
	/// </summary>
	/// <param name="Action">Move action type.</param>
	/// <param name="StringParam">Addition param for move action</param>
	/// <param name="SendBackEvent">Process this move locally (false by default)</param>
	public void GameplaySendMove(GamePlayActionStack.ActionTypeEnum Action, string StringParam, bool SendBackEvent = false)
	{
		GameplaySendMove(Action, 0, StringParam, 0, 0, 0, 0, SendBackEvent);
	}

	/// <summary>
	/// Send move by Photon event (Photon turn extention).
	/// </summary>
	/// <param name="Action">Move action type.</param>
	/// <param name="PawnID">Moving pawnID</param>
	/// <param name="EffectName">Card effect class name</param>
	/// <param name="TargetPawnID">Move target pawn ID</param>
	/// <param name="TargetPosition">Target position</param>
	/// <param name="TargetRotation">Target rotation index (0-5)</param>
	/// <param name="HandIndex">Card hand index</param>
	/// <param name="SendBackEvent">Process this move locally (false by default)</param>
	public void GameplaySendMove(
		GamePlayActionStack.ActionTypeEnum Action,
		int PawnID,
		string EffectName,
		int TargetPawnID,
		int TargetPosition,
		int TargetRotation,
		int HandIndex,
		bool SendBackEvent = false)
	{
		GameplayPlayerMove move = new GameplayPlayerMove ();
		move.Action = Action;
		move.PawnID = PawnID;
		move.EffectName = EffectName;
		move.TargetPawnID = TargetPawnID;
		move.TargetPosition = TargetPosition;
		move.TargetRotation = TargetRotation;
		move.HandIndex = HandIndex;

		Debug.Log ("[SEND EVENT]:" + JsonUtility.ToJson (move).ToString());
		this.turnManager.SendMove (JsonUtility.ToJson (move), false, SendBackEvent);
	}

	public void GameplayTurnDone() {
		this.turnManager.SendMove (this.turnManager.Turn, true);
	}

	#region Turn extention functions
	public void StartTurn() {
		Debug.Log ("[START NEW TURN]");
		if (PhotonNetwork.isMasterClient) {
			this.turnManager.BeginTurn();
		}
	}

	#endregion

	private void OnMyTurnBegin(int turn) {
		Debug.Log ("[on my turn]:" + turn);
		gPlay.TakeTurn ();
	}

	private void OnEnemyTurnBegin(int turn) {
		Debug.Log ("[on enemy turn]:" + turn);
		if (turn == this.turnManager.Turn) {
			GameplayTurnDone ();
		}
		gPlay.OnGiveTurn ();
	}

	public void OnHandleNewTurnEvent(int turn) {
		Debug.Log ("[HANDLE TURN BEGINS]:" + turn);
		if (gPlay.YouStarted) {
			if (turn % 2 == 0) {
				OnEnemyTurnBegin (turn);
			} else {
				OnMyTurnBegin (turn);
			}
		} else {
			if (turn % 2 == 0) {
				OnMyTurnBegin (turn);
			} else {
				OnEnemyTurnBegin (turn);
			}
		}
	}

	#region Core Gameplay Callbacks
	public void OnTurnBegins(int turn) {
		Debug.Log ("[ON TURN BEGINS]:" + turn);
		if (PhotonNetwork.isMasterClient) {
			GameplaySendMove (GamePlayActionStack.ActionTypeEnum.newTurn, "", true);
		}
	}

	public void OnTurnCompleted(int turn) {
		Debug.Log ("[TURN COMPLETED]:" + turn);
		StartTurn ();
	}

	public void OnPlayerMove(PhotonPlayer sender, int turn, object move) {
		GameplayPlayerMove gPlayMove = JsonUtility.FromJson<GameplayPlayerMove> (move.ToString ());
		Debug.Log ("[RECEIVE EVENT]:" + move.ToString ());

		if (gPlay.skipAnimations) {
			gPlay.RefreshPawnsHealth ();
		}

		if ((gPlay.skipAnimations) && (turn == this.turnManager.Turn)) {
			gPlay.skipAnimations = false;
		}

		if (gPlayMove.Action == GamePlayActionStack.ActionTypeEnum.firstPlayer) {
			if (sender != PhotonNetwork.player) {
				gPlay.SetAsSecondPlayer ();
			}
		} else if (gPlayMove.Action == GamePlayActionStack.ActionTypeEnum.setHero) {
			if (sender == PhotonNetwork.player) {
				gPlay.SetMyHero (gPlayMove.EffectName);//hacky way, one string in struckture
			} else {
				gPlay.SetEnemyHero (gPlayMove.EffectName);
			}
		} else if (gPlayMove.Action == GamePlayActionStack.ActionTypeEnum.signalReady) {
			if (sender != PhotonNetwork.player) {
				gPlay.EnemyIsReady = true;
				gPlay.MatchStart ();
			}
		} else if (gPlayMove.Action == GamePlayActionStack.ActionTypeEnum.redraw) {
			if (sender != PhotonNetwork.player) {
				gPlay.EnemyRemoveCardFromHand (gPlayMove.HandIndex);
				gPlay.EnemyCardDraw ();
			}
		} else if (gPlayMove.Action == GamePlayActionStack.ActionTypeEnum.draw) {
			if (sender != PhotonNetwork.player) {
				gPlay.EnemyCardDraw ();
			} else {
				gPlay.Draw (true);
			}
		} else if (gPlayMove.Action == GamePlayActionStack.ActionTypeEnum.play) {
			if (sender != PhotonNetwork.player) {
				gPlay.PutEnemyPawnOnBoard (gPlayMove.EffectName,
					gPlayMove.PawnID, gPlayMove.TargetRotation,
					gPlayMove.HandIndex, gPlayMove.TargetPosition);
			} else {
				gPlay.PutEnemyPawnOnBoard (gPlayMove.EffectName,
					gPlayMove.PawnID, gPlayMove.TargetRotation,
					gPlayMove.HandIndex, gPlayMove.TargetPosition, false);
			}
		} else if (gPlayMove.Action == GamePlayActionStack.ActionTypeEnum.move) {
			if (sender != PhotonNetwork.player) {
				gPlay.ChangeEnemyPawnPos (gPlayMove.PawnID, gPlayMove.TargetRotation,
					gPlayMove.TargetPosition);
			} else {
				gPlay.ChangeEnemyPawnPos (gPlayMove.PawnID, gPlayMove.TargetRotation,
					gPlayMove.TargetPosition);
			}
		} else if (gPlayMove.Action == GamePlayActionStack.ActionTypeEnum.moveConfirm) {
			if (sender != PhotonNetwork.player) {
				gPlay.ConfirmEnemyPawnPos (gPlayMove.EffectName, gPlayMove.PawnID,
					gPlayMove.TargetRotation, gPlayMove.TargetPosition);
			} else {
				gPlay.ConfirmEnemyPawnPos (gPlayMove.EffectName, gPlayMove.PawnID,
					gPlayMove.TargetRotation, gPlayMove.TargetPosition);
			}
		} else if (gPlayMove.Action == GamePlayActionStack.ActionTypeEnum.playItem) {
			if (sender != PhotonNetwork.player) {
				gPlay.PutEnemyWeaponOnBoard (gPlayMove.EffectName, gPlayMove.TargetRotation,
					gPlayMove.HandIndex, gPlayMove.TargetPosition);
			} else {
				gPlay.PutEnemyWeaponOnBoard (gPlayMove.EffectName, gPlayMove.TargetRotation,
					gPlayMove.HandIndex, gPlayMove.TargetPosition, false);
			}
		} else if (gPlayMove.Action == GamePlayActionStack.ActionTypeEnum.playEffect) {
			if (sender != PhotonNetwork.player) {
				gPlay.PlayEnemyEffectOnBoard (gPlayMove.EffectName, gPlayMove.TargetPawnID,
					gPlayMove.TargetPosition, gPlayMove.HandIndex);
			} else {
				gPlay.PlayEnemyEffectOnBoard (gPlayMove.EffectName, gPlayMove.TargetPawnID,
					gPlayMove.TargetPosition, gPlayMove.HandIndex, false);
			}
		} else if (gPlayMove.Action == GamePlayActionStack.ActionTypeEnum.itemConfirm) {
			if (sender != PhotonNetwork.player) {
				gPlay.GiveWeaponToEnemyPawn (gPlayMove.EffectName, gPlayMove.TargetPosition,
					gPlayMove.TargetRotation);
			} else {
				gPlay.GiveWeaponToEnemyPawn (gPlayMove.EffectName, gPlayMove.TargetPosition,
					gPlayMove.TargetRotation);
			}
		} else if (gPlayMove.Action == GamePlayActionStack.ActionTypeEnum.attack) {
			if (sender != PhotonNetwork.player) {
				gPlay.DoAttack (gPlayMove.PawnID, gPlayMove.TargetPosition, true);
			} else {
				gPlay.DoAttack (gPlayMove.PawnID, gPlayMove.TargetPosition, true);
			}
		} else if (gPlayMove.Action == GamePlayActionStack.ActionTypeEnum.newTurn) {
			OnHandleNewTurnEvent (turn);
		} else if (gPlayMove.Action == GamePlayActionStack.ActionTypeEnum.giveup) {
			if (sender != PhotonNetwork.player) {
				gPlay.WinGame(0);
			} else {
				gPlay.DefeatGame(0);
			}
		} else {
			Debug.LogWarning ("Unsupported move:" + move.ToString ());
		}
	}

	public void OnPlayerFinished(PhotonPlayer photonPlayer, int turn, object move) {
		Debug.Log ("[RECEIVE TURN DONE]:" + photonPlayer.NickName);
	}

	public void OnTurnTimeEnds(int turn) {
		Debug.Log ("[TURN TIME ENDS]:" + turn);
		OnTurnCompleted(turn);
		/*if (gPlay.myTurn) {
			gPlay.GiveTurn ();
		}*/
	}
	#endregion

}
