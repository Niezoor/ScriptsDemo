using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using PlayFab.ClientModels;
using PlayFab;


public class GamePlay : MonoBehaviour
{
	public static int IndexMAX = 23;
	[System.Serializable]
	public struct sBoard {
		public GameObject BoardPiece;
		public GameObject Pawn;
		public int a;
		public int b;
		public int c;
	};
	public sBoard[] Board = new sBoard[IndexMAX];

	private int aMax = 3;
	private int aMin = -3;
	private int bMax = 4;
	public static int BoardRangeMax = 6;

	private int boardIDCurrent = 0;//master player start at id 0, second player at id 1000
	private int boardEffectIDCurrent = -1000;//enemy effects start at id -1000

	public enum attackDirections {
		UpLeft, Up, UpRight, DownRight, Down, DownLeft, Other
	};

	public enum TargetSettings {
		All					= 0,
		WithFriendlyPawn	= 1<<0,
		WithEnemyPawn		= 1<<1,	
		WithFriendlyHero	= 1<<2,
		WithEnemyHero		= 1<<3,
		Recolor				= 1<<4,
		WithPawnsLock		= 1<<5,
		WithOwner			= 1<<6,
		MustBeCalled		= 1<<7
	};

	public WinScreen WinScreenComponent;

	public GameObject PlayerPawnPre;

	public GameObject AIEnemy;
	private AI AIComponent;

	public GameObject TutorialComponentPrefab;
	private TutorialGameplay TutorialComponent;
	public bool TutorialMode = false;

	public NewTurnButton EndTurnButton;

	public GameObject myDeckPosition;
	public GameObject enemyDeckPosition;
	public Transform myEndDeckPosition;
	public Transform enemyEndDeckPosition;

	public Transform myHeroPlace;
	public Transform enemyHeroPlace;

	public Transform CardHighlightPosLeft;
	public Transform CardHighlightPosRight;
	private GameObject cardHighlight;
	private GameObject cardWeaponHighlight;

	public GameObject RPCReceiver;
	private PhotonView myPView;
	public GameObject CardReversePrefab;

	public Hand HandComp; //tmp
	public Hand EnemyHandComp; //tmp
	public StartHand StartHandComponent;
	public CardsBase CardsComp;
	public HeroesBase HeroesComp;
	public GameObject myHero;
	public GameObject myDeck;
	public GameObject enemyDeck;
	public GameObject enemyHero;
	public GameObject deckPrefab;
	public GameObject testDeckPrefab;//for development
	public GameObject CardsCompPrefab;//for development
	public GameObject HeroesCompPrefab;//for development
	public TextMeshPro ActualManaStateText;
	private Vector3 origScaleManaText = new Vector3 (0, 0, 0);
	public AnimationCurve deckAnimCurve;
	//private Vector3 deckScale = new Vector3(4471.853f, 8101.316f, 1100.103f);
	public bool YouStarted = true;
	public float upEnemyCard = 200f;
	public Color enemyColor;//tmp for development
	public Color BoardPieceNormalColor;
	public Color BoardPieceInactiveColor;
	private bool boardPieceNormalColorIsSet = true;
	/* Particle beam */
	public GameObject MarkBeam;
	private GameObject MarkBeamOb;
	private GameObject MarkBeamEndOb;
	public GameObject AttackMarkBeamOb;
	public GameObject AttackMarkBeamEndOb;
	public GameObject AttackMarkPrefab;
	//private GameObject AttackMarkOb;

	public Canvas VSCanvas;
	public Canvas MainCanvas;
	public Canvas StartHandMenuCanvas;
	public Text   StartHandNotificationText;
	public Animator NewTurnAnimation;

	public Transform TargetDescriptionNotificationPanel;
	public Text TargetDescriptionNotificationText;
	public Text StartHandTimeStatusText;
	private Vector3 TargetDescriptionNotificationPanelOriginalPos;

	public Pawn WeaponToPlay;
	private Pawn EnemyWeaponToPlay;
	private int LastEffectIndex = -1;

	public bool myTurn = false;
	public bool onlineMode = true;
	private bool startSequnceStarted = false;
	private bool enemyMarkBeamShowed = false;

	public bool IAmReady = false;
	public bool IAmConnected = false;
	public bool EnemyIsReady = false;
	public bool EnemyConnected = false;

	public int ManaNextTurn;
	//private int PawnMovesCurrent = 0;

	public delegate void TargetCallback (int TargetBoardFieId);
	private TargetCallback TargetSelectCallback = null;
	private bool TargetSelectCallbackMustBeCalled = false;

	public Animator WarningAnimationController;
	public Text WarningText;

	public OnlinePlayersName OnlinePlayersNameComponent;
	public GameLog GameLogComponent;
	public GamePlayCore GameplayCoreComponent;
	public GameObject NetManager;
	public MyNetworkManager MyNetManager;
	public GamePlayActionStack ActionStackComponent { get { return GetComponent<GamePlayActionStack> (); } }
	private GamePlayTurnManager MyTurnManager { get { return GetComponent<GamePlayTurnManager> (); } }

	private int InitCardDeckNumber = Deck.deckSize - 1;

	[System.Serializable]
	public struct pawnListClass
	{
		public Pawn pawn;
		public int pawnID;
	};

	public List<pawnListClass> GamePawnsList = new List<pawnListClass> ();

	[Header("Game settings")]
	public int Mana = 1;
	public int ManaMax = 10;
	//public int PawnMovesMax = 1;
	public int startHandCardsNumber = 3;
	[Tooltip("Time in seconds")]
	public int startHandCardsTimeout = 15;
	[Tooltip("Time in seconds")]
	public int TurnDuration = 90;
	[Tooltip("Time in seconds")]
	public int TurnDurationAlarmStartAt = 60;
	[HideInInspector]
	public int TurnRemainningTime = -1;

	public bool gameEnd = false;
	public bool skipAnimations = false;
	private Coroutine turnTimer = null;
	private Coroutine startHandTimer = null;

	[Header("Game statistics")]
	public int DamageDone = 0;
	public int MovesDone = 0;

	[Header("Game audio")]
	public AudioClip CountDownAudioClip;
	public AudioClip GiveTurnAudioClip;
	public AudioSource GameplayAudio;

	[Header("Error handling")]
	public Canvas ErrorCanvas;
	public Text ErrorText;

	public List<int> TargetsList = new List<int> ();

	[Header("Component settings")]
	public bool UsePhotonEventsIsteadOfRPC = false;
	public bool BoardClientsForceSync = false;
	public bool BoardClientsForceSyncPositions = false;

	/* Game Rules:
   	* 1 Turn rules:
 	* 1.1 On turn player can move his pawn once, cost 1 action point
 	* 1.2 On turn player can attack once by every pawn.
 	* 1.3 On turn player can play and move as much cards/pawns as is able to pay they 'action points' cost.
    * 1.4 Each turn the max player action points value increased by 1.
    * 1.5 At the begging of the turn action points regenerates to the max value.
    * 1.6 At begining of match every player draw 3 card (initial draw).
    * 1.7 At initial draw player can schoose card to redraw (one by one).
    *  
    * 2 Attack rules:
    * 2.1 Pawn must can attack in set direction.
    * 2.2 If Pawn attack and enemy pawn can attack in pawn direction,
    *     than pawns gives damage to each other.
    * 2.3 If pawn attack with close range (melee) distance cannot be grater than 1.
    * 2.4 If pawn attack with long range his damage can be blocked.
    * 2.5 If pawn attack with long range and enemy pawn attack with close range,
    *     and the distance between them is grater than 1,
    *     pawn does not received damage from enemy pawn.
    * 
    * 3 Heroes rules:
    * 3.1 Heroes cannot move.
    * 3.2 Heroes can attack.
    * 3.3 If enemy heroes die, you will win.
    *  
    * 4 Weapon rules:
    * 4.1 You can apply weapon to any pawn.
    * 4.2 Weapon stay on board after pawn death (not supported).
    */

	void Awake () {
		if (GameObject.Find ("CardsBase")) {
			CardsComp = GameObject.Find ("CardsBase").GetComponent<CardsBase> ();
		} else {
			GameObject GameOb = Instantiate (CardsCompPrefab);
			CardsComp = GameOb.GetComponent<CardsBase> ();
		}
		ManaNextTurn = Mana;
		EndTurnButton.interactable = false;
		UpdateManaState ();
		//MyTurnManager.StartTurnManager (GetComponent<GamePlay>());
		GameplayAudio = this.GetComponent<AudioSource>();
	}

	void Start () {
		if (!YouStarted) {
			SetAsSecondPlayer ();
		}
		if (RPCReceiver) {
			myPView = RPCReceiver.GetComponent<PhotonView> ();
		}
		if (GameObject.Find ("HeroesBase")) {
			HeroesComp = GameObject.Find ("HeroesBase").GetComponent<HeroesBase>();
		} else {
			GameObject GameOb = Instantiate (HeroesCompPrefab);
			HeroesComp = GameOb.GetComponent<HeroesBase> ();
		}
		enemyDeck = (GameObject)Instantiate (deckPrefab);
		enemyDeck.transform.localPosition = new Vector3 (0, 0, 0);
		enemyHero = HeroesComp.SpawnNewHero ();
		enemyHero.transform.SetParent(transform, true);
		enemyHero.GetComponent<Pawn>().gamePlayComp = this.GetComponent<GamePlay> ();

		origScaleManaText = ActualManaStateText.transform.localScale;
		TargetDescriptionNotificationPanelOriginalPos = TargetDescriptionNotificationPanel.transform.localPosition;

		NetManager = GameObject.Find ("NetworkManager");

		if (NetManager) {
			onlineMode = true;
			MyNetManager = NetManager.GetComponent<MyNetworkManager> ();
			MyNetManager.UnSetObjectsToDestroy ();
			if (MyNetManager.DeckToPlay != null) {
				myDeck = MyNetManager.DeckToPlay;//NetManager.transform.GetChild (0).gameObject;
				myDeck.transform.SetParent (myDeckPosition.transform, false);
				//myDeck.transform.localScale = deckScale;
				//myDeck.transform.localPosition = new Vector3 (0, 0, 0);
				//myHero = myDeck.transform.GetChild (0).gameObject;
				myHero = myDeck.GetComponent<Deck>().Hero;
				myHero.transform.SetParent(transform, true);
				myHero.GetComponent<SpriteRenderer> ().sortingOrder = 0;
				myHero.GetComponent<Pawn>().gamePlayComp = this.GetComponent<GamePlay> ();
			}

			MyTurnManager.StartTurnManager (GetComponent<GamePlay>());

			if (MyNetManager.GameMode == MyNetworkManager.gameModeEnum.training) {
				onlineMode = false;
				AIComponent = AIEnemy.GetComponent<AI> ();
				AIComponent.AIStartPlay (); //AI spawns his own deck
				Destroy (RPCReceiver); //We dont need it for playing versus AI
			} else if (MyNetManager.GameMode == MyNetworkManager.gameModeEnum.tutorial) {
				onlineMode = false;
				TutorialMode = true;
				if (TutorialComponentPrefab != null) {
					GameObject tutorialObject = Instantiate (TutorialComponentPrefab, Camera.main.transform);
					tutorialObject.GetComponent<Canvas> ().worldCamera = Camera.main;
					TutorialComponent = tutorialObject.GetComponent<TutorialGameplay> ();
				}
				Destroy (RPCReceiver); //We dont need it for playing versus AI
			} else {
				Destroy (AIEnemy); //We dont need it for multiplayer
			}
			if (TutorialMode) {
				Destroy (myDeck);
				Destroy (myHero);
				myDeck = (GameObject)Instantiate (TutorialComponent.MyTutorialDeckPrefab);
				//myHero = myDeck.transform.GetChild (0).gameObject;
				myHero = myDeck.GetComponent<Deck>().Hero;
				myHero.transform.SetParent(transform, true);
				myHero.GetComponent<Pawn>().gamePlayComp = this.GetComponent<GamePlay> ();
				/*if (enemyHero) {
					enemyDeck.GetComponent<Deck> ().SetHero (enemyHero);
				}*/
				TutorialComponent.TutorialInit (GetComponent<GamePlay>());
			}
		} else {
			//for development only
			onlineMode = false;
			if (TutorialMode) {
				if (TutorialComponentPrefab != null) {
					GameObject tutorialObject = Instantiate (TutorialComponentPrefab, Camera.main.transform);
					tutorialObject.GetComponent<Canvas> ().worldCamera = Camera.main;
					TutorialComponent = tutorialObject.GetComponent<TutorialGameplay> ();
				}
				myDeck = (GameObject)Instantiate (TutorialComponent.MyTutorialDeckPrefab);
				//myHero = myDeck.transform.GetChild (0).gameObject;
				myHero = myDeck.GetComponent<Deck>().Hero;
				myHero.transform.SetParent(transform, true);
				myHero.GetComponent<Pawn>().gamePlayComp = this.GetComponent<GamePlay> ();
				/*if (enemyHero) {
					enemyDeck.GetComponent<Deck> ().SetHero (enemyHero);
				}*/
				TutorialComponent.TutorialInit (GetComponent<GamePlay>());
			} else {
				myDeck = (GameObject)Instantiate (testDeckPrefab);
				//myHero = myDeck.transform.GetChild (0).gameObject;
				myHero = myDeck.GetComponent<Deck>().Hero;
				myHero.transform.SetParent(transform, true);
				myHero.GetComponent<Pawn>().gamePlayComp = this.GetComponent<GamePlay> ();
				AIComponent = AIEnemy.GetComponent<AI> ();
				AIComponent.AIStartPlay (); //AI spawns his own deck
			}
			/*if (enemyHero) {
				enemyDeck.GetComponent<Deck> ().SetHero (enemyHero);
			}*/
		}
		myTurn = false;
		BoardInit();
		if (onlineMode) {
			
			if (PhotonNetwork.isMasterClient) {
				if (UsePhotonEventsIsteadOfRPC) {
					if (!MyNetManager.ReJoining) {
						MyTurnManager.GameplaySendMove (
							GamePlayActionStack.ActionTypeEnum.firstPlayer,
							0, "", 0, 0, 0, 0);
					}
				} else {
					myPView.RPC ("YouAreSecond", PhotonTargets.Others);
				}
			}
			if (UsePhotonEventsIsteadOfRPC) {
				if (!MyNetManager.ReJoining) {
					MyTurnManager.GameplaySendMove (
						GamePlayActionStack.ActionTypeEnum.setHero, myHero.GetComponent<Hero> ().Name);
				}
			} else {
				myPView.RPC ("RPCSetEnemyHero", PhotonTargets.Others, myHero.GetComponent<Hero> ().Name);
			}
			/*if (enemyHero) {
				enemyDeck.GetComponent<Deck> ().SetHero (enemyHero);
			}*/
		}
		if (NetManager != null) {
			if (!MyNetManager.ReJoining) {
				myDeck.GetComponent<Deck> ().ShuffleDeck ();
			} else {
				skipAnimations = true;
				IAmReady = true;
				EnemyIsReady = true;
			}
		} else {
			myDeck.GetComponent<Deck> ().ShuffleDeck ();
		}
		IAmConnected = true;
		StartSequence ();

		if (AIComponent != null) {
			AIComponent.AIInitDraw ();
		}
		//SetAsSecondPlayer ();
	}

	void Update () {
		
	}

	public void SetEnemyHero(string name) {
		/*enemyHero = HeroesComp.GetHeroByName (name);
		if (enemyHero == null) {
			Debug.LogError ("Cannot find hero: " + name);
		} else {
			enemyHero = enemyDeck.GetComponent<Deck> ().SetHero (enemyHero, true);
		}*/
		if (NetManager != null) {
			MyNetManager.HideSceneLoadScreen ();
		}
		HeroesComp.SetupHero (enemyHero, name);
		enemyHero.GetComponent<SpriteRenderer> ().sortingOrder = 0;
		EnemyConnected = true;
		StartSequence ();
	}

	public void SetMyHero(string name) {
		/*myHero = HeroesComp.GetHeroByName (name);
		if (myHero == null) {
			Debug.LogError ("Cannot find hero: " + name);
		} else {
			myHero = myDeck.GetComponent<Deck> ().SetHero (myHero, true);
		}*/
		Debug.LogWarning ("SetMyHero setup");
		HeroesComp.SetupHero (myHero, name);
		myHero.GetComponent<SpriteRenderer> ().sortingOrder = 0;
		Debug.LogWarning ("Enemy hero pos" + enemyHero.transform.localPosition.y);
	}

	#region START_SEQUENCE
	private void StartSequence() {
		if (IAmConnected && EnemyConnected) {
			if (skipAnimations) {
				StartSeq1 ();
				StartSeq2 (true);
				Debug.LogWarning ("Enemy hero pos" + enemyHero.transform.localPosition.y);
				KeepStartHand ();
			} else {
				StartCoroutine (ShowStartSequnce ());
			}
		}
		Debug.LogWarning ("Enemy hero pos" + enemyHero.transform.localPosition.y);
	}

	private void StartSeq1() {
		Debug.Log ("Match started");
		float heroStartScale = 0.3f;
		VSCanvas.enabled = true;
		VSCanvas.GetComponent<Animator> ().SetTrigger ("Start");
		myDeck.GetComponent<SmothTransform> ().smoothTransformScaleRunning = false;
		enemyDeck.GetComponent<SmothTransform> ().smoothTransformScaleRunning = false;
		enemyDeck.transform.localPosition =  new Vector3 (0, 0, 0);
		enemyDeck.transform.localRotation = Quaternion.identity;
		enemyDeck.GetComponent<Deck> ().SpawnReverses (Deck.deckSize);
		myDeck.transform.SetParent (myEndDeckPosition.transform, false);
		enemyDeck.transform.SetParent (enemyEndDeckPosition.transform, false);
		myDeck.transform.localPosition = new Vector3 (0, 0, 0);
		enemyDeck.transform.localPosition =  new Vector3 (0, 0, 0);
		myDeck.transform.localRotation = Quaternion.identity;
		enemyDeck.transform.localRotation = Quaternion.identity;
		myDeck.transform.localScale = new Vector3 (0.8f, 0.8f, 0.8f);
		enemyDeck.transform.localScale = new Vector3 (0.8f, 0.8f, 0.8f);

		myHero.GetComponent<SpriteRenderer> ().sortingOrder = 11;
		enemyHero.GetComponent<SpriteRenderer>().sortingOrder = 11;

		myHero.transform.SetParent (myDeckPosition.transform);
		myHero.transform.localPosition = new Vector3 (0, 0, 0);
		myHero.transform.localRotation = new Quaternion (0, 0, 0, 0);
		myHero.transform.localScale = new Vector3 (heroStartScale,heroStartScale,heroStartScale);
		enemyHero.transform.SetParent (enemyDeckPosition.transform);
		enemyHero.transform.localPosition = new Vector3 (0, 0, 0);
		enemyHero.transform.localRotation = new Quaternion (0, 0, 0, 0);
		enemyHero.transform.localScale = new Vector3 (heroStartScale, heroStartScale, heroStartScale);
	}

	private void StartSeq2(bool skipAnim = false) {
		VSCanvas.GetComponent<Animator> ().SetTrigger ("End");
		myHero.transform.SetParent(transform, true);
		enemyHero.transform.SetParent(transform, true);
		Vector3 myNewPos = myHero.transform.localPosition;
		Vector3 enemyNewPos = enemyHero.transform.localPosition;
		myNewPos.z = -5;
		enemyNewPos.z = -5;
		if (!skipAnim) {
			myHero.GetComponent<SmothTransform> ().SmothTransformTo (myNewPos, 5);
			enemyHero.GetComponent<SmothTransform> ().SmothTransformTo (enemyNewPos, 5);
		} else {
			myHero.transform.localPosition = myNewPos;
			enemyHero.transform.localPosition = enemyNewPos;
		}

		myHero.GetComponent<SpriteRenderer> ().sortingOrder = 0;
		startSequnceStarted = true;
		PutHeroOnBoard(myHero, YouStarted, YouStarted ? -1 : -2, skipAnim);
		PutHeroOnBoard(enemyHero, !YouStarted, YouStarted ? -2 : -1, skipAnim);
		Debug.LogWarning ("Enemy hero pos" + enemyHero.transform.localPosition.y);
		Pawn heroPawn = enemyHero.GetComponent<Pawn> ();
		heroPawn.Friendly = false;
		heroPawn.RegisterDeathCallback (WinGame);
		enemyHero.GetComponent<SpriteRenderer>().sortingOrder = 0;
		heroPawn = myHero.GetComponent<Pawn> ();
		heroPawn.Friendly = true;
		heroPawn.isFirstPlay = false;
		heroPawn.EnablePawn ();
		heroPawn.DisablePawn ();
		heroPawn.SetAttackOnlyMode ();//Rule 3.1, 3.2
		heroPawn.RegisterDeathCallback(DefeatGame);
		if (TutorialMode) {
			//HandComp.SetupStartHand ();
			TutorialComponent.StartTutorial ();
		}
		Debug.LogWarning ("Enemy hero pos" + enemyHero.transform.localPosition.y);
	}

	IEnumerator ShowStartSequnce() {
		while (!EnemyConnected) {
			yield return new WaitForSeconds (0.05f);
		}
		yield return new WaitForSeconds (0.05f);
		StartSeq1 ();
		yield return new WaitForSeconds (3f);
		myHero.transform.SetParent(transform.root, true);
		enemyHero.transform.SetParent(transform.root, true);
		VSCanvas.GetComponent<Animator> ().SetTrigger ("End");
		yield return new WaitForSeconds (1f);
		StartSeq2 ();
		if (NetManager != null) {
			if (!MyNetManager.ReJoining) {
				GameplayCoreComponent.SaveDeck ();
				//HandComp.SetupStartHand ();
				if (TutorialMode) {
					KeepStartHand ();
					StartHandMenuCanvas.enabled = false;
				} else {
					StartHandMenuCanvas.enabled = true;
					StartCoroutine (InitDraw (startHandCardsNumber));
				}
			}
		} else {
			if (TutorialMode) {
				KeepStartHand ();
				StartHandMenuCanvas.enabled = false;
			} else {
				StartHandMenuCanvas.enabled = true;
				StartCoroutine (InitDraw (startHandCardsNumber));
			}
		}
		/* for tests only
		yield return new WaitForSeconds (2f);
		GameplayCoreComponent.SaveGameState ();
		yield return new WaitForSeconds (2f);
		GameplayCoreComponent.LoadGameState ();*/
		yield return null;
	}

	private void PutHeroOnBoard(GameObject PlayerPawn, bool switchPos, int pawnID, bool skipAnim) {
		int boardPos;
		int pawnRotIndex;

		Debug.LogWarning ("PutHeroOnBoard");

		if (switchPos) {
			boardPos = 0;
			pawnRotIndex = 0;
		} else {
			boardPos = IndexMAX - 1;
			pawnRotIndex = 3;
		}
		PutPawnOnPosisionImpl(PlayerPawn, boardPos);

		pawnListClass panwToAdd = new pawnListClass ();
		panwToAdd.pawn = PlayerPawn.GetComponent<Pawn>();
		panwToAdd.pawnID = pawnID;
		GamePawnsList.Add(panwToAdd);
		//PlayerPawn.GetComponent<Pawn> ().SetPawnRotation (pawnRotIndex);
		PlayerPawn.GetComponent<Pawn> ().pawnBoardID = pawnID;
		float scale = PlayerPawn.GetComponent<Pawn>().onPlayScale;
		Transform startPos = GetBoardPosByIndex (boardPos);
		Vector3 newPos = new Vector3(0,0,-0.07f);
		if (PlayerPawn == myHero) {
			Debug.LogWarning ("MY PutHeroOnBoard skip " + PlayerPawn.transform.localPosition.y);
			PlayerPawn.transform.SetParent (myHeroPlace);
		} else {
			Debug.LogWarning ("ENEMy PutHeroOnBoard skip " + PlayerPawn.transform.localPosition.y);
			PlayerPawn.transform.SetParent (enemyHeroPlace);
		}
		if (skipAnim) {
			PlayerPawn.GetComponent<SmothTransform> ().smoothTransformPosRunning = false;
			PlayerPawn.GetComponent<SmothTransform> ().smoothTransformScaleRunning = false;
			PlayerPawn.transform.localPosition = newPos;
			PlayerPawn.transform.localRotation = Quaternion.Euler (new Vector3 (0, 0, 0));
			PlayerPawn.transform.localScale = new Vector3 (scale, scale, scale);
			Debug.LogWarning ("PutHeroOnBoard skip " + PlayerPawn.transform.localPosition.y);
		} else {
			Debug.LogWarning ("PutHeroOnBoard no skip");
			PlayerPawn.GetComponent<SmothTransform> ().SmothTransformTo (newPos, Quaternion.Euler (new Vector3 (0, 0, 0)), 3);
		}
		PlayerPawn.GetComponent<Pawn>().boardPosisionIndex = boardPos;
		PlayerPawn.GetComponent<Pawn>().boardPosisionIndexPrev = boardPos;
		PlayerPawn.GetComponent<Pawn>().boardSavedPosisionIndexPrev = boardPos;
		PlayerPawn.GetComponent<Pawn>().boardSavedRotationIndexPrev = pawnRotIndex;
		PlayerPawn.GetComponent<Pawn>().RotationPosIndex = pawnRotIndex;
		if (!skipAnim) {
			PlayerPawn.GetComponent<SmothTransform> ().SmoothScaleTo (new Vector3 (scale, scale, scale), 3);
		}
		PlayerPawn.GetComponent<SmothTransform> ().SmoothTransformDisabled = true;
		//PlayerPawn.transform.localScale = new Vector3 (0.4f, 0.4f, 0.4f);
		//PlayerPawn.transform.localPosition = new Vector3(startPos.localPosition.x , startPos.localPosition.y, -0.08f);
		//PlayerPawn.transform.localRotation = new Quaternion (0, 0, 0, 0);
		if (skipAnim) {
			PutHeroImpl (PlayerPawn, boardPos);
		} else {
			StartCoroutine (PutHeroOnBoardTask (PlayerPawn, boardPos));
		}
		//PlayerPawn.transform.Find ("Desc").transform.localRotation = new Quaternion(0,0,0,0);
		//PlayerPawn.GetComponent<Pawn> ().OnPlayStart ();
		//PlayerPawn.GetComponent<Pawn> ().OnPlayEnd ();
	}

	private void PutHeroImpl(GameObject PlayerPawn, int posIdx) {
		Transform pawnConfig =  PlayerPawn.GetComponent<Pawn> ().PawnConfigPosHandler;
		pawnConfig.transform.SetParent (this.transform);
		pawnConfig.GetComponent<SmothTransform> ().enabled = false;
		Vector3 newPawnPos = Board [posIdx].BoardPiece.transform.localPosition;
		newPawnPos.z = Pawn.PawnPosDown;
		pawnConfig.transform.localPosition = newPawnPos;

		if (YouStarted) {
			if (PlayerPawn == myHero) {
				pawnConfig.transform.localRotation = new Quaternion (0, 0, 0, 0);
			} else {
				pawnConfig.transform.localRotation = new Quaternion (0, 0, 180, 0);
			}
		} else {
			if (PlayerPawn == myHero) {
				pawnConfig.transform.localRotation = new Quaternion (0, 0, 180, 0);
			} else {
				pawnConfig.transform.localRotation = new Quaternion (0, 0, 0, 0);
			}
		}
		PlayerPawn.GetComponent<Pawn> ().ApplyConfig ();
	}

	IEnumerator PutHeroOnBoardTask (GameObject PlayerPawn, int posIdx) {
		while (PlayerPawn.GetComponent<SmothTransform> ().smoothTransformPosRunning == true) {
			yield return new WaitForSeconds (0.1f);
		}

		PutHeroImpl (PlayerPawn, posIdx);
	}
	#endregion

	private void BoardInit() {
		GameObject boardPieceOb;
		int axisA = 0;
		int axisB = 0;
		int axisC = 0;

		for (int i = 0; i < IndexMAX; i++) {
			int index = i + 1;
			boardPieceOb = GameObject.Find("HCG_board_piece" + index);
			Board[i].BoardPiece = boardPieceOb;
			Board[i].a = axisA;
			Board[i].b = axisB;
			Board[i].c = axisC;
			//Debug.Log("Add boad piece (" + axisA + ", " + axisB + ", " + axisC + ") OB:" + boardPieceOb.name);
			axisA--;
			axisB++;
			if ((axisA < aMin) || (axisB > bMax)) {
				axisB = 0;
				axisC++;
				axisA = axisC;
			}
			if (axisA > aMax) {
				axisA--;
				axisB++;
			}
		}
	}

	private GameObject GetRevers(GameObject fromDeck) {
		//GameObject card = null;
		Deck deckComp = fromDeck.GetComponent<Deck> ();
		if (!deckComp.RefercesSpawned) {
			//deckComp.cardsInDeck = 30;
			deckComp.SpawnDeck ();
		}
		if (fromDeck.transform.childCount > 0) {
			GameObject card = fromDeck.transform.GetChild (0).gameObject;
			if (card.GetComponent<Hero> () != null) {
				if (fromDeck == myDeck) {
					//myHero = card;
					//myHero.transform.SetParent (transform, true);
				} else {
					Destroy (card);//destroy hero card, new was created on start
					//enemyHero = card;
					//enemyHero.transform.SetParent (transform, true);
				}
				//card = fromDeck.transform.GetChild (0).gameObject;
			}
		}
		/*if (fromDeck.transform.childCount > 0) {
			card = fromDeck.transform.GetChild (0).gameObject;

		} else if (fromDeck.GetComponent<Deck> ().cardsInDeck > 0) {
			if (fromDeck.GetComponent<Deck> ().cardsInDeck > 1) {
				fromDeck.GetComponent<Deck> ().SpawnReverses (2);
			} else {
				fromDeck.GetComponent<Deck> ().SpawnReverses (1);
			}
			card = fromDeck.transform.GetChild (0).gameObject;
		}*/
		//return card;
		return deckComp.GetRevers();
	}

	public void EnemyCardDraw() {
		GameObject card = GetRevers(enemyDeck);
		if (skipAnimations) {
			AddEnemyCardToHandNoAnim (card);
		} else {
			StartCoroutine (AddEnemyCardToHand (card));
		}
	}

	public void EnemyInitCardDraw() {
		if (skipAnimations) {
			for (int i = 0; i < startHandCardsNumber; i++) {
				EnemyCardDraw ();
			}
		} else {
			StartCoroutine (EnemyInitCardDrawSync ());
		}
	}

	IEnumerator EnemyInitCardDrawSync() {
		while (!startSequnceStarted) {
			yield return new WaitForSeconds (0.05f);//wait for reach the set deck posistion
		}
		for (int i = 0; i < startHandCardsNumber; i++) {
			EnemyCardDraw ();
			yield return new WaitForSeconds (1f);
		}
	}

	public void EnemyRemoveCardFromHand(int handCardIndex) {
		EnemyHandComp.RemoveCardFromHandWithDestroy (handCardIndex);
	}

	private void AddEnemyCardToHandNoAnim(GameObject card) {
		if (card) {
			card.transform.SetParent (enemyDeck.transform);
			if (!EnemyHandComp.AddCardToHand (card))
				Destroy (card);
		}
	}

	IEnumerator AddEnemyCardToHand(GameObject card) {
		while (!startSequnceStarted) {
			yield return new WaitForSeconds (0.05f);//wait for reach the set deck posistion
		}
		yield return new WaitForSeconds (0.5f);
		if (card) {
			card.transform.SetParent (enemyDeck.transform);
			card.transform.localPosition = new Vector3 (0, 0, 0);
			card.transform.localRotation = new Quaternion (0, 0, 0, 0);
			card.GetComponent<SmothTransform> ().SmothTransformTo (new Vector3 (0, upEnemyCard, 0), 5);
			if (card.GetComponent<AudioSource> () != null) {
				card.GetComponent<AudioSource> ().volume = 0.5f;
			}
			card.GetComponent<CardInteraction>().PlayDrawSound();
			yield return new WaitForSeconds (0.5f);
			if (!EnemyHandComp.AddCardToHand (card)) {
				Destroy (card);
			}
		}
	}

	public void EnableHandShowHideMode() {
		HandComp.HandShowHideModeEnabled = true;//check phisical screen size here and scale the board;
	}

	#region START_HAND
	private IEnumerator HandTimerTask() {
		for (int i = startHandCardsTimeout; i >= 0; i--) {
			StartHandTimeStatusText.text = i.ToString ();
			if (!IAmReady && i == 2) {
				GameplayAudio.PlayOneShot (CountDownAudioClip);
			}
			yield return new WaitForSeconds (1);
		}
		StartHandComponent.KeepHand ();
	}

	private void StopHandTimer() {
		if (startHandTimer != null) {
			StopCoroutine (startHandTimer);
			StartHandTimeStatusText.text = "";
		}
	}

	private void StartHandTimer() {
		startHandTimer = StartCoroutine (HandTimerTask());
	}
	#endregion

	public void KeepStartHand() {
		if (!IAmReady) {
			StopHandTimer ();
			IAmReady = true;
			if (onlineMode) {
				if (UsePhotonEventsIsteadOfRPC) {
					if (!MyNetManager.ReJoining) {
						MyTurnManager.GameplaySendMove (GamePlayActionStack.ActionTypeEnum.signalReady, "");
						GameplayCoreComponent.SaveDeck ();
					}
				} else {
					myPView.RPC ("SignalMatchReady", PhotonTargets.Others);
				}
			}
			MatchStart ();
			HandComp.SetupNormalHand ();
			EnableHandShowHideMode ();
			if (!HandComp.UseHandCanvas) {
				HandComp.ShowHand ();
			}
			HandComp.RefreshTouchOfCardsInHand ();
			//StartTurnTimer ();
		}
	}

	public void MatchStart() {
		Debug.Log ("Players ready (me:" + IAmReady + " enemy:" + EnemyIsReady + ")");
		if (IAmReady) {
			if (EnemyIsReady) {
			    StartHandMenuCanvas.enabled = false;
				HandComp.RefreshCardInHand ();
				myTurn = false;
				if (YouStarted) {
					//EndTurnButton.ChangeButtonToMyTurn ();
					//EndTurnButton.interactable = true;
					//myTurn = true;
					if (onlineMode) {
						if (!MyNetManager.ReJoining) {
							if (UsePhotonEventsIsteadOfRPC) {
								MyTurnManager.StartTurn ();
							} else {
								TakeTurn ();
							}
						}
					} else {
						TakeTurn ();
					}
				} else {
					EndTurnButton.ChangeButtonToEnemyTurn();
				}
			} else {
				StartHandNotificationText.text = "Oczekiwanie na przeciwnika";
			}
		}
	}

	IEnumerator DrawNextStartCardTask(GameObject CardToChange, int CardNumber, int HandIndex) {
		//HandComp.RemoveCardFromHand (CardToChange);
		CardToChange.transform.SetParent (myDeck.transform, true);
		Quaternion rot = Quaternion.identity;
		rot.eulerAngles = new Vector3(0, 180, 0);

		GameObject cardRevers = Instantiate (myDeck.GetComponent<Deck> ().CardRevers);
		CardInteraction reversInter = cardRevers.GetComponent<CardInteraction> ();
		cardRevers.transform.position = CardToChange.transform.position;
		cardRevers.transform.rotation = rot;
		reversInter.SetObjectVisible(false);
		reversInter.SetCardOrder(40);
		reversInter.PlayDrawBackSound ();
		CardToChange.transform.SetParent (cardRevers.transform);
		CardToChange.transform.localPosition = new Vector3 (0, 0, 0);
		rot.eulerAngles = new Vector3(0, 0, 0);
		CardToChange.transform.localRotation = rot;

		cardRevers.transform.SetParent (myDeck.transform);
		cardRevers.GetComponent<SmothTransform> ().SmothTransformTo (new Vector3 (0, 0, 0), rot, 5);
		cardRevers.transform.localScale = myDeck.GetComponent<Deck>().CardScale;
		while(cardRevers.transform.rotation.eulerAngles.y < 260) {
			yield return new WaitForSeconds (0.1f);
		}
		cardRevers.GetComponent<CardInteraction> ().SetObjectVisible(true);
		CardToChange.GetComponent<CardInteraction> ().SetObjectVisible(false);
		yield return new WaitForSeconds (0.3f);

		string newCard = myDeck.GetComponent<Deck> ().SwapCard (CardNumber);
		if (onlineMode) {
			if (UsePhotonEventsIsteadOfRPC) {
				MyTurnManager.GameplaySendMove (GamePlayActionStack.ActionTypeEnum.redraw, 0, "",
					0,0,0,HandIndex);
			} else {
				myPView.RPC ("RPCRemoveCardFromHand", PhotonTargets.Others, HandIndex);
				myPView.RPC ("SignalCardDraw", PhotonTargets.Others);
			}
		}
		GameObject newRevers = GetRevers (myDeck);
		myDeck.GetComponent<Deck> ().SpawnReverses (1, false);
		//TODO:copy from draw function
		GameObject card = CardsComp.SpawnCardByName (newCard);
		if (card) {
			CardInteraction CardInter = card.GetComponent<CardInteraction> ();
			CardInter.SetObjectVisible (false);
			Vector3 startScale = card.transform.localScale;

			if (newRevers) {
				CardInter.SetCardOrder (43);
				newRevers.transform.SetParent (CardHighlightPosRight);
				card.transform.SetParent (CardHighlightPosRight);
				card.transform.localScale = newRevers.transform.localScale;
				card.transform.SetParent (newRevers.transform);
				card.transform.localPosition = new Vector3 (0, 0, 0);
				rot.eulerAngles = new Vector3 (0, 180, 0);
				card.transform.localRotation = rot;
				rot.eulerAngles = new Vector3 (0, 180, 0);
				CardInter.PlayDrawSound();
				newRevers.GetComponent<SmothTransform> ().SmothTransformTo (new Vector3 (0, 0, 0), rot, deckAnimCurve, 3);
				int timeout = 20;
				while (newRevers.transform.rotation.eulerAngles.y < 3 || newRevers.transform.rotation.eulerAngles.y > 300) {
					if (timeout < 0) {
						break;
					}
					timeout--;
					yield return new WaitForSeconds (0.1f);
				}
				newRevers.GetComponent<CardInteraction> ().SetObjectVisible (false);
				CardInter.SetObjectVisible (true);
			}
		
			if (IAmReady) {
				card.GetComponent<CardInteraction> ().SetCardInterationsEnable (true);
				HandComp.AddCardToHand (card);
			} else {
				StartHandComponent.AddCardToStartHand (card, CardNumber);
			}
		}
		//copy from draw function end

		//StartCoroutine(DrawTask (true, newCard));
		while (CardToChange.GetComponent<SmothTransform> ().smoothTransformPosRunning == true) {
			yield return new WaitForSeconds (0.1f);
		}
		Destroy (CardToChange);
		Destroy (cardRevers);
		Destroy (newRevers);
	}

	public void DrawNextStartCard(GameObject CardToChange, int CardNumber, int HandIndex) {
		StartCoroutine (DrawNextStartCardTask (CardToChange, CardNumber, HandIndex));
	}

	IEnumerator InitDraw(int cardsNumber) {
		//if (onlineMode) {
		//	myPView.RPC ("SignalInitCardDraw", PhotonTargets.Others);
		//}//to remove init draw rcp flow
		if (NetManager != null) {
			if (MyNetManager.ReJoining) {
				StartHandMenuCanvas.enabled = false;
				KeepStartHand ();
			} else {
				for (int i = 0; i < cardsNumber; i++) {
					StartCoroutine (DrawTask (true, ""));
					yield return new WaitForSeconds (1f);
				}
				StartHandTimer ();
			}
		} else {
			for (int i = 0; i < cardsNumber; i++) {
				StartCoroutine (DrawTask (true, ""));
				yield return new WaitForSeconds (1f);
			}
			StartHandTimer ();
		}
	}

	public void Draw(bool localOnly = false) {
		if (skipAnimations) {
			DrawNoAnim (false, "", localOnly);
		} else {
			StartCoroutine (DrawTask (false, "", localOnly));
		}
	}

	public void Draw(string cardName, bool localOnly = false) {
		if (skipAnimations) {
			DrawNoAnim (false, cardName, localOnly);
		} else {
			StartCoroutine (DrawTask (false, cardName, localOnly));
		}
	}

	private void DrawNoAnim(bool initDraw, string cardName, bool localOnly = false) {
		GameObject card;

		Debug.Log ("Draw card " + cardName);

		if (cardName.Length > 0) {
			card = CardsComp.SpawnCardByName (cardName);
		} else {
			card = myDeck.GetComponent<Deck> ().GetNextCard (CardsComp);
		}
		if (card) {
			GameObject cardRevers = GetRevers (myDeck);
			CardInteraction CardInter = card.GetComponent<CardInteraction> ();
			CardInter.SetObjectVisible (false);

			Vector3 startScale = card.transform.localScale;

			if (cardRevers) {
				Destroy (cardRevers);
				CardInter.SetObjectVisible (true);
			}

			if (initDraw) {
				if (!IAmReady) {
					StartHandComponent.AddCardToStartHand (card, InitCardDeckNumber);
					InitCardDeckNumber--;
				} else {
					initDraw = false;
				}
			}
			if (onlineMode) {
				if (!localOnly) {
					if (UsePhotonEventsIsteadOfRPC) {
						MyTurnManager.GameplaySendMove (GamePlayActionStack.ActionTypeEnum.draw, "");
					} else {
						myPView.RPC ("SignalCardDraw", PhotonTargets.Others);
					}
				}
			}
			card.GetComponent<SmothTransform> ().enabled = true;
			card.GetComponent<CardInteraction> ().EnableCard();
			HandComp.EnableOtherCards ();
			if (!initDraw) {
				CardInter.enabled = true;
				if (!HandComp.AddCardToHand (card)) {
					Destroy (card);//Hand is full, destroy cards then. Maybe some other actions in the future.
				} else {
					HandComp.RefreshCardInHand ();
					card.transform.localScale = startScale;
					card.GetComponent<CardInteraction> ().SetCardInterationsEnable (true);
				}
			}
		}
	}

	IEnumerator DrawTask(bool initDraw, string cardName, bool localOnly = false) {
		GameObject card;

		Debug.Log ("Draw card " + cardName);

		if (cardName.Length > 0) {
			card = CardsComp.SpawnCardByName (cardName);
		} else {
			card = myDeck.GetComponent<Deck> ().GetNextCard (CardsComp);
		}
		if (card) {
			GameObject cardRevers = GetRevers (myDeck);
			CardInteraction CardInter = card.GetComponent<CardInteraction> ();
			CardInter.SetObjectVisible (false);
			Vector3 startScale = card.transform.localScale;

			if (cardRevers) {
				Quaternion rot = Quaternion.identity;
				CardInter.SetCardOrder (43);
				cardRevers.transform.SetParent (CardHighlightPosRight);
				card.transform.SetParent (CardHighlightPosRight);
				card.transform.localScale = cardRevers.transform.localScale;
				card.transform.SetParent (cardRevers.transform);
				card.transform.localPosition = new Vector3 (0, 0, 0);
				rot.eulerAngles = new Vector3 (0, 180, 0);
				card.transform.localRotation = rot;
				rot.eulerAngles = new Vector3(0, 180, 0);
				//cardRevers.GetComponent<SmothTransform> ().SmothTransformTo (new Vector3 (0, 0, 0), 5);
				//cardRevers.GetComponent<SmothTransform> ().SmothTransformTo (rot, 5);
				CardInter.PlayDrawSound();
				cardRevers.GetComponent<SmothTransform> ().SmothTransformTo (new Vector3 (0, 0, 0), rot, deckAnimCurve, 3);
				//Debug.Log ("    ====== rot y : " + cardRevers.transform.rotation.eulerAngles.y);
				int timeout = 20;
				while (cardRevers.transform.rotation.eulerAngles.y < 3 || cardRevers.transform.rotation.eulerAngles.y > 300) {
					//Debug.Log ("    ====== 2 rot y : " + cardRevers.transform.rotation.eulerAngles.y);
					if (timeout < 0) {
						break;
					}
					timeout--;
					yield return new WaitForSeconds (0.1f);
				}
				//Debug.Log ("    ====== exit rot y : " + cardRevers.transform.rotation.eulerAngles.y);
				cardRevers.GetComponent<CardInteraction> ().SetObjectVisible(false);
				CardInter.SetObjectVisible (true);
			}

			if (initDraw) {
				//card.GetComponent<CardInteraction> ().ChangeCardSignalColor (true);
				if (!IAmReady) {
					StartHandComponent.AddCardToStartHand (card, InitCardDeckNumber);
					InitCardDeckNumber--;
				} else {
					initDraw = false;
				}
				//tmp card.GetComponent<CardInteraction> ().StartDraw = true;

			}// else {
			if (onlineMode) {
				if (!localOnly) {
					if (UsePhotonEventsIsteadOfRPC) {
						MyTurnManager.GameplaySendMove (GamePlayActionStack.ActionTypeEnum.draw, "");
					} else {
						myPView.RPC ("SignalCardDraw", PhotonTargets.Others);
					}
				}
			}

			yield return new WaitForSeconds (1f);

			Destroy (cardRevers);
			card.GetComponent<SmothTransform> ().enabled = true;
			card.GetComponent<CardInteraction> ().EnableCard();
			HandComp.EnableOtherCards ();
			if (!initDraw) {
				CardInter.enabled = true;
				card.GetComponent<SmothTransform> ().SmoothScaleTo (startScale, 5);
				if (!HandComp.AddCardToHand (card)) {
					Debug.Log ("Hand full - remove card:" + cardName);
					Destroy (card);//Hand is full, destroy cards then. Maybe some other actions in the future.
				} else {
					HandComp.RefreshCardInHand ();
					//cardRevers.GetComponent<SmothTransform> ().SmothTransformTo (new Vector3 (0, 0, 0), 5);
					yield return new WaitForSeconds (0.8f);
					card.GetComponent<CardInteraction> ().SetCardInterationsEnable (true);
				}
			}
		}
	}

	public void ShowMarkForEnemy(int BoardPosIndex, int handIndex) {
		if  ((BoardPosIndex >= 0) && (BoardPosIndex < IndexMAX)) {
			if (onlineMode) {
				if (LastEffectIndex != BoardPosIndex) {
					enemyMarkBeamShowed = true;
					myPView.RPC ("RPCShowMarkForEnemy", PhotonTargets.Others, BoardPosIndex, handIndex);
					LastEffectIndex = BoardPosIndex;
				}
			}
		} else {
			Debug.LogError ("error index is out of range");
		}
	}

	/// <summary>
	/// ONLY LOCALLY! Place pawn on this position on board in gameplay.
	/// </summary>
	/// <param name="pawn">Game object of this pawn.</param>
	/// <param name="index">Board index to place. Can be get by Gameplay.Board[index]</param>
	public void PutPawnOnPosisionImpl(GameObject pawn, int index)
	{
		if (index < IndexMAX) {
			Board [index].Pawn = pawn;
			pawn.GetComponent<Pawn> ().boardPosisionIndex = index;
			pawn.GetComponent<Pawn> ().boardSavedPosisionIndexPrev = index;
		} else {
			Debug.Log ("error index is out of range");
		}
		CheckPawnAttackConfigVisible ();
		UpdatePawnsState ();
	}

	/// <summary>
	/// SEND EVENT TO SECOND CLIENT! Place pawn on this position on board in gameplay.
	/// </summary>
	/// <param name="pawn">Game object of this pawn.</param>
	/// <param name="index">Board index to place. Can be get by Gameplay.Board[index]</param>
	/// <param name="playFromHand">True if pawn first time appear on the board (no previous board location)</param>
	public void PutPawnOnPosision(GameObject pawn, int index, bool playFromHand)
	{
		if (index < IndexMAX) {
			Board [index].Pawn = pawn;
			if (pawn != myHero && pawn != enemyHero) {
				Pawn pawnComp = pawn.GetComponent<Pawn> ();
				if (playFromHand) {
					//SetCardBoardID (pawnComp);
					GameLogComponent.AddLogAction (GamePlayActionStack.ActionTypeEnum.play, pawnComp);
					if (onlineMode) {
						if (UsePhotonEventsIsteadOfRPC) {
							MyTurnManager.GameplaySendMove (GamePlayActionStack.ActionTypeEnum.play,
								pawnComp.pawnBoardID, pawnComp.Name, 0, index, pawnComp.RotationPosIndex,
								pawnComp.handIndex);
						} else {
							myPView.RPC ("RPCPutPawnPawnOnBoard", PhotonTargets.Others,
								pawnComp.Name, pawnComp.pawnBoardID,
								pawnComp.RotationPosIndex, pawnComp.handIndex, index);
						}
					}
				} else {
					if (onlineMode) {
						if (UsePhotonEventsIsteadOfRPC) {
							MyTurnManager.GameplaySendMove (GamePlayActionStack.ActionTypeEnum.move,
								pawnComp.pawnBoardID, pawnComp.Name, 0, index, pawnComp.RotationPosIndex, 0);
						} else {
							myPView.RPC ("RPCChangingPlacePawnOnPos", PhotonTargets.Others,
								pawnComp.pawnBoardID, pawnComp.RotationPosIndex, index);
						}
					}
				}
			}
		} else {
			Debug.Log ("error index is out of range");
		}
		CheckPawnAttackConfigVisible ();
		UpdatePawnsState ();
	}

	public void SetCardBoardID(Pawn pawnComp) {
		pawnComp.pawnBoardID = boardIDCurrent++;
		pawnListClass panwToAdd = new pawnListClass ();
		panwToAdd.pawn = pawnComp;
		panwToAdd.pawnID = pawnComp.pawnBoardID;
		GamePawnsList.Add (panwToAdd);
	}

	private void AddPawn(Pawn pawnComp, int pawnID) {
		pawnListClass panwToAdd = new pawnListClass ();
		panwToAdd.pawn = pawnComp;
		panwToAdd.pawnID = pawnID;
		GamePawnsList.Add(panwToAdd);
	}

	public void PutWeaponOnBoard(GameObject pawn, int index) {
		if (index < IndexMAX) {
			Pawn pawnComp = pawn.GetComponent<Pawn> ();
			WeaponToPlay = pawnComp;
			if (onlineMode) {
				if (UsePhotonEventsIsteadOfRPC) {
					MyTurnManager.GameplaySendMove (GamePlayActionStack.ActionTypeEnum.playItem,
						pawnComp.CardID, pawnComp.Name, pawnComp.pawnBoardID,
						index, pawnComp.RotationPosIndex, pawnComp.handIndex);
				} else {
					myPView.RPC ("RPCPutWeaponOnBoard", PhotonTargets.Others, pawnComp.Name, pawnComp.RotationPosIndex, pawnComp.handIndex, index);
				}
			}
		} else {
			Debug.Log ("error index is out of range");
		}
	}

	public void UpdatePawnRot(GameObject pawn) {
		Pawn pawnComp = pawn.GetComponent<Pawn> ();
		int posIdx = GetBoardIndexByPawn (pawnComp.pawnBoardID);
		if (onlineMode) {
			if (UsePhotonEventsIsteadOfRPC) {
				MyTurnManager.GameplaySendMove (GamePlayActionStack.ActionTypeEnum.move,
					pawnComp.pawnBoardID, pawnComp.Name, 0, pawnComp.boardPosisionIndex, pawnComp.RotationPosIndex, 0);
			} else {
				myPView.RPC ("RPCChangingPlacePawnOnPos", PhotonTargets.Others, pawnComp.pawnBoardID, pawnComp.RotationPosIndex, posIdx);
			}
		}
		CheckPawnAttackConfigVisible ();
	}

	private void CheckAndCorrectPawnPosition(GameObject pawn, int boardPosIdx) {
		if (Board [boardPosIdx].Pawn != pawn) {
			Debug.LogWarning ("CORRECTING: Pawn incorect position: " + pawn + " posIdx: " + boardPosIdx);
			//check current position
			int currIdx = GetBoardIndexByPawn (pawn);
			if (currIdx != -1) {
				if (currIdx != boardPosIdx) {
					RemovePawnFromPosisionImpl (currIdx);
				}
			}
			pawn.GetComponent<Pawn> ().boardPosisionIndex = boardPosIdx;
			pawn.GetComponent<Pawn> ().boardSavedPosisionIndexPrev = boardPosIdx;
			PutPawnOnPosisionImpl (pawn, boardPosIdx);
		}
	}

	public void ConfirmPawnOnPosision(GameObject pawn, int index, bool positionChanged) {
		if ((index >= 0) && (index <= IndexMAX) && (pawn != null)) {
			Pawn pawnComp = pawn.GetComponent<Pawn> ();
			if (pawnComp != null) {
				Debug.Log ("Confirm Pawn Pos:" + index + " named:" + pawnComp.Name);
				CheckAndCorrectPawnPosition (pawn, index);
				if (onlineMode) {
					if (UsePhotonEventsIsteadOfRPC) {
						MyTurnManager.GameplaySendMove (GamePlayActionStack.ActionTypeEnum.moveConfirm,
							pawnComp.pawnBoardID, pawnComp.Name, 0, pawnComp.boardPosisionIndex, pawnComp.RotationPosIndex, 0);
					} else {
						myPView.RPC ("RPCConfirmPawnPos", PhotonTargets.Others, pawnComp.Name, pawnComp.pawnBoardID, pawnComp.RotationPosIndex, index);
					}
				}
			}
		}
	}

	IEnumerator ShowCardInHighlight(GameObject card) {
		if (cardHighlight != null) {
			StartCoroutine (HidePawnInHighlightTask ());
			yield return new WaitForSeconds (0.5f);
			Destroy (cardHighlight);
		}
		cardHighlight = card;
		if (cardHighlight != null) {
			cardHighlight.transform.SetParent (CardHighlightPosLeft, false);
			cardHighlight.transform.localPosition = new Vector3 (0, 5, 0);
			cardHighlight.GetComponent<CardInteraction> ().SetCardOrder (32);
			cardHighlight.GetComponent<SmothTransform> ().SmothTransformTo (new Vector3 (0, 0, 0), 20);
			yield return new WaitForSeconds (2f);
			if (cardHighlight != null) {
				cardHighlight.GetComponent<SmothTransform> ().SmothTransformTo (new Vector3 (0, 5, 0), 20);
				yield return new WaitForSeconds (1f);
			}
		}
		if (cardHighlight != null) {
			Destroy (cardHighlight);
		}
	}

	public void ShowPawnInHighlight(Pawn pawn) {
		StartCoroutine (ShowPawnInHighlightTask(pawn));
	}

	public void HidePawnInHighlight() {
		StartCoroutine (HidePawnInHighlightTask());
	}

	private IEnumerator ShowPawnInHighlightTask(Pawn pawn) {
		Pawn cardHighlightPawn;
		bool rightPos = false;
		if (cardHighlight != null) {
			StartCoroutine (HidePawnInHighlightTask ());
			yield return new WaitForSeconds (0.5f);
			Destroy (cardHighlight);
		}
		cardHighlight = CardsComp.SpawnCardByName (pawn.Name);
		if (cardHighlight != null) {
			if ((YouStarted && Board [pawn.boardPosisionIndex].a >= 1) ||
				(!YouStarted && Board [pawn.boardPosisionIndex].a <= -1)){
				rightPos = true;
			} else {
				rightPos = false;
			}
			if (rightPos) {
				cardHighlight.transform.SetParent (CardHighlightPosRight, false);
				cardHighlight.transform.localPosition = new Vector3 (5, 0, 0);
			} else {
				cardHighlight.transform.SetParent (CardHighlightPosLeft, false);
				cardHighlight.transform.localPosition = new Vector3 (-5, 0, 0);
			}
			cardHighlight.GetComponent<CardInteraction> ().SetCardOrder (32);
			cardHighlightPawn = cardHighlight.GetComponent<CardInteraction> ().pawnComponent;
			cardHighlight.GetComponent<CardInteraction> ().DisableCard();
			cardHighlightPawn.SetAttack (pawn.Attack);
			cardHighlightPawn.SetHealth (pawn.Health);
			string belowText = "";
			if (pawn.Burning) {
				belowText = "Podpalony";
			}
			if (pawn.Frozen) {
				if (belowText.Length > 0) {
					belowText = belowText + ",\nZamrożony";
				} else {
					belowText = "Zamrożony";
				}
			}
			if (pawn.Poisoned) {
				if (belowText.Length > 0) {
					belowText = belowText + ",\nNie może być leczony";
				} else {
					belowText = "Nie może być leczony";
				}
			}
			if (belowText.Length > 0) {
				cardHighlight.GetComponent<CardInteraction> ().SetBelowText (belowText);
			}
			cardHighlight.GetComponent<Animation> ().Play ();
			if (pawn.WeaponName.Length > 0) {
				cardWeaponHighlight = CardsComp.SpawnCardByName (pawn.WeaponName);
				cardWeaponHighlight.transform.SetParent (cardHighlight.transform, false);
				if (rightPos) {
					cardWeaponHighlight.transform.localPosition = new Vector3 (-11f, -1.8f, 0);
				} else {
					cardWeaponHighlight.transform.localPosition = new Vector3 (11f, -1.8f, 0);
				}
				cardWeaponHighlight.transform.localScale = new Vector3 (0.8f, 0.8f, 0.8f);
				cardHighlightPawn.PawnConfiguration = pawn.PawnConfiguration;
				cardHighlightPawn.ApplyConfig ();
				cardHighlightPawn.WeaponName = pawn.WeaponName;
				cardHighlightPawn.WeaponUseCount = pawn.WeaponUseCount;
				cardHighlightPawn.WeaponUseCounterText.text = pawn.WeaponUseCount.ToString ();
				//cardWeaponHighlight.GetComponent<Animation> ().Play ();
			}
			cardHighlight.GetComponent<SmothTransform> ().SmothTransformTo (new Vector3 (0, 0, 0), 20);
		}
	}

	private IEnumerator HidePawnInHighlightTask() {
		if (cardHighlight != null) {
			if (cardHighlight.transform.parent == CardHighlightPosRight) {
				cardHighlight.GetComponent<SmothTransform> ().SmothTransformTo (new Vector3 (4, 0, 0), 12);
			} else {
				cardHighlight.GetComponent<SmothTransform> ().SmothTransformTo (new Vector3 (-4, 0, 0), 12);
			}
			//yield return new WaitForSeconds (0.5f);//skip procedural anim becouse of Unity card Anim
			Destroy (cardHighlight);
			Destroy (cardWeaponHighlight);
		}
		yield return null;
	}

	public void ChangeEnemyPawnPos(int pawnID, int rotIdx, int index) {
		Debug.Log ("ChangeEnemyPawnPos:" + pawnID + " idx:" + index);
		int posIdx = GetBoardIndexByPawn (pawnID);
		if (posIdx != -1) {
			if (!IsFreePosision (index)) {
				if (Board [index].Pawn.GetComponent<Pawn> ().pawnBoardID != pawnID) {
					if (!Board [index].Pawn.GetComponent<Pawn> ().Death) {
						GameObject ErrorPawn = Board [index].Pawn;
						Pawn ErrorPawnComp = ErrorPawn.GetComponent<Pawn> ();
						Debug.LogError ("Enemy put pawn on NOT empty board position. Here is:" + ErrorPawnComp.Name +
						" health:" + ErrorPawnComp.Health + " REMOVING");
						ErrorPawnComp.PawnDie ();
					}
				}
			}
			GameObject pawn = Board [posIdx].Pawn;
			Pawn pawnComp = pawn.GetComponent<Pawn> ();
			if (posIdx != index) {
				PutPawnOnPosisionImpl (pawn, index);
				RemovePawnFromPosisionImpl (posIdx);
				pawnComp.SetAttackOnlyMode();
			}
			pawn.GetComponent<SpriteRenderer> ().sortingOrder = 10;
			Vector3 newPawnPos = Board [index].BoardPiece.transform.localPosition;
			newPawnPos.z = Pawn.PawnPosUp;
			pawnComp.boardSavedRotationIndexPrev = rotIdx;
			pawnComp.SetPawnRotation (rotIdx);
			if (skipAnimations) {
				pawn.transform.localPosition = newPawnPos;
			} else {
				pawn.GetComponent<SmothTransform> ().SmothTransformTo (newPawnPos, 10f);
			}
		}
		UpdatePawnsState ();
	}

	public void ConfirmEnemyPawnPos(string cardName, int pawnID, int pawnRotationIndex, int boardPosIndex) {
		Debug.Log ("ConfirmEnemyPawnPos:" + pawnRotationIndex + " ID:" + pawnID);
		int posIdx = GetBoardIndexByPawn (pawnID);
		if (posIdx != -1) {
			GameObject pawn = Board [posIdx].Pawn;
			pawn.GetComponent<SpriteRenderer> ().sortingOrder = 0;
			Vector3 newPawnPos = new Vector3 (0, 0, 0);
			newPawnPos = Board [boardPosIndex].BoardPiece.transform.localPosition;
			newPawnPos.z = Pawn.PawnPosDown;
			if (skipAnimations) {
				pawn.transform.localPosition = newPawnPos;
			} else {
				pawn.GetComponent<SmothTransform> ().SmothTransformTo (newPawnPos, 10f);
			}
			PutPawnOnPosisionImpl (pawn, boardPosIndex);
			pawn.GetComponent<Pawn> ().boardPosisionIndex = boardPosIndex;
			pawn.GetComponent<Pawn> ().SetPawnRotation (pawnRotationIndex);
			//pawn.GetComponent<Pawn> ().CallOnMoveCallback (boardPosIndex);//call locally only now, because of sync missmatch
			if (posIdx != boardPosIndex) {
				RemovePawnFromPosisionImpl (posIdx);
			}
		}
		UpdatePawnsState ();
	}

	public Transform PutEnemyCardOnBoard(string cardName, int pawnID,
		int pawnRotationIndex, int handIndex, int boardPosIndex, bool Enemy = true)
	{
		Debug.Log ("Put enemy card on board - name: " + cardName + ", " + pawnRotationIndex + ", " + handIndex + ", " + boardPosIndex);
		Transform pawnTransform;
		Pawn pawnComp;

		if (handIndex >= 0 && EnemyHandComp.HandCards [handIndex] != null) {
			//consider about some play from enemy hand animation
			Vector3 newPos = EnemyHandComp.HandCards [handIndex].transform.localPosition;
			newPos.y = newPos.y + 2;
			EnemyHandComp.HandCards [handIndex].GetComponent<SmothTransform> ().SmothTransformTo (newPos, 5);
		}
		GameObject cardtoplay = CardsComp.SpawnCardByName (cardName);
		if (!skipAnimations) {
			if (handIndex >= 0) {
				GameObject card = CardsComp.SpawnCardByName (cardName);
				StartCoroutine (ShowCardInHighlight (card));
			}
		}
		pawnTransform = cardtoplay.transform.Find ("Pawn");
		pawnTransform.SetParent (this.transform, false);
		pawnTransform.gameObject.GetComponent<KeepParentRenderLayer> ().KeepingActive = false;
		pawnTransform.GetComponent<KeepParentRenderLayer>().KeepingActive = false;
		pawnTransform.gameObject.GetComponent<SpriteRenderer> ().sortingOrder = 20;
		pawnComp = pawnTransform.gameObject.GetComponent<Pawn> ();
		if (Enemy) {
			pawnComp.Friendly = false;
			pawnComp.SetBorder (enemyColor);
			if (pawnComp.CardType == CardsBase.CardTypesEnum.Pawn) {
				pawnTransform.gameObject.GetComponent<SpriteRenderer> ().color = enemyColor;
			}
		}
		pawnComp.gamePlayComp = this.GetComponent<GamePlay> ();
		pawnComp.pawnBoardID = pawnID;
		pawnComp.boardPosisionIndex = boardPosIndex;
		pawnComp.boardSavedPosisionIndexPrev = boardPosIndex;
		pawnComp.boardSavedRotationIndexPrev = pawnRotationIndex;
		/*if (NetManager == null || NetManager.GetComponent<MyNetworkManager> ().gameMode == MyNetworkManager.gameModeEnum.training) {
			if (!TutorialMode) {
				pawnComp.AITriggerEffectCallback = AIComponent.AIPawnEffectCallback;
			}
		}*/
		//if (YouStarted) {
			pawnComp.SetPawnRotation (pawnRotationIndex);
		//}
		AddPawn (pawnComp, pawnID);

		if (handIndex != -1) {
			GameLogComponent.AddLogAction (GamePlayActionStack.ActionTypeEnum.play, pawnComp);
		}
		if (Enemy) {
			EnemyHandComp.RemoveCardFromHandWithDestroy (handIndex);
		} else {
			HandComp.RemoveCardFromHandWithDestroy (cardName);
		}
		Destroy (cardtoplay);
		return pawnTransform;
	}

	public GameObject PutEnemyPawnOnBoard(string cardName, int pawnID, int pawnRotationIndex,
		int handIndex, int boardPosIndex, bool Enemy = true)
	{
		Pawn pawnComp;
		Vector3 newPawnPos;
		Quaternion newPawnRot;

		Transform pawnTransform = PutEnemyCardOnBoard (cardName, pawnID,
			pawnRotationIndex, handIndex, boardPosIndex, Enemy);
		if (YouStarted) {
			newPawnRot = new Quaternion (0, 0, 0, 0);
		} else {
			newPawnRot = new Quaternion (0, 0, 180, 0);
		}
		pawnComp = pawnTransform.gameObject.GetComponent<Pawn> ();
		//AddPawn (pawnComp, pawnID);
		pawnComp.SetPawnRotation (pawnRotationIndex);
		newPawnPos = Board[boardPosIndex].BoardPiece.transform.localPosition;
		newPawnPos.z = -0.4f;
		if (skipAnimations) {
			pawnTransform.localPosition = newPawnPos;
			pawnTransform.localRotation = newPawnRot;
		} else {
			pawnTransform.gameObject.GetComponent<SmothTransform> ().SmothTransformTo (newPawnPos, newPawnRot, 5);
		}
		pawnTransform.gameObject.GetComponent<PolygonCollider2D> ().enabled = true;
		float newscale = pawnComp.onPlayScale;
		pawnTransform.localScale = new Vector3(newscale, newscale, newscale);

		if (!Enemy) {
			boardIDCurrent = pawnID + 1;
		}

		pawnComp.isFirstPlay = true;

		PutPawnOnPosisionImpl (pawnTransform.gameObject, boardPosIndex);
		pawnComp.CallLocalPlayConfirmCallback ();
		return pawnTransform.gameObject;
	}

	public GameObject PutEnemyWeaponOnBoard(string weaponName, int weaponRotationIndex,
		int handIndex, int boardPosIndex, bool Enemy = true)
	{
		Pawn pawnComp;
		Vector3 newPawnPos;

		Transform pawnTransform = PutEnemyCardOnBoard (weaponName, -1,
			weaponRotationIndex, handIndex, boardPosIndex, Enemy);
		pawnComp = pawnTransform.gameObject.GetComponent<Pawn> ();
		pawnComp.pawnBoardID = -10;//Weapon ID
		pawnComp.SetPawnRotation (weaponRotationIndex);
		newPawnPos = Board[boardPosIndex].BoardPiece.transform.localPosition;
		newPawnPos.z = -0.4f;
		if (skipAnimations) {
			pawnTransform.localPosition = newPawnPos;
		} else {
			pawnTransform.gameObject.GetComponent<SmothTransform> ().SmothTransformTo (newPawnPos, 5);
		}
		float newscale = pawnComp.onPlayScaleAsWeapon;
		pawnTransform.localScale = new Vector3(newscale, newscale, newscale);
		EnemyWeaponToPlay = pawnComp;

		return pawnTransform.gameObject;
	}

	public GameObject PlayEnemyEffectOnBoard(string effectName, int startBoardPosIndex,
		int EndboardPosIndex, int handIndex, bool Enemy = true)
	{
		Color spriteTmpColor;
		Transform pawnTransform = PutEnemyCardOnBoard (effectName,
			boardEffectIDCurrent++, 0, handIndex, EndboardPosIndex, Enemy);
		pawnTransform.GetComponent<Pawn> ().TriggerEffect (startBoardPosIndex, EndboardPosIndex);
		spriteTmpColor = pawnTransform.GetComponent<SpriteRenderer> ().color;
		spriteTmpColor.a = 0;
		pawnTransform.GetComponent<SpriteRenderer> ().color = spriteTmpColor;
		pawnTransform.localPosition = new Vector3 (1000, 1000, 1000); //out of board
		Destroy (pawnTransform.gameObject, 10);

		return pawnTransform.gameObject;
	}

	public void PlayEffectOnBoard(string effectName, int startBoardPosIndex, int endBoardPosIndex, int handIndex) {
		if (onlineMode) {
			if (UsePhotonEventsIsteadOfRPC) {
				MyTurnManager.GameplaySendMove (GamePlayActionStack.ActionTypeEnum.playEffect,
					CardsComp.GetIdByName(effectName), effectName, startBoardPosIndex, endBoardPosIndex, 0, handIndex);
			} else {
				myPView.RPC ("RPCPlayEffectCardOnBoard", PhotonTargets.Others, effectName, startBoardPosIndex, endBoardPosIndex, handIndex);
			}
		}
	}

	public void ShowEnemyMarkBeam(int boardPosIndex, int handIndex) {
		enemyMarkBeamShowed = true;
		CreateEnemyAttackBeam (enemyHero.GetComponent<Pawn>().boardPosisionIndex, boardPosIndex);
	}

	/// <summary>
	/// SEND EVENT TO SECOND CLIENT! Remove pawn from this position on board in gameplay.
	/// </summary>
	/// <param name="index">Board index to free.</param>
	public void RemovePawnFromPosision(int index) {
		if ((index >= 0) && (index <= IndexMAX)) {
			GameObject pawn = Board [index].Pawn;
			if (pawn != null && pawn != myHero) {
				Pawn pawnComp = pawn.GetComponent<Pawn> ();
				if (pawn != null && onlineMode) {
					if (UsePhotonEventsIsteadOfRPC) {
						MyTurnManager.GameplaySendMove (GamePlayActionStack.ActionTypeEnum.move,
							pawnComp.pawnBoardID, pawnComp.Name, 0, pawnComp.boardPosisionIndex,
							pawnComp.RotationPosIndex, 0);
					} else {
						myPView.RPC ("RPCChangingPlacePawnOnPos", PhotonTargets.Others,
							pawnComp.pawnBoardID, pawnComp.RotationPosIndex, index);
					}
				} else {
					Debug.Log ("offline or pawn here is null, index: " + index);
				}
				RemovePawnFromPosisionImpl (index);
			}
		}
	}

	/// <summary>
	/// ONLY LOCALLY! Remove pawn from this position on board in gameplay.
	/// </summary>
	/// <param name="index">Board index to free.</param>
	public void RemovePawnFromPosisionImpl(int index) {
		Board[index].Pawn = null;
		CheckPawnAttackConfigVisible ();
	}

	/// <summary>
	/// Send event to second player about applied item/weapon to pawn.
	/// </summary>
	/// <param name="weaponName">Weapon card name.</param>
	/// <param name="WeaponOwnerBoardIndex">Board position index of item owner.</param>
	/// <param name="WeaponRotationIndex">Item rotation index.</param>
	public void GiveWeaponToPawn(string weaponName, int WeaponOwnerBoardIndex, int WeaponRotationIndex) {
		if (onlineMode) {
			if (UsePhotonEventsIsteadOfRPC) {
				MyTurnManager.GameplaySendMove (GamePlayActionStack.ActionTypeEnum.itemConfirm,
					CardsComp.GetIdByName(weaponName), weaponName, 0, WeaponOwnerBoardIndex, WeaponRotationIndex, 0);
			} else {
				myPView.RPC ("RPCGiveWeaponToPawn", PhotonTargets.Others, weaponName, WeaponOwnerBoardIndex, WeaponRotationIndex);
			}
		}
	}

	/// <summary>
	/// CALLED BY SECOND CLIENT! Informs about item/weapon applied to pawn.
	/// </summary>
	/// <param name="weaponName">Weapon card name.</param>
	/// <param name="WeaponOwnerBoardIndex">Board position index of item owner.</param>
	/// <param name="WeaponRotationIndex">Item rotation index.</param>
	public void GiveWeaponToEnemyPawn(string weaponName, int WeaponOwnerBoardIndex, int WeaponRotationIndex) {
		/*if (WeaponToPlay.Name != weaponName) {
      Debug.LogError ("Something went wrong with playing weapon:" + weaponName + " and gameplay use:" + WeaponToPlay.Name);
    }*/
		if (EnemyWeaponToPlay != null) {
			EnemyWeaponToPlay.SetPawnRotation (WeaponRotationIndex);
			if (skipAnimations) {
				EnemyWeaponToPlay.AddWeaponToPawn (WeaponOwnerBoardIndex, false);
			} else {
				EnemyWeaponToPlay.AddWeaponToPawn (WeaponOwnerBoardIndex, false);
			}
			EnemyWeaponToPlay = null;
		}
	}

	/// <summary>
	/// CALLED BY SECOND MASTER CLIENT! Sets this player as second (second player moves first).
	/// </summary>
	public void SetAsSecondPlayer() {
		Debug.Log ("Set as second player");
		YouStarted = false;
		myTurn = false;
		boardIDCurrent = 1000;
		EndTurnButton.interactable = false;
		DisableAllPawnOnBoard ();
		transform.Rotate (Vector3.forward, 180);
		if (YouStarted) {
			PutPawnOnPosisionImpl(myHero, 0);
			PutPawnOnPosisionImpl(enemyHero, IndexMAX - 1);
			myHero.GetComponent<Pawn>().boardSavedRotationIndexPrev = 0;
			myHero.GetComponent<Pawn> ().RotationPosIndex = 0;
			enemyHero.GetComponent<Pawn>().boardSavedRotationIndexPrev = 3;
			enemyHero.GetComponent<Pawn>().RotationPosIndex = 3;
		} else {
			PutPawnOnPosisionImpl(myHero, IndexMAX - 1);
			PutPawnOnPosisionImpl(enemyHero, 0);
			myHero.GetComponent<Pawn>().boardSavedRotationIndexPrev = 3;
			myHero.GetComponent<Pawn>().RotationPosIndex = 3;
			enemyHero.GetComponent<Pawn>().boardSavedRotationIndexPrev = 0;
			enemyHero.GetComponent<Pawn>().RotationPosIndex = 0;
		}
	}

	private IEnumerator UpdateManaStateTask() {
		Vector3 origScaleAttackText = origScaleManaText;
		float delta = 0.1f;
		Vector3 newScale = new Vector3 (origScaleAttackText.x + delta,
			origScaleAttackText.y + delta,
			origScaleAttackText.z + delta);
		ActualManaStateText.gameObject.GetComponent<SmothTransform> ().SmoothScaleTo (newScale, 10);
		if (Mana < 0) {
			Mana = 0;
		}
		ActualManaStateText.SetText(Mana.ToString ());
		yield return new WaitForSeconds (0.4f);
		ActualManaStateText.gameObject.GetComponent<SmothTransform> ().SmoothScaleTo (origScaleAttackText, 10);
	}

	public void UpdateManaState(bool skipCheck = false) {
		StartCoroutine (UpdateManaStateTask ());
		if (!skipCheck) {
			if (Mana <= 0) {
				SetPawnsToAttackOnlyMode ();
				EndTurnButton.SignalTurnReady ();
			} else {
				if (HandComp && !HandComp.CardCanBePlayed && !CheckAnyPawnToMove ()) {
					EndTurnButton.SignalTurnReady ();
				}
			}
		}
		HandComp.RefreshCardInHand ();
	}

	public void GetCardCost(int cost) {
		Mana -= cost;
		UpdateManaState ();
		HandComp.RefreshCardInHand ();
	}

	#region GIVE_TURN
	private void GiveTurnCallbacks() {
		for (int i = 0; i < IndexMAX; i++) {
			if ((Board [i].Pawn != null) &&
				(!Board [i].Pawn.GetComponent<Pawn>().Friendly)) {
				Board [i].Pawn.GetComponent<Pawn> ().OnNewTurn ();
			}
		}
	}

	private void GiveTurnEnd() {
		EnemyCardDraw ();
		CheckDeadPawns ();
	}

	private IEnumerator GiveTurnTask(bool localOnly) {
		int timeout = 100;

		while (ActionStackComponent.ActionStarted == true && timeout > 0) {
			timeout--;
			yield return new WaitForSeconds (0.1f);
		}
		//ChooseRandomTarget ();
		Debug.Log ("End turn: (give turn)");

		GiveTurnCallbacks ();
		timeout = 100;
		while (ActionStackComponent.ActionStarted == true) {
			timeout--;
			yield return new WaitForSeconds (0.1f);
		}
		GiveTurnEnd ();
	}

	private void GiveTurnNoAnim(bool localOnly) {
		//ChooseRandomTarget ();
		Debug.Log ("End turn: (give turn)");
		GiveTurnCallbacks ();
		GiveTurnEnd ();
	}

	public void OnGiveTurn(bool localOnly = false) {
		if (!gameEnd) {
			if (myTurn) {
				myTurn = false;
				EndTurnButton.interactable = false;
				GameplayAudio.PlayOneShot (GiveTurnAudioClip);
				StopTurnTimer ();
				EndTurnButton.ChangeButtonToEnemyTurn();
				SetBoardPiecesNormalColor ();
				UnsetOtherPawnsOnBoard ();
				ChooseRandomTarget ();
				ResetPawnsState ();
				DisableAllPawnOnBoard ();
				if (skipAnimations) {
					GiveTurnNoAnim (localOnly);
				} else {
					StartCoroutine (GiveTurnTask (localOnly));
				}
			}
		}
	}

	public void GiveTurn(bool localOnly = false) {
		if (myTurn) {
			OnGiveTurn ();
			if (onlineMode) {
				if (!localOnly) {
					if (UsePhotonEventsIsteadOfRPC) {
						MyTurnManager.GameplayTurnDone ();
					} else {
						myPView.RPC ("RPCGiveTurn", PhotonTargets.Others);
					}
				}
			} else {
				if (TutorialMode) {
					TutorialComponent.EndTurn ();
				} else {
					AIComponent.AITurn ();
				}
			}
		}
	}

	#endregion

	#region TAKE_TURN

	private void TakeTurnShowAnim() {
		NewTurnAnimation.SetTrigger ("ShowNotification");
	}

	private void TakeTurnProcess() {
		if (ManaNextTurn < ManaMax) {
			ManaNextTurn++;
		}
		Mana = ManaNextTurn;
		EndTurnButton.ChangeButtonToMyTurn();
		EndTurnButton.interactable = true;
		Draw (true);
		ResetPawnsState ();
		UpdateManaState ();
		if (myHero != null) {
			myHero.GetComponent<Pawn> ().AttackOnly = true;//Rule 3.1, 3.2
			myHero.GetComponent<Pawn> ().AttackAlready = false;//Rule 3.1, 3.2
		}
		for (int i = 0; i < IndexMAX; i++) {
			if ((Board [i].Pawn != null) &&
				(Board [i].Pawn.GetComponent<Pawn>().Friendly)) {
				Board [i].Pawn.GetComponent<Pawn> ().OnNewTurn ();
			}
		}
	}

	private void TakeTurnEnd() {
		CheckDeadPawns ();
		//SyncBoard ();
		StartTurnTimer();
	}

	private IEnumerator TakeTurnTask() {
		TakeTurnShowAnim ();
		yield return new WaitForSeconds (1.5f);
		TakeTurnProcess ();
		while (ActionStackComponent.ActionStarted == true) {
			yield return new WaitForSeconds (0.1f);
		}
		TakeTurnEnd ();
	}

	private void TakeTurnNoAnim() {
		//TakeTurnShowAnim ();
		TakeTurnProcess ();
		TakeTurnEnd ();
	}

	public void TakeTurn() {
		if (!gameEnd) {
			if (!myTurn) {
				myTurn = true;
				Debug.Log ("Start new turn");
				if (skipAnimations) {
					TakeTurnNoAnim ();
				} else {
					StartCoroutine (TakeTurnTask ());
				}
			}
		}
	}

	#endregion

	private IEnumerator TurnTimerTask() {
		yield return new WaitForSeconds (TurnDuration - TurnDurationAlarmStartAt);
		if (myTurn) {
			EndTurnButton.ChangeButtonToAlarmMode ();
			yield return new WaitForSeconds (TurnDurationAlarmStartAt);
			if (myTurn) {
				GiveTurn ();
			}
		}
	}

	private void StopTurnTimer() {
		if (!UsePhotonEventsIsteadOfRPC) {
			if (turnTimer != null) {
				StopCoroutine (turnTimer);
			}
		}
	}

	private void StartTurnTimer() {
		if (!UsePhotonEventsIsteadOfRPC) {
			turnTimer = StartCoroutine (TurnTimerTask ());
		}
	}

	public void DisableAllPawnOnBoard() {
		if (WeaponToPlay != null) {
			WeaponToPlay.AddWeaponToPawn (WeaponToPlay.boardPosisionIndex);
		}
		EnableHandShowHideMode ();
		foreach (pawnListClass pawnToRet in GamePawnsList) {
			Pawn pawnOnBoard = pawnToRet.pawn;
			if (pawnOnBoard != null) {
				if ((pawnOnBoard.gameObject != null) &&
				   (pawnOnBoard.Friendly)) {
					pawnOnBoard.DisablePawn ();
				}
			}
		}
	}

	public void DisableOtherPawnOnBoard(GameObject thisPawn) {
		if (WeaponToPlay != null) {
			WeaponToPlay.AddWeaponToPawn (WeaponToPlay.boardPosisionIndex);
		}
		HandComp.HideHand ();
		HandComp.HandShowHideModeEnabled = false;
		foreach (pawnListClass pawnToRet in GamePawnsList) {
			Pawn pawnOnBoard = pawnToRet.pawn;
			if (pawnOnBoard != null) {
				if ((pawnOnBoard.gameObject != null) &&
				   (pawnOnBoard.pawnBoardID != thisPawn.GetComponent<Pawn> ().pawnBoardID) &&
				   (pawnOnBoard.gameObject != enemyHero) &&
				   (pawnOnBoard.Friendly)) {
					pawnOnBoard.DisablePawn ();
				}
			}
		}
	}

	public void EnableOtherPawnOnBoard(GameObject thisPawn) {
		Debug.Log ("Enable other pawns on board (" + thisPawn + ")");
		if (myTurn) {
			EnableHandShowHideMode ();
			foreach (pawnListClass pawnToRet in GamePawnsList) {
				Pawn pawnOnBoard = pawnToRet.pawn;
				if (pawnOnBoard != null) {
					if ((pawnOnBoard.gameObject != null) &&
					   (pawnOnBoard.pawnBoardID != thisPawn.GetComponent<Pawn> ().pawnBoardID) &&
					   (pawnOnBoard.gameObject != enemyHero) &&
					   (pawnOnBoard.Friendly)) {
						pawnOnBoard.EnablePawn ();
					}
				}
			}
		}
	}

	public void EnableOtherPawnOnBoard() {
		Debug.Log ("Enable other pawns on board");
		if (myTurn) {
			EnableHandShowHideMode ();
			foreach (pawnListClass pawnToRet in GamePawnsList) {
				Pawn pawnOnBoard = pawnToRet.pawn;
				if (pawnOnBoard != null) {
					if ((pawnOnBoard.gameObject != null) &&
					   (pawnOnBoard.gameObject != enemyHero) &&
					   (pawnOnBoard.Friendly)) {
						pawnOnBoard.EnablePawn ();
					}
				}
			}
		}
	}

	public void UnsetOtherPawnsOnBoard(GameObject thisPawn) {
		ChooseRandomTarget ();
		if (WeaponToPlay != null) {
			WeaponToPlay.AddWeaponToPawn (WeaponToPlay.boardPosisionIndex);
		}
		foreach (pawnListClass pawnToRet in GamePawnsList) {
			Pawn pawnOnBoard = pawnToRet.pawn;
			if (pawnOnBoard != null) {
				if ((pawnOnBoard.gameObject != null) &&
				   (pawnOnBoard.gameObject != thisPawn) &&
				   (pawnOnBoard.gameObject != enemyHero) &&
				   (pawnOnBoard.Friendly) &&
				   (pawnOnBoard.Selected)) {
					pawnOnBoard.ConfirmPosition ();
				}
			}
		}
	}

	public void UnsetOtherPawnsOnBoard() {
		ChooseRandomTarget ();
		if (WeaponToPlay != null) {
			WeaponToPlay.AddWeaponToPawn (WeaponToPlay.boardPosisionIndex);
		}
		foreach (pawnListClass pawnToRet in GamePawnsList) {
			Pawn pawnOnBoard = pawnToRet.pawn;
			if (pawnOnBoard != null) {
				if ((pawnOnBoard.gameObject != null) &&
				   (pawnOnBoard.gameObject != enemyHero) &&
				   (pawnOnBoard.Friendly) &&
				   (pawnOnBoard.Selected)) {
					pawnOnBoard.ConfirmPosition ();
				}
			}
		}
	}

	private void SetPawnsToAttackOnlyMode() {
		foreach (pawnListClass pawnToRet in GamePawnsList) {
			Pawn pawnOnBoard = pawnToRet.pawn;
			if (pawnOnBoard != null) {
				if ((pawnOnBoard.gameObject != null) &&
				   (pawnOnBoard.gameObject != enemyHero) &&
				   (pawnOnBoard.Friendly)) {
					pawnOnBoard.SetAttackOnlyMode ();
				}
			}
		}
	}

	private void UpdatePawnsState() {
		GamePawnsList.RemoveAll (PawnIsNull);
		foreach (pawnListClass pawnToRet in GamePawnsList) {
			Pawn pawnOnBoard = pawnToRet.pawn;
			if (pawnOnBoard != null) {
				if (pawnOnBoard.gameObject != null) {
					pawnOnBoard.CallOnBoardUpdateCallback ();
				}
			}
		}
	}

	private bool CheckAnyPawnToMove() {
		bool rv = false;
		GamePawnsList.RemoveAll (PawnIsNull);
		foreach (pawnListClass pawnToRet in GamePawnsList) {
			Pawn pawnOnBoard = pawnToRet.pawn;
			if (pawnOnBoard != null) {
				if (pawnOnBoard.gameObject != null) {
					if (pawnOnBoard.Friendly) {
						rv |= pawnOnBoard.AnyMoveAvailable();
					}
				}
			}
		}
		return rv;
	}

	public void RefreshPawnsHealth() {
		GamePawnsList.RemoveAll (PawnIsNull);
		foreach (pawnListClass pawnToRet in GamePawnsList) {
			Pawn pawnOnBoard = pawnToRet.pawn;
			if (pawnOnBoard != null) {
				if (pawnOnBoard.gameObject != null) {
					pawnOnBoard.RefreshHealth (pawnOnBoard.Health);
				}
			}
		}
	}

	private void ResetPawnsState() {
		GamePawnsList.RemoveAll (PawnIsNull);
		foreach (pawnListClass pawnToRet in GamePawnsList) {
			Pawn pawnOnBoard = pawnToRet.pawn;
			if (pawnOnBoard != null) {
				/*if ((pawnOnBoard.gameObject != null) &&
				    (pawnOnBoard.Friendly)) {*/
				if (pawnOnBoard.gameObject != null) {
					pawnOnBoard.ResetState ();
				}
			}
		}
	}

	private static bool PawnIsNull(pawnListClass pawnOnList) {
		return pawnOnList.pawn == null;
	}

	public bool IsFreePosision(int i) {
		if (i < IndexMAX && i >= 0) {
			if (Board [i].Pawn == null) {
				return true;
			} else if (Board [i].Pawn.GetComponent<Pawn>().Death) {
				return true;
			} else {
				return false;
			}
		}
		return false;
	}

	private bool IsFreePosision(int i, GameObject pawnToExcept) {
		if (i < IndexMAX && i >= 0) {
			if (Board [i].Pawn != null && Board [i].Pawn == pawnToExcept) {
				return true;
			} else if (Board [i].Pawn == null) {
				return true;
			} else if (Board [i].Pawn.GetComponent<Pawn>().Death) {
				return true;
			} else {
				return false;
			}
		}
		return false;
	}

	private int GetBoardIndexByPawn(GameObject pawnOb) {
		for (int i = 0; i < IndexMAX; i++) {
			if (Board[i].Pawn == pawnOb)  {
				return i;
			}
		}
		return -1;
	}

	public Pawn GetBoardPawnByID(int pawnID) {
		Debug.Log ("Get Pawn ID:" + pawnID);
		Pawn rv = null;
		for (int i = 0; i < IndexMAX; i++) {
			if (Board[i].Pawn != null) {
				if (Board [i].Pawn.GetComponent<Pawn> ().pawnBoardID == pawnID) {
					rv = Board [i].Pawn.GetComponent<Pawn> ();
					Debug.Log ("found pawn on board by id:" + pawnID + " pawn:" + rv.Name);
					break;
				}
			}
		}
		if (rv == null) {
			foreach (pawnListClass pawnToRet in GamePawnsList) {
				if (pawnToRet.pawnID == pawnID) {
					rv = pawnToRet.pawn;
					Debug.Log ("found pawn on list by id:" + pawnID + " pawn:" + rv.Name);
					break;
				}
			}
		}
		return rv;
	}

	private int GetBoardIndexByPawn(int pawnID) {
		//Debug.Log ("Get Pawn ID:" + pawnID);
		for (int i = 0; i < IndexMAX; i++) {
			if (Board[i].Pawn != null) {
				int id = Board [i].Pawn.GetComponent<Pawn> ().pawnBoardID;
				if (id == pawnID) {
					return i;
				}
			}
		}
		return -1;
	}

	public Transform GetBoardPosByIndex(int A, int B, int C) {
		for (int i = 0; i < IndexMAX; i++) {
			if (Board[i].a == A && Board[i].b == B && Board[i].c == C) {
				return Board[i].BoardPiece.transform;
			}
		}
		return null;
	}

	public Transform GetBoardPosByIndex(int index) {
		if (index >= IndexMAX || index < 0)
			return null;
		return Board[index].BoardPiece.transform;
	}

	private int GetBoardIndexByCoord(int A, int B, int C) {
		for (int i = 0; i < IndexMAX; i++) {
			if (Board[i].a == A && Board[i].b == B && Board[i].c == C) {
				return i;
			}
		}
		return -1;
	}

	public List<int> GetBoardIndexesAround(int index, int range) {
		List<int> rv = new List<int> ();
		int a, b, c;
		int i, j, k;

		rv.Add (index);

		for (k = 0; k < range; k++) {
			a = Board [index].a;
			b = Board [index].b;
			c = Board [index].c;
			for (i = 0; i < 7; i++) {
				for (j = 0; j < k; j++) {
					if (i == 0) {
						b++;
						c++;
					} else if (i == 1) {
						a++;
						b--;
					} else if (i == 2) {
						b--;
						c--;
					} else if (i == 3) {
						a--;
						c--;
					} else if (i == 4) {
						a--;
						b++;
					} else if (i == 5) {
						b++;
						c++;
					} else {
						a++;
						c++;
					}
					int next = GetBoardIndexByCoord (a, b, c);
					Debug.Log (" consider pos " + next + " a" + a + "b" + b + "c" + c);
					if (next >= 0) {
						if (i == 0 && j == k) {
							Debug.Log (" add positions around " + index + ": " + next);
							rv.Add (next);
						} else if (i != 0) {
							Debug.Log (" add positions around " + index + ": " + next);
							rv.Add (next);
						}
					}
				}
			}
		}
		return rv;
	}

	#if DO_NOT_USE_IT
	private Transform GetBoardPosUp(int index)
	{
	int A, B, C;

	A = Board[index].a; B = Board[index].b; C = Board[index].c;
	return GetBoardPosByIndex(A, B+1, C+1);
	}

	private Transform GetBoardPosDown(int index)
	{
	int A, B, C;

	A = Board[index].a; B = Board[index].b; C = Board[index].c;
	return GetBoardPosByIndex(A, B-1, C-1);
	}

	private Transform GetBoardPosUpLeft(int index)
	{
	int A, B, C;

	A = Board[index].a; B = Board[index].b; C = Board[index].c;
	return GetBoardPosByIndex(A+1, B, C+1);
	}

	private Transform GetBoardPosUpRight(int index)
	{
	int A, B, C;

	A = Board[index].a; B = Board[index].b; C = Board[index].c;
	return GetBoardPosByIndex(A-1, B+1, C);
	}

	private Transform GetBoardPosDownLeft(int index)
	{
	int A, B, C;

	A = Board[index].a; B = Board[index].b; C = Board[index].c;
	return GetBoardPosByIndex(A+1, B-1, C);
	}

	private Transform GetBoardPosDownRight(int index)
	{
	int A, B, C;

	A = Board[index].a; B = Board[index].b; C = Board[index].c;
	return GetBoardPosByIndex(A-1, B, C-1);
	}
	#endif
	public int GetBoardIndexUp(int index)
	{
		if (index >= 0) {
			int A, B, C;

			A = Board [index].a;
			B = Board [index].b;
			C = Board [index].c;
			return GetBoardIndexByCoord (A, B + 1, C + 1);
		} return -1;
	}

	public int GetBoardIndexDown(int index)
	{
		if (index >= 0) {
			int A, B, C;

			A = Board[index].a; B = Board[index].b; C = Board[index].c;
			return GetBoardIndexByCoord(A, B - 1, C - 1);
		} return -1;
	}

	public int GetBoardIndexUpLeft(int index)
	{
		if (index >= 0) {
			int A, B, C;

			A = Board [index].a;
			B = Board [index].b;
			C = Board [index].c;
			return GetBoardIndexByCoord (A + 1, B, C + 1);
		} return -1;
	}

	public int GetBoardIndexUpRight(int index)
	{
		if (index >= 0) {
			int A, B, C;

			A = Board [index].a;
			B = Board [index].b;
			C = Board [index].c;
			return GetBoardIndexByCoord (A - 1, B + 1, C);
		} return -1;
	}

	public int GetBoardIndexDownLeft(int index)
	{
		if (index >= 0) {
			int A, B, C;

			A = Board [index].a;
			B = Board [index].b;
			C = Board [index].c;
			return GetBoardIndexByCoord (A + 1, B - 1, C);
		} return -1;
	}

	public int GetBoardIndexDownRight(int index)
	{
		if (index >= 0) {
			int A, B, C;

			A = Board [index].a;
			B = Board [index].b;
			C = Board [index].c;
			return GetBoardIndexByCoord (A - 1, B, C - 1);
		} return -1;
	}

	private attackDirections GetOpposedAttackDir(attackDirections Dir) {
		attackDirections ret = attackDirections.Up;
		if (Dir == attackDirections.UpLeft) {
			ret = attackDirections.DownRight;
		} else if (Dir == attackDirections.Up) {
			ret = attackDirections.Down;
		} else if (Dir == attackDirections.UpRight) {
			ret = attackDirections.DownLeft;
		} else if (Dir == attackDirections.DownRight) {
			ret = attackDirections.UpLeft;
		} else if (Dir == attackDirections.Down) {
			ret = attackDirections.Up;
		} else if (Dir == attackDirections.DownLeft) {
			ret = attackDirections.UpRight;
		}
		return ret;
	}

	private int DirToPawnConfigIndex(attackDirections Dir, int Rot, bool Opposite) {
		/*   _________
	    *   /    1    \
		*  /0         2\
		* /             \
		* \             /
		*  \5         3/
		*   \____4____/
		*/
		int DirIdx = 0;
		if (Dir == attackDirections.UpLeft) {
			DirIdx = 0;
		} else if (Dir == attackDirections.Up) {
			DirIdx = 1;
		} else if (Dir == attackDirections.UpRight) {
			DirIdx = 2;
		} else if (Dir == attackDirections.DownRight) {
			DirIdx = 3;
		} else if (Dir == attackDirections.Down) {
			DirIdx = 4;
		} else if (Dir == attackDirections.DownLeft) {
			DirIdx = 5;
		} else {
			DirIdx = 1;//up on dir Other
		}
		if (Opposite) {
			DirIdx += 3;
			if (DirIdx > 5) {
				DirIdx -= 6;
			}
		}
		int conIdx = DirIdx - Rot;
		if (conIdx < 0) {
			conIdx = (DirIdx - Rot) + 6;
		}
		if (conIdx > 5) {
			Debug.LogWarning ("Returned config log index: " + conIdx +
				" Dir:" + Dir + " Rot:" + Rot + " Opposite:" + Opposite);
		}
		return conIdx;
	}

	private void WaitForAttackFinish(Pawn AttackPawn1, Pawn AttackPawn2) {
		StartCoroutine (WaitForAttackFinishTask (AttackPawn1, AttackPawn2));
	}

	private IEnumerator WaitForAttackFinishTask(Pawn AttackPawn1, Pawn AttackPawn2) {
		int timeout = 100;// wait 10 seconds max
		while (timeout >= 0) {
			yield return new WaitForSeconds (0.1f);
			timeout--;
			if ((AttackPawn1.AttackDone && AttackPawn2.AttackDone) ||
				(AttackPawn1 == null) || (AttackPawn2 == null)){
				break;
			}
		}
		ActionStackComponent.DoNextAction ();
	}

	public void KillPawn (Pawn pawnToKill) {
		if (pawnToKill != null) {
			if (!pawnToKill.DeathPending) {
				pawnToKill.DeathPending = true;
				pawnToKill.CallOnDeathCallback ();
				ActionStackComponent.DoActionAnimation (
					GamePlayActionStack.ActionTypeEnum.death,
					pawnToKill, null, 0, 0
				);
				foreach (pawnListClass pawnToRet in GamePawnsList) {
					Pawn pawnOnBoard = pawnToRet.pawn;
					if (pawnOnBoard != null) {
						if ((pawnOnBoard.gameObject != null) &&
						   (pawnOnBoard.Friendly)) {
							pawnOnBoard.CallOnSomeOneDiedCallback (pawnToKill.boardPosisionIndex);
						}
					}
				}
			}
		} else {
			Debug.LogWarning ("missing pawn to kill");
		}
	}

	public void ShowKillPawnAnimation (Pawn pawnToKill) {
		if (pawnToKill != null) {
			pawnToKill.PawnDie (skipAnimations);
		}
		ActionStackComponent.DoNextAction ();
	}

	public void CheckDeadPawns() {
		//for (int i = 0; i < IndexMAX; i++) {
		List<Pawn> pawnsToKill = new List<Pawn>();
		if (enemyHero != null) {
			if ((enemyHero.GetComponent<Pawn> ().Health <= 0) || (enemyHero.GetComponent<Pawn> ().Death)) {
				KillPawn (enemyHero.GetComponent<Pawn> ());
			}
		}
		if (myHero != null) {
			if ((myHero.GetComponent<Pawn> ().Health <= 0) || (myHero.GetComponent<Pawn> ().Death)) {
				KillPawn (myHero.GetComponent<Pawn> ());
			}
		}
		GamePawnsList.RemoveAll (PawnIsNull);
		foreach (pawnListClass pawnToRet in GamePawnsList) {
			if (pawnToRet.pawn != null) {	
				Pawn pawn = pawnToRet.pawn;//Board [i].Pawn.GetComponent<Pawn> ();
				if ((pawn.Health <= 0) || (pawn.Death)) {
					pawnsToKill.Add (pawn);
				}
			}
		}
		foreach (Pawn toKill in pawnsToKill) {
			KillPawn (toKill);
		}
		UpdatePawnsState ();
	}

	/// <summary>
	/// Local only. Deal damage to any unit on this board position index.
	/// </summary>
	/// <param name="DamageDealer">Pawn component of damage dealer card.</param>
	/// <param name="PositionIndex">Target board position index.</param>
	/// <param name="dmgValue">The damage value.</param>
	public void DoDamageOnBoard(Pawn DamageDealer, int PositionIndex, int DamageValue) {
		//add to action stack
		if (DamageDealer.Friendly) {
			DamageDone += DamageValue;
		}
		DoDamageOnBoardImpl(PositionIndex, DamageValue);
		if (Board [PositionIndex].Pawn != null) {
			GameLogComponent.AddLogAction (GamePlayActionStack.ActionTypeEnum.attack,
				DamageDealer, Board [PositionIndex].Pawn.GetComponent<Pawn> ().pawnBoardID);
		}
	}

	/// <summary>
	/// Call by RPC. Deal damage to any unit on this board position index.
	/// </summary>
	/// <param name="PositionIndex">Target board position index.</param>
	/// <param name="dmgValue">The damage value.</param>
	public void DoDamageOnBoardImpl(int PositionIndex, int dmgValue) {
		if ((PositionIndex >= 0) && (PositionIndex <= IndexMAX)) {
			if (Board [PositionIndex].Pawn != null) {
				Board [PositionIndex].Pawn.GetComponent<Pawn> ().TakeDamage (dmgValue);
				Board [PositionIndex].Pawn.GetComponent<Pawn> ().ShowDamageIndicator (dmgValue);//meybe later done by effects animation system
				CheckDeadPawns ();
			}
		}
	}

	public attackDirections GetAttackDirection(int myPawnPosIdx, int enemyPawnPosInx) {
		attackDirections attackDir = attackDirections.Up;
		int index = myPawnPosIdx;
		int startIndex = index;
		int directionIdx = 0;
		bool dirFound = false;

		do {
			bool goToNextDir = false;
			int newIndex;

			if (directionIdx == 0) {
				newIndex = GetBoardIndexUp (index);
				attackDir = attackDirections.Up;
			} else if (directionIdx == 1) {
				newIndex = GetBoardIndexDown (index);
				attackDir = attackDirections.Down;
			} else if (directionIdx == 2) {
				newIndex = GetBoardIndexUpLeft (index);
				attackDir = attackDirections.UpLeft;
			} else if (directionIdx == 3) {
				newIndex = GetBoardIndexUpRight (index);
				attackDir = attackDirections.UpRight;
			} else if (directionIdx == 4) {
				newIndex = GetBoardIndexDownLeft (index);
				attackDir = attackDirections.DownLeft;
			} else if (directionIdx == 5) {
				newIndex = GetBoardIndexDownRight (index);
				attackDir = attackDirections.DownRight;
			} else {
				break;
			}

			index = newIndex;

			if (newIndex != -1) {
				if (newIndex == enemyPawnPosInx) {
					dirFound = true;
					break;
				}
			} else {
				goToNextDir = true;
			}

			if (goToNextDir) {
				directionIdx++;
				index = startIndex;
			}
		} while (true);

		if (!dirFound) {
			attackDir = attackDirections.Other;
		}

		return attackDir;
	}

	/// <summary>
	/// Make attack.
	/// </summary>
	/// <returns>True if attack is possible.</returns>
	/// <param name="myPawnPosIdx">Attaker pawn board index.</param>
	/// <param name="enemyPawnPosInx">Target pawn board index.</param>
	public bool DoAttack(int myPawnPosIdx, int enemyPawnPosInx, bool localOnly = false) {
		attackDirections attackDir = attackDirections.Up;
		int index = myPawnPosIdx;
		int startIndex = index;
		int directionIdx = 0;
		int boardDistance = 1;
		bool dirFound = false;

		do {
			bool goToNextDir = false;
			int newIndex;

			if (directionIdx == 0) {
				newIndex = GetBoardIndexUp (index);
				attackDir = attackDirections.Up;
			} else if (directionIdx == 1) {
				newIndex = GetBoardIndexDown (index);
				attackDir = attackDirections.Down;
			} else if (directionIdx == 2) {
				newIndex = GetBoardIndexUpLeft (index);
				attackDir = attackDirections.UpLeft;
			} else if (directionIdx == 3) {
				newIndex = GetBoardIndexUpRight (index);
				attackDir = attackDirections.UpRight;
			} else if (directionIdx == 4) {
				newIndex = GetBoardIndexDownLeft (index);
				attackDir = attackDirections.DownLeft;
			} else if (directionIdx == 5) {
				newIndex = GetBoardIndexDownRight (index);
				attackDir = attackDirections.DownRight;
			} else {
				break;
			}

			index = newIndex;

			if (newIndex != -1) {
				if (newIndex == enemyPawnPosInx) {
					dirFound = true;
					break;
				}
			} else {
				goToNextDir = true;
			}

			if (goToNextDir) {
				directionIdx++;
				index = startIndex;
				boardDistance = 1;
			} else {
				boardDistance++;
			}
		} while (true);

		if (!dirFound) {
			attackDir = attackDirections.Other;
		}

		bool rv = DoAttack (myPawnPosIdx, enemyPawnPosInx, attackDir, boardDistance, localOnly);
		return rv;
	}

	public bool DoAttack(int myPawnPosIdx, int enemyPawnPosInx, attackDirections attackDirection, int onBoardDistance, bool localOnly = false) {
		//SetBoardPiecesNormalColor ();
		if (!localOnly) {
			if (onlineMode) {
				if (UsePhotonEventsIsteadOfRPC) {
					MyTurnManager.GameplaySendMove (GamePlayActionStack.ActionTypeEnum.attack, myPawnPosIdx,
						"", 0, enemyPawnPosInx, (int)attackDirection, onBoardDistance);
				} else {
					myPView.RPC ("RPCDoAttack", PhotonTargets.Others, myPawnPosIdx, enemyPawnPosInx, attackDirection, onBoardDistance);
				}
			}
		}
		return DoAttackImpl (myPawnPosIdx, enemyPawnPosInx, attackDirection, onBoardDistance);
	}

	public bool DoAttackImpl(int myPawnPosIdx, int enemyPawnPosInx, attackDirections attackDirection, int onBoardDistance) {
		bool rv = false;
		//if (ActionStackComponent.ActionStarted == false) {
			rv = DoAttackImplTask (myPawnPosIdx, enemyPawnPosInx, attackDirection, onBoardDistance);
			/*if (rv) {
				if (!skipAnimations) {
					ActionStackComponent.ActionStarted = true;
				}
			}
		} else {
			rv = true;
			ActionStackComponent.AddAction (GamePlayActionStack.ActionTypeEnum.attack,
				myPawnPosIdx, enemyPawnPosInx, attackDirection, onBoardDistance);
		}*/

		return rv;
	}

	public bool DoAttackImplTask(int myPawnPosIdx, int enemyPawnPosInx, attackDirections attackDirection, int onBoardDistance) {
		bool attacked = false;
		bool contrAttacked = false;

		Debug.Log("Do attack on pawn: " + myPawnPosIdx + " to enemy: " + enemyPawnPosInx + ". Distance is: " + onBoardDistance);
		if ((Board[myPawnPosIdx].Pawn != null) && (Board[enemyPawnPosInx].Pawn != null)) {
			Pawn FriendlyPawn = Board [myPawnPosIdx].Pawn.GetComponent<Pawn> ();
			Pawn UnFriendlyPawn = Board [enemyPawnPosInx].Pawn.GetComponent<Pawn> ();
			int ConfigIndex = DirToPawnConfigIndex (attackDirection, FriendlyPawn.RotationPosIndex, false);
			int EnemyConfigIndex = DirToPawnConfigIndex (attackDirection, UnFriendlyPawn.RotationPosIndex, true);
			attackDirections EnemyAttackDirection = GetOpposedAttackDir (attackDirection);
			int MyDamage = FriendlyPawn.Attack;
			int EnemyDamage = UnFriendlyPawn.Attack;

			if (attackDirection == attackDirections.Other) {
				FriendlyPawn.GiveDamage (UnFriendlyPawn, attackDirection, skipAnimations);
				attacked = true;
			} else {
				if ((FriendlyPawn.PawnConfiguration [ConfigIndex].melee) && (onBoardDistance <= 1)) {
					//Rule 2.1, 2.3
					Debug.Log ("melee attack (idx: " + ConfigIndex + ")");
					FriendlyPawn.GiveDamage (UnFriendlyPawn, attackDirection, skipAnimations);
					FriendlyPawn.ResetConfigPosHighlight ();
					attacked = true;
					if (UnFriendlyPawn.PawnConfiguration [EnemyConfigIndex].melee) {
						//Rule 2.2
						Debug.Log ("enemy melee attack (idx: " + ConfigIndex + ")");
						UnFriendlyPawn.GiveDamage (FriendlyPawn, EnemyAttackDirection, skipAnimations);
						contrAttacked = true;
					} else {
						Debug.Log ("  no enemy attack at config index: " + EnemyConfigIndex);
					}
					if ((UnFriendlyPawn.PawnConfiguration [EnemyConfigIndex].attack) &&
					    ((FriendlyPawn.PawnConfiguration [ConfigIndex].block == false) ||
					    (UnFriendlyPawn.IgnorsShields))) {
						//Rule 2.2, 2.4, 2.5
						Debug.Log ("enemy distance attack (idx: " + ConfigIndex + ")");
						UnFriendlyPawn.GiveDamage (FriendlyPawn, EnemyAttackDirection, skipAnimations);
						contrAttacked = true;
					}
				}
				if (((FriendlyPawn.PawnConfiguration [ConfigIndex].attack) &&
					(UnFriendlyPawn.PawnConfiguration [EnemyConfigIndex].block == false)) ||
					(FriendlyPawn.IgnorsShields)) {
					//Rule 2.1, 2.4
					Debug.Log ("distance attack (idx: " + ConfigIndex + ")");
					FriendlyPawn.GiveDamage (UnFriendlyPawn, attackDirection, skipAnimations);
					attacked = true;
					if ((FriendlyPawn.Friendly != UnFriendlyPawn.Friendly) || (UnFriendlyPawn.FriendlyFireEnabled)) {
						if ((UnFriendlyPawn.PawnConfiguration [EnemyConfigIndex].attack) &&
						   ((FriendlyPawn.PawnConfiguration [ConfigIndex].block == false) ||
						   (UnFriendlyPawn.IgnorsShields))) {
							//Rule 2.2, 2.4, 2.5
							Debug.Log ("enemy distance attack (idx: " + ConfigIndex + ")");
							UnFriendlyPawn.GiveDamage (FriendlyPawn, EnemyAttackDirection, skipAnimations);
							contrAttacked = true;
						} else if ((UnFriendlyPawn.PawnConfiguration [EnemyConfigIndex].melee) && (onBoardDistance <= 1)) {
							Debug.Log ("enemy melee attack (idx: " + ConfigIndex + ")");
							UnFriendlyPawn.GiveDamage (FriendlyPawn, EnemyAttackDirection, skipAnimations);
							contrAttacked = true;
						}
					}
				} else {
					Debug.Log ("friendly attack" + ConfigIndex + " " + FriendlyPawn.PawnConfiguration [ConfigIndex].attack +
						" ufriendly block " + EnemyConfigIndex + " " + UnFriendlyPawn.PawnConfiguration [EnemyConfigIndex].block);
				}
			}
			if (!skipAnimations) {
				if (attacked && contrAttacked) {
					ActionStackComponent.DoActionAnimation (
						GamePlayActionStack.ActionTypeEnum.attackAndCounter,
						FriendlyPawn, UnFriendlyPawn,
						MyDamage, EnemyDamage,
						attackDirection, onBoardDistance);
				} else if (attacked) {
					ActionStackComponent.DoActionAnimation (
						GamePlayActionStack.ActionTypeEnum.attack,
						FriendlyPawn, UnFriendlyPawn,
						MyDamage, EnemyDamage,
						attackDirection, onBoardDistance);
				}
			}
			CheckPawnAttackConfigVisible ();
			CheckDeadPawns ();
			if (!FriendlyPawn.Friendly || !UnFriendlyPawn.Friendly) {
				if (attacked && contrAttacked) {
					GameLogComponent.AddLogAction (GamePlayActionStack.ActionTypeEnum.attackAndCounter, FriendlyPawn, UnFriendlyPawn.pawnBoardID);
				} else if (attacked) {
					GameLogComponent.AddLogAction (GamePlayActionStack.ActionTypeEnum.attack, FriendlyPawn, UnFriendlyPawn.pawnBoardID);
				}
			}
		} else {
			Debug.LogError("Missing pawns for attack");
		}
		SetBoardPiecesNormalColor ();
		UpdatePawnsState ();
		return attacked;
	}

	public void ShowAttackAnimation(Pawn FriendlyPawn, Pawn UnFriendlyPawn,
		int FriendDamage, int EnemyDamage,
		attackDirections attackDirection, bool attack, bool counter)
	{
		if (attack) {
			FriendlyPawn.GiveDamageAnimation (UnFriendlyPawn, attackDirection, FriendDamage, skipAnimations);
		}
		if (counter) {
			attackDirections EnemyAttackDirection = GetOpposedAttackDir (attackDirection);
			UnFriendlyPawn.GiveDamageAnimation (FriendlyPawn, EnemyAttackDirection, EnemyDamage, skipAnimations);
		}
		WaitForAttackFinish (FriendlyPawn, UnFriendlyPawn);
	}

	private bool CheckCanAttack(Pawn pawn, int enemyPawnPosInx, attackDirections attackDirection, int onBoardDistance) {
		bool canAttack = false;
		Pawn FriendlyPawn;
		Pawn UnFriendlyPawn;
		if ((pawn != null) && (Board[enemyPawnPosInx].Pawn != null)) {
			FriendlyPawn = pawn;// Board [myPawnPosIdx].Pawn.GetComponent<Pawn> ();
			UnFriendlyPawn = Board [enemyPawnPosInx].Pawn.GetComponent<Pawn> ();
			if (FriendlyPawn.Fake || UnFriendlyPawn.Fake) {
				return false;
			}
			int ConfigIndex = DirToPawnConfigIndex (attackDirection, FriendlyPawn.RotationPosIndex, false);
			int EnemyConfigIndex = DirToPawnConfigIndex (attackDirection, UnFriendlyPawn.RotationPosIndex, true);
			if ((FriendlyPawn.PawnConfiguration [ConfigIndex].melee) && (onBoardDistance <= 1)) {
				canAttack = true;
				FriendlyPawn.ShowConfigPosHighlight (ConfigIndex, true, false);
			}
			if (FriendlyPawn.PawnConfiguration [ConfigIndex].attack) {
				if ((UnFriendlyPawn.PawnConfiguration [EnemyConfigIndex].block == false || FriendlyPawn.IgnorsShields)) {
					canAttack = true;
					FriendlyPawn.ShowConfigPosHighlight (ConfigIndex, true, false);
				} else {
					UnFriendlyPawn.ShowConfigPosHighlight (ConfigIndex, false, true);
				}
			}
		} else {
			Debug.LogError("Missing pawns for attack rules");
		}
		//Debug.Log("Pawn can attack = " + canAttack);
		return canAttack;
	}

	public GameObject CreateAttackMark(int boardIndex, int attackValue) {
		Vector3 markPos;
		Vector3 prevScale;
		SmothTransform Animations;
		GameObject mark = (GameObject)Instantiate (AttackMarkPrefab);//, MainCanvas.transform);
		mark.transform.SetParent (this.transform);
		markPos = Board [boardIndex].BoardPiece.transform.localPosition;
		mark.transform.localRotation = Quaternion.Euler(new Vector3(0,0,0));
		prevScale = mark.transform.localScale;
		mark.transform.localScale = new Vector3 (prevScale.x + 0.5f, prevScale.y + 0.5f, prevScale.z + 0.5f);
		Animations = mark.GetComponent<SmothTransform> ();
		Animations.SmothTransformTo(Quaternion.Euler(new Vector3(0,0,225)), 5);
		//mark.transform.Rotate(new Vector3(0,0,45));
		Animations.SmoothScaleTo (prevScale, 10);
		markPos.z = -0.5f;
		mark.transform.localPosition = markPos;
		/*Vector3 markPos = Board [boardIndex].BoardPiece.transform.position;
	    markPos.z = markPos.z-0.5f;
	    mark.transform.position = markPos;
	    mark.GetComponent<Text> ().text = attackValue.ToString ();*/
		return mark;
	}

	public void CreateMarkBeamFullControll(int startBoardIndex, int endBoardIndex) {
		bool first = false;
		Vector3 startPos = Board [startBoardIndex].BoardPiece.transform.position;
		Vector3 endPos = Board [endBoardIndex].BoardPiece.transform.position;
		Vector3 dir = endPos - startPos;
		Vector3 mid = (dir) / 2.0f + startPos;
		Quaternion toRot = Quaternion.FromToRotation(Vector3.up, dir);
		if (MarkBeamOb == null) {
			MarkBeamOb = (GameObject)Instantiate (MarkBeam);
			MarkBeamOb.transform.SetParent (transform.root);
			first = true;
		}
		//MarkBeamOb.transform.position = mid;
		Quaternion currRot = MarkBeamOb.transform.localRotation;
		Vector3 eCurrRot = currRot.eulerAngles;
		eCurrRot.y = -90;
		eCurrRot.z = 90;
		eCurrRot.x = toRot.eulerAngles.z - 90;

		if (!YouStarted) {
			eCurrRot.x += 180;
		}
		//MarkBeamOb.transform.localRotation = Quaternion.Euler(eCurrRot);
		Vector3 newScale = MarkBeamOb.transform.localScale;
		newScale.z = (dir.magnitude/4.5f) - 0.1f;
		newScale.z = newScale.z / transform.localScale.z;
		//MarkBeamOb.transform.localScale = newScale;
		//Vector3 moveUp = MarkBeamOb.transform.localPosition;
		//moveUp.z -= 0.3f;
		//MarkBeamOb.transform.localPosition = moveUp;
		if (first) {
			MarkBeamOb.transform.position = mid;
			MarkBeamOb.transform.localRotation = Quaternion.Euler(eCurrRot);
			MarkBeamOb.transform.localScale = newScale;
		} else {
			MarkBeamOb.GetComponent<SmothTransform> ().SmoothGlobalTransformTo (mid, 15);
			MarkBeamOb.GetComponent<SmothTransform> ().SmothTransformTo (Quaternion.Euler (eCurrRot), 15);
			MarkBeamOb.GetComponent<SmothTransform> ().SmoothScaleTo (newScale, 15);
		}
	}

	private void CreateAttackBeamImpl(int startBoardIndex, int endBoardIndex, bool enemy) {
		bool first = false;
		Vector3 startPos = Board [startBoardIndex].BoardPiece.transform.position;
		Vector3 endPos = Board [endBoardIndex].BoardPiece.transform.position;
		Vector3 dir = endPos - startPos;
		Vector3 mid = (dir) / 2.0f + startPos;
		Quaternion toRot = Quaternion.FromToRotation(Vector3.up, dir);
		if (AttackMarkBeamOb == null) {
			AttackMarkBeamOb = (GameObject)Instantiate (MarkBeam);
			AttackMarkBeamOb.transform.SetParent (transform.root);
			first = true;
		}
		//AttackMarkBeamOb.transform.position = mid;
		Quaternion currRot = AttackMarkBeamOb.transform.localRotation;
		Vector3 eCurrRot = currRot.eulerAngles;
		eCurrRot.y = -90;
		eCurrRot.z = 90;
		eCurrRot.x = toRot.eulerAngles.z - 90;

		//if (!YouStarted) {
			//eCurrRot.x += 180;
		//}
		//AttackMarkBeamOb.transform.localRotation = Quaternion.Euler(eCurrRot);
		Vector3 newScale = AttackMarkBeamOb.transform.localScale;
		newScale.z = (dir.magnitude/4.5f) - 0.1f;
		newScale.z = newScale.z / transform.localScale.z;
		if (first) {
			AttackMarkBeamOb.transform.position = mid;
			AttackMarkBeamOb.transform.localRotation = Quaternion.Euler(eCurrRot);
			AttackMarkBeamOb.transform.localScale = newScale;
		} else {
			AttackMarkBeamOb.GetComponent<SmothTransform> ().SmoothGlobalTransformTo (mid, 15);
			AttackMarkBeamOb.GetComponent<SmothTransform> ().SmothTransformTo (Quaternion.Euler (eCurrRot), 15);
			AttackMarkBeamOb.GetComponent<SmothTransform> ().SmoothScaleTo (newScale, 15);
		}
	}

	public void CreateAttackBeam(int startBoardIndex, int endBoardIndex) {
		CreateAttackBeamImpl (startBoardIndex, endBoardIndex, false);
	}

	private void CreateEnemyAttackBeam(int startBoardIndex, int endBoardIndex) {
		CreateAttackBeamImpl (startBoardIndex, endBoardIndex, true);
	}

	public void DestroyMarkBeamFullControll() {
		if (MarkBeamOb) {
			//Debug.Log ("Destroy mark beam");
			//Destroy (MarkBeamEndOb);
			Destroy (MarkBeamOb);
			//Destroy (AttackMarkOb);
			if (onlineMode && enemyMarkBeamShowed) {
				enemyMarkBeamShowed = false;
				myPView.RPC ("RPCDestroyMarkBeam", PhotonTargets.Others);
			}
		}
	}

	public void DestroyMarkBeam() {
		if (AttackMarkBeamOb) {
			//Destroy (AttackMarkBeamEndOb);
			Destroy (AttackMarkBeamOb);
			//Destroy (AttackMarkOb);
			if (onlineMode && enemyMarkBeamShowed) {
				enemyMarkBeamShowed = false;
				myPView.RPC ("RPCDestroyMarkBeam", PhotonTargets.Others);
			}
		}
	}

	public void SetBoardPiecesNormalColor() {
		Debug.Log ("Set board normal colors");
		if (!boardPieceNormalColorIsSet) {
			boardPieceNormalColorIsSet = true;
			for (int i = 0; i < IndexMAX; i++) {
				Board [i].BoardPiece.GetComponent<MeshRenderer> ().material.color = BoardPieceNormalColor;
			}
		}
	}

	private void SetAllBoardPiecesInactive() {
		boardPieceNormalColorIsSet = false;
		for (int i = 0; i < IndexMAX; i++) {
			Board[i].BoardPiece.GetComponent<MeshRenderer> ().material.color = BoardPieceInactiveColor;
		}
	}

	public void UnSelectAllTargets() {
		for (int i = 0; i < IndexMAX; i++) {
			if (Board [i].Pawn != null && Board [i].Pawn.GetComponent<Pawn> ().Target) {
				Board [i].Pawn.GetComponent<Pawn> ().UnSetAsTarget ();
			}
		}
	}

	public void HideTargetNotification() {
		HandComp.EnableOtherCards ();
		TargetDescriptionNotificationPanel.GetComponent<SmothTransform> ().SmothTransformTo (TargetDescriptionNotificationPanelOriginalPos, 15);
	}

	public void ShowTargetNotification(string message) {
		SmothTransform STrans = TargetDescriptionNotificationPanel.GetComponent<SmothTransform> ();
		TargetDescriptionNotificationText.text = message;
		Vector3 newPos = new Vector3 (
			TargetDescriptionNotificationPanelOriginalPos.x,
			273f,
			TargetDescriptionNotificationPanelOriginalPos.z);
		STrans.SmothTransformTo (newPos, 15);
		Debug.Log ("curr Y pos is " + TargetDescriptionNotificationPanel.transform.localPosition.y);
	}

	private void ChooseRandomTarget() {
		List<int> pawns = new List<int> ();
		bool found = false;
		if ((TargetSelectCallback != null) && (TargetSelectCallbackMustBeCalled)) {
			TargetSelectCallbackMustBeCalled = false;
			for (int i = 0; i < IndexMAX; i++) {
				if ((Board [i].Pawn != null) &&
					(!Board [i].Pawn.GetComponent<Pawn> ().Death) &&
					(Board [i].Pawn.GetComponent<Pawn> ().Target)){
					pawns.Add (i);
					found = true;
					//SelectTaget (Board [i].Pawn, TargetSelectCallback);
				}
			}
		}
		if (found) {
			System.Random rnd = new System.Random ();
			int idx = rnd.Next (pawns.Count);
			SelectTaget (Board [(int)pawns[idx]].Pawn, TargetSelectCallback);
		}
	}

	public void SelectTaget (GameObject selectedPawn, TargetCallback callback) {
		if ((selectedPawn != null) && (callback != null)) {
			callback (selectedPawn.GetComponent<Pawn> ().boardPosisionIndex);
			TargetSelectCallback = null;//clear when its done
		}
		UnSelectAllTargets ();
		SetBoardPiecesNormalColor ();
		EnableOtherPawnOnBoard ();
		HideTargetNotification ();
		HandComp.EnableOtherCards ();
	}

	public void SetTaget (GameObject pawn, TargetCallback callback) {
		if (!pawn.GetComponent<Pawn> ().Death) {
			if (callback != null) {
				pawn.GetComponent<Pawn> ().SetAsTarget (callback);
			}
			TargetsList.Add (pawn.GetComponent<Pawn> ().boardPosisionIndex);
		}
	}

	/// <summary>
	/// Set targets to attack.
	/// </summary>
	/// <returns>List of founded targets indexes.</returns>
	/// <param name="pawn">Pawn that want to attack.</param>
	/// <param name="FriendlyFire">Pawn have friendly fire.</param>
	/// <param name="setTargets">If false setting targets will be skiped.</param>
	public List<int> SetAttackTargets(Pawn pawn, bool FriendlyFire, bool setTargets) {
		int index = pawn.boardPosisionIndex;
		int startIndex = index;
		int directionIdx = 0;
		List<int> targetsIndex = new List<int> ();

		if (setTargets) {
			UnSelectAllTargets ();
			TargetSelectCallbackMustBeCalled = false;
		}

		pawn.ResetConfigPosHighlight ();

		do {
			bool goToNextDir = false;
			attackDirections attackDir;
			int boardDistance = 1;
			int newIndex;
	
			if (directionIdx == 0) {
				newIndex = GetBoardIndexUp (index);
				attackDir = attackDirections.Up;
			} else if (directionIdx == 1) {
				newIndex = GetBoardIndexDown (index);
				attackDir = attackDirections.Down;
			} else if (directionIdx == 2) {
				newIndex = GetBoardIndexUpLeft (index);
				attackDir = attackDirections.UpLeft;
			} else if (directionIdx == 3) {
				newIndex = GetBoardIndexUpRight (index);
				attackDir = attackDirections.UpRight;
			} else if (directionIdx == 4) {
				newIndex = GetBoardIndexDownLeft (index);
				attackDir = attackDirections.DownLeft;
			} else if (directionIdx == 5) {
				newIndex = GetBoardIndexDownRight (index);
				attackDir = attackDirections.DownRight;
			} else {
				break;
			}

			index = newIndex;

			if (newIndex != -1) {
				int ConfigIndex = DirToPawnConfigIndex (attackDir, pawn.RotationPosIndex, false);
				if (ConfigIndex >= 0 && ConfigIndex <= 5) {
					if (!pawn.PawnConfiguration [ConfigIndex].attack) {
						goToNextDir = true;
					}
				} else {
					Debug.LogWarning("Bad config index: " + ConfigIndex);
				}
				if (!IsFreePosision (newIndex)) {
					if (Board [newIndex].Pawn.GetComponent<Pawn> ().Friendly != pawn.Friendly || pawn.FriendlyFireEnabled) {
						if (pawn.gameObject != Board [newIndex].Pawn) {// make sure it not attack itself
							if (CheckCanAttack (pawn, newIndex, attackDir, boardDistance)) {
								if (setTargets) {
									SetTaget(Board [newIndex].Pawn, pawn.PawnAttackTargetCallback);
									TargetSelectCallback = pawn.PawnAttackTargetCallback;
								}
								targetsIndex.Add(newIndex);
							}
						}
					}
					goToNextDir = true;
				}
			} else {
				goToNextDir = true;
			}

			if (goToNextDir) {
				directionIdx++;
				index = startIndex;
				boardDistance = 1;
			} else {
				boardDistance++;
			}
		} while (true);

		return targetsIndex;
	}

	public bool SetTargetsOnBoard(TargetSettings settings, GameObject Owner, TargetCallback callback) {
		bool TargetFound = false;

		TargetsList.Clear ();

		if ((settings & TargetSettings.WithPawnsLock) == TargetSettings.WithPawnsLock) {
			DisableAllPawnOnBoard ();
		}
		for (int i = 0; i < IndexMAX; i++) {
			if (Board [i].Pawn != null) {
				if (((settings & TargetSettings.WithFriendlyPawn) == TargetSettings.WithFriendlyPawn) &&
				    (Board [i].Pawn.GetComponent<Pawn> ().Friendly == true) && (Board [i].Pawn != myHero) &&
					(Board [i].Pawn != Owner))
				{
					SetTaget (Board [i].Pawn, callback);
					TargetFound = true;
				}
				if (((settings & TargetSettings.WithEnemyPawn) == TargetSettings.WithEnemyPawn) &&
					(Board [i].Pawn.GetComponent<Pawn> ().Friendly == false) && (Board [i].Pawn != enemyHero) &&
					(Board [i].Pawn != Owner))
				{
					SetTaget (Board [i].Pawn, callback);
					TargetFound = true;
				}
				if (((settings & TargetSettings.WithFriendlyHero) == TargetSettings.WithFriendlyHero) &&
					(Board [i].Pawn == myHero) && (Board [i].Pawn != Owner))
				{
					SetTaget (Board [i].Pawn, callback);
					TargetFound = true;
				}
				if (((settings & TargetSettings.WithEnemyHero) == TargetSettings.WithEnemyHero) &&
					(Board [i].Pawn == enemyHero) && (Board [i].Pawn != Owner))
				{
					SetTaget (Board [i].Pawn, callback);
					TargetFound = true;
				}
				if (((settings & TargetSettings.WithOwner) == TargetSettings.WithOwner))
				{
					SetTaget (Board [i].Pawn, callback);
					TargetFound = true;
				}
			}
		}
		if (TargetFound) {
			Debug.Log("Target found");
			HandComp.DisableOtherCards (null);
			TargetSelectCallback = callback;
			if ((settings & TargetSettings.MustBeCalled) == TargetSettings.MustBeCalled) {
				TargetSelectCallbackMustBeCalled = true;
			}
		} else {
			Debug.Log("Target NOT found");
			if ((settings & TargetSettings.WithPawnsLock) == TargetSettings.WithPawnsLock) {
				EnableOtherPawnOnBoard ();
			}
		}

		return TargetFound;
	}

	public int GetClosestOverMouseAnyIndexPosition(Pawn pawn, bool withPawn, bool withFriendlyPawn, bool withUnfriendly, bool recolor) {
		int indexToRet = -1;
		float distance = 0;
		float distanceLast = 10000;
		Vector3 dist;
		Vector3 curPos;
		Vector3 cursorWorldPos;

		if (withPawn) {
			if (recolor) {
				SetAllBoardPiecesInactive ();
			}
		}

		for (int i = 0; i < IndexMAX; i++) {
			if ((withPawn) && (Board [i].Pawn == null)) {
				continue;
			}
			if (withPawn) {
				if (pawn == Board [i].Pawn.GetComponent<Pawn> ()) {
					continue;
				}
				if ((withFriendlyPawn) && (Board [i].Pawn.GetComponent<Pawn> ().Friendly == false)) {
					continue;
				}
				if ((withUnfriendly) && (Board [i].Pawn.GetComponent<Pawn> ().Friendly == true)) {
					continue;
				}
				if ((Board [i].Pawn == myHero) || (Board [i].Pawn == enemyHero)) {
					continue;
				}
				Board [i].BoardPiece.GetComponent<MeshRenderer> ().material.color = BoardPieceNormalColor;
			}
			dist = Camera.main.WorldToScreenPoint (Board [i].BoardPiece.transform.position);
			curPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, dist.z);
			cursorWorldPos = Camera.main.ScreenToWorldPoint(curPos);
			distance = Vector2.Distance (Board [i].BoardPiece.transform.position, cursorWorldPos);
			if (distance < distanceLast) {
				//Debug.Log("Find move index to return:" + indexToRet);
				indexToRet = i;
				distanceLast = distance;
			}
		}
		Debug.Log("Find move index to return:" + indexToRet);
		return indexToRet;
	}
		
	private int GetClosestOverMouseAvailableIndexPositionImplement(Pawn pawn, int index,
		bool specialMovement, bool moveOnly, bool attackOnly) {
		int newIndex = 0;
		int indexToRet = -1;
		float distance = 0;
		float distanceLast = 10000;
		int startIndex = index;
		bool canAttack = false;
		Vector3 dist;
		Vector3 curPos;
		Vector3 cursorWorldPos;

		SetAllBoardPiecesInactive ();

		if (IsFreePosision (index, pawn.gameObject)) {
			Board [index].BoardPiece.GetComponent<MeshRenderer> ().material.color = BoardPieceNormalColor;
			dist = Camera.main.WorldToScreenPoint (Board [index].BoardPiece.transform.position);
			curPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, dist.z);
			cursorWorldPos = Camera.main.ScreenToWorldPoint(curPos);
			distanceLast = distance = Vector3.Distance (Board [index].BoardPiece.transform.position, cursorWorldPos);
			indexToRet = index;
		}
		for (int i = 0; i < 2;i++) {
			if (attackOnly) {
				i = 1;
			}
			bool checkAttack = (i == 1) ? true : false;
			if (checkAttack) {
				startIndex = index = pawn.boardPosisionIndexPrev;
			}
			if (moveOnly && checkAttack) {
				break;
			}
			if (index >= 0) {
				attackDirections attackDir;
				int directionIdx = 0;
				int boardDistance = 1;

				do {
					bool goToNextDir = false;
					if (directionIdx == 0) {
						newIndex = GetBoardIndexUp (index);
						attackDir = attackDirections.Up;
					} else if (directionIdx == 1) {
						newIndex = GetBoardIndexDown (index);
						attackDir = attackDirections.Down;
					} else if (directionIdx == 2) {
						newIndex = GetBoardIndexUpLeft (index);
						attackDir = attackDirections.UpLeft;
					} else if (directionIdx == 3) {
						newIndex = GetBoardIndexUpRight (index);
						attackDir = attackDirections.UpRight;
					} else if (directionIdx == 4) {
						newIndex = GetBoardIndexDownLeft (index);
						attackDir = attackDirections.DownLeft;
					} else if (directionIdx == 5) {
						newIndex = GetBoardIndexDownRight (index);
						attackDir = attackDirections.DownRight;
					} else {
						break;
					}

					index = newIndex;

					if (newIndex != -1) {
						dist = Camera.main.WorldToScreenPoint (Board [index].BoardPiece.transform.position);
						curPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, dist.z);
						cursorWorldPos = Camera.main.ScreenToWorldPoint(curPos);
						distance = Vector2.Distance (Board [newIndex].BoardPiece.transform.position, cursorWorldPos);
						if (checkAttack) {
							//Debug.Log("Check attack(only:" + attackOnly + ")");
							int ConfigIndex = DirToPawnConfigIndex (attackDir, pawn.RotationPosIndex, false);
							if (!pawn.PawnConfiguration [ConfigIndex].attack) {
								goToNextDir = true;
							}
							if (!IsFreePosision (newIndex, pawn.gameObject)) {
								Board [newIndex].BoardPiece.GetComponent<MeshRenderer> ().material.color = BoardPieceNormalColor;
								if (Board [newIndex].Pawn.GetComponent<Pawn> ().Friendly == false) {
									if (distance < distanceLast) {
										/*canAttack = CheckCanAttack (pawn, newIndex, attackDir, boardDistance);
										if (canAttack) {
											CreateAttackBeam (pawn.boardPosisionIndex, newIndex);
											//red dot
											if (AttackMarkOb == null) {
												AttackMarkOb = CreateAttackMark(newIndex, 0);
											}
										}
										pawn.SetCanAttack (canAttack, newIndex, attackDir, boardDistance);
										if (canAttack) {
											//Debug.Log("Find attack index to return:" + indexToRet);
											indexToRet = pawn.boardPosisionIndexPrev;
										}*/ //depracated
									} else {
										canAttack = false;
									}
									distanceLast = distance;
								}
								goToNextDir = true;
							}
						} else {//check for move
							//Debug.Log("Check move(only:" + moveOnly + ") dist last:" + distanceLast + " dist:" + distance);
							if (((boardDistance <= 1) || (specialMovement)) && (IsFreePosision (newIndex, pawn.gameObject))) {
								Board [newIndex].BoardPiece.GetComponent<MeshRenderer> ().material.color = BoardPieceNormalColor;
								if (distance < distanceLast) {
									//Debug.Log("Find move index to return:" + indexToRet);
									indexToRet = newIndex;
									distanceLast = distance;
									canAttack = false;
								}
							}

							if ((!specialMovement) || (!IsFreePosision (newIndex, pawn.gameObject)))
								goToNextDir = true;
						}
					} else {
						goToNextDir = true;
					}

					if (goToNextDir) {
						directionIdx++;
						index = startIndex;
						boardDistance = 1;
					} else {
						boardDistance++;
					}
				} while (true);
			}
		}
		if (!canAttack) {
			//Debug.Log ("Destroy mark beam");
			pawn.PawnCanAttack = false;
			DestroyMarkBeam ();
		}
		//Debug.Log("Returned index" + indexToRet);
		return indexToRet;
	}

	public int GetClosestOverMouseAvailableIndexPosition(Pawn pawn, int index,
		bool specialMovement, bool moveOnly, bool attackOnly) {
		return GetClosestOverMouseAvailableIndexPositionImplement(pawn, index, specialMovement, moveOnly, attackOnly);
	}

	public int GetClosestOverMouseAvailableStartIndexPosition(Pawn pawn) {
		return GetClosestOverMouseAvailableIndexPositionImplement(pawn, GetBoardIndexByPawn(myHero), false, true, false);
	}

	private void ResetPawnAttackConfigVisible() {
		for (int i = 0; i < IndexMAX; i++) {
			if (!IsFreePosision (i)) {
				Pawn pawn = Board [i].Pawn.GetComponent<Pawn> ();
				for (int j = 0; j < Pawn.PawnConfigNumber; j++) {
					pawn.MovePawnConfigPos (j, true);
				}
			}
		}
	}

	private void CheckPawnAttackConfigVisible() {
		ResetPawnAttackConfigVisible ();
		for (int i = 0; i < IndexMAX; i++) {
			int directionIdx = 0;
			do {
				attackDirections attackDir;
				int newIndex;

				if (directionIdx == 0) {
					newIndex = GetBoardIndexUp (i);
					attackDir = attackDirections.Up;
				} else if (directionIdx == 1) {
					newIndex = GetBoardIndexDown (i);
					attackDir = attackDirections.Down;
				} else if (directionIdx == 2) {
					newIndex = GetBoardIndexUpLeft (i);
					attackDir = attackDirections.UpLeft;
				} else if (directionIdx == 3) {
					newIndex = GetBoardIndexUpRight (i);
					attackDir = attackDirections.UpRight;
				} else if (directionIdx == 4) {
					newIndex = GetBoardIndexDownLeft (i);
					attackDir = attackDirections.DownLeft;
				} else if (directionIdx == 5) {
					newIndex = GetBoardIndexDownRight (i);
					attackDir = attackDirections.DownRight;
				} else {
					break;
				}

				if (newIndex != -1) {
					GameObject pawnOb = Board [i].Pawn;
					if (pawnOb != null) {
						Pawn pawn = pawnOb.GetComponent<Pawn>();
						int ConfigIndex = DirToPawnConfigIndex (attackDir, pawn.RotationPosIndex, false);
						SetAttackTargets(pawn, pawn.Friendly, false);
						if (pawn.PawnConfiguration [ConfigIndex].attack || pawn.PawnConfiguration [ConfigIndex].melee) {
							if (!IsFreePosision (newIndex)) {
								Pawn pawnSecond = Board [newIndex].Pawn.GetComponent<Pawn>();
								int EnemyConfigIndex = DirToPawnConfigIndex (attackDir, pawnSecond.RotationPosIndex, true);
								if (pawnSecond.PawnConfiguration [EnemyConfigIndex].attack ||
									pawnSecond.PawnConfiguration [EnemyConfigIndex].melee) {
									//Debug.Log ("Move config position on idx " + i + " and " + newIndex);
									pawn.MovePawnConfigPos(ConfigIndex, false);
									pawnSecond.MovePawnConfigPos(EnemyConfigIndex, false);
								}
							}
						}
					}
				}
				directionIdx++;
			} while (true);
		}
	}

	public void WinGame(int i) {
		if (!gameEnd) {
			gameEnd = true;
			ActionStackComponent.DoActionAnimation (GamePlayActionStack.ActionTypeEnum.win);
		}
	}

	public void DefeatGame(int i) {
		if (!gameEnd) {
			gameEnd = true;
			ActionStackComponent.DoActionAnimation (GamePlayActionStack.ActionTypeEnum.defeat);
		}
	}

	public void GiveUp() {
		if (UsePhotonEventsIsteadOfRPC) {
			DefeatGame(0);
			MyTurnManager.GameplaySendMove (GamePlayActionStack.ActionTypeEnum.giveup, "");
		}
	}

	public void EndGame(bool win) {
		gameEnd = true;
		StopTurnTimer ();
		DisableBoard ();
		WinScreenComponent.ShowWinscreen (win);
	}

	public void DisableBoard() {
		Debug.Log ("Disable board");
		HandComp.DisableOtherCards (null);
		StopTurnTimer ();
		gameEnd = true;
		for (int i = 0; i < IndexMAX; i++) {
			//Debug.Log ("Pawn try disabled Index: " + i);
			if ((Board [i].Pawn != null) && (Board [i].Pawn.GetComponent<Pawn> ().Friendly)) {
				Board [i].Pawn.GetComponent<Pawn> ().DisablePawn ();
			}
		}
	}

	private void SyncBoard() {
		if (onlineMode) {
			for (int i = 0; i < IndexMAX; i++) {
				//Debug.Log ("Pawn try disabled Index: " + i);
				if (Board [i].Pawn != null) {
					Pawn pawnToSync = Board [i].Pawn.GetComponent<Pawn> ();
					if (!pawnToSync.Death) {
						myPView.RPC ("RPCSyncPawn",
							PhotonTargets.Others,
							pawnToSync.Name,
							pawnToSync.pawnBoardID,
							pawnToSync.boardPosisionIndex,
							pawnToSync.RotationPosIndex,
							pawnToSync.Health,
							pawnToSync.Attack);
					}
				}
			}
		}
	}

	public void HandleBoardSync(string pawnName, int pawnBoardID, int boardPosisionIndex,
		int boardRotationIndex , int pawnHealth, int pawnAttack)
	{
		Pawn foundedPawn = GetBoardPawnByID (pawnBoardID);
		string message = "";
		string eventName = "";
		Dictionary<string, object>  eventReport = new Dictionary<string, object> ();
		bool errorFound = false;

		if (foundedPawn != null) {
			eventName = "state_missmatch";
			if ((foundedPawn.boardPosisionIndex != boardPosisionIndex) ||
			    (foundedPawn.RotationPosIndex != boardRotationIndex)) {
				errorFound = true;
				eventReport.Add ("LocalBoardPosIdx", foundedPawn.boardPosisionIndex.ToString ());
				eventReport.Add ("ReportedBoardPosIdx", boardPosisionIndex.ToString ());
				eventReport.Add ("LocalRotationIdx", foundedPawn.RotationPosIndex.ToString ());
				eventReport.Add ("ReportedRotationIdx", boardRotationIndex.ToString ());
				message = "Critical clients board missmatch: Bad position state boardIdx:" +
					boardPosisionIndex + " Pawn Rot:" + boardRotationIndex;
				if (BoardClientsForceSync || BoardClientsForceSyncPositions) {
					ChangeEnemyPawnPos (pawnBoardID, boardRotationIndex, boardPosisionIndex);
					ConfirmEnemyPawnPos (pawnName, pawnBoardID, boardRotationIndex, boardPosisionIndex);
				}
			}
			if (foundedPawn.Health != pawnHealth) {
				errorFound = true;
				eventReport.Add ("LocalHealth", foundedPawn.Health.ToString ());
				eventReport.Add ("ReportedHealth", pawnHealth.ToString ());
				message = "Critical clients board missmatch" + pawnName + ": Bad health value:" +
					foundedPawn.Health + " Should be:" + pawnHealth;
				if (BoardClientsForceSync) {
					foundedPawn.SetHealth (pawnHealth);
				}
			}
			if (foundedPawn.Attack != pawnAttack) {
				errorFound = true;
				eventReport.Add ("LocalAttack", foundedPawn.Attack.ToString ());
				eventReport.Add ("ReportedAttack", pawnAttack.ToString ());
				message = "Critical clients board missmatch " + pawnName + ": Bad attack value:" +
					foundedPawn.Attack + " Should be:" + pawnAttack;
				if (BoardClientsForceSync) {
					foundedPawn.SetAttack (pawnAttack);
				}
			}
		} else {
			errorFound = true;
			eventName = "pawns_missmatch";
			eventReport.Add ("ReportedBoardPosIdx", boardPosisionIndex.ToString ());
			message = "Critical clients board missmatch: Cannot find pawn ID:" +
				pawnBoardID + " Named:" + pawnName;
		}
		if (errorFound) {
			eventReport.Add ("PawnName", pawnName);
			eventReport.Add ("PawnBoardID", pawnBoardID.ToString ());
			eventReport.Add ("EnemyPlayer", OnlinePlayersNameComponent.myPlayerName);

			Dictionary<string, object>  globalEventReport = new Dictionary<string, object> ();
			globalEventReport.Add("Player", OnlinePlayersNameComponent.myPlayerName);
			globalEventReport.Add("Enemy", OnlinePlayersNameComponent.enemyPlayerName);
			Debug.LogError (message);

			WriteClientPlayerEventRequest request = new WriteClientPlayerEventRequest ();
			WriteTitleEventRequest globalRequest = new WriteTitleEventRequest ();
			request.EventName = eventName;
			request.Body = eventReport;

			globalRequest.EventName = eventName;
			globalRequest.Body = globalEventReport;

			PlayFabClientAPI.WritePlayerEvent (request, OnPlaySendEventSuccess, OnPlayFabError);
			PlayFabClientAPI.WriteTitleEvent (globalRequest, OnPlaySendEventSuccess, OnPlayFabError);

			ErrorCanvas.enabled = true;
			ErrorText.text = "Pojedynek:" + OnlinePlayersNameComponent.myPlayerName +
				" vs " + OnlinePlayersNameComponent.enemyPlayerName +
				"\n Czas:" + System.DateTime.Now + "\n" + message;
		}
	}

	private void OnPlaySendEventSuccess(WriteEventResponse result)
	{
		Debug.LogWarning ("Report sended");
	}

	private void OnPlayFabError(PlayFabError error)
	{
		Debug.LogWarning ("Got an error: " + error.ErrorMessage);
	}

	public void ShowWarning(string msg) {
		WarningText.text = msg;
		WarningAnimationController.SetTrigger ("ShowWarning");
	}
}