using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;
using System.IO;

public class GamePlayCore : MonoBehaviour {
	public GamePlay GameplayComponent;
	private static string[] separators = new string[] {","};
	GetUserDataResult lastGetDataResult;

	public static string CurrentDeckProperty = "CurrDeck";
	public static string CurrentDeckHeroProperty = "CurrDeckHero";
	public static string CurrentDeckNameProperty = "CurrDeckName";

	[Serializable]
	public class pawnStatus
	{
		public int CardId;
		public string Owner;
		public int boardPosisionIndex;
		public int RotationPosIndex;
		public int Health;
		public int Attack;
		public int BuffedHealth;
		public int BuffedAttack;
		public bool Frozen;
		public bool Burning;
		public int BurningTurnsLeft;
		public bool Poisoned;
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	#region DECK
	public void SaveDeck() {
		UpdateUserDataRequest request = new UpdateUserDataRequest ();
		request.Data = new Dictionary<string, string> ();
		string cardsInDeckName = "";
		Deck deck = GameplayComponent.myDeck.GetComponent<Deck> ();
		for (int j = 0; j < Deck.deckSize; j++) {
			cardsInDeckName += deck.CardNames[j]+separators[0];
		}
		request.Data.Add (CurrentDeckProperty, cardsInDeckName);
		request.Data.Add (CurrentDeckHeroProperty, deck.Hero.GetComponent<Hero>().Name);
		request.Data.Add (CurrentDeckNameProperty, deck.DeckName);

		PlayFabClientAPI.UpdateUserData (request, DeckSaved, OnPlayFabError);
	}

	private void DeckSaved(UpdateUserDataResult result) {
		Debug.Log ("Deck for gameplay saved");
	}

	public void LoadDeck() {
		GetUserDataRequest request = new GetUserDataRequest ();
		PlayFabClientAPI.GetUserData (request, DeckLoaded, OnPlayFabError);
	}

	private void DeckLoaded(GetUserDataResult result) {
		Debug.Log ("Deck for gameplay loaded");
		if (result.Data.ContainsKey (CurrentDeckProperty)) {
			string allCards = result.Data [CurrentDeckProperty].Value;
			string[] cards = allCards.Split (separators, StringSplitOptions.RemoveEmptyEntries);
			Deck deck = GameplayComponent.myDeck.GetComponent<Deck> ();
			for (int j = 0; j < Deck.deckSize; j++) {
				deck.CardNames[j] = cards[j];
			}
		}
	}
	#endregion

	#region HAND
	public void SaveHand() {
		UpdateUserDataRequest request = new UpdateUserDataRequest ();
		request.Data = new Dictionary<string, string> ();
		string cardsInHandID = "";
		foreach (GameObject cardOb in GameplayComponent.HandComp.HandCards) {
			CardInteraction cardInter = cardOb.GetComponent<CardInteraction> ();
			if (cardInter != null) {
				if (cardInter.pawnComponent != null) {
					cardsInHandID += cardInter.pawnComponent.CardID.ToString () + separators[0];
				}
			}
		}
		request.Data.Add ("CurrHand", cardsInHandID);
		Deck deck = GameplayComponent.myDeck.GetComponent<Deck> ();
		request.Data.Add ("CurrCardsInDeck", deck.cardsInDeck.ToString());

		PlayFabClientAPI.UpdateUserData (request, HandSaved, OnPlayFabError);
	}

	private void HandSaved(UpdateUserDataResult result) {
		Debug.Log ("Hand for gameplay saved");
	}

	public void LoadHand() {
		if (lastGetDataResult != null) {
			if (lastGetDataResult.Data.ContainsKey ("CurrHand")) {
				string allID = lastGetDataResult.Data ["CurrHand"].Value;
				string[] cards = allID.Split (separators, StringSplitOptions.RemoveEmptyEntries);
				foreach (GameObject card in GameplayComponent.HandComp.HandCards) {
					Destroy (card);
				}
				foreach (string card in cards) {
					GameObject newHandCard = GameplayComponent.CardsComp.SpawnCardByID (int.Parse(card));
					GameplayComponent.HandComp.AddCardToHand (newHandCard);
				}
			}
		}
	}
	#endregion

	#region GAMESTATE
	private static string AddSerialInt(int val) {
		return val.ToString () + separators [0];
	}

	private static string AddSerialSting(string val) {
		return val + separators [0];
	}

	private static string AddSerialBool(bool val) {
		return (val ? "1" : "0") + separators [0];
	}

	public static string SerializePawnState(Pawn pawn) {
		//cardID,owner,pos,rot,health,attack,healthbuff,attackbuff,frozen,burned,burningturns,poisoned,effectserial
		pawnStatus pawnSerial = new pawnStatus();
		pawnSerial.CardId = pawn.CardID;
		//pawnSerial.Owner = pawn.owner;
		pawnSerial.boardPosisionIndex = pawn.boardPosisionIndex;
		pawnSerial.RotationPosIndex = pawn.RotationPosIndex;
		pawnSerial.Health = pawn.Health;
		pawnSerial.Attack = pawn.Attack;
		pawnSerial.BuffedHealth = pawn.BuffedHealth;
		pawnSerial.BuffedAttack = pawn.BuffedAttack;
		pawnSerial.Frozen = pawn.Frozen;
		pawnSerial.Burning = pawn.Burning;
		pawnSerial.BurningTurnsLeft = pawn.BurningTurnsLeft;
		pawnSerial.Poisoned = pawn.Poisoned;

		return JsonUtility.ToJson(pawnSerial);
	}

	public pawnStatus DeserializePawnState(string serialData) {
		if (serialData != null) {
			pawnStatus pawnToSync = new pawnStatus ();
			Debug.Log ("Deserialize: " + serialData);
			serialData = serialData.Substring (10);
			pawnToSync = JsonUtility.FromJson<pawnStatus> (serialData);
			Debug.Log ("Deserialize: " + pawnToSync.Health);
			return pawnToSync;
		} else {
			return null;
		}
	}

	public void SaveGameState() {
		foreach (GamePlay.pawnListClass pawnToRet in GameplayComponent.GamePawnsList) {
			ExecuteCloudScriptRequest request = new ExecuteCloudScriptRequest ();
			string state = SerializePawnState (pawnToRet.pawn);
			request.FunctionName = "UpdatePawnState";
			request.FunctionParameter = new {pawnID = pawnToRet.pawnID.ToString(),
				pawnState = state};
			Debug.Log ("TRY SAVE PAWN TO COULD: " + pawnToRet.pawnID.ToString() + "state:" + state);
			PlayFabClientAPI.ExecuteCloudScript (request, SaveGameStateResult, OnPlayFabError);
		}
	}

	private void SaveGameStateResult(ExecuteCloudScriptResult result) {
		Debug.Log ("SAVE PAWN TO COULD: " + result.Logs.ToArray());
	}

	public void LoadGameState() {
		ExecuteCloudScriptRequest request = new ExecuteCloudScriptRequest ();
		request.FunctionName = "GetPawnsState";
		PlayFabClientAPI.ExecuteCloudScript (request, LoadPawnStateFromCloud, OnPlayFabError);
	}

	private void LoadPawnStateFromCloud(ExecuteCloudScriptResult result) {
		Debug.Log ("PAWNS IN COULD: " + result.FunctionResult);
		JsonObject jsonResult = (JsonObject)result.FunctionResult;
		//for (int i = 0; i <= jsonResult.Count; i++) {
		foreach (string key in jsonResult.Keys) {
			Pawn pawn = FindPawnOnBoard (int.Parse (key));
			pawnStatus pawnToSync = new pawnStatus ();
			object value;
			jsonResult.TryGetValue(key, out value);
			Debug.Log (" parsed '" + value.ToString() + "'");
			pawnToSync = DeserializePawnState (value.ToString());
			//pawnStatus pawnToSync = JsonUtility.FromJson( JsonUtility.FromJson<pawnStatus> (serialData);
			if (pawnToSync != null) {
				if (pawn != null) {
					Debug.Log ("apply: " + pawnToSync.Health);
					GameplayComponent.ConfirmEnemyPawnPos (pawn.Name,
						pawn.pawnBoardID,
						pawnToSync.RotationPosIndex,
						pawnToSync.boardPosisionIndex);
					pawn.BuffedAttack = pawnToSync.BuffedAttack;
					pawn.BuffedHealth = pawnToSync.BuffedHealth;
					pawn.SetHealth (pawnToSync.Health);
					pawn.SetAttack (pawnToSync.Attack);
					if (pawnToSync.Frozen) {
						pawn.Freeze ();
					}
					if (pawnToSync.Burning) {
						pawn.SetOnFire (pawnToSync.BurningTurnsLeft);
					}
					if (pawnToSync.Poisoned) {
						pawn.Poisoned = true;
					}
				}
			}
		}
	}

	private Pawn FindPawnOnBoard(int id) {
		foreach (GamePlay.pawnListClass pawnToRet in GameplayComponent.GamePawnsList) {
			if (pawnToRet.pawnID == id) {
				return pawnToRet.pawn;
			}
		}
		return null;
	}
	#endregion

	private void OnPlayFabError(PlayFabError error)
	{
		Debug.LogError ("Got an error: " + error.ErrorMessage);
	}
}
