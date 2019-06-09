using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GamePlay))]
public class GamePlayActionStack : MonoBehaviour {
	[System.Serializable]
	public enum ActionTypeEnum
	{
		attack,//0
		attackAndCounter,
		death,
		play,//3
		playEffect,//4
		playItem,//5
		move,//6
		moveConfirm,//7
		itemConfirm,
		buff,//9
		heal,
		effect,
		firstPlayer,//12
		newTurn,//13
		setHero,//14
		signalReady,//15
		draw,//16
		redraw,
		giveup,
		win,//19
		defeat,
		none,
	}

	[System.Serializable]
	public class ActionClass
	{
		public ActionTypeEnum ActionType = ActionTypeEnum.attack;
		public Pawn myPawn;
		public int myPawnParam;
		public Pawn enemyPawn;
		public int enemyPawnParam;
		public GamePlay.attackDirections attackDirection;
		public int onBoardDistance;
	}

	public List<ActionClass> ActionStack = new List<ActionClass> ();
	public bool ActionStarted = false;
	public int ActionCurrent = 0;
	public int ActionsNumber = 0;

	public GamePlay GamePlayComponent { get { return GetComponent<GamePlay> (); } }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void DoNextAction() {
		Debug.Log (" == DoNextAction " + ActionCurrent);
		if (ActionsNumber > ActionCurrent) {
			ActionClass[] actions = ActionStack.ToArray();
			ActionClass action = actions[ActionCurrent];
			ActionCurrent++;
			if (action.ActionType == ActionTypeEnum.attack) {
				GamePlayComponent.ShowAttackAnimation (
					action.myPawn,
					action.enemyPawn,
					action.myPawnParam,
					action.enemyPawnParam,
					action.attackDirection,
					true,
					false
				);
			} else if (action.ActionType == ActionTypeEnum.attackAndCounter) {
				GamePlayComponent.ShowAttackAnimation (
					action.myPawn,
					action.enemyPawn,
					action.myPawnParam,
					action.enemyPawnParam,
					action.attackDirection,
					true,
					true
				);
			} else if (action.ActionType == ActionTypeEnum.death) {
				GamePlayComponent.ShowKillPawnAnimation (
					action.myPawn
				);
			} else if (action.ActionType == ActionTypeEnum.win) {
				GamePlayComponent.EndGame (true);
			} else if (action.ActionType == ActionTypeEnum.defeat) {
				GamePlayComponent.EndGame (false);
			} else {
				Debug.LogError ("Unsupported action animation type: " + action.ActionType);
			}
		} else {
			ActionStack.Clear ();
			ActionCurrent = 0;
			ActionsNumber = 0;
			ActionStarted = false;
		}
	}

	private void AddAction(ActionTypeEnum actionType) {
		ActionsNumber++;
		ActionClass action = new ActionClass ();
		action.ActionType = actionType;
		ActionStack.Add (action);
	}

	private void AddAction(ActionTypeEnum actionType, Pawn myPawn, Pawn enemyPawn, GamePlay.attackDirections attackDirection, int onBoardDistance) {
		ActionsNumber++;
		ActionClass action = new ActionClass ();
		action.ActionType = actionType;
		action.myPawn = myPawn;
		action.enemyPawn= enemyPawn;
		action.attackDirection= attackDirection;
		action.onBoardDistance = onBoardDistance;
		ActionStack.Add (action);
	}

	private void AddAction(ActionTypeEnum actionType,
		Pawn myPawn, Pawn enemyPawn,
		int mypawnParam, int enemypawnParam,
		GamePlay.attackDirections attackDirection, int onBoardDistance)
	{
		ActionsNumber++;
		ActionClass action = new ActionClass ();
		action.ActionType = actionType;
		action.myPawn = myPawn;
		action.myPawnParam = mypawnParam;
		action.enemyPawn = enemyPawn;
		action.enemyPawnParam = enemypawnParam;
		action.attackDirection= attackDirection;
		action.onBoardDistance = onBoardDistance;
		ActionStack.Add (action);
	}

	public void DoActionAnimation(ActionTypeEnum actionType)
	{
		Debug.Log (" == Add animation action " + actionType);
		AddAction (actionType);
		if (!ActionStarted) {
			ActionStarted = true;
			DoNextAction ();
		}
	}

	public void DoActionAnimation(ActionTypeEnum actionType,
		Pawn myPawn, Pawn enemyPawn,
		GamePlay.attackDirections attackDirection, int onBoardDistance)
	{
		Debug.Log (" == Add animation action " + myPawn.pawnBoardID + " " + actionType);
		AddAction (actionType,
			myPawn, enemyPawn,
			attackDirection, onBoardDistance);
		if (!ActionStarted) {
			ActionStarted = true;
			DoNextAction ();
		}
	}

	public void DoActionAnimation(ActionTypeEnum actionType,
		Pawn myPawn, Pawn enemyPawn,
		int mypawnParam, int enemypawnParam,
		GamePlay.attackDirections attackDirection, int onBoardDistance)
	{
		Debug.Log (" == Add animation action " + myPawn.pawnBoardID + " " + actionType);
		AddAction (actionType,
			myPawn, enemyPawn,
			mypawnParam, enemypawnParam,
			attackDirection, onBoardDistance);
		if (!ActionStarted) {
			ActionStarted = true;
			DoNextAction ();
		}
	}
}
