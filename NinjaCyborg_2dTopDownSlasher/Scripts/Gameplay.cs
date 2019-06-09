using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using TMPro;

public class Gameplay : MonoBehaviour {
	[System.Serializable]
	public class PlayerInstance {
		public string Name;
		public int ControllID;
		public Color Color = Color.white;
		public PlayerHUD HUD;
		public PlayerScore ScoreUI;

		public bool Joined;
		public int Score;
		public SNCharacter PlayerCharacter;
		public SNUserController User;
	}

	[System.Serializable]
	public class SpawnPoint {
		public Transform point;
		public Vector2 startDirection;
	}

	public List<PlayerInstance> PlayerInstances = new List <PlayerInstance> ();
	public List<SpawnPoint> SpawnPoints = new List <SpawnPoint> ();
	public GameObject PlayerPrefab;

	public Canvas ScoreCanvas;
	public Canvas InfoCanvas;
	public TextMeshProUGUI GameStateDescText;

	public CameraFollow cameraFollow;

	public bool RoundStarted = false;

	public int RoundsCount;
	private int currentRound;

	//private Player Input;
	// Use this for initialization
	void Start () {
		//Input = ReInput.players.GetSystemPlayer ();
		ScoreCanvas.enabled = false;
		Application.targetFrameRate = 60;
		StartRound ();
		//Time.timeScale = 0.2f;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void CheckGameCondition() {
		int alive = 0;
		foreach (PlayerInstance player in PlayerInstances) {
			if (player.PlayerCharacter != null && !player.PlayerCharacter.dead) {
				alive++;
			}
		}
		if (alive == 1) {
			EndRound ();
		}
	}

	private void InstatiatePlayers() {
		for (int k = 0; k < SpawnPoints.Count; k++) {
			SpawnPoint temp = SpawnPoints [k];
			int randIdx = Random.Range (0, SpawnPoints.Count);
			SpawnPoints [k] = SpawnPoints [randIdx];
			SpawnPoints [randIdx] = temp;
		}
		for(int i = 0; i < PlayerInstances.Count; i++) {
			if (PlayerInstances [i].Joined) {
				GameObject gob = Instantiate (PlayerPrefab, SpawnPoints [i].point.transform.position, Quaternion.identity);
				SNCharacter player = gob.transform.GetChild (0).GetComponent<SNCharacter> ();
				SNCharacter snChar = gob.GetComponent<SNCharacter> ();
				if (player != null) {
					SNUserController user = gob.GetComponent<SNUserController> ();
					//player.SetCharacterColor (PlayerInstances [i].Color);
					user.Input = ReInput.players.GetPlayer (PlayerInstances [i].ControllID);
					//user.InputID = PlayerInstances [i].ControllID;
					player.movementController.lastInputMove = SpawnPoints [i].startDirection;
					//player.gplay = this;
					//player.HUD = PlayerInstances [i].HUD;
					//player.HUD.InitHUD (player, PlayerInstances [i].Color);
					//player.HealthBarComponent.MaxHealth = player.Health;
					PlayerInstances [i].ScoreUI.PlayerName.text = PlayerInstances [i].Name;
					PlayerInstances [i].ScoreUI.PlayerName.color = PlayerInstances [i].Color;
					PlayerInstances [i].PlayerCharacter = player;
					PlayerInstances [i].User = user;
					//CamFollow.Targets.Add (player);
				}
				if (snChar && snChar.characterTransform) {
					cameraFollow.Targets.Add (snChar.characterTransform);
				}
			}
		}
	}

	private void StartRound() {
		StartCoroutine (StartRoundTask ());
	}

	private IEnumerator StartRoundTask() {
		InfoCanvas.enabled = true;
		GameStateDescText.text = "READY";
		InstatiatePlayers ();
		yield return new WaitForSeconds (2);
		GameStateDescText.text = "FIGHT!";
		yield return new WaitForSeconds (1);
		RoundStarted = true;
		InfoCanvas.enabled = false;
		foreach (PlayerInstance player in PlayerInstances) {
			if ((player.User != null) && (player.User.Input != null)) {
				player.User.controllable = true;
			}
		}
	}

	private void EndRound() {
		StartCoroutine (EndRoundTask ());
	}

	private IEnumerator EndRoundTask() {
		bool gameEnd = false;
		ScoreCanvas.enabled = true;
		RoundStarted = false;
		foreach (PlayerInstance player in PlayerInstances) {
			if (player.PlayerCharacter != null) {
				player.User.controllable = false;
				//player.Score += player.PlayerCharacter.KillCount;
				//player.PlayerCharacter.KillCount = 0;
				player.ScoreUI.UpdateScore (player.Score);
				if (player.Score >= 19) {
					gameEnd = true;
				}
			}
		}
		yield return new WaitForSeconds (5);
		cameraFollow.Targets.Clear ();
		foreach (PlayerInstance player in PlayerInstances) {
			if (player.PlayerCharacter != null) {
				Destroy (player.PlayerCharacter.movementController.gameObject);
			}
		}
		ScoreCanvas.enabled = false;
		currentRound++;
		if (!gameEnd) {
			StartRound ();
		} else {
			Debug.LogWarning ("GAME END");
		}
	}
}
