using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour {

	public GamePlay GameplayComponent;
	public GameObject AIDeckPrefab;
	private Deck AIDeck;
	private Hero AIHero;
	//private Hand AIHand;
	private CardsBase AllCards;
	public List<string> AIHandCards = new List<string> ();
	private int AIPawnIDNext = 1000;
	[System.Serializable]
	public enum AIDificultEnum {
		easy,
		medium,
		hard,
	}

	public AIDificultEnum AIDificult;

	public int mana = 0;
	public int spendMana = 0;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		//Debug.Log ("check random: " + Random.Range (0, 3));
	}

	public void AIStartPlay() {
		//AIHand = GameplayComponent.EnemyHandComp;
		GameObject DeckGO = (GameObject)Instantiate(AIDeckPrefab);
		AIDeck = DeckGO.GetComponent<Deck> ();
		AIHero = AIDeck.Hero.GetComponent<Hero> ();
		string heroName = AIHero.Name;
		Destroy (AIHero);

		if (GameplayComponent.enemyDeck != null) {
			Destroy (GameplayComponent.enemyDeck);
		}
		GameplayComponent.enemyDeck = DeckGO;
		GameplayComponent.SetEnemyHero (heroName);
		AIDeck.ShuffleDeck ();
	}

	public void AIInitDraw() {
		for (int i = 0; i < GameplayComponent.startHandCardsNumber; i++) {
			AICardDraw (true);
		}
		GameplayComponent.EnemyInitCardDraw ();
		GameplayComponent.EnemyIsReady = true;
		GameplayComponent.MatchStart ();
	}

	public void AICardDraw(bool initDraw) {
		GameObject card = AIDeck.GetNextCard (GameplayComponent.CardsComp);
		if (card) {
			if (!initDraw) {
				//GameplayComponent.EnemyCardDraw ();
			}
			AIHandCards.Add (card.GetComponent<CardInteraction> ().CardName);
			Destroy (card);
		}
	}

	public void AITurn() {
		if (!GameplayComponent.gameEnd) {
			AICardDraw (false);
			if (mana < GameplayComponent.ManaMax) {
				mana++;
			}
			spendMana = 0;
			foreach (GamePlay.pawnListClass pawnToRet in GameplayComponent.GamePawnsList) {
				Pawn pawnOnBoard = pawnToRet.pawn;
				if (pawnOnBoard != null) {
					if ((pawnOnBoard.gameObject != null) &&
						(!pawnOnBoard.Friendly)) {
						pawnOnBoard.isFirstPlay = false;
						pawnOnBoard.AttackAlready = false;
						pawnOnBoard.AttackOnly = false;
					}
				}
			}
			/*AI turn logic*/
			/* LVL: easy */
			if (AIDificult == AIDificultEnum.easy) {
				StartCoroutine (PlayRandomCardFromHand ());
			} else if (AIDificult == AIDificultEnum.medium) {
				MediumMove ();
			}
		}
	}

	private void MediumMove() {
		StartCoroutine (PlayRandomCardFromHand ());
	}

	private List<Pawn> GetPawnsOnBoard() {
		List<Pawn> rv = new List<Pawn> ();
		for (int i = 0; i < GamePlay.IndexMAX; i++) {
			if (GameplayComponent.Board [i].Pawn != null) {
				GameObject pawnObject = GameplayComponent.Board [i].Pawn;
				if (pawnObject != GameplayComponent.enemyHero) {
					Pawn pawnToRet = pawnObject.GetComponent<Pawn> ();
					if (!pawnToRet.Friendly) {
						rv.Add (pawnToRet);
					}
				}
			}
		}
		return rv;
	}

	private IEnumerator RandomMoves() {
		yield return new WaitForSeconds (1f);
		List<Pawn> pawnsOnBoard = GetPawnsOnBoard ();

		foreach (Pawn pawnToMove in pawnsOnBoard) {
			int direct = Random.Range (0, 3);
			if ((mana - spendMana) > 0) {
				if (!GameplayComponent.gameEnd) {
					if (Move (pawnToMove, pawnToMove.SpecialMovement, direct)) {
						spendMana++;
						yield return new WaitForSeconds (1f);
					}
				}
			}
			if (!GameplayComponent.gameEnd) {
				if (AttackAllPosiblePawns (pawnToMove)) {
					yield return new WaitForSeconds (1f);
				}
			}
		}
		yield return new WaitForSeconds (1f);
		if (!GameplayComponent.gameEnd) {
			GameplayComponent.TakeTurn ();
		}
	}

	/// <summary>
	/// Move AI pawn.
	/// </summary>
	/// <param name="pawnToMove">Pawn to move.</param>
	/// <param name="longMove">Move to the end of board or not.</param>
	/// <param name="direct">Prefer direction 0-down left, 1-down, 2-down right.</param>
	private bool Move(Pawn pawnToMove, bool longMove, int direct) {
		bool newPosFound = false;
		int newPosIdx = -1;
		int startPos = pawnToMove.boardPosisionIndex;
		int overrideDir = 0;

		if (pawnToMove.isFirstPlay || pawnToMove.AttackOnly || pawnToMove.Frozen) {
			return newPosFound;
		}

		//Debug.Log ("AI move pawn: " + pawnToMove);

		while (true) {
			if (direct == 0) {
				startPos = GameplayComponent.GetBoardIndexDownLeft (startPos);
			} else if (direct == 1) {
				startPos = GameplayComponent.GetBoardIndexDown (startPos);
			} else if (direct == 2) {
				startPos = GameplayComponent.GetBoardIndexDownRight (startPos);
			}
			//Debug.Log ("AI move pawn new pos: " + startPos);
			if (startPos != -1) {
				if (GameplayComponent.IsFreePosision (startPos)) {
					newPosFound = true;
					newPosIdx = startPos;
				} else {
					if (longMove) {
						break;
					}
				}
			} else {
				if (!newPosFound) {
					if (overrideDir <= 2 && !longMove) {
						direct = overrideDir;
						overrideDir++;
						startPos = pawnToMove.boardPosisionIndex;
						continue;
					} else {
						break;
					}
				} else {
					break;
				}
			}
			if (!longMove) {
				break;
			}
		}
		if (newPosFound) {
			int pos = newPosIdx;
			int rot = 3;
			if (pos == 2 || pos == 3 || pos == 4) {
				rot = 4;
			} else if (pos == 14 || pos == 4 || pos == 9) {
				rot = 2;
			}
			GameplayComponent.ConfirmEnemyPawnPos (pawnToMove.Name, pawnToMove.pawnBoardID, rot, pos);
		}
		return newPosFound;
	}

	/// <summary>
	/// AI pawn attack if possible.
	/// </summary>
	/// <param name="pawnToMove">Pawn that AI want to attack.</param>
	private bool AttackAllPosiblePawns(Pawn pawnToMove) {
		bool attacked = false;
		int randomTarget = -1;
		if (!pawnToMove.AttackAlready && (pawnToMove.Charge || !pawnToMove.isFirstPlay) && !pawnToMove.Frozen) {
			List<int> targets = GameplayComponent.SetAttackTargets (pawnToMove, pawnToMove.FriendlyFireEnabled, false);

			if (targets.Count > 0) {
				randomTarget = targets [Random.Range (0, targets.Count)];
			
				if (randomTarget != -1) {
					if (GameplayComponent.DoAttack (pawnToMove.boardPosisionIndex, randomTarget)) {
						attacked = true;
					}
				}
			} else {
				Debug.Log ("AI:No pawns to attack:" + pawnToMove.Name + " id:" + pawnToMove.pawnBoardID); 
			}
		}
		return attacked;
	}

	private List<string> GetPossibleCardsFromHand() {
		List<string> rv = new List<string> ();
		if (AIHandCards.Count > 0) {
			foreach (string cardName in AIHandCards) {
				CardsBase.CardDescriptionClass cardDesc = GameplayComponent.CardsComp.FindCardDescByName (cardName);
				if (cardDesc != null) {
					if (cardDesc.Cost <= (mana - spendMana)) {
						rv.Add (cardName);
					}
				}
			}
		}
		return rv;
	}

	private IEnumerator PlayRandomCardFromHand() {
		List<string> skipCard = new List<string> ();
		while (true) {
			List<string> cardsToPlay = GetPossibleCardsFromHand ();
			if (skipCard.Count > 0) {
				foreach (string card in skipCard) {
					cardsToPlay.Remove (card);
				}
			}
			if (cardsToPlay.Count > 0) {
				string cardToPlayName = cardsToPlay[Random.Range (0, cardsToPlay.Count)];
				int handIndex;
				int boardPosIndex = 0;
				bool playcard = true;

				handIndex = AIHandCards.IndexOf (cardToPlayName);
				if (GameplayComponent.IsFreePosision (17)) {
					boardPosIndex = 17;
				} else if (GameplayComponent.IsFreePosision (18)) {
					boardPosIndex = 18;
				} else if (GameplayComponent.IsFreePosision (21)) {
					boardPosIndex = 21;
				} else {
					playcard = false;
					break;
				}
				if (playcard) {
					CardsBase.CardDescriptionClass cardDesc = GameplayComponent.CardsComp.FindCardDescByName (cardToPlayName);
					if (cardDesc != null) {
						yield return new WaitForSeconds (1f);
						if (cardDesc.CardMode == CardsBase.CardTypesEnum.Pawn) {
							GameObject pawnObject = GameplayComponent.PutEnemyPawnOnBoard (cardToPlayName, AIPawnIDNext, 3, handIndex, boardPosIndex);
							yield return new WaitForSeconds (1f);
							GameplayComponent.ConfirmEnemyPawnPos (cardToPlayName, AIPawnIDNext, 3, boardPosIndex);
							spendMana += cardDesc.Cost;
							AIHandCards.Remove (cardToPlayName);
							if (pawnObject != null) {
								Pawn pawnComp = pawnObject.GetComponent<Pawn> ();
								pawnComp.isFirstPlay = true;
								if (pawnComp.AITriggerEffectCallback != null) {
									List<int> targets = pawnComp.AITriggerEffectCallback (pawnComp.boardPosisionIndex);
									if (targets != null) {
										if (targets.Count > 0) {
											ShooseRandomTarget (cardToPlayName, pawnComp.boardPosisionIndex, -1, targets);
											yield return new WaitForSeconds (1f);
										} else {
											skipCard.Add (cardToPlayName);
										}
									} else {
										skipCard.Add (cardToPlayName);
									}
								}
							}
						} else if (cardDesc.CardMode == CardsBase.CardTypesEnum.Effect) {
							//GameObject pawnObject = GameplayComponent.PutEnemyCardOnBoard (cardToPlayName, -1, 0, -1, -1).gameObject;
							GameObject cardObject = GameplayComponent.CardsComp.SpawnCardByName (cardToPlayName);
							GameObject pawnObject = cardObject.transform.Find ("Pawn").gameObject;
							cardObject.transform.localPosition = new Vector3 (1000, 1000, 1000);
							if (pawnObject != null) {
								Pawn pawnComp = pawnObject.GetComponent<Pawn> ();
								pawnComp.gamePlayComp = GameplayComponent;
								List<int> targets = pawnComp.AITriggerEffectCallback (pawnComp.boardPosisionIndex);
								if (targets != null) {
									if (cardDesc.EffectComponent.Equals ("VerticalLineAttack")) {
										if (targets.Count > 0) {
											GameplayComponent.PlayEnemyEffectOnBoard (pawnComp.Name, Random.Range (0, 5), targets [Random.Range (0, targets.Count)], handIndex);
											yield return new WaitForSeconds (1f);
										} else {
											skipCard.Add (cardToPlayName);
										}
									} else {
										if (targets.Count > 0) {
											ShooseRandomTarget (pawnComp.Name, -1, handIndex, targets);
											yield return new WaitForSeconds (1f);
										} else {
											skipCard.Add (cardToPlayName);
										}
									}
									spendMana += cardDesc.Cost;
									AIHandCards.Remove (cardToPlayName);
								} else {
									skipCard.Add (cardToPlayName);
								}
							}
							Destroy (cardObject, 10);
						} else if (cardDesc.CardMode == CardsBase.CardTypesEnum.Weapon) {
							GamePlay.TargetSettings settings = GamePlay.TargetSettings.WithEnemyPawn;
							GameplayComponent.SetTargetsOnBoard (settings, null, null);
							List<int> targets = GameplayComponent.TargetsList;
							if (targets != null) {
								if (targets.Count > 0) {
									int target = targets [Random.Range (0, targets.Count)];
									GameplayComponent.PutEnemyWeaponOnBoard (cardToPlayName, 3, handIndex, target);
									yield return new WaitForSeconds (1f);
									GameplayComponent.GiveWeaponToEnemyPawn (cardToPlayName, target, 3);
									spendMana += cardDesc.Cost;
									AIHandCards.Remove (cardToPlayName);
									yield return new WaitForSeconds (1f);
								} else {
									skipCard.Add (cardToPlayName);
								}
							} else {
								skipCard.Add (cardToPlayName);
							}
						}

						AIPawnIDNext++;
					}
				}
			} else {
				break;
			}
			if ((mana - spendMana) <= 1) {
				break;
			}
		}
		StartCoroutine (RandomMoves ());
	}

	public void AIPawnEffectCallback(string pawnName, int pawnPosIndex, int handIndex, List<int> targetsList) {
		ShooseRandomTarget (pawnName, pawnPosIndex, handIndex, targetsList);
	}

	public void ShooseRandomTarget(string pawnName, int pawnPosIndex, int handIndex, List<int> targetList) {
		if (targetList.Count > 0) {
			int target = Random.Range (0, targetList.Count);

			GameplayComponent.PlayEnemyEffectOnBoard (pawnName, pawnPosIndex, targetList[target], handIndex);
		}
	}
}
