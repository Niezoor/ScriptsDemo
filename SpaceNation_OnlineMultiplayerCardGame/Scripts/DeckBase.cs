using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using PlayFab.ClientModels;
using PlayFab;

public class DeckBase : MonoBehaviour {
	
	public static int DecksNumberMax = 6;
	public int DecksNumber = 0;
	public string CurrentFileVersion = "v002";
	public CardsBase CardsBaseComponent;
	public Canvas WaitForDataScreen;
	public bool DataLoaded = false;
	public bool DataSaved = false;

	private string[] separators = new string[] {"[sep]", ","};

	[System.Serializable]
	public class DeckFromDeckBase
	{
		public string DeckName = "empty";
		public string HeroName = "empty";
		public string[] CardsNames = new string[Deck.deckSize];
	}
	public DeckFromDeckBase[] MyDecks = new DeckFromDeckBase[DecksNumberMax];
	//public List<DeckFromDeckBase> MyDeckList = new List<DeckFromDeckBase> ();
	public DeckFromDeckBase[] PremadeDecks = new DeckFromDeckBase[DecksNumberMax];
	public DeckFromDeckBase FromPrevGame = new DeckFromDeckBase();

	void Awake() {
		for (int i = 0; i < DecksNumberMax; i++) {
			MyDecks[i] = new DeckFromDeckBase ();
			MyDecks[i].DeckName = "empty";
			MyDecks[i].HeroName = "empty";
			//MyDecks[i].CardsNames = new string[Deck.deckSize];
		}
	}

	public DeckFromDeckBase AddEmptyDeck() {
		Debug.Log ("Create empty deck number " + DecksNumber);
		for (int i = 0; i < DecksNumberMax; i++) {
			if ( MyDecks[i].DeckName.Equals("empty")) {
				return MyDecks[i];
			}
		}
		return null;
	}

	private void CopyDeck(int dst, int src) {
		MyDecks [dst].DeckName = MyDecks [src].DeckName;
		MyDecks [dst].HeroName = MyDecks [src].HeroName;
		for (int j = 0; j < Deck.deckSize; j++) {
			MyDecks [dst].CardsNames[j] = MyDecks [src].CardsNames[j];
		}
	}

	public void DeleteDeck(int deckNumber) {
		//Destroy(MyDecks [deckNumber]);
		Debug.Log("Delete deck number " + deckNumber);
		if (!(MyDecks [deckNumber].DeckName.Equals ("empty"))) {
			CopyDeck (deckNumber, deckNumber + 1);
			DecksNumber--;
		} else {
			Debug.LogError ("Cannot find deck to delete " + deckNumber);
		}
		for (int i = deckNumber; i < DecksNumberMax-1; i++) {
			Debug.Log("move deck " + i + "from " + (i+1));
			CopyDeck (i, i + 1);
		}
		MyDecks [DecksNumberMax - 1].DeckName = "empty";
		SaveDecksList ();
	}

	/*private void SaveDecksListLocal(DeckFromDeckBase[] Decks) {
		BinaryFormatter BFormatter = new BinaryFormatter ();
		FileStream file = File.Create (Application.persistentDataPath + "/gamedata.dat");

		DeckData[] DecksData = new DeckData[DecksNumberMax];
		for (int i = 0; i < DecksNumberMax; i++) {
			DecksData [i] = new DeckData ();
		};
		DecksNumber = 0;
		for (int i = 0; i < DecksNumberMax-1; i++) {
			if (!(Decks [i].DeckName.Equals ("empty")))
				DecksNumber++;
		}
		for (int i = 0; i < Decks.Length; i++) {
			Debug.Log ("Serialize deck name " + Decks [i].DeckName + " with hero " + Decks [i].HeroName);
			DecksData [i].file_version = CurrentFileVersion;
			DecksData [i].deck_name = Decks [i].DeckName;
			DecksData [i].hero_name = Decks [i].HeroName;
			for (int j = 0; j < Deck.deckSize; j++) {
				DecksData [i].card_names[j] = Decks [i].CardsNames[j];
			}
			DecksData [i].decks_number = DecksNumber;
		}
		BFormatter.Serialize (file, DecksData);
		file.Close ();
	}*/

	public void SaveDecksList() {//to server
		DecksNumber = 0;
		waitForDataScreen (true);
		DataSaved = true;
		Debug.Log ("Saving decks to server");
		for (int i = 0; i < DecksNumberMax-1; i++) {
			if (!(MyDecks [i].DeckName.Equals ("empty"))) {
				DecksNumber++;
			}
		}
		for (int i = 0; i < DecksNumber; i++) {
			if (!MyDecks [i].DeckName.Equals ("empty")) {
				UpdateUserDataRequest request = new UpdateUserDataRequest ();
				request.Data = new Dictionary<string, string> ();
				string cardsInDeckName = "";

				request.Data.Add ("Deck" + i + "Name", MyDecks [i].DeckName);
				request.Data.Add ("Deck" + i + "Hero", MyDecks [i].HeroName);
				for (int j = 0; j < Deck.deckSize; j++) {
					int cardID = CardsBaseComponent.GetIdByName (MyDecks [i].CardsNames [j]);
					if (cardID != -1) {
						cardsInDeckName += cardID.ToString () + separators [0];
					} else {
						Debug.LogWarning ("Cannot find card :" + MyDecks [i].CardsNames [j]);
					}
				}
				request.Data.Add ("Deck" + i + "Cards", cardsInDeckName);
				//Debug.Log ("save deck cards: " + cardsInDeckName);
				PlayFabClientAPI.UpdateUserData (request, PlayerDataSaved, OnPlayFabError);
			}
		}
		UpdateUserDataRequest req = new UpdateUserDataRequest ();
		req.Data = new Dictionary<string, string> ();
		req.Data.Add ("DecksNumber", DecksNumber.ToString());
		PlayFabClientAPI.UpdateUserData (req, PlayerDataSavedLast, OnPlayFabError);
		DataSaved = true;
	}

	private IEnumerator LoadDecksFromServerTask(GetUserDataResult result) {
		while (CardsBaseComponent.DataLoaded == false) {
			yield return new WaitForSeconds (0.1f);
		}
		Debug.Log ("Player data loaded.");
		if (result.Data.ContainsKey ("DecksNumber")) {
			DecksNumber = int.Parse (result.Data ["DecksNumber"].Value);
			for (int i = 0; i < DecksNumber; i++) {
				string cardsAll = result.Data ["Deck" + i + "Cards"].Value;
				string[] cards = cardsAll.Split (separators, StringSplitOptions.RemoveEmptyEntries);
				MyDecks [i].DeckName = result.Data ["Deck" + i + "Name"].Value;
				MyDecks [i].HeroName = result.Data ["Deck" + i + "Hero"].Value;

				for (int j = 0; j < Deck.deckSize; j++) {
					string cardname = "[empty]";
					if (j < cards.Length) {
						cardname = cards [j];
					}
					if (CardsBaseComponent.FindCardDescByName (cardname) == null) {
						MyDecks [i].CardsNames [j] = "";
					} else {
						MyDecks [i].CardsNames [j] = cardname;
					}
				}
			}
			for (int i = 0; i < DecksNumber; i++) {
				if (MyDecks [i].DeckName.Equals ("empty")) {
					DeleteDeck (i);
				}
			}
		} else {
			DecksNumber = 0;
		}
		//Premade decks now be available always
		/*if (!result.Data.ContainsKey ("PremadeDecksLoaded")) {
			Debug.Log ("Load premade decks");
			for(int i = 0; i < PremadeDecks.Length; i++) {
				MyDecks [DecksNumber].DeckName = PremadeDecks[i].DeckName;
				MyDecks [DecksNumber].HeroName = PremadeDecks[i].HeroName;
				for (int j = 0; j < Deck.deckSize; j++) {
					if (CardsBaseComponent.FindCardDescByName (PremadeDecks[i].CardsNames [j]) == null) {
						MyDecks [DecksNumber].CardsNames [j] = "";
					} else {
						MyDecks [DecksNumber].CardsNames [j] = PremadeDecks[i].CardsNames [j];
					}
				}
				DecksNumber++;
			}
			UpdateUserDataRequest req = new UpdateUserDataRequest ();
			req.Data = new Dictionary<string, string> ();
			req.Data.Add ("PremadeDecksLoaded", "1");
			PlayFabClientAPI.UpdateUserData (req, PlayerDataSaved, OnPlayFabError);
			SaveDecksList ();
		}*/
		if (!result.Data.ContainsKey (LocalPlayer.TutorialFlag)) {
			StartCoroutine (WaitForDataAndStartTutorial ());
		}
		if (result.Data.ContainsKey (MyNetworkManager.RoomNameProperty)) {
			if ((result.Data.ContainsKey (GamePlayCore.CurrentDeckProperty)) &&
			    (result.Data.ContainsKey (MyNetworkManager.GameModeProperty))) {
				string cardsAll = result.Data [GamePlayCore.CurrentDeckProperty].Value;
				string[] cards = cardsAll.Split (separators, StringSplitOptions.RemoveEmptyEntries);
				FromPrevGame.DeckName = result.Data [GamePlayCore.CurrentDeckNameProperty].Value;
				FromPrevGame.HeroName = result.Data [GamePlayCore.CurrentDeckHeroProperty].Value;

				for (int j = 0; j < Deck.deckSize; j++) {
					string cardname = "[empty]";
					if (j < cards.Length) {
						cardname = cards [j];
					}
					if (CardsBaseComponent.FindCardDescByName (cardname) == null) {
						FromPrevGame.CardsNames [j] = "";
					} else {
						FromPrevGame.CardsNames [j] = cardname;
					}
				}
				StartCoroutine (WaitForDataAndBackToGame (
					result.Data [MyNetworkManager.RoomNameProperty].Value,
					result.Data [MyNetworkManager.GameModeProperty].Value));
			} else {
				Debug.LogWarning ("Cannot find previous game data");
				waitForDataScreen (false);
			}
		} else {
			Debug.LogWarning ("Cannot find previous game name");
			waitForDataScreen (false);
		}
		//waitForDataScreen (false);
		DataLoaded = true;
	}

	private void LoadDecksFromServer(GetUserDataResult result) {
		StartCoroutine (LoadDecksFromServerTask (result));
	}

	public void LoadDecksList() {
		Debug.Log ("Loading decks to server");
		DataLoaded = false;
		waitForDataScreen (true);
		GetUserDataRequest request = new GetUserDataRequest ();
		PlayFabClientAPI.GetUserData (request, LoadDecksFromServer, OnPlayFabError);
	}

	/*private void LoadDecksListLocal(DeckFromDeckBase[] Decks) {
		if (File.Exists (Application.persistentDataPath + "/gamedata.dat")) {
			bool fileOK = false;
			BinaryFormatter BFormatter = new BinaryFormatter();
			FileStream file = File.Open(Application.persistentDataPath + "/gamedata.dat", FileMode.Open);
			if (file.Length > 0) {
				DeckData[] DecksData = (DeckData[])BFormatter.Deserialize (file);

				for (int i = 0; i < Decks.Length; i++) {
					string ver = DecksData [i].file_version;
					if (CurrentFileVersion.Equals (ver)) {
						fileOK = true;
					} else {
						fileOK = false;
						break;
					}
					Decks [i].DeckName = DecksData [i].deck_name;
					Decks [i].HeroName = DecksData [i].hero_name;
					for (int j = 0; j < Deck.deckSize; j++) {
						if (CardsBaseComponent.FindCardDescByName (DecksData [i].card_names [j]) == null) {
							Decks [i].CardsNames [j] = "";
						} else {
							Decks [i].CardsNames [j] = DecksData [i].card_names [j];
						}
					}
					DecksNumber = DecksData [i].decks_number;
				}
			}
			file.Close ();
			if (!fileOK) {
				File.Delete (Application.persistentDataPath + "/gamedata.dat");
			}
		}
	}*/

	private IEnumerator WaitForDataAndStartTutorial() {
		MyNetworkManager netManager = GameObject.Find ("NetworkManager").GetComponent<MyNetworkManager> ();
		if (netManager) {
			while (DataSaved) {
				yield return new WaitForSeconds (0.5f);
			}
			netManager.SetGameModeTutorial ();
			netManager.StartGame ();
		}
		yield return null;
	}

	private IEnumerator WaitForDataAndBackToGame(string RoomName, string gameMode) {
		MyNetworkManager netManager = GameObject.Find ("NetworkManager").GetComponent<MyNetworkManager> ();
		DeckTableController deckTable = GameObject.Find ("DeckSelect-Canvas").GetComponent<DeckTableController> ();
		if (netManager != null) {
			while (DataSaved) {
				yield return new WaitForSeconds (0.5f);
			}

			netManager.GameMode = (MyNetworkManager.gameModeEnum)Enum.Parse(typeof(MyNetworkManager.gameModeEnum), gameMode);
			GameObject deck = deckTable.SpawnDeck (FromPrevGame.DeckName,
				                  FromPrevGame.HeroName, FromPrevGame.CardsNames);
			//deck.GetComponent<Deck> ().SpawnReverses (Deck.deckSize);
			deck.transform.localScale = new Vector3 (0, 0, 0);
			netManager.PrevRoomName = RoomName;
			netManager.MainMenuComponent.SetupPlayButtonReconnect ();
			netManager.SetDeckToPlay (deck);
			if (netManager.AutoRejoin) {
				netManager.RejoinGame ();
			}
		}
		yield return null;
	}

	private void waitForDataScreen(bool show) {
		if (WaitForDataScreen != null) {
			WaitForDataScreen.enabled = show;
		} else {
			if (GameObject.Find ("WaitForDataImage-Canvas") != null) {
				WaitForDataScreen = GameObject.Find ("WaitForDataImage-Canvas").GetComponent<Canvas> ();
			}
			if (WaitForDataScreen != null) {
				WaitForDataScreen.enabled = show;
			}
		}
	}

	private void PlayerDataSaved(UpdateUserDataResult result)
	{
		Debug.Log ("PLayer Data saving.");
		//waitForDataScreen (false);
	}

	private void PlayerDataSavedLast(UpdateUserDataResult result)
	{
		Debug.Log ("Player Data saved.");
		DataSaved = false;
		waitForDataScreen (false);
	}

	private void OnPlayFabError(PlayFabError error)
	{
		Debug.LogWarning ("Got an error: " + error.ErrorMessage);
		waitForDataScreen (false);
	}
}

/*[Serializable] //use to save decks localy
class DeckData
{
	public string file_version;
	public string deck_name;
	public string hero_name;
	public string[] card_names = new string[Deck.deckSize];
	public int decks_number;
}*/
