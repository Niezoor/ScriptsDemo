using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLog : MonoBehaviour {
	public GamePlay gameplayComponent;
	public GameObject LogPanelPrefab;
	public Transform LogListTransform;
	public float InLogScale = 10;
	public int LogPanelsCountMax = 5;

	public Canvas FocusPanelCanvas;
	public GameObject LogFocusPanelPrefab;
	public Transform LogFocusMainTransform;
	public Transform LogFocusListTransform;
	public float InLogFocusScale = 10;

	public GameObject AttackPicturePrefab;
	public GameObject AttackAndCounterPicturePrefab;
	public GameObject HealPicturePrefab;
	public GameObject BuffPicturePrefab;
	public Vector3 InfoPicureLocalPosition;
	public Vector3 InfoWithCardPicureLocalPosition;
	public float InfoPicureLocalScale;
	public int InfoPictureRenderLayer;

	private GameObject CurrentInfoPictureObject;

	public List <GameObject> PanelsInList = new List <GameObject> ();
	public List <GameObject> SpawnedCardsInFocusPanel = new List <GameObject> ();

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void CreateFocusPanel(GameLogPanel.TargetClass targetCard, GameObject focusPanelObject, bool pawnOnly, bool friendly) {
		Transform pawnTransform = null;

		GameObject card = gameplayComponent.CardsComp.SpawnCardByName (targetCard.targetName);

		if (card != null) {
			pawnTransform = card.transform.Find ("Pawn");
			card.GetComponent<CardInteraction> ().SetCardOrder (210);
			pawnTransform.GetComponent<SpriteRenderer> ().color = new Color (1, 1, 1, 1);
		} else {//no in card base? it should be Hero
			GameObject hero = gameplayComponent.HeroesComp.GetHeroByName (targetCard.targetName);
			if (hero != null) {
				pawnTransform = hero.transform;
				pawnOnly = true;
			}
		}

		if (pawnTransform != null) {
			Pawn pawnComp = pawnTransform.GetComponent<Pawn> ();
			pawnComp.SetAttack (targetCard.targetAttack);
			pawnComp.SetHealth (targetCard.targetHealth);
			if (pawnOnly) {
				pawnComp.DetachPawn ();
				pawnTransform.gameObject.GetComponent<SpriteRenderer> ().sortingOrder = 2100;
				pawnTransform.SetParent (focusPanelObject.transform, false);
				pawnTransform.transform.localScale = new Vector3 (InLogFocusScale-10, InLogFocusScale-10, 1);
				pawnTransform.transform.localPosition = new Vector3 (0, 0, 0);
				pawnTransform.transform.localRotation = new Quaternion (0, 0, 0, 0);
				Destroy (card);
			} else {
				card.transform.SetParent (focusPanelObject.transform, false);
				card.transform.localScale = new Vector3 (InLogFocusScale, InLogFocusScale, 1);
				card.transform.localPosition = new Vector3 (0, -50, 0);
				card.transform.localRotation = new Quaternion (0, 0, 0, 0);
			}

			if (friendly == false) {
				if (pawnComp.CardType == CardsBase.CardTypesEnum.Pawn) {
					pawnTransform.gameObject.GetComponent<SpriteRenderer> ().color = gameplayComponent.enemyColor;
				}
				pawnComp.Friendly = false;
				pawnComp.SetBorder (gameplayComponent.enemyColor);
			}

			SpawnedCardsInFocusPanel.Add (focusPanelObject);
		} else {
			Destroy (focusPanelObject);
		}
	}

	private void CreateInfoPicture(GamePlayActionStack.ActionTypeEnum action, bool withCard) {
		if (action == GamePlayActionStack.ActionTypeEnum.attack) {
			CurrentInfoPictureObject = Instantiate (AttackPicturePrefab);
		} else if (action == GamePlayActionStack.ActionTypeEnum.attackAndCounter) {
			CurrentInfoPictureObject = Instantiate (AttackAndCounterPicturePrefab);
		} else if (action == GamePlayActionStack.ActionTypeEnum.buff) {
			CurrentInfoPictureObject = Instantiate (BuffPicturePrefab);
		} else if (action == GamePlayActionStack.ActionTypeEnum.heal) {
			CurrentInfoPictureObject = Instantiate (HealPicturePrefab);
		}
		if (CurrentInfoPictureObject != null) {
			CurrentInfoPictureObject.transform.SetParent (LogFocusMainTransform);
			if (withCard) {
				CurrentInfoPictureObject.transform.localPosition = InfoWithCardPicureLocalPosition;
			} else {
				CurrentInfoPictureObject.transform.localPosition = InfoPicureLocalPosition;
			}
			/*CurrentInfoPictureObject.transform.localScale =
				new Vector3 (InfoPicureLocalScale/CurrentInfoPictureObject.transform.lossyScale.x,
							InfoPicureLocalScale/CurrentInfoPictureObject.transform.lossyScale.y,
							InfoPicureLocalScale/CurrentInfoPictureObject.transform.lossyScale.z);*/
			CurrentInfoPictureObject.GetComponent<SpriteRenderer>().sortingOrder = InfoPictureRenderLayer;
		}
	}

	public void ShowLogFocus(GameObject panel) {
		GameObject foundedPanel = null;
		//HideLogFocus ();//hide if was showed already
		foreach (GameObject founded in PanelsInList) {
			if (founded == panel) {
				foundedPanel = founded;
				break;
			}
		}
		if (foundedPanel != null) {
			bool pOnly = false;
			FocusPanelCanvas.enabled = true;
			GameObject panelOb = Instantiate (LogFocusPanelPrefab);
			GameLogPanel Panel = foundedPanel.GetComponent<GameLogPanel> ();
			panelOb.transform.SetParent(LogFocusMainTransform, false);
			if (Panel.pawnOnPanelPawnComponent.CardType == CardsBase.CardTypesEnum.Effect) {
				pOnly = false;
			} else {
				pOnly = Panel.pawnOnly;
			}
			GameLogPanel.TargetClass target = new GameLogPanel.TargetClass ();
			target.targetName = Panel.pawnOnPanelPawnComponent.Name;
			target.targetAttack = Panel.pawnOnPanelPawnComponent.Attack;
			target.targetHealth = Panel.pawnOnPanelPawnComponent.Health;
			CreateFocusPanel (target, panelOb, pOnly, Panel.pawnOnPanelPawnComponent.Friendly);
			CreateInfoPicture (Panel.PawnAction, !Panel.pawnOnly);
			//w-a
			Destroy (CurrentInfoPictureObject);
			CreateInfoPicture (Panel.PawnAction, !Panel.pawnOnly);
			//w-a end
			if (Panel.PawnAction != GamePlayActionStack.ActionTypeEnum.play) {
				foreach (GameLogPanel.TargetClass targetCard in Panel.Targets) {
					GameObject panelTargetOb = Instantiate (LogFocusPanelPrefab);
					panelTargetOb.transform.SetParent (LogFocusListTransform, false);
					CreateFocusPanel (targetCard, panelTargetOb, true, targetCard.friendly);
				}
			}
		} else {
			Debug.LogError("Cannot find panel in internal list");
		}
	}

	public void HideLogFocus() {
		if (FocusPanelCanvas.enabled) {
			foreach (GameObject card in SpawnedCardsInFocusPanel) {
				Destroy (card);
			}
			SpawnedCardsInFocusPanel.Clear ();
			if (CurrentInfoPictureObject != null) {
				Destroy (CurrentInfoPictureObject);
			}
			FocusPanelCanvas.enabled = false;
		}
	}

	public GameLogPanel AddLogAction(GamePlayActionStack.ActionTypeEnum action, Pawn pawnToLog, int TargetBoardID) {
		return AddLogAction (action, pawnToLog, TargetBoardID, 0, 0);
	}

	/// <summary>
	/// Add new event to short game actions history.
	/// </summary>
	/// <returns>Added log panel.</returns>
	/// <param name="action">Action to add.</param>
	/// <param name="pawnToLog">Pawn to add to log, you will see it as thumbnail.</param>
	/// <param name="TargetBoardID">Pawn board id that is target of this action.</param>
	/// <param name="LogValue">Some action can have some additional value (ex, buff or heal value).</param>
	/// <param name="LogValue2">Some action can have some additional value (ex, buff or heal value).</param>
	public GameLogPanel AddLogAction(GamePlayActionStack.ActionTypeEnum action, Pawn pawnToLog, int TargetBoardID, int LogValue, int LogValue2) {
		GameLogPanel Panel = null;

		if (pawnToLog == null) {
			Debug.LogError ("You need to set Pawn to log object component");
			return null;
		}
		if (LogListTransform.GetChild (0) != null) {
			Debug.Log ("Add new action to log: " + action + " for pawn:" + pawnToLog.Name + " id: " + pawnToLog.pawnBoardID +
			" last id:" + LogListTransform.GetChild (0).GetComponent<GameLogPanel> ().PawnID);
			GameLogPanel LastPanel = LogListTransform.GetChild (0).GetComponent<GameLogPanel> ();
			if ((LastPanel.PawnID == pawnToLog.pawnBoardID) &&
			    ((LastPanel.PawnAction == action) || (LastPanel.PawnAction == GamePlayActionStack.ActionTypeEnum.play))) {
				Debug.Log ("Add new target to last action");
				Panel = LogListTransform.GetChild (0).GetComponent<GameLogPanel> ();
			} else {
				Debug.Log ("Add new action");
				Panel = AddLogAction (action, pawnToLog);
			}
		} else {
			Debug.Log ("Add new action");
			Panel = AddLogAction (action, pawnToLog);
		}
		if (Panel != null) {
			Pawn pawnToTarget = gameplayComponent.GetBoardPawnByID (TargetBoardID);

			Panel.PawnAction = action;
			if (pawnToTarget != null) {
				bool found = false;
				foreach (GameLogPanel.TargetClass targetCard in Panel.Targets) {
					if (targetCard.targetID == pawnToTarget.pawnBoardID) {
						found = true;
						targetCard.targetAttack = pawnToTarget.Attack;
						targetCard.targetHealth = pawnToTarget.Health;
						break;
					}
				}
				if (!found) {
					GameLogPanel.TargetClass newtarget = new GameLogPanel.TargetClass ();
					newtarget.targetName = pawnToTarget.Name;
					newtarget.targetAttack = pawnToTarget.Attack;
					newtarget.targetHealth = pawnToTarget.Health;
					newtarget.targetID = pawnToTarget.pawnBoardID;
					newtarget.friendly = pawnToTarget.Friendly;
					if (action == GamePlayActionStack.ActionTypeEnum.buff) {
						newtarget.targetAttack += LogValue2;
						newtarget.targetHealth += LogValue;
					}
					//if (action == GamePlayActionStack.ActionTypeEnum.attack) {
						//newtarget.targetHealth -= pawnToLog.Attack;
					//} else if (action == GamePlayActionStack.ActionTypeEnum.attackAndCounter) {
						//newtarget.targetHealth -= pawnToLog.Attack;
						//Panel.pawnOnPanelPawnComponent.SetHealth (Panel.pawnOnPanelPawnComponent.Health - pawnToTarget.Attack);
					//}
					Debug.Log ("Add new action target: " + newtarget);
					Panel.Targets.Add (newtarget);
				}
			} else {
				Debug.LogError ("cannot find target on board - board ID:" + TargetBoardID);
			}
		}
		return Panel;
	}


	public GameLogPanel AddLogAction(GamePlayActionStack.ActionTypeEnum action, Pawn pawnToLog) {
		Debug.Log ("Add new action to log: " + action + " for pawn:" + pawnToLog.Name);

		if (action == GamePlayActionStack.ActionTypeEnum.newTurn) {
			LogListTransform.GetChild (0).GetComponent<GameLogPanel> ().PawnID = -1;
			return null;
		}

		GameObject panelOb = Instantiate (LogPanelPrefab);
		GameLogPanel Panel = panelOb.GetComponent<GameLogPanel> ();
		panelOb.transform.SetParent (LogListTransform, false);
		panelOb.transform.SetAsFirstSibling();
		GameObject card = gameplayComponent.CardsComp.SpawnCardByName (pawnToLog.Name);
		Transform pawnTransform;

		if (!card) {
			GameObject hero = gameplayComponent.HeroesComp.GetHeroByName (pawnToLog.Name);
			pawnTransform = hero.transform;
		} else {
			pawnTransform = card.transform.Find ("Pawn");
		}

		Pawn pawnComp = pawnTransform.GetComponent<Pawn> ();
		if (pawnComp != null) {
			pawnTransform.SetParent (panelOb.transform, false);
			pawnTransform.GetComponent<SpriteRenderer> ().color = new Color (1, 1, 1, 1);//spawn card creates invisible cards now
			pawnTransform.localScale = new Vector3 (InLogScale, InLogScale, 0.1f);
			pawnTransform.localPosition = new Vector3 (-200, 0, 0);
			pawnTransform.localRotation = new Quaternion (0, 0, 0, 0);
			pawnTransform.gameObject.GetComponent<KeepParentRenderLayer> ().KeepingActive = false;
			pawnTransform.GetComponent<KeepParentRenderLayer> ().KeepingActive = false;
			pawnTransform.gameObject.GetComponent<SmothTransform> ().SmothTransformTo (new Vector3 (0, 0, 0), 10);
			if (pawnToLog.Friendly == false) {
				pawnTransform.gameObject.GetComponent<SpriteRenderer> ().color = gameplayComponent.enemyColor;
				pawnComp.Friendly = false;
				pawnComp.SetBorder (gameplayComponent.enemyColor);
			}

			if (action == GamePlayActionStack.ActionTypeEnum.play) {
				Panel.pawnOnly = false;
			} else {
				Panel.pawnOnly = true;
			}

			Panel.PawnOnPanel = pawnTransform.gameObject;
			Panel.PawnAction = action;
			Panel.pawnOnPanelPawnComponent = pawnComp;
			Panel.pawnOnPanelPawnComponent.SetAttack (pawnToLog.Attack);
			Panel.pawnOnPanelPawnComponent.SetHealth (pawnToLog.Health);
			Panel.PawnID = pawnToLog.pawnBoardID;
			Panel.gameLogComp = GetComponent<GameLog> ();
			if (card) {
				Destroy (card);
			}
			PanelsInList.Add (panelOb);
			if (PanelsInList.Count > LogPanelsCountMax) {
				Destroy (PanelsInList [0]);
				PanelsInList.RemoveAt (0);
			}
		} else {
			Debug.LogError ("cannot find pawn in gameplay");
			Destroy (panelOb);
		}
		return Panel;
	}

}
