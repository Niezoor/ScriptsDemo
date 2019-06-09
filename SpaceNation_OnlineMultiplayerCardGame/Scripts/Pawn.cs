using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using ParticlePlayground;
using UnityEngine.EventSystems;

public class Pawn : MonoBehaviour {
	public static float PawnPosUp = -0.4f;
	public static float PawnPosDown = -0.08f;
	public static int PawnConfigNumber = 6;
	[System.Serializable]
	public class PawnConfigurationClass {
		public bool block;
		public bool attack;
		public bool melee;
	}

	[System.Serializable]
	public enum PawnShootingModeEnum {
		singleShot,
		simultaneousShot,
		autoAim,
		projecitileSimul,
		projecitileSingle,
		laser,
	}

	public PawnConfigurationClass[] OriginalPawnConfiguration = new PawnConfigurationClass[PawnConfigNumber];
	private bool originalConfigApplied = false;
	private bool OriginalSpecialMovement = false;

	public float onPlayScale = 0.16f;
	public float onPlayScaleAsWeapon = 0.12f;
	public float onCardScale = 0.85f;

	[Header("Pawn setup")]
	/// <summary>Pawn/Card ID.</summary>
	public int CardID;
	/// <summary>Pawn/Card name.</summary>
	public string Name;
	/// <summary>Pawn/Card description.</summary>
	public string Desc;

	public CardsBase.CardTypesEnum CardType;
	public PawnConfigurationClass[] PawnConfiguration = new PawnConfigurationClass[PawnConfigNumber];

	/// <summary>Health value with buffs and debuffs.</summary>
	public  int Health;
	/// <summary>Maximum health value without buffs. To check pawn is hurt</summary>
	public  int MaxHealth;
	/// <summary>All health buffs and debuffs value<summary>
	public int BuffedHealth = 0;
	/// <summary>Original health value<summary>
	private int originalHealth;
	/// <summary>Attack value with buffs and debuffs.</summary>
	public  int Attack;
	/// <summary>Maximum attack value</summary>
	public  int MaxAttack;
	/// <summary>All attack buffs and debuffs value<summary>
	public int BuffedAttack = 0;
	/// <summary>Original attack value<summary>
	private int originalAttack;

	/// <summary>Currently displayed health value.</summary>
	private  int DisplayedHealth;
	/// <summary>Currently displayed attack value.</summary>
	private  int DisplayedAttack;

	public bool SpecialMovement;
	/// <summary>Can shout to friends, now it heal instead of deal damage.</summary>
	public bool FriendlyFireEnabled = false;

	public bool Friendly = true;
	public bool PawnCanAttack = false;

	[Header("Pawn effects states")]
	public bool Poisoned = false;
	public bool Burning = false;
	public int BurningTurnsLeft = 0;
	public bool Frozen = false;
	private bool FrozenNextTurn = false;

	[Header("Pawn item/weapon")]
	public string WeaponName;
	public int WeaponUseCount = 0;
	public int WeaponDamage = 0;
	private GameObject WeaponOb;
	public bool ItemApplyConfig = false;
	public bool ItemMergeConfig = false;

	[Header("Pawn board position")]
	public int RotationPosIndex = -1;
	public int handIndex = 0;
	public int pawnBoardID = -1;

	public int boardPosisionIndex = -1;
	public int boardPosisionIndexPrev;
	public int boardSavedPosisionIndexPrev = -1;
	public int boardSavedRotationIndexPrev = -1;

	[Header("Pawn object components")]
	public GameObject Character;
	public Transform PawnConfigPosHandler;
	public Transform[] PawnConfigPositions = new Transform[PawnConfigNumber];
	public GameObject BlockPre;
	public GameObject MeleeAttackPre;
	public GameObject DistAttackPre;
	public GameObject SpecialMovemPre;
	public SpriteRenderer Select;
	public SpriteRenderer InnerSelect;
	public SpriteRenderer Border;
	public GameObject ReceivedDamageFloatText;
	public BoxCollider BulletStopCollider;
	public TextMeshPro AttackText;
	public TextMeshPro HealthText;
	public TextMeshPro WeaponUseCounterText;
	public PawnRotateButton RotateButton;

	public GameObject HealParticleEffect;
	public GameObject HealTargetParticleEffect;
	public GameObject FireParticleEffect;
	private GameObject FireParticleEffectOb;
	public GameObject BuffParticleEffect;
	public GameObject FrozenPrefab;
	private GameObject FrozenPrefabOb;

	public Color FriendlyMarkColor;
	public Color EnemyMarkColor;
	public Color ConfigNormalColor;
	public Color ConfigCanAttackColor;

	public TMP_FontAsset pawnNormalHealthFontAsset;
	public TMP_FontAsset pawnHurtHealthFontAsset;
	public TMP_FontAsset pawnBuffedHealthFontAsset;

	public TMP_FontAsset pawnNormalAttackFontAsset;
	public TMP_FontAsset pawnBuffedAttackFontAsset;

	public PolygonCollider2D PawnCollider;

	public GameObject PawnZZZPrefab;
	private List<GameObject> ZZZObjects = new List<GameObject> ();

	[Header("Pawn audio components")]
	public AudioClip DeathSound;
	public AudioClip MoveSound;
	public AudioClip SelectSound;
	public AudioClip PlaySound;
	public AudioClip BuffSound;
	public AudioClip HealSound;
	public AudioSource AudioSourceComponent;

	[Header("Pawn Animations")]
	public string DeathAnimationName;
	public Animation AnimationComponent;

	[Header("Pawn shoting")]
	public GameObject ShotParticlePrefab;
	public PawnShootingModeEnum ShootingMode = PawnShootingModeEnum.singleShot;
	public float ShootInterval;

	public bool IgnorsShields = false;

	[Header("Pawn states")]
	public bool Fake = false;
	public bool Death = false;
	public bool DeathBloked = false;
	public bool DeathPending = false;
	public bool Selected = false;
	public bool Target   = false;
	public bool Charge = false;
	public bool RotationDisabled = false;
	public bool ManaConsumed = false;
	private GameObject SpecialMovementOb;

	public GamePlay gamePlayComp;

	private Transform boardTransform;

	[System.Serializable]
	public enum pawnStates {
		//| move | select | attack |
		idle,        //| X    | X      | X      |
		onboard_idle,//| X    | V      | V      |
		playable,    //| V    | X      | V      |
	};
	public pawnStates pawnState;
	public bool AttackAlready = false;
	public bool AttackOnly = false;
	public bool AttackDone = false;

	//private Vector3 dist;
	private float startRot;
	private float startAngle;
	private bool isNowDrag = false;
	private bool isDragSetup = false;
	private bool isNowRotate = false;
	private bool isNowPressed = false;
	public bool isFirstPlay = true;
	public bool isPosisionChanged = false;

	private SpriteRenderer cardSpriteRender;
	private SmothTransform SmoothTrans;
	private bool clickDetect = false;
	private bool clickStillDetect = false;
	private Vector2 startMousePos;
	private float maxClickDelta = 0.02f;
	public bool PawnUnpressed = false;

	Vector3 origScaleHealthText;
	Vector3 origScaleAttackText;

	private bool isIsHighlightSetuped = false;
	private bool isIsHighlightShowing = false;
	public float HighLightShowDelay = 0.2f;

	private int enemyToAttackIndex = -1;
	private int enemyPawnkDistance = -1;
	private GamePlay.attackDirections directionToAttack;
	private Pawn enemyPawnToAttack;
	private int BulletDamageValue;
	private int WaitForEventsNumber;
	private int WaitForEventsNumberCurrent;

	/* card callbacks */
	public delegate int  CardPlayCallback (int TargetBoardFieId);
	public delegate void CardCallback     (int TargetBoardFieId);
	public delegate void TriggerCallback  (int StartBoardFieldId, int TargetBoardFieId);
	public delegate List<int> AITriggerCallback  (int BoardFieldId);
	public CardPlayCallback  OnPlayCallback = null;
	public CardCallback      OnPlayCancelCallback  = null;
	public CardCallback      OnApplyItemCallback  = null;//callback on item/weapon object
	public CardCallback      OnDetachItemCallback  = null;//it should be registered by item/weapon on pawn object
	public CardCallback      OnDeselectCallback  = null;
	public CardCallback      OnChangePositionCallback  = null;
	public CardCallback      AttackRulesOverrideCallback  = null;
	public CardCallback      OnBoardUpdate = null;

	public GamePlay.TargetCallback PawnTargetCallback = null;
	public TriggerCallback TriggerEffectCallback = null;
	public AITriggerCallback AITriggerEffectCallback = null;

	private List<CardCallback> onGetDamageCallbackList = new List<CardCallback> ();
	private List<CardCallback> onDeathCallbackList = new List<CardCallback> ();
	private List<CardCallback> onPlayConfirmCallbackList = new List<CardCallback> ();
	private List<CardCallback> onLocalPlayConfirmCallbackList = new List<CardCallback> ();
	private List<CardCallback> onAttackCallbackList = new List<CardCallback> ();
	private List<CardCallback> onMoveCallbackList = new List<CardCallback> ();

	private List<CardCallback> onNewTurnCallbackList = new List<CardCallback> ();
	private List<CardCallback> onKillCallbackList = new List<CardCallback> ();
	private List<CardCallback> onSomeOneDiedCallbackList = new List<CardCallback> ();
	private List<CardCallback> onBoardUpdateList = new List<CardCallback> ();

	private List<Coroutine> highlightAnimTasks = new List<Coroutine> ();

	public int[] PawnEffectParameters;
	public GameObject PawnEffectParticle;

	[Header("Pawn component configuration")]
	public bool UseRotateButton = false;
	public bool CanRotateAlways = false;

	void Awake() {
		isFirstPlay = true;
		AttackDone = true;
		if (WeaponName.Length == 0) {
			//if (WeaponUseCounterText) {
			//	WeaponUseCounterText.text = "";
			//}
			WeaponUseCount = 0;
			pawnState = pawnStates.idle;
		}
		if (PawnCollider == null) {
			PawnCollider = this.GetComponent<PolygonCollider2D> ();
		}
		AudioSourceComponent = this.GetComponent<AudioSource> ();
		AnimationComponent = this.GetComponent<Animation> ();
		SmoothTrans = GetComponent<SmothTransform> ();
	}

	// Use this for initialization
	void Start () {
		GameObject gameplay = GameObject.Find("Gameplay");

		if (gameplay != null) {
			gamePlayComp = gameplay.GetComponent<GamePlay> ();
		}
		cardSpriteRender = GetComponent<SpriteRenderer>();

		GetComponent<Renderer>().shadowCastingMode =  UnityEngine.Rendering.ShadowCastingMode.On;
		GetComponent<Renderer>().receiveShadows = true;

		origScaleHealthText = HealthText.gameObject.transform.localScale;
		origScaleAttackText = AttackText.gameObject.transform.localScale;

		//SetAttack (Attack);
		//SetHealth (Health);
		//WeaponName = "";
		//ApplyConfig ();
		//SetSelect (false);
		//GetComponent<PolygonCollider2D> ().enabled = false;
	}

	// Update is called once per frame
	void Update () {
		/*if (pawnState == pawnStates.onboard_idle) {
      if (gamePlayComp != null) {
        if (gamePlayComp.Board [boardPosisionIndex].Pawn != this.gameObject) {
          Debug.LogError ("Bad pawn and gameplay position synchronization: " + boardPosisionIndex + " pawn: " + Name);
        }
      }
    }*/

		if (isNowPressed) {
			ShowPawnHighlight ();
		} else {
			HidePawnHighlight ();
		}
		if (isNowDrag) {
			if (CardType == CardsBase.CardTypesEnum.Pawn) {
				if (!isDragSetup) {
					isDragSetup = true;
					onDragStart ();
				}
				onDrag ();
			}
		} else if (pawnState == pawnStates.playable) {
			if (Input.GetMouseButton (0) && ((!AttackOnly) || CanRotateAlways)) {
				//Debug.Log ("change pawn rotation");
				Vector2 mouse = Camera.main.ScreenToViewportPoint (Input.mousePosition);
				Vector3 objpos = Camera.main.WorldToViewportPoint (PawnConfigPosHandler.transform.position);
				Vector2 relobjpos = new Vector2 (objpos.x, objpos.y);
				Vector2 relmousepos = new Vector2 (mouse.x , mouse.y ) - relobjpos;
				float angle = Vector2.Angle (Vector2.up, relmousepos);
				if (relmousepos.x > 0)
					angle = -1*angle;
				if (!isNowRotate) {
					startAngle = angle;
					startRot = PawnConfigPosHandler.transform.localEulerAngles.z;
					isNowRotate = true;
				}
				if (!RotationDisabled) {
					if ((UseRotateButton && RotateButton.ButtonPressed) ||
						(!UseRotateButton)){
						angle = angle - startAngle;
						//Debug.Log("set angle = " + (startRot + angle));
						Quaternion quat = Quaternion.identity;
						//if (!gamePlayComp.YouStarted) {
						//  quat.eulerAngles = new Vector3 (0, 0, startRot + angle + 180);
						//} else {
						quat.eulerAngles = new Vector3 (0, 0, startRot + angle);
						//}
						PawnConfigPosHandler.transform.localRotation = quat;
						PawnConfigPosHandler.GetComponent<SmothTransform> ().smoothTransformRotRunning = false;
					}
				}
				if (clickDetect == false) {
					startMousePos = mouse;
					clickDetect = true;
					clickStillDetect = true;
				}
				if (maxClickDelta < Vector3.Distance (startMousePos, mouse)) {
					clickStillDetect = false;
				}
			} else {
				//Debug.Log ("change pawn rotation calibration");
				if (clickDetect == true) {
					if (clickStillDetect) {
						onClick ();
					}
					clickDetect = false;
				}
				if (isNowRotate) {
					isNowRotate = false;
					if (PawnConfigPosHandler.transform.localEulerAngles.z > 330 && PawnConfigPosHandler.transform.localEulerAngles.z <= 360 ||
						PawnConfigPosHandler.transform.localEulerAngles.z > 0 && PawnConfigPosHandler.transform.localEulerAngles.z <= 30) {
						if (!gamePlayComp.YouStarted) {
							SetPawnRotation (3);
						} else {
							SetPawnRotation (0);
						}
					} else if (PawnConfigPosHandler.transform.localEulerAngles.z > 30 && PawnConfigPosHandler.transform.localEulerAngles.z <= 90) {
						if (!gamePlayComp.YouStarted) {
							SetPawnRotation (2);
						} else {
							SetPawnRotation (5);
						}
					} else if (PawnConfigPosHandler.transform.localEulerAngles.z > 90 && PawnConfigPosHandler.transform.localEulerAngles.z <= 150) {
						if (!gamePlayComp.YouStarted) {
							SetPawnRotation (1);
						} else {
							SetPawnRotation (4);
						}
					} else if (PawnConfigPosHandler.transform.localEulerAngles.z > 150 && PawnConfigPosHandler.transform.localEulerAngles.z <= 210) {
						if (!gamePlayComp.YouStarted) {
							SetPawnRotation (0);
						} else {
							SetPawnRotation (3);
						}
					} else if (PawnConfigPosHandler.transform.localEulerAngles.z > 210 && PawnConfigPosHandler.transform.localEulerAngles.z <= 270) {
						if (!gamePlayComp.YouStarted) {
							SetPawnRotation (5);
						} else {
							SetPawnRotation (2);
						}
					} else if (PawnConfigPosHandler.transform.localEulerAngles.z > 270 && PawnConfigPosHandler.transform.localEulerAngles.z <= 330) {
						if (!gamePlayComp.YouStarted) {
							SetPawnRotation (4);
						} else {
							SetPawnRotation (1);
						}
					}
					if (CardType == CardsBase.CardTypesEnum.Pawn) {
						gamePlayComp.UpdatePawnRot (this.gameObject);
					}
				}
			}
		}
	}

	/*private bool TestRaycast() {
    bool rv = false;
    if (Input.touchCount > 0) {
      Ray raycast = Camera.main.ScreenPointToRay (Input.GetTouch (0).position);
      RaycastHit raycastHit;
      if (Physics.Raycast (raycast, out raycastHit)) {
        if (raycastHit.collider == PawnCollider) {
          Debug.Log ("click detected");
          rv = true;
        } else {
          Debug.LogWarning ("wrong object - dropped:" + raycastHit.collider);
        }
      } else {
        RaycastHit2D rayHit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));

        if (rayHit.collider == PawnCollider) {
          Debug.Log ("2D click detected");
          rv = true;
        }
      }
    } else {
      Debug.Log ("clicked with mouse - skipping");
      rv = true;
    }

    return rv;
  }*/

	private void ShowPawnHighlight() {
		if (isIsHighlightSetuped == false) {
			StartCoroutine(ShowPawnHighlightTask());
			isIsHighlightSetuped = true;
		}
	}

	private void HidePawnHighlight() {
		isIsHighlightSetuped = false;
		if (gamePlayComp) {
			if (isIsHighlightShowing) {
				isIsHighlightShowing = false;
				gamePlayComp.HidePawnInHighlight ();
			}
		}
	}

	private IEnumerator ShowPawnHighlightTask() {
		if (gamePlayComp) {
			yield return new WaitForSeconds (HighLightShowDelay);
			isIsHighlightShowing = true;
			if (isIsHighlightSetuped) {
				gamePlayComp.ShowPawnInHighlight (GetComponent<Pawn> ());
			}
		}
		yield return null;
	}

	public void SetPawnRotation(int index) {
		if (RotationPosIndex != index) {
			PlayAudio (SelectSound);
		}
			Debug.Log ("Set pawn rotation index " + index);
			float rot = 0;
			RotationPosIndex = index;
			RefreshManaConsumptionOnMove ();
			SmothTransform SmTransComp = PawnConfigPosHandler.GetComponent<SmothTransform> ();
			Quaternion quat = new Quaternion (0, 0, 0, 0);// = Quaternion.identity;
			if (index == 0) {
				rot = 0;
			} else if (index == 5) {
				rot = 60;
			} else if (index == 4) {
				rot = 120;
			} else if (index == 3) {
				rot = 180;
			} else if (index == 2) {
				rot = 240;
			} else {//index == 1
				rot = 300;
			}
			if (gamePlayComp == null) {
				if (GameObject.Find ("Gameplay") != null) {
					gamePlayComp = GameObject.Find ("Gameplay").GetComponent<GamePlay> ();
				}
			} else {
				if (!gamePlayComp.YouStarted) {
					rot += 180;
				}
			}
			quat.eulerAngles = new Vector3 (0, 0, rot);
			SmTransComp.SmothTransformTo (quat, 10f);
			if (pawnState == pawnStates.playable) {
				SetAttackTarget ();
			}
	}

	private void onClick() {
		Debug.Log (" click detected: start");
		if (!AttackOnly) {
			int bIndexPosition = -1;

			if (isFirstPlay) {
				bIndexPosition = gamePlayComp.GetClosestOverMouseAvailableStartIndexPosition (this.GetComponent<Pawn> ());
			} else {
				bIndexPosition = gamePlayComp.GetClosestOverMouseAvailableIndexPosition (this.GetComponent<Pawn> (),
					boardSavedPosisionIndexPrev, SpecialMovement, AttackAlready, AttackOnly);
			}
			if (bIndexPosition != -1) {
				Vector3 dist = Camera.main.WorldToScreenPoint (gamePlayComp.Board [bIndexPosition].BoardPiece.transform.position);
				Vector3 curPos = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, dist.z);
				Vector3 worldPos = Camera.main.ScreenToWorldPoint (curPos);
				Debug.Log (" click detected: closes board posistion idx:" + bIndexPosition);
				Debug.Log (" click detected: mouse pos " + worldPos);
				if ((bIndexPosition > 0) &&
					(Vector3.Distance (worldPos, gamePlayComp.Board [bIndexPosition].BoardPiece.transform.position) < 1f)) {
					OnMouseDown ();
					if (!isDragSetup) {
						isDragSetup = true;
						onDragStart ();
					}
					onDrag ();
					isPosisionChanged = true;
					OnMouseUp ();
					onDragStop ();
					Debug.Log (" click detected: click processed");
				} else {
					Debug.Log (" click detected: click rejected");
				}
				Debug.Log (" click detected: distance:" + Vector3.Distance (worldPos, gamePlayComp.Board [bIndexPosition].BoardPiece.transform.position));
			}
		}
	}

	public void DetachPawn() {
		KeepParentRenderLayer PawnKeepComp;

		PawnKeepComp = transform.GetComponent<KeepParentRenderLayer> ();
		PawnKeepComp.KeepingActive = false;
		if (cardSpriteRender == null) {
			cardSpriteRender = GetComponent<SpriteRenderer>();
		}
		cardSpriteRender.sortingOrder = 10;
		transform.SetParent (GameObject.Find ("Gameplay").transform);
		if (gamePlayComp == null) {
			gamePlayComp = GameObject.Find ("Gameplay").GetComponent<GamePlay> ();
		}
		if (CardType == CardsBase.CardTypesEnum.Effect) {
			Color color = GetComponent<SpriteRenderer> ().color;
			color.a = 0;
			GetComponent<SpriteRenderer> ().color = color;
		}
	}

	public void MovePawnToStartBoardPos(int pawnPosIndex) {
		Vector3 newPawnPos = new Vector3(0, 0, 0);
		Quaternion newPawnRot;
		Vector3 newPawnScale;

		boardPosisionIndex = pawnPosIndex;
		if (gamePlayComp == null) {
			gamePlayComp = GameObject.Find ("Gameplay").GetComponent<GamePlay> ();
		}
		newPawnPos = gamePlayComp.Board [pawnPosIndex].BoardPiece.transform.localPosition;
		newPawnPos.z = PawnPosUp;
		//transform.Find ("Desc").gameObject.GetComponent<KeepOriginalRotation> ().SetKeepingActive (true);
		if (gamePlayComp.YouStarted) {
			SetPawnRotation (0);
			boardSavedRotationIndexPrev = 0;
			newPawnRot = new Quaternion (0, 0, 0, 0);
		} else {
			//SetPawnRotation (0);
			SetPawnRotation (3);
			boardSavedRotationIndexPrev = 3;
			newPawnRot = new Quaternion (0, 0, 180, 0);
		}
		//newPawnRot.x = boardTransform.localRotation.x;
		SmoothTrans.SmothTransformTo (newPawnPos, newPawnRot, 10f);
		transform.localRotation = newPawnRot;
		if (CardType == CardsBase.CardTypesEnum.Weapon) {
			newPawnScale = new Vector3 (onPlayScaleAsWeapon, onPlayScaleAsWeapon, onPlayScaleAsWeapon);
		} else {
			newPawnScale = new Vector3 (onPlayScale, onPlayScale, onPlayScale);
		}
		SmoothTrans.SmoothScaleTo (newPawnScale, 8);
	}

	public void MovePawnToBoardPos(int pawnPosIndex) {
		Debug.Log ("Moving pawn to pos index:" + pawnPosIndex);
		//Vector3 newPawnPos = new Vector3 (0, 0, 0);
		Vector3 newPawnPos = gamePlayComp.Board [pawnPosIndex].BoardPiece.transform.localPosition;
		newPawnPos.z = PawnPosUp;
		boardPosisionIndex = pawnPosIndex;
		SmoothTrans.SmothTransformTo (newPawnPos, 10f);
		SetPawnRotation (RotationPosIndex);
	}

	public bool OnPlayStart() {
		bool canBePlayed = true;
		boardPosisionIndex = 0;

		gamePlayComp.SetBoardPiecesNormalColor ();
		gamePlayComp.UnsetOtherPawnsOnBoard ();
		//gamePlayComp.DisableOtherPawnOnBoard (this.gameObject);
		if (CardType == CardsBase.CardTypesEnum.Pawn) {
			EnablePawn ();
			boardPosisionIndex = gamePlayComp.GetClosestOverMouseAvailableStartIndexPosition(this.GetComponent<Pawn>());
			transform.localRotation = new Quaternion (0, 0, 0, 0);
			SmoothTrans.SmothTransformTo (new Quaternion(0, 0, 0, 0), 10f);
		}
		if (CardType == CardsBase.CardTypesEnum.Weapon) {
			EnablePawn ();
			boardPosisionIndex = gamePlayComp.GetClosestOverMouseAnyIndexPosition (GetComponent<Pawn>(), true, true, false, true);
			transform.localRotation = new Quaternion (0, 0, 0, 0);
			SmoothTrans.SmothTransformTo (new Quaternion(0, 0, 0, 0), 10f);
		}
		if (boardPosisionIndex == -1) {
			canBePlayed = false;
		}
		return canBePlayed;
	}

	public int OnPlay() {
		int ReturnValue = 0;
		int index = 0;

		if (CardType == CardsBase.CardTypesEnum.Pawn) {
			index = gamePlayComp.GetClosestOverMouseAvailableStartIndexPosition (this.GetComponent<Pawn> ());
		} else if (CardType == CardsBase.CardTypesEnum.Weapon) {
			index = gamePlayComp.GetClosestOverMouseAnyIndexPosition (GetComponent<Pawn>(), true, true, false, true);//with any pawn
		} else {
			index = 0;
		}
		if (index >= 0) {
			boardPosisionIndex = index;
			if (CardType != CardsBase.CardTypesEnum.Effect) {
				DetachPawn ();
			}
			if (CardType != CardsBase.CardTypesEnum.Effect) {
				MovePawnToStartBoardPos (index);
			}

			if (OnPlayCallback != null) {
				ReturnValue = OnPlayCallback (boardPosisionIndex);
			}
			if (CardType == CardsBase.CardTypesEnum.Pawn) {
				ReturnValue = 0;
			}
		}

		return ReturnValue;
	}

	public void OnPlayEnd() {
		gamePlayComp.SetCardBoardID (GetComponent<Pawn> ());
		if (CardType == CardsBase.CardTypesEnum.Pawn) {
			Vector3 newPawnPos = new Vector3 (0, 0, 0);
			newPawnPos = gamePlayComp.Board [boardPosisionIndex].BoardPiece.transform.localPosition;
			newPawnPos.z = PawnPosUp;
			if (SmoothTrans == null) {
				SmoothTrans = GetComponent<SmothTransform> ();
			}
			pawnState = pawnStates.playable;
			boardPosisionIndexPrev = boardPosisionIndex;
			SmoothTrans.SmothTransformTo (newPawnPos, 10f);
			SetSelect (true);
			SetPawnRotation (RotationPosIndex);
			//transform.Find ("Desc").gameObject.transform.rotation = new Quaternion(0,0,-180,0);
			GetComponent<PolygonCollider2D> ().enabled = true;
			gamePlayComp.PutPawnOnPosision (this.gameObject, boardPosisionIndex, true);
			if (!Charge) {
				ShowZZZAnimation ();
			}
		} else if (CardType == CardsBase.CardTypesEnum.Weapon) {
			SetSelect (true);
			pawnState = pawnStates.playable;
			SetPawnRotation (RotationPosIndex);
			GetComponent<PolygonCollider2D> ().enabled = true;
			gamePlayComp.PutWeaponOnBoard (this.gameObject, boardPosisionIndex);
		} else {
			gamePlayComp.SetBoardPiecesNormalColor ();
			gamePlayComp.EnableOtherPawnOnBoard (gameObject);
		}
		GetComponent<SpriteRenderer> ().color = new Color (1, 1, 1, 1);
		CallPlayConfirmCallback ();
		CallLocalPlayConfirmCallback ();
		PlayAudio (PlaySound);
		UpdateMoveInfo ();
	}

	public void OnPlayCancel() {
		if (CardType != CardsBase.CardTypesEnum.Effect) {
			KeepParentRenderLayer PawnKeepComp;

			PawnKeepComp = transform.GetComponent<KeepParentRenderLayer> ();
			PawnKeepComp.KeepingActive = true;
		}
		if (gamePlayComp) {
			gamePlayComp.SetBoardPiecesNormalColor ();
			gamePlayComp.EnableOtherPawnOnBoard (gameObject);
		}

		SmoothTrans.SmoothScaleTo (new Vector3(onCardScale, onCardScale, onCardScale), 15);

		if (OnPlayCancelCallback != null) {
			OnPlayCancelCallback (boardPosisionIndex);
		}
	}

	public void PawnAttackTargetCallback(int targetBoardIdx) {
		ConfirmPosition ();
		gamePlayComp.DoAttack (boardPosisionIndex, targetBoardIdx);
	}

	public void SetCanAttack(bool canAttack, int enemyBoardIndex, GamePlay.attackDirections direction, int attackDistance) {
		isNowPressed = false;//hide pawn highlight
		if (!AttackAlready) {
			PawnCanAttack = canAttack;
			enemyToAttackIndex = enemyBoardIndex;
			directionToAttack = direction;
			enemyPawnkDistance = attackDistance;
		}
	}

	public void SetSelect(bool toSet) {
		Debug.Log ("Pawn ID: " + pawnBoardID + "set select " + toSet);
		if (toSet && !Selected) {
			if (Friendly) {
				Select.color = FriendlyMarkColor;
				if (UseRotateButton && !RotationDisabled) {
					RotateButton.EnableButton ();
				}
			} else {
				Select.color = EnemyMarkColor;
			}
			Color selectColor = Select.color;
			selectColor.a = 1;
			Selected = true;
			if (gamePlayComp != null) {
				//gamePlayComp.DisableOtherPawnOnBoard (this.gameObject);
				gamePlayComp.UnsetOtherPawnsOnBoard (this.gameObject);
			}
			SetSelect (selectColor);
			PlayAudio (SelectSound);
		} else if (!toSet && Selected) {
			Color selectColor = Select.color;

			selectColor.a = 0;
			if (gamePlayComp != null) {
				cardSpriteRender.sortingOrder = 0;
				Vector3 newPawnPos = new Vector3 (0, 0, 0);
				newPawnPos = transform.localPosition;//gamePlayComp.Board [boardPosisionIndex].BoardPiece.transform.localPosition;
				newPawnPos.z = PawnPosDown;
				SmoothTrans.SmothTransformTo (newPawnPos, 10f);
				SetPawnRotation(RotationPosIndex);
			}
			Selected = false;
			SetSelect (selectColor);
			if (UseRotateButton && !RotationDisabled) {
				RotateButton.DisableButton ();
			}
			if (OnDeselectCallback != null) {
				OnDeselectCallback (boardPosisionIndex);
			}
		}
	}

	public void SetSelect(Color color) {
		Select.color = color;
	}

	public void SetAsTarget(GamePlay.TargetCallback callback) {
		PawnTargetCallback = callback;
		if (Friendly) {
			Select.color = FriendlyMarkColor;
		} else {
			Select.color = EnemyMarkColor;
		}
		Color selectColor = Select.color;
		selectColor.a = 1;
		Select.color = selectColor;
		Vector3 newPawnPos = transform.localPosition;
		newPawnPos.z = PawnPosUp;
		SmoothTrans.SmothTransformTo (newPawnPos, 10f);
		cardSpriteRender.sortingOrder = 10;
		Target = true;
	}

	public void UnSetAsTarget() {
		Color selectColor = Select.color;
		selectColor.a = 0;
		Select.color = selectColor;
		Vector3 newPawnPos = transform.localPosition;
		newPawnPos.z = PawnPosDown;
		SmoothTrans.SmothTransformTo (newPawnPos, 10f);
		cardSpriteRender.sortingOrder = 0;
		Target = false;
	}

	public void DisablePawn() {
		//Debug.Log ("Pawn disabled ID: " + pawnBoardID);
		if (pawnState != pawnStates.idle) {
			if (Selected) {
				SetSelect (false);
				ConfirmPosition ();
			}
			pawnState = pawnStates.idle;
		}
	}

	public void EnablePawn() {
		//Debug.Log ("Pawn enabled ID: " + pawnBoardID);
		//if (!isFirstPlay) {
		pawnState = pawnStates.onboard_idle;
		//}
	}

	public void ResetState() {
		isFirstPlay = false;
		AttackAlready = false;
		AttackOnly = false;
		ManaConsumed = false;
		if (Friendly) {
			pawnState = pawnStates.onboard_idle;
			/*//show pawn to move
		Color selectColor = Select.color;
		selectColor.a = 1f;
		SetSelect (selectColor);*/
		}
		HideZZZAnimation ();
	}

	public void OnNewTurn() {
		if (Burning) {
			TakeDamage (1);
			BurningTurnsLeft--;
			if (BurningTurnsLeft <= 0) {
				Burning = false;
				Destroy (FireParticleEffectOb);
			}
		}
		if (FrozenNextTurn) {
			FrozenNextTurn = false;
		} else if (Frozen) {
			Frozen = false;
		}
		if (!Frozen && !Burning) {
			Freeze (false);
		}
		CallOnNewTurnCallback (boardPosisionIndex);
	}

	public void SetAttackOnlyMode() {
		if (!isFirstPlay) {
			Debug.Log ("Set attack only for pawn: " + Name);
			AttackOnly = true;
			if (InnerSelect != null) {
				Color inSelectColor = Select.color;
				inSelectColor.a = 0;
				InnerSelect.color = inSelectColor;
			}
		}
	}

	private void onDragStart()
	{
		if (gamePlayComp == null) {
			gamePlayComp = GameObject.Find ("Gameplay").GetComponent<GamePlay> ();
		}
		SetSelect (true);
		isPosisionChanged = false;
	}

	private void onDrag() {
		int index = -1;
		if (isFirstPlay) {
			index = gamePlayComp.GetClosestOverMouseAvailableStartIndexPosition (this.GetComponent<Pawn> ());
		} else {
			index = gamePlayComp.GetClosestOverMouseAvailableIndexPosition (this.GetComponent<Pawn> (),
				boardSavedPosisionIndexPrev, SpecialMovement, AttackAlready, AttackOnly);
		}
		if (index >= 0) {
			if (boardPosisionIndex != boardPosisionIndexPrev) {
				isNowPressed = false;//hide pawn highlight
			}
			if (!AttackOnly) {
				if (boardPosisionIndex != index) {
					PlayAudio (MoveSound);
				}
				boardPosisionIndex = index;
				if (boardPosisionIndex != boardPosisionIndexPrev) {
					isPosisionChanged = true;
					if (OnChangePositionCallback != null) {
						Debug.Log ("On position change callback call");
						OnChangePositionCallback (boardPosisionIndex);
					}
				}
				MovePawnToBoardPos (index);
			}
		}
	}

	private void onDragStop()
	{
		isDragSetup = false;
		gamePlayComp.RemovePawnFromPosision(boardPosisionIndexPrev);
		gamePlayComp.PutPawnOnPosision(this.gameObject, boardPosisionIndex, false);
		if (PawnCanAttack) {
			ConfirmPosition ();
			if (!gamePlayComp.DoAttack (boardPosisionIndex, enemyToAttackIndex, directionToAttack, enemyPawnkDistance)) {
				PawnCanAttack = false;
			}
			SetSelect (false);
			gamePlayComp.SetBoardPiecesNormalColor ();
			gamePlayComp.EnableOtherPawnOnBoard (gameObject);
		}
	}

	void OnMouseEnter()
	{
		if (!EventSystem.current.IsPointerOverGameObject ()) {
			/*if (false){//pawnState == pawnStates.onboard_idle) {
      Vector3 newPawnPos = new Vector3 (0, 0, 0);
      newPawnPos = gamePlayComp.Board [boardPosisionIndex].BoardPiece.transform.localPosition;
      newPawnPos.z = -0.4f;
      SmoothTrans.SmothTransformTo (newPawnPos, transform.localRotation, 10f);
    }*/
		}
	}

	void OnMouseExit()
	{
		if (!EventSystem.current.IsPointerOverGameObject ()) {
			/*if (false){//pawnState == pawnStates.onboard_idle) {
      Vector3 newPawnPos = new Vector3 (0, 0, 0);
      newPawnPos = gamePlayComp.Board [boardPosisionIndex].BoardPiece.transform.localPosition;
      newPawnPos.z = -0.08f;
      SmoothTrans.SmothTransformTo (newPawnPos, transform.localRotation, 10f);
    }*/
			isNowPressed = false;
		}
	}

	void OnMouseDown()
	{
		if (!EventSystem.current.IsPointerOverGameObject ()) {
			if (pawnState != pawnStates.idle && !Frozen) {
				if (!Target) {
					isNowDrag = true;
					//dist = Camera.main.WorldToScreenPoint (transform.position);
					cardSpriteRender.sortingOrder = 10;
					//pawnState = pawnStates.playable;
				}
			}
			isNowPressed = true;
			startMousePos = Camera.main.ScreenToViewportPoint (Input.mousePosition);
		}
	}

	public void OnMouseUp()
	{
		if (!EventSystem.current.IsPointerOverGameObject ()) {
			isNowDrag = false;
			isNowPressed = false;
			PawnUnpressed = false;

			if (maxClickDelta < Vector3.Distance (startMousePos,
				Camera.main.ScreenToViewportPoint (Input.mousePosition))) {
				PawnUnpressed = true;
			}

			if (Target && gamePlayComp != null) {
				if (!PawnUnpressed) {
					gamePlayComp.SelectTaget (this.gameObject, PawnTargetCallback);
				}
			} else if (pawnState != pawnStates.idle && !Frozen) {
				if (isDragSetup) {
					onDragStop ();
				}
				SetAttackTarget ();
				if (!isPosisionChanged) {
					//Debug.Log ("pawn state is on board idle");
					if (pawnState == pawnStates.playable) {
						gamePlayComp.EnableOtherPawnOnBoard (gameObject);
						ConfirmPosition ();
						gamePlayComp.SetBoardPiecesNormalColor ();
					} else {
						if (PawnCanAttack) {
							PawnCanAttack = false;
						} else {
							SetSelect (true);
							pawnState = pawnStates.playable;
						}
					}
				} else {
					RefreshManaConsumptionOnMove();
					boardPosisionIndexPrev = boardPosisionIndex;
					//SetSelect (true);
					pawnState = pawnStates.playable;
				}
			}
			if (gamePlayComp)
				gamePlayComp.DestroyMarkBeam ();
		}
	}

	public void SetAttackTarget() {
		if (CanAttack()) {
			if (gamePlayComp == null) {
				gamePlayComp = GameObject.Find ("Gameplay").GetComponent<GamePlay> ();
			}
			gamePlayComp.SetAttackTargets (GetComponent<Pawn> (), FriendlyFireEnabled, true);
		}
		if (AttackRulesOverrideCallback != null) {
			AttackRulesOverrideCallback (boardPosisionIndex);
		}
	}

	private void ConsumeMana(int manaValue) {
		if (!ManaConsumed) {
			if (gamePlayComp.Mana >= manaValue) {
				ManaConsumed = true;
				gamePlayComp.Mana -= manaValue;
				Debug.Log (" Mana consumed ");
				gamePlayComp.UpdateManaState (true);//disable other pawns on confirm
			}
		}
	}

	private void UnConsumeMana(int manaValue) {
		if (ManaConsumed) {
			ManaConsumed = false;
			gamePlayComp.Mana += manaValue;
			Debug.Log (" Mana unconsumed ");
			gamePlayComp.UpdateManaState ();
		}
	}

	private void RefreshManaConsumptionOnMove() {
		if (CardType == CardsBase.CardTypesEnum.Pawn) {
			if (!isFirstPlay) {
				if (ManaConsumed) {
					if (CanRotateAlways) {
						if (boardSavedPosisionIndexPrev == boardPosisionIndex) {
							UnConsumeMana(1);
						}
					} else {
						if ((boardSavedPosisionIndexPrev == boardPosisionIndex) &&
							(boardSavedRotationIndexPrev == RotationPosIndex)) {
							UnConsumeMana(1);
						}
					}
				} else {
					if (CanRotateAlways) {
						if (boardSavedPosisionIndexPrev != boardPosisionIndex) {
							ConsumeMana(1);
						}
					} else {
						if ((boardSavedPosisionIndexPrev != boardPosisionIndex) ||
							(boardSavedRotationIndexPrev != RotationPosIndex)) {
							ConsumeMana(1);
						}
					}
				}
			}
		}
	}

	public void ConfirmPosition() {
		pawnState = pawnStates.onboard_idle;
		if (cardSpriteRender != null) {
			cardSpriteRender.sortingOrder = 0;
		}
		if (gamePlayComp == null) {
			gamePlayComp = GameObject.Find ("Gameplay").GetComponent<GamePlay> ();
		}
		gamePlayComp.UnSelectAllTargets ();
		SetSelect (false);
		Vector3 newPawnPos = new Vector3 (0, 0, 0);
		if (gamePlayComp == null) {
			gamePlayComp = GameObject.Find("Gameplay").GetComponent<GamePlay> ();
		}
		if (GetComponent<Hero>() == null) {
			newPawnPos = gamePlayComp.Board [boardPosisionIndex].BoardPiece.transform.localPosition;
		} else {
			newPawnPos = transform.localPosition;
		}
		newPawnPos.z = PawnPosDown;
		SmoothTrans.SmothTransformTo (newPawnPos, 10f);
		if ((isFirstPlay) && (CardType == CardsBase.CardTypesEnum.Weapon)) {
			AddWeaponToPawn (boardPosisionIndex);
			gamePlayComp.GiveWeaponToPawn (Name, boardPosisionIndex, RotationPosIndex);
		}
		if (CardType == CardsBase.CardTypesEnum.Pawn) {
			bool moveDetected = false;

			if (CanRotateAlways) {
				if ((!isFirstPlay) &&
					(boardSavedPosisionIndexPrev != boardPosisionIndex)) {
					moveDetected = true;
				} else {
					moveDetected = false;
				}
			} else {
				if (((!isFirstPlay)) &&
					((boardSavedPosisionIndexPrev != boardPosisionIndex) ||
						(boardSavedRotationIndexPrev != RotationPosIndex))) {
					moveDetected = true;
				} else {
					moveDetected = false;
				}
			}

			if (moveDetected) {
				ManaConsumed = false;
				Debug.Log (" Mana changed unconsumed ");
				gamePlayComp.UpdateManaState ();
				gamePlayComp.ConfirmPawnOnPosision (this.gameObject, boardPosisionIndex, true);
				SetAttackOnlyMode ();// move only once
				CallOnMoveCallback (boardPosisionIndex);
			} else {
				UnConsumeMana(1);
				gamePlayComp.ConfirmPawnOnPosision (this.gameObject, boardPosisionIndex, false);
			}
			boardSavedPosisionIndexPrev = boardPosisionIndex;
			boardSavedRotationIndexPrev = RotationPosIndex;
		}
	}

	public void TriggerEffect(int boardStartIndex, int boardEndIndex) {
		Debug.Log ("Trigger card effect: start:" + boardStartIndex + " end:" + boardEndIndex);
		if (gamePlayComp == null) {
			gamePlayComp = GameObject.Find ("Gameplay").GetComponent<GamePlay> ();
		}
		if (TriggerEffectCallback != null) {
			TriggerEffectCallback (boardStartIndex, boardEndIndex);
		}
	}

	private bool CanAttack() {
		if (((!isFirstPlay) || (Charge)) &&
			(!AttackAlready) &&
			(CardType == CardsBase.CardTypesEnum.Pawn)) {
			return true;
		}
		return false;
	}

	private bool CanMove() {
		return !AttackOnly;
	}

	public bool AnyMoveAvailable() {
		bool canMove = false;
		bool canAttack = false;
		List<int> atackTargets = new List<int> ();

		canMove |= AttackOnly;
		atackTargets = gamePlayComp.SetAttackTargets (GetComponent<Pawn> (), FriendlyFireEnabled, false);
		if ((atackTargets.Count > 0) && (CanAttack())) {
			canAttack = true;
		}

		return (canMove || canAttack);
	}

	#region ADD_WEAPON
	public void AddWeaponToPawn(int boardIndex, bool InformSecondPlayer = true) {
		gamePlayComp.WeaponToPlay = null;
		isFirstPlay = false;
		pawnState = pawnStates.idle;
		if (gamePlayComp.skipAnimations) {
			GameObject Wowner = AddWeaponToPawnBegin (boardIndex, InformSecondPlayer);
			if (Wowner != null) {
				AddWeaponToPawnEnd (Wowner);
			}
		} else {
			StartCoroutine (AddWeaponToPawnTask (boardIndex, InformSecondPlayer));
		}
	}

	private GameObject AddWeaponToPawnBegin(int weaponOwnerBoardIndex, bool InformSecondPlayer) {
		Debug.Log ("Add weapon to pawn on index: " + weaponOwnerBoardIndex);
		GameObject pawnOb = gamePlayComp.Board [weaponOwnerBoardIndex].Pawn;
		if (pawnOb != null) {
			Pawn pawn = pawnOb.GetComponent<Pawn> ();

			pawn.DetachWeapon ();//Detach any other item if exist

			if (ItemApplyConfig) {
				int relatedRot = RotationPosIndex - pawn.RotationPosIndex;
				if (relatedRot < 0)
					relatedRot += 6;
				for (int i = 0; i < PawnConfigNumber; i++) {
					if (ItemMergeConfig) {
						if (PawnConfiguration [i].block) {
							pawn.PawnConfiguration [relatedRot].block = true;
						}
						if (PawnConfiguration [i].melee) {
							pawn.PawnConfiguration [relatedRot].melee = true;
							pawn.PawnConfiguration [relatedRot].attack = false;
						}
						if (PawnConfiguration [i].attack) {
							pawn.PawnConfiguration [relatedRot].attack = true;
							pawn.PawnConfiguration [relatedRot].melee = false;
						}
					} else {
						if (PawnConfiguration [i].block) {
							pawn.PawnConfiguration [relatedRot] = PawnConfiguration [i];
							pawn.PawnConfiguration [relatedRot].block = true;
						} else {
							pawn.PawnConfiguration [relatedRot] = PawnConfiguration [i];
						}
					}
					relatedRot++;
					if (relatedRot > 5)
						relatedRot -= 6;
				}
			}
			pawn.WeaponName = Name;
			pawn.WeaponUseCount = Health;
			pawn.WeaponDamage = Attack;
			pawn.Attack = pawn.Attack + Attack;
			if (cardSpriteRender == null) {
				cardSpriteRender = GetComponent<SpriteRenderer>();
			}
			cardSpriteRender.sortingOrder = 10;
			pawn.WeaponUseCounterText.SetText(pawn.WeaponUseCount.ToString ());
			//pawn.DestroyConfig ();//done by ApplyConfig
			pawn.ApplyConfig ();
			SmoothTrans.SmoothScaleTo (new Vector3(onPlayScale, onPlayScale, onPlayScale), 15);
			if (InformSecondPlayer) {//this function can be call localy on from be called from the second client
				gamePlayComp.GiveWeaponToPawn (Name, weaponOwnerBoardIndex, RotationPosIndex);
			}
			if (OnApplyItemCallback != null) {
				Debug.Log ("Apply item callback call");
				OnApplyItemCallback (pawn.boardPosisionIndex);
			}

			pawn.WeaponOb = this.gameObject;
		} else {
			Debug.LogError ("Cannot get pawn to give him weapon on board idx: " + weaponOwnerBoardIndex);
		}

		return pawnOb;
	}

	private void AddWeaponToPawnEnd(GameObject pawnOb) {
		Debug.Log ("Weapon applied");
		//Destroy (this.gameObject);
		Color newColor = new Color (1f, 1f, 1f, 0f);
		GetComponent<SpriteRenderer> ().color = newColor;
		GetComponent<PolygonCollider2D> ().enabled = false;
		SetBorder (newColor);
		SetSelect (false);
		transform.SetParent(pawnOb.transform);
	}

	private IEnumerator AddWeaponToPawnTask(int weaponOwnerBoardIndex, bool InformSecondPlayer) {
		GameObject Wowner = AddWeaponToPawnBegin (weaponOwnerBoardIndex, InformSecondPlayer);
		yield return new WaitForSeconds (1f);
		if (Wowner != null) {
			AddWeaponToPawnEnd (Wowner);
		}
		yield return null;
	}

	public void DetachWeapon() {
		Debug.Log ("Weapon detached");
		if (WeaponOb != null) {
			WeaponName = "";
			Attack = Attack - WeaponDamage;
			WeaponDamage = 0;
			//DestroyConfig ();//done by ApplyConfig
			RestoreOriginalPawnConfig();
			if (OnDetachItemCallback != null) {
				OnDetachItemCallback (boardPosisionIndex);
			}
			WeaponUseCounterText.SetText( "");
			Destroy (WeaponOb);
		}
	}

	#endregion

	#region PAWN_FIGHT
	private void ShotEvent(PlaygroundEventParticle EvParticle) {
		Debug.Log ("Event triggered " + EvParticle);
		WaitForEventsNumberCurrent++;
		if (WaitForEventsNumberCurrent <= WaitForEventsNumber) {
			if (enemyPawnToAttack != null) {
				int dmg = BulletDamageValue;
				if ((Friendly && enemyPawnToAttack.Friendly) ||
					(!Friendly && !enemyPawnToAttack.Friendly)) {
					enemyPawnToAttack.HealFromProjectile (pawnBoardID, dmg);
				} else {
					enemyPawnToAttack.TakeDamageFromProjectile (dmg);
				}
			}
		}
		if (WaitForEventsNumberCurrent == WaitForEventsNumber) {
			AttackDone = true;
			if (enemyPawnToAttack != null) {
				enemyPawnToAttack.RecevingDamageDone ();
				/*if (enemyPawnToAttack.Health <= 0) {
        Debug.Log ("Call on kill callback - health: " + enemyPawnToAttack.Health);
        CallOnKillCallback (enemyPawnToAttack.boardPosisionIndex);
      }*/
			}
		}
	}

	private void ShotEventScifiPart() {
		Debug.Log ("Event triggered ");
		WaitForEventsNumberCurrent++;
		if (WaitForEventsNumberCurrent <= WaitForEventsNumber) {
			if (enemyPawnToAttack != null) {
				int dmg = BulletDamageValue;
				if ((Friendly && enemyPawnToAttack.Friendly) ||
					(!Friendly && !enemyPawnToAttack.Friendly)) {
					enemyPawnToAttack.HealFromProjectile (pawnBoardID, dmg);
				} else {
					enemyPawnToAttack.TakeDamageFromProjectile (dmg);
				}
			}
		}
		if (WaitForEventsNumberCurrent == WaitForEventsNumber) {
			AttackDone = true;
			if (enemyPawnToAttack != null) {
				enemyPawnToAttack.RecevingDamageDone ();
				/*if (enemyPawnToAttack.Health <= 0) {
        Debug.Log ("Call on kill callback - health: " + enemyPawnToAttack.Health);
        CallOnKillCallback (enemyPawnToAttack.boardPosisionIndex);
      }*/
			}
		}
	}

	private void Shot(int Damage, float ParticleRotationZ) {
		Debug.Log ("Shot with dmg" + Damage + " Z bullet rotation:" + ParticleRotationZ);
		BulletDamageValue = Damage;
		Vector3 ParticlePos = gamePlayComp.Board [boardPosisionIndex].BoardPiece.transform.localPosition;
		Quaternion ParticleRot = Quaternion.identity;
		ParticlePos.z = -0.5f;
		ParticleRot.eulerAngles = new Vector3 (0, 0, ParticleRotationZ);
		GameObject ParticleOb = Instantiate (ShotParticlePrefab, gamePlayComp.transform);
		ParticleOb.transform.localRotation = ParticleRot;
		ParticleOb.transform.localPosition = ParticlePos;
		PlaygroundParticlesC Particle = ParticleOb.GetComponent<PlaygroundParticlesC> ();
		PlaygroundC.GetEvent (0, Particle).particleEvent += ShotEvent;
		Particle.emit = true;
	}

	private void ShotAutoAim(int Damage, float ParticleRotationZ) {
		Debug.Log ("Shot with dmg" + Damage + " Z bullet rotation:" + ParticleRotationZ);
		BulletDamageValue = Damage;
		Vector3 ParticlePos = gamePlayComp.Board [boardPosisionIndex].BoardPiece.transform.localPosition;
		Quaternion ParticleRot = Quaternion.identity;
		ParticlePos.z = -0.5f;
		ParticleRot.eulerAngles = new Vector3 (0, 0, ParticleRotationZ);
		GameObject ParticleOb = Instantiate (ShotParticlePrefab, gamePlayComp.transform);
		GameObject Target = ParticleOb.transform.GetChild (0).gameObject;
		Target.transform.SetParent (enemyPawnToAttack.transform);
		Target.transform.localPosition = new Vector3 (0, 0, 0);
		ParticleOb.transform.localRotation = ParticleRot;
		ParticleOb.transform.localPosition = ParticlePos;
		PlaygroundParticlesC Particle = ParticleOb.GetComponent<PlaygroundParticlesC> ();
		PlaygroundC.GetEvent (0, Particle).particleEvent += ShotEvent;
		Particle.emit = true;
	}

	private void SetParticleSortingOrder(GameObject part) {
		ParticleSystemRenderer PSR = part.GetComponent<ParticleSystemRenderer> ();
		if (PSR != null) {
			PSR.sortingOrder = 100;
		}
	}

	private void SetParticlesSortingOrder(GameObject part) {
		foreach (Transform child in part.transform) {
			if (child != null) {
				GameObject nextPart = child.gameObject;
				SetParticleSortingOrder (nextPart);
				SetParticlesSortingOrder (nextPart);
			}
		}
	}

	private void ShotScifiProjectile(int Damage, float ParticleRotationZ) {
		Debug.Log ("Shot with dmg" + Damage + " Z bullet rotation:" + ParticleRotationZ);
		BulletDamageValue = Damage;
		Vector3 ParticlePos = gamePlayComp.Board [boardPosisionIndex].BoardPiece.transform.localPosition;
		Quaternion ParticleRot = Quaternion.identity;
		ParticlePos.z = -0.5f;
		ParticleRot.eulerAngles = new Vector3 (ParticleRotationZ, -90, 90);
		GameObject projectile = Instantiate(ShotParticlePrefab, gamePlayComp.transform) as GameObject;
		projectile.transform.localRotation = ParticleRot;
		projectile.transform.localPosition = ParticlePos;
		projectile.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
		SetParticleSortingOrder (projectile);
		SetParticlesSortingOrder (projectile);
		Debug.Log ("particle pos:" + projectile.transform.localPosition + " rotation:" + projectile.transform.localRotation);
		SciFiProjectileScript SciFiProjectileComp = projectile.GetComponent<SciFiProjectileScript>();
		SciFiProjectileComp.impactNormal = new Vector3(0,0,ParticleRotationZ);
		SciFiProjectileComp.SetOwnerCollider(BulletStopCollider);
		SciFiProjectileComp.ParticleEventHandlerCallback = ShotEventScifiPart;
		SciFiProjectileComp.PawnGameObject = this.gameObject;
		projectile.GetComponent<Rigidbody>().AddForce(projectile.transform.forward * 1000);
	}

	private void ShotScifiProjectileLaserBeam(int Damage, float ParticleRotationZ) {
		Debug.Log ("Shot with dmg" + Damage + " Z bullet rotation:" + ParticleRotationZ);
		GameObject beamStart;
		GameObject beam;
		LineRenderer line;

		BulletDamageValue = Damage;

		beamStart = Instantiate(ShotParticlePrefab, gamePlayComp.transform) as GameObject;
		beam = Instantiate(PawnEffectParticle, gamePlayComp.transform) as GameObject;
		line = beam.GetComponent<LineRenderer>();
		SetParticleSortingOrder (beamStart);

		Vector3 ParticlePos = gamePlayComp.Board [boardPosisionIndex].BoardPiece.transform.localPosition;
		Quaternion ParticleRot = Quaternion.identity;
		ParticlePos.z = -0.5f;
		ParticleRot.eulerAngles = new Vector3 (ParticleRotationZ, -90, 90);
		beamStart.transform.localRotation = ParticleRot;
		beamStart.transform.localPosition = ParticlePos;
		beamStart.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
		line.transform.localRotation = ParticleRot;
		line.transform.localPosition = ParticlePos;

		Vector3 end = beamStart.transform.GetChild(0).transform.position;
		Vector3 start = beamStart.transform.position;

		//line.SetVertexCount(2);obsolete
		line.positionCount = 2;
		line.SetPosition(0, start);
		line.SetPosition(1, end);
		float distance = Vector3.Distance(start, end);
		line.sharedMaterial.mainTextureScale = new Vector2(distance, 1);
		line.sharedMaterial.mainTextureOffset -= new Vector2(Time.deltaTime, 0);
		line.sortingOrder = 30;
		ShotEventScifiPart ();
	}

	private IEnumerator ShootingTask(int Damage, Pawn PawnToTakeDamage, float shootingDirectionZ, float RotationZSciFiProj) {
		WaitForEventsNumberCurrent = 0;
		if (ShootingMode == PawnShootingModeEnum.singleShot) {
			WaitForEventsNumber = Damage;
			for (int i = 0; i < Damage; i++) {
				Shot (1, shootingDirectionZ);
				yield return new WaitForSeconds (ShootInterval);
			}
		} else if (ShootingMode == PawnShootingModeEnum.simultaneousShot) {
			WaitForEventsNumber = 1;
			Shot (Damage, shootingDirectionZ);
		} else if (ShootingMode == PawnShootingModeEnum.autoAim) {
			WaitForEventsNumber = 1;
			ShotAutoAim (Damage, shootingDirectionZ);
		} else if (ShootingMode == PawnShootingModeEnum.projecitileSingle) {
			WaitForEventsNumber = Damage;
			for (int i = 0; i < Damage; i++) {
				ShotScifiProjectile (1, RotationZSciFiProj);
				yield return new WaitForSeconds (ShootInterval);
			}
		} else if (ShootingMode == PawnShootingModeEnum.projecitileSimul) {
			WaitForEventsNumber = 1;
			ShotScifiProjectile (Damage, RotationZSciFiProj);
		} else if (ShootingMode == PawnShootingModeEnum.laser) {
			WaitForEventsNumber = 1;
			ShotScifiProjectileLaserBeam (Damage, RotationZSciFiProj);
		}
		yield return new WaitForSeconds (10);
		if (AttackDone != true) {
			AttackDone = true;
			enemyPawnToAttack.RecevingDamageDone ();
			/*if (enemyPawnToAttack.Health <= 0) {
        Debug.Log ("Call on kill callback - health: " + enemyPawnToAttack.Health);
        CallOnKillCallback (enemyPawnToAttack.boardPosisionIndex);
      } else {
        Debug.Log ("NOTT !! Call on kill callback - health: " + enemyPawnToAttack.Health);
      }*/
		}
		yield return null;
	}

	/// <summary>
	/// Pawn will trigger his attack procedure to attack another pawn.
	/// </summary>
	/// <param name="PawnToTakeDamage">Pawn to take damage.</param>
	/// <param name="attackDirection">Attack direction enumerator.</param>
	public void GiveDamageAnimation(Pawn PawnToTakeDamage, GamePlay.attackDirections attackDirection, int Damage, bool skipAnim) {
		if (!Frozen) {
			float RotationZ = 0;
			float RotationZSciFiProj = 0;
			AttackDone = false;
			if (attackDirection == GamePlay.attackDirections.Up) {
				RotationZ = 0;
				RotationZSciFiProj = 270;
			} else if (attackDirection == GamePlay.attackDirections.UpRight) {
				RotationZ = 300;
				RotationZSciFiProj = 210;
			} else if (attackDirection == GamePlay.attackDirections.DownRight) {
				RotationZ = 240;
				RotationZSciFiProj = 150;
			} else if (attackDirection == GamePlay.attackDirections.Down) {
				RotationZ = 180;
				RotationZSciFiProj = 90;
			} else if (attackDirection == GamePlay.attackDirections.DownLeft) {
				RotationZ = 120;
				RotationZSciFiProj = 30;
			} else if (attackDirection == GamePlay.attackDirections.UpLeft) {
				RotationZ = 60;
				RotationZSciFiProj = 330;
			} else {
				RotationZ = 0;
				RotationZSciFiProj = 270;
			}

			if (gamePlayComp == null) {
				gamePlayComp = GameObject.Find("Gameplay").GetComponent<GamePlay> ();
			}

			enemyPawnToAttack = PawnToTakeDamage;
			enemyPawnToAttack.PrepareToReceiveDamage (RotationZ);
			StartCoroutine (ShootingTask (Damage, PawnToTakeDamage, RotationZ, RotationZSciFiProj));
		}
	}

	public void GiveDamage(Pawn PawnToTakeDamage, GamePlay.attackDirections attackDirection, bool skipAnim) {
		if (!Frozen) {
			AttackAlready = true;
			CallOnAttackCallback (PawnToTakeDamage.boardPosisionIndex);
			if ((Friendly && PawnToTakeDamage.Friendly) ||
				(!Friendly && !PawnToTakeDamage.Friendly)) {
				PawnToTakeDamage.Health = PawnToTakeDamage.CalcHeal (Attack);
			} else {
				PawnToTakeDamage.TakeDamage (Attack);
				if (PawnToTakeDamage.Health <= 0) {
					if (PawnToTakeDamage.DeathBloked == false) {
						PawnToTakeDamage.Death = true;
					}
				}
				if (Friendly) {
					gamePlayComp.DamageDone += Attack;
				}
			}
			AttackDone = true;

			//in gameplay
			if (PawnToTakeDamage.Health <= 0) {
				Debug.Log ("Call on kill callback - health: " + PawnToTakeDamage.Health);
				CallOnKillCallback (PawnToTakeDamage.boardPosisionIndex);
			}
			if (WeaponName.Length > 0) {
				WeaponUseCount--;
				WeaponUseCounterText.SetText( WeaponUseCount.ToString ());
				if (WeaponUseCount <= 0) {
					DetachWeapon ();
				}
			}
		}
		// in gameplay
	}

	private IEnumerator ReceiveDamageMark(int DmgValue) {
		int dmg = -DmgValue;
		GameObject ReceivedDamageFloatTextOb = Instantiate(ReceivedDamageFloatText);
		ReceivedDamageFloatTextOb.transform.SetParent (this.transform.Find("Desc").transform, false);
		ReceivedDamageFloatTextOb.transform.localPosition = new Vector3 (0, 0, -1);
		ReceivedDamageFloatTextOb.GetComponent<TextMeshPro> ().text = dmg.ToString();
		ReceivedDamageFloatTextOb.GetComponent<SmothTransform> ().SmothTransformTo (new Vector3 (0, 5, -1), 2);
		yield return new WaitForSeconds (2f);
		if (ReceivedDamageFloatTextOb != null) {
			Destroy (ReceivedDamageFloatTextOb);
		}
	}

	/// <summary>
	/// Pawn receive damage, invisible for user, trigger CallOnGetDamageCallback, Without 3d collider events.
	/// </summary>
	/// <param name="damageValue">Damage value.</param>
	public void TakeDamage (int damageValue) {
		Debug.Log("This pawn id: " + pawnBoardID + " receive dmg: " + damageValue);

		if (gamePlayComp == null) {
			gamePlayComp = GameObject.Find("Gameplay").GetComponent<GamePlay> ();
		}
		CallOnGetDamageCallback ();
		TakeDamageImpl (damageValue);
	}

	/// <summary>
	/// Pawn receive damage from projectile, trigger by projectile hit event.
	/// Must call PrepareToReceiveDamage before, and RecevingDamageDone after.
	/// </summary>
	/// <param name="damageValue">Projectile damage value.</param>
	public void TakeDamageFromProjectile(int damageValue) {
		Debug.Log("This pawn id: " + pawnBoardID + " receive dmg: " + damageValue);

		if (gamePlayComp == null) {
			gamePlayComp = GameObject.Find("Gameplay").GetComponent<GamePlay> ();
		}
		//TakeDamageImpl (damageValue);
		ShowDamageIndicator(damageValue);
	}

	public void TakeDamageImpl(int damageValue) {
		if (BuffedHealth > 0) {
			if (BuffedHealth >= damageValue) {
				BuffedHealth -= damageValue;
			} else {
				BuffedHealth = 0;
			}
		}
		Health -= damageValue;
	}

	/// <summary>
	/// Show user that pawn received damage.
	/// </summary>
	/// <param name="damageValue">Damage value.</param>
	public void ShowDamageIndicator(int damageValue) {
		StartCoroutine (ReceiveDamageMark (damageValue));
		DisplayedHealth -= damageValue;
		if (DisplayedHealth < Health) {
			DisplayedHealth = Health;
		}
		RefreshHealth (DisplayedHealth);
	}

	/// <summary>
	/// Prepare pawn to receive damage by projectile, its enable 3d collider.
	/// </summary>
	/// <param name="BulletDirectionRotationZ">Rotatation that projectile comes from, for 3d collider.</param>
	public void PrepareToReceiveDamage(float BulletDirectionRotationZ) {
		Quaternion BulletDirectionRotation = Quaternion.identity;
		BulletDirectionRotation.eulerAngles = new Vector3 (0, 0, BulletDirectionRotationZ);
		BulletStopCollider.transform.localRotation = BulletDirectionRotation;
		BulletStopCollider.enabled = true;
		//CallOnGetDamageCallback ();
	}

	public void RecevingDamageDone() {
		if (BulletStopCollider != null) {
			BulletStopCollider.enabled = false;
		}
		if (gamePlayComp == null) {
			gamePlayComp = GameObject.Find("Gameplay").GetComponent<GamePlay> ();
		}
		if (gamePlayComp != null) {
			gamePlayComp.CheckDeadPawns ();
		}
		//gamePlayComp.AttackDone ();do next action in stack
	}
	#endregion

	public void SetOnFire(int ForTurns) {
		Burning = true;
		BurningTurnsLeft = ForTurns;
		//Frozen = false;
		//FrozenNextTurn = false;
		Freeze (false);
		FireParticleEffectOb = Instantiate (FireParticleEffect, this.transform);
		FireParticleEffectOb.transform.localPosition = new Vector3 (0, -4, 0);
		FireParticleEffectOb.transform.localScale = new Vector3 (2, 2, 2);
	}

	public void Freeze(bool toSet = true) {
		if (toSet) {
			FrozenNextTurn = true;
			if (Burning) {
				Destroy (FireParticleEffectOb);
				Burning = false;
				BurningTurnsLeft = 0;
			}
			transform.Find ("Character").GetComponent<SpriteRenderer> ().color = new Color (0.2f, 0.6f, 1f, 1f);
			if (FrozenPrefab != null) {
				FrozenPrefabOb = Instantiate (FrozenPrefab, this.transform);
				FrozenPrefabOb.transform.localScale = new Vector3 (0, 0, 0);
				FrozenPrefabOb.transform.localPosition = new Vector3 (0, -0.85f, -1.05f);
				FrozenPrefabOb.GetComponent<SmothTransform> ().SmoothScaleTo (new Vector3 (1, 1, 1), 15);
			}
		} else {
			FrozenNextTurn = false;
			if (FrozenPrefabOb != null) {
				Destroy (FrozenPrefabOb);
			}
			Character.GetComponent<SpriteRenderer> ().color = new Color (1f, 1f, 1f, 1f);
		}
		Frozen = toSet;
	}

	public int CalcHeal(int healValue, int health = 0) {
		if (health == 0) {
			health = Health;
		}
		int rv = health;
		if (health < MaxHealth && !Poisoned) {
			if ((health + healValue) > MaxHealth) {
				rv = MaxHealth + BuffedHealth;
			} else {
				rv = health + healValue;
			}
		}
		return rv;
	}

	private void ShowBuffEffect() {
		if (BuffParticleEffect != null) {
			GameObject Gob = Instantiate (BuffParticleEffect, this.transform, false);
			Gob.transform.localPosition = new Vector3 (0, -1.5f, 0);
			Gob.transform.localScale = new Vector3 (3, 3, 3);
			Destroy (Gob, 3);
		}
	}

	public void ShowHealEffect() {
		if (HealParticleEffect != null) {
			Instantiate (HealParticleEffect, HealthText.transform, false);
		}
		if (HealTargetParticleEffect != null) {
			GameObject Gob = Instantiate (HealTargetParticleEffect, this.transform, false);
			Gob.transform.localPosition = new Vector3 (0, -1.5f, 0);
			Gob.transform.localScale = new Vector3 (2, 2, 2);
			Destroy (Gob, 3);
		}
	}

	public void HealFromProjectile(int HealDealerBoardID, int healValue) {
		Debug.Log ("Pawn heal by value: " + healValue);
		if (!Poisoned) {
			if (Health < MaxHealth) {
				if (gamePlayComp != null) {
					gamePlayComp.GameLogComponent.AddLogAction (GamePlayActionStack.ActionTypeEnum.heal,
						gamePlayComp.GetBoardPawnByID (HealDealerBoardID), pawnBoardID);
				}
			}
			SetHealth (CalcHeal(healValue, DisplayedHealth));
			ShowHealEffect ();
		}
	}

	public void Heal(int HealDealerBoardID, int healValue) {
		Debug.Log ("Pawn heal by value: " + healValue);
		if (!Poisoned) {
			if (Health < MaxHealth) {
				if (gamePlayComp != null) {
					gamePlayComp.GameLogComponent.AddLogAction (GamePlayActionStack.ActionTypeEnum.heal,
						gamePlayComp.GetBoardPawnByID (HealDealerBoardID), pawnBoardID);
				}
			}
			SetHealth (CalcHeal(healValue));
			PlayAudio (HealSound);
			ShowHealEffect ();
		}
	}

	#region PAWN_VALUES
	private IEnumerator BuffAttackTask(int value) {
		if (value != 0) {
			float delta = 2;
			Vector3 newScale = new Vector3 (origScaleAttackText.x + delta,
				origScaleAttackText.y + delta,
				origScaleAttackText.z + delta);
			if (value > 0) {
				ShowBuffEffect ();
			}
			AttackText.gameObject.GetComponent<SmothTransform> ().SmoothScaleTo (newScale, 10);
			SetAttack (Attack);
			PlayAudio (BuffSound);
			yield return new WaitForSeconds (0.4f);
			AttackText.gameObject.GetComponent<SmothTransform> ().SmoothScaleTo (origScaleAttackText, 10);
		}
	}

	private IEnumerator BuffHealthTask(int value) {
		if (value != 0) {
			float delta = 2;
			Vector3 newScale = new Vector3 (origScaleHealthText.x + delta,
				origScaleHealthText.y + delta,
				origScaleHealthText.z + delta);
			if (value > 0) {
				ShowBuffEffect ();
			}
			HealthText.gameObject.GetComponent<SmothTransform> ().SmoothScaleTo (newScale, 10);
			SetHealth (Health);
			PlayAudio (BuffSound);
			yield return new WaitForSeconds (0.4f);
			HealthText.gameObject.GetComponent<SmothTransform> ().SmoothScaleTo (origScaleHealthText, 10);
		}
	}

	public void BuffAttack(Pawn buffDealer, int value) {
		if (gamePlayComp != null) {
			if (value > 0) {
				Debug.Log ("buffed by pawn :" + buffDealer + " name: " + buffDealer.Name  + " boardID: " + buffDealer.pawnBoardID);
				gamePlayComp.GameLogComponent.AddLogAction (GamePlayActionStack.ActionTypeEnum.buff,
					buffDealer == null ? gamePlayComp.GetBoardPawnByID (buffDealer.pawnBoardID) : buffDealer,
					pawnBoardID, 0, value);
			}
		}
		MaxAttack += value;
		BuffedAttack += value;
		Attack += value;
		if (gamePlayComp.skipAnimations) {
			SetAttack (Attack);//copy BuffAttackTask
		} else {
			StartCoroutine (BuffAttackTask (value));
		}
	}

	public void BuffHealth(Pawn buffDealer, int value) {
		if (gamePlayComp != null) {
			if (value > 0) {
				gamePlayComp.GameLogComponent.AddLogAction (GamePlayActionStack.ActionTypeEnum.buff,
					buffDealer, pawnBoardID, value, 0);
			}
		}
		Health -= BuffedHealth;
		BuffedHealth += value;
		if (BuffedHealth < 0) {
			BuffedHealth = 0;
		}
		Health += BuffedHealth;
		if (gamePlayComp.skipAnimations) {
			SetHealth (Health);//copy BuffHealthTask
		} else {
			StartCoroutine (BuffHealthTask (value));
		}
	}

	public void RefreshAttack(int value) {
		DisplayedAttack = value;
		if (AttackText != null) {
			if (CardType == CardsBase.CardTypesEnum.Effect) {
				AttackText.SetText ("");
			} else if (CardType == CardsBase.CardTypesEnum.Weapon) {
				if (value > 0) {
					AttackText.SetText ("+" + value);
				} else {
					AttackText.SetText ("");
				}
			} else {
				AttackText.SetText ("" + value);
				if (BuffedAttack > 0) {
					AttackText.font = pawnBuffedAttackFontAsset;
				} else {
					AttackText.font = pawnNormalAttackFontAsset;
				}
			}
		}
	}

	public void RefreshHealth(int value) {
		DisplayedHealth = value;
		WeaponUseCounterText.SetText ("");
		if (HealthText != null) {
			if (CardType == CardsBase.CardTypesEnum.Effect) {
				HealthText.SetText ("");
			} else if (CardType == CardsBase.CardTypesEnum.Weapon) {
				WeaponUseCounterText.SetText ("" + value);
				HealthText.SetText ("");
			} else {
				HealthText.SetText ("" + value);
				if (value < MaxHealth) {
					HealthText.font = pawnHurtHealthFontAsset;
				} else if (BuffedHealth > 0) {
					HealthText.font = pawnBuffedHealthFontAsset;
				} else {
					HealthText.font = pawnNormalHealthFontAsset;
				}
			}
		}
	}

	public void SetAttack(int value) {
		Attack = value;
		RefreshAttack (value);
	}

	public void SetHealth(int value) {
		Health = value;
		//Debug.Log ("Set health: " + value);
		RefreshHealth(value);
	}
	#endregion

	#region PAWN_DIE
	private IEnumerator PawnDieTask () {
		PawnDieBegin ();
		yield return new WaitForSeconds (0.2f);
		if (CardType == CardsBase.CardTypesEnum.Pawn) {
			PlayAudio (DeathSound);
		}
		PlayAnimation (DeathAnimationName);
		yield return new WaitForSeconds (1f);
		PawnDieEnd ();
		yield return null;
	}

	private void PawnDieBegin() {
		Debug.Log ("Im dead :(");
		pawnState = pawnStates.idle;
		if (Selected) {
			SetSelect (false);
		}
		if (gamePlayComp == null) {
			gamePlayComp = GameObject.Find("Gameplay").GetComponent<GamePlay> ();
		}
		if (Friendly) {
			gamePlayComp.SetBoardPiecesNormalColor ();
			gamePlayComp.EnableOtherPawnOnBoard ();
		}
	}

	private void PawnDieEnd() {
		//CallOnDeathCallback ();//call by gameplay
		//gamePlayComp.RemovePawnFromPosisionImpl(boardPosisionIndex);
		if (gamePlayComp.myHero != this.gameObject) {
			Destroy (this.gameObject);
		}
	}

	public void PawnDie(bool skipAnim = false) {
		if (skipAnim) {
			PawnDieBegin ();
			PawnDieEnd ();
		} else {
			StartCoroutine (PawnDieTask ());
		}
	}
	#endregion

	#region PAWN_AUDIO
	private void PlayAudio(AudioClip clip) {
		if (clip != null) {
			AudioSourceComponent.PlayOneShot (clip);
		}
	}
	#endregion

	#region PAWN_ANIM
	private void PlayAnimation(string clip) {
		AnimationComponent.Play (clip);
	}
	#endregion

	public void SetBorder(Color BorderColor) {
		if (Border != null) {
			Border.color = BorderColor;
		}
	}

	private void ShowZZZAnimation() {
		StartCoroutine (ShowZZZAnimationTask (3));
	}

	private void HideZZZAnimation() {
		foreach (GameObject Gob in ZZZObjects) {
			Destroy (Gob);
		}
		ZZZObjects.Clear ();
	}

	private IEnumerator ShowZZZAnimationTask(int count) {
		for (int i = 0; i < count; i++) {
			GameObject Gob = Instantiate (PawnZZZPrefab, this.transform);
			Gob.transform.localScale = new Vector3 (1, 1, 1);
			ZZZObjects.Add (Gob);
			Gob.GetComponent<Animation> ().Play ();
			yield return new WaitForSeconds(Gob.GetComponent<Animation> ().clip.length/count);
		}
	}

	private void DestroyConfig() {
		if (SpecialMovementOb != null) {
			Destroy (SpecialMovementOb);
		}
		for (int i = 0; i < PawnConfigNumber; i++) {
			foreach (Transform childTransform in PawnConfigPositions [i])
				Destroy (childTransform.gameObject);
		}
	}

	public void SetConfig(CardsBase.PawnConfigSet[] PawnConfig, bool SpMovement) {
		SpecialMovement = SpMovement;

		for (int i = 0; i < PawnConfigNumber; i++) {
			if (PawnConfig [i] == CardsBase.PawnConfigSet.Block) {
				PawnConfiguration[i].block = true;
			} else if (PawnConfig [i] == CardsBase.PawnConfigSet.Melee) {
				PawnConfiguration[i].melee = true;
			} else if (PawnConfig [i] == CardsBase.PawnConfigSet.Distance) {
				PawnConfiguration[i].attack = true;
			} else if (PawnConfig [i] == CardsBase.PawnConfigSet.MeleeAndBlock) {
				PawnConfiguration[i].block = true;
				PawnConfiguration[i].melee = true;
			} else if (PawnConfig [i] == CardsBase.PawnConfigSet.DistanceAndBlock) {
				PawnConfiguration[i].block = true;
				PawnConfiguration[i].attack = true;
			}
		}
	}

	public void ApplyConfig() {
		//Debug.Log ("apply pawn config");
		DestroyConfig();//Destroy previouse pawn attachments before apply new
		SetAttack (Attack);
		SetHealth (Health);
		//if (GetComponent<Hero> () == null) {
		for (int i = 0; i < PawnConfigNumber; i++) {
			GameObject GOb;
			if (PawnConfiguration [i].attack) {
				GOb = (GameObject)Instantiate (DistAttackPre, new Vector3 (0, 0, 0), new Quaternion (0, 0, 0, 0));
				GOb.transform.SetParent (PawnConfigPositions [i], false);
			}
			if (PawnConfiguration [i].melee) {
				GOb = (GameObject)Instantiate (MeleeAttackPre, new Vector3 (0, 0, 0), new Quaternion (0, 0, 0, 0));
				GOb.transform.SetParent (PawnConfigPositions [i], false);
			}
			if (PawnConfiguration [i].block) {
				GOb = (GameObject)Instantiate (BlockPre, new Vector3 (0, 0, 0), new Quaternion (0, 0, 0, 0));
				GOb.transform.SetParent (PawnConfigPositions [i], false);
			}
		}

		if (SpecialMovement) {
			SpecialMovementOb = (GameObject)Instantiate (SpecialMovemPre, new Vector3 (0, 0, 0), Quaternion.identity);
			SpecialMovementOb.transform.SetParent (transform, false);
		}
		//}

		if (originalConfigApplied == false) {
			//Debug.Log ("save original config");
			originalConfigApplied = true;
			OriginalSpecialMovement = SpecialMovement;
			for (int i = 0; i < PawnConfigNumber; i++) {
				OriginalPawnConfiguration [i].attack = PawnConfiguration[i].attack;
				OriginalPawnConfiguration [i].melee = PawnConfiguration[i].melee;
				OriginalPawnConfiguration [i].block = PawnConfiguration[i].block;
			}
			originalHealth = Health;
			originalAttack = Attack;
			MaxHealth = Health;
			MaxAttack = Attack;
		}
	}

	public void MovePawnConfigPos(int ConfigIndex, bool NormalPos) {
		if (PawnConfigPositions [ConfigIndex].childCount > 0) {
			GameObject GOb = PawnConfigPositions [ConfigIndex].GetChild (0).gameObject;
			if (GOb != null) {
				SmothTransform SmTrans = GOb.GetComponent<SmothTransform> ();
				if (SmTrans != null) {
					Vector3 newPos = new Vector3 (0, 0, 0);
					if (!NormalPos) {
						newPos.x = 1.5f;
					}
					SmTrans.SmothTransformTo (newPos, 10);
				}
			}
		}
	}

	private IEnumerator ShowConfigPosHighlightAnimTask(SmothTransform SmTrans, float state1, float state2) {
		while (true) {
			SmTrans.SmoothScaleTo(new Vector3(state1, state1, state1), 8);
			yield return new WaitForSeconds (0.3f);
			SmTrans.SmoothScaleTo(new Vector3(state2, state2, state2), 8);
			yield return new WaitForSeconds (0.3f);
		}
	}

	/// <summary>
	/// Show highlight for pawn config.
	/// </summary>
	/// <param name="ConfigIndex">Config index to highlight.</param>
	/// <param name="ShowAttack">True to highlight attack, flase to change to normal.</param>
	/// <param name="ShowBlock">True to highlight block, flase to change to normal.</param>
	public void ShowConfigPosHighlight(int ConfigIndex, bool ShowAttack, bool ShowBlock) {
		if (PawnConfigPositions [ConfigIndex].childCount > 0) {
			Transform GOb = null;
			Transform GOb2 = null;

			if (gamePlayComp != null) {
				if (gamePlayComp.myTurn && Friendly && AttackAlready) {
					return;
				} else if (!gamePlayComp.myTurn && !Friendly && AttackAlready) {
					return;
				} else if (Frozen) {
					return;
				}
			}

			if (ShowAttack) {
				if (PawnConfigPositions [ConfigIndex].childCount > 0) {
					GOb = PawnConfigPositions [ConfigIndex].GetChild (0);
				}
			}
			if (ShowBlock) {
				if (PawnConfiguration [ConfigIndex].attack || PawnConfiguration [ConfigIndex].melee) {
					if (PawnConfigPositions [ConfigIndex].childCount > 1) {
						GOb2 = PawnConfigPositions [ConfigIndex].GetChild (1);
					}
				} else {
					if (PawnConfigPositions [ConfigIndex].childCount > 1) {
						GOb2 = PawnConfigPositions [ConfigIndex].GetChild (1);
					} else if (PawnConfigPositions [ConfigIndex].childCount > 0) {
						GOb2 = PawnConfigPositions [ConfigIndex].GetChild (0);
					}
				}
			}
			if (GOb != null) {
				//if (GOb.GetComponent<SpriteRenderer> ().color != ConfigCanAttackColor) {
					SmothTransform SmTrans = GOb.GetComponent<SmothTransform> ();
					if (SmTrans != null) {
						SmTrans.SmoothScaleTo(new Vector3(5, 5, 5), 5);
						highlightAnimTasks.Add (StartCoroutine(ShowConfigPosHighlightAnimTask (SmTrans, 4, 5.5f)));
					}
					GOb.GetComponent<SpriteRenderer> ().color = ConfigCanAttackColor;
					GOb.GetComponent<KeepParentRenderLayer> ().KeepingActive = false;
					GOb.GetComponent<SpriteRenderer> ().sortingOrder = 20;
				//}
			}
			if (GOb2 != null) {
				//if (GOb2.GetComponent<SpriteRenderer> ().color != ConfigCanAttackColor) {
					SmothTransform SmTrans = GOb2.GetComponent<SmothTransform> ();
					if (SmTrans != null) {
						SmTrans.SmoothScaleTo(new Vector3(5, 5, 5), 5);
						highlightAnimTasks.Add (StartCoroutine(ShowConfigPosHighlightAnimTask (SmTrans, 4, 5.5f)));
					}
					GOb2.GetComponent<SpriteRenderer> ().color = ConfigCanAttackColor;
					GOb2.GetComponent<KeepParentRenderLayer> ().KeepingActive = false;
					GOb2.GetComponent<SpriteRenderer> ().sortingOrder = 20;
				//}
			}
		}
	}

	/// <summary>
	/// Reset highlight for pawn config, sets all to normal state
	/// </summary>
	public void ResetConfigPosHighlight() {
		for (int i = 0; i < PawnConfigNumber; i++) {
			Transform GOb = null;
			Transform GOb2 = null;
			Color colorToSet = ConfigCanAttackColor;

			foreach (Coroutine cor in highlightAnimTasks) {
				if (cor != null) {
					StopCoroutine (cor);
				}
			}
			highlightAnimTasks.Clear ();

			if (gamePlayComp != null) {
				if (gamePlayComp.myTurn && Friendly && AttackAlready) {
					colorToSet = ConfigNormalColor;
				}
			}

			if (PawnConfigPositions [i].childCount > 0) {
				GOb = PawnConfigPositions [i].GetChild (0);
			}
			if (PawnConfigPositions [i].childCount > 1) {
				GOb2 = PawnConfigPositions [i].GetChild (1);
			}

			if (GOb != null) {
				if (GOb.GetComponent<SpriteRenderer> ().color != colorToSet) {
					SmothTransform SmTrans = GOb.GetComponent<SmothTransform> ();
					if (SmTrans != null) {
						SmTrans.SmoothScaleTo(new Vector3(4, 4, 4), 5);
					}
					GOb.GetComponent<SpriteRenderer> ().color = colorToSet;
					GOb.GetComponent<KeepParentRenderLayer> ().KeepingActive = true;
				}
			}
			if (GOb2 != null) {
				if (GOb2.GetComponent<SpriteRenderer> ().color != colorToSet) {
					SmothTransform SmTrans = GOb2.GetComponent<SmothTransform> ();
					if (SmTrans != null) {
						SmTrans.SmoothScaleTo(new Vector3(4, 4, 4), 5);
					}
					GOb2.GetComponent<SpriteRenderer> ().color = colorToSet;
					GOb2.GetComponent<KeepParentRenderLayer> ().KeepingActive = true;
				}
			}
		}
	}

	private void RestoreOriginalPawnConfig() {
		SpecialMovement = OriginalSpecialMovement;
		for (int i = 0; i < PawnConfigNumber; i++) {
			PawnConfiguration [i].attack = OriginalPawnConfiguration[i].attack;
			PawnConfiguration [i].melee = OriginalPawnConfiguration[i].melee;
			PawnConfiguration [i].block = OriginalPawnConfiguration[i].block;
		}
		//Health = originalHealth;
		//Attack = originalAttack;
		ApplyConfig ();
	}

	public void RestoreOriginalPawnMovement() {
		SpecialMovement = OriginalSpecialMovement;
		ApplyConfig ();
	}

	/*Pawn callbacks*/
	private void CallAllCallbacks(List<CardCallback> callbackList) {
		foreach (CardCallback callb in callbackList) {
			if (callb != null) {
				callb (boardPosisionIndex);
			} else {
				callbackList.Remove (callb);
			}
		}
	}

	private void CallAllCallbacks(List<CardCallback> callbackList, int callbackParam) {
		foreach (CardCallback callb in callbackList) {
			if (callb != null) {
				callb (callbackParam);
			} else {
				callbackList.Remove (callb);
			}
		}
	}

	private void CallOnGetDamageCallback() {
		if (!Death) {
			CallAllCallbacks (onGetDamageCallbackList);
		}
	}

	public void CallOnDeathCallback() {
		CallAllCallbacks (onDeathCallbackList);
	}

	private void CallPlayConfirmCallback() {
		CallAllCallbacks (onPlayConfirmCallbackList);
	}

	public void CallLocalPlayConfirmCallback() {
		CallAllCallbacks (onLocalPlayConfirmCallbackList);
	}

	private void CallOnAttackCallback(int target) {
		CallAllCallbacks (onAttackCallbackList, target);
	}

	private void CallOnNewTurnCallback(int target) {
		if (!Death) {
			UpdateMoveInfo ();
			CallAllCallbacks (onNewTurnCallbackList, target);
		}
	}

	private void CallOnKillCallback(int target) {
		CallAllCallbacks (onKillCallbackList, target);
	}

	public void RegisterOnGetDamageCallback(CardCallback callback) {
		onGetDamageCallbackList.Add (callback);
	}

	public void RegisterPlayConfirmCallback(CardCallback callback) {
		onPlayConfirmCallbackList.Add (callback);
	}

	public void RegisterLocalPlayConfirmCallback(CardCallback callback) {
		onLocalPlayConfirmCallbackList.Add (callback);
	}

	public void RegisterDeathCallback(CardCallback callback) {
		onDeathCallbackList.Add (callback);
	}

	public void RegisterOnAttackCallback(CardCallback callback) {
		Debug.Log ("Register new RegisterOnAttackCallback");
		onAttackCallbackList.Add (callback);
	}

	public void RemoveOnAttackCallback(CardCallback callback) {
		Debug.Log ("Register new RegisterOnAttackCallback");
		onAttackCallbackList.Remove (callback);
	}

	public void RegisterOnNewTurnCallback(CardCallback callback) {
		onNewTurnCallbackList.Add (callback);
	}

	public void RegisterOnKillCallback(CardCallback callback) {
		onKillCallbackList.Add (callback);
	}

	public void RegisterOnSomeOneDiedCallback(CardCallback callback) {
		onSomeOneDiedCallbackList.Add (callback);
	}

	public void CallOnSomeOneDiedCallback(int target) {
		CallAllCallbacks (onSomeOneDiedCallbackList, target);
	}

	public void CallOnMoveCallback(int target) {
		CallAllCallbacks (onMoveCallbackList, target);
	}

	public void RegisterOnMoveCallback(CardCallback callback) {
		onMoveCallbackList.Add (callback);
	}

	public void CallOnBoardUpdateCallback() {
		CallAllCallbacks (onBoardUpdateList);
	}

	private void UpdateMoveInfo() {
		if (InnerSelect != null) {
			Color inSelectColor = Select.color;

			//show pawn to move
			if (((gamePlayComp != null && gamePlayComp.myTurn &&
				gamePlayComp.Mana > 0 && !AttackOnly) || (isFirstPlay && gamePlayComp.myTurn)) && Friendly)
			{
				inSelectColor.a = 1f;
				InnerSelect.color = inSelectColor;
			}
		}
	}

	public void RegisterOnBoardUpdateCallback(CardCallback callback) {
		onBoardUpdateList.Add (callback);
	}
}