using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using TMPro;

public class CardInteraction : MonoBehaviour
{
	public string CardName;
	public string CardDescription;
    public int CardCost;

	[System.Serializable]
	public enum CardRarityEnum {
		common,
		gold,
		diamond
	}

	[System.Serializable]
	public enum CardRoleEnum {
		offence,
		defence,
		support
	}

	public CardRarityEnum CardRarity = CardRarityEnum.common;
	public CardRoleEnum CardRole = CardRoleEnum.offence;

	[Header("Audio")]
	public AudioClip DrawSound;
	public AudioClip DrawBackSound;
	public AudioClip PlaySound;
	public AudioClip MoveSound;
	public AudioClip ClickSound;

    public float OverMouseUpPosY;
    public float OverMouseScale;
    public float PlayOnPositionY;
	public float StartHandChangeOnPositionY;
	public float OnTouchDragYPosOffset = -0.5f;
    public GameObject CardPrefab;
	public GameObject CardBlurEffect;
	//public bool deck_build_mode_enabled = false;

	public TextMeshPro CardCostText;
	public TextMeshPro CardNameText;
	public TextMeshPro CardDescText;
	public TextMeshPro CardBelowText;
	public TextMeshPro CardTypeDescText;

    private int cardOrder;

    //private float OverMouseUpToPosY;
    private Vector3 dist;
    //private Vector3 startPos;
    //private Vector3 startScale;
    //private Vector3 startRotation;
    private Vector3 VectorOverMouseScale;
    private int startLayer;
    private SpriteRenderer cardSpriteRender;
    private GameObject CardHighlight;
	private CardInteraction CardHighlightInteraction;

    public Transform pawnTransform;
	private Vector3 pawnStartPosition;
	//public Vector3 startPawnScale;

	private SmothTransform SmoothTrComp;
    public Pawn pawnComponent;
	private Hand HandComp;
	public GamePlay GameplayController;
	//private DeckCardsScroll DeckScrollComp;
	//private bool isInDeck = false;
	public bool inDeckInteractions = false;
	public bool StartDraw = false;

	public int cardHandIndex = 0;
	public Color CanBePlayedColor;
	public Color CanNotBePlayedColor;
	private bool signalStatus = false;

	public bool isMouseOver = false;
	public bool isNowDrag = false;
	//private bool isNowPressed = false;
	public bool isNowTouched = false;
	public bool isDisableTouchEnter = false;
	private bool isTouchSetup = false;
    private bool isDragSetup = false;
    private bool isIddleSetup = true;
    private bool isCardHiglightSpawned = false;
	public bool interactions_enabled = false;
	//private bool deck_build_mode_enabled_setup = false;
    private bool isOnPlaySetup = false;
	private bool isOnPlayPos = false;
	public bool longPressDetected = false;

	public bool simulateTouches = true;

	public CardPanel CardPanelComp;

	/*click detection*/
	public int HandDragMultiply = 140;
	public int HandDragPlayPosY = 280;
	private bool isHandDrawSetup = false;
	private bool isHandDrawPlayed = false;
	private float clickTimeCurrent = 0;
	private Vector2 startMousePos;
	/* swipe detection*/

    void Start()
	{
		//Debug.Log ("New card starts life");
		if (GameObject.Find ("Gameplay") != null) {
			GameplayController = GameObject.Find ("Gameplay").GetComponent<GamePlay> ();
		}
		cardSpriteRender = GetComponent<SpriteRenderer> ();
		if (transform.Find ("Pawn")) {
			pawnTransform = transform.Find ("Pawn");
			pawnStartPosition = pawnTransform.localPosition;
			//startPawnScale = pawnTransform.localScale;
			pawnComponent = pawnTransform.GetComponent<Pawn> ();
		}
		startLayer = cardOrder * 10;
		cardSpriteRender.sortingOrder = startLayer;
		//startPos = transform.position;
		//startScale = transform.localScale;
		//startRotation = transform.localEulerAngles;
		VectorOverMouseScale = new Vector3 (OverMouseScale, OverMouseScale, OverMouseScale);

		GetComponent<Renderer> ().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
		GetComponent<Renderer> ().receiveShadows = true;

		if (GameObject.Find ("Hand") != null) {
			HandComp = GameObject.Find ("Hand").GetComponent<Hand> ();
		}
		/*if (GameObject.Find ("DeckTable") != null) {
			DeckScrollComp = GameObject.Find ("DeckTable").GetComponent<DeckCardsScroll> ();
		}*/
		SmoothTrComp = GetComponent<SmothTransform> ();
		SetCardCost (CardCost);
	}

	void Update() {
        if (interactions_enabled) {
			Vector3 curPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, dist.z);
			Vector3 worldPos = Camera.main.ScreenToWorldPoint(curPos);
			if (isNowTouched) {
				if (worldPos.y > (PlayOnPositionY + OnTouchDragYPosOffset)) {
					Debug.Log ("Can be played by touch");
					isMouseOver = false;
					isNowDrag = true;
					isTouchSetup = true;
				} else {
					if (!HandComp.UseHandCanvas) {
						HandComp.ShowHand ();
					}
					if (isTouchSetup) {
						isTouchSetup = false;
						//isNowTouched = false;
						isMouseOver = false;
					} else {
						isMouseOver = true;
					}
					isNowDrag = false;
				}
			} else if (isTouchSetup) {
				isTouchSetup = false;
				isNowTouched = false;
				isMouseOver = false;
				isNowDrag = false;
			}
            if ((isMouseOver) && (!isNowDrag)) {
                OnOver();
                isDragSetup = false;
                isIddleSetup = false;
            } else if (isNowDrag) {
				//hand card click detection
				if (HandComp.UseHandCanvas) {
					if (!isHandDrawPlayed) {
						bool playOK = false;
						if (clickTimeCurrent == 0) {
							startMousePos = Camera.main.ScreenToViewportPoint (Input.mousePosition);
							clickTimeCurrent = 1f;
						}
						Vector3 MousePosCurrent = Camera.main.ScreenToViewportPoint (Input.mousePosition);
						float newYCardPos = -(startMousePos.y - MousePosCurrent.y) * HandDragMultiply;
						if (newYCardPos > 0) {
							Vector3 newPos = this.transform.localPosition;
							newPos.y = newYCardPos;
							this.transform.localPosition = newPos;
							isHandDrawSetup = true;
							if (newYCardPos > HandDragPlayPosY) {
								playOK = true;
							}
							//Debug.Log ("New hand drag:" + newYCardPos + " y1:" + startMousePos.y + " y2:" + MousePosCurrent.y);
						}
						if (longPressDetected) {
							Debug.Log ("long press on card");
							isHandDrawSetup = true;
							isHandDrawPlayed = true;
						} else {
							if (!playOK) {
								return;
							} else {
								Debug.Log ("play ok");
								isHandDrawPlayed = true;
							}
						}
						clickTimeCurrent = 0;
						PlayPlaySound ();
					}
				}
				//hand card click detection - end
				float OnPosY = StartDraw ? StartHandChangeOnPositionY : PlayOnPositionY;
				//Debug.Log ("worldPos.y" + worldPos.y);
                if (!isDragSetup) {
					HandComp.DisableOtherCards (this.gameObject);
                    if (isCardHiglightSpawned) {
                        //Destroy(CardHighlight);
						if (HandComp != null) {
							HandComp.SetCardFocus (-1);
						}
						CardHighlight.GetComponent<CardInteraction>().SetObjectVisible(false);
                        isCardHiglightSpawned = false;
                    }
                    SetObjectVisible(true);
                    cardSpriteRender.sortingOrder = 110;

                    isDragSetup = true;
                    isIddleSetup = false;

					GameplayController.UnsetOtherPawnsOnBoard ();
					GameplayController.SetBoardPiecesNormalColor ();
                }
				if (worldPos.y > OnPosY) {
					HandComp.HideHandSimple (this.gameObject);
					if (StartDraw) {
						isNowDrag = false;
						isNowTouched = false;
						//consider to remove this code
						//GameplayController.DrawNextStartCard (this.gameObject);
					} else {
						if (CanBePlayed ()) {
							OnPlay (curPos);
						} else {
							OnPlayCancel ();
						}
					}
                } else {
					if (!HandComp.UseHandCanvas) {
						HandComp.ShowHand ();
					}
                    OnDrag(curPos);
                    if (isOnPlaySetup)
                    {
                        isOnPlaySetup = false;
						pawnTransform.SetParent (transform);
						pawnTransform.GetComponent<SmothTransform> ().SmothTransformTo (pawnStartPosition, Quaternion.identity, 10f);
						pawnComponent.OnPlayCancel();
                        SetObjectVisible(true);
                    }
                }
            } else {
                if (isOnPlaySetup) {
					float OnPosY = StartDraw ? StartHandChangeOnPositionY : PlayOnPositionY;
					if (worldPos.y <= OnPosY) {
						OnPlayCancel ();
					}
                    isOnPlaySetup = false;
					if (isOnPlayPos) {
						OnPlayEnd ();
					} else {
						pawnTransform.SetParent (transform);
						pawnTransform.GetComponent<SmothTransform> ().SmothTransformTo (pawnStartPosition, Quaternion.identity, 10f);
						GameplayController.EnableHandShowHideMode ();
						pawnComponent.OnPlayCancel ();
						if (pawnTransform) {
							KeepParentRenderLayer PawnKeepComp;

							PawnKeepComp = pawnTransform.GetComponent<KeepParentRenderLayer> ();
							PawnKeepComp.KeepingActive = true;
						}
						ReturnToStartPosision ();
					}
                }
                if (!isIddleSetup) {
                    isDragSetup = false;
                    SetObjectVisible(true);
					HandComp.EnableOtherCards ();
                    if (isCardHiglightSpawned) {
                        //Destroy(CardHighlight);
						if (HandComp != null) {
							HandComp.SetCardFocus (-1);
						}
						CardHighlight.GetComponent<CardInteraction>().SetObjectVisible(false);
                        isCardHiglightSpawned = false;
                    }
                    isIddleSetup = true;
					ReturnToStartPosision();
                }
            }
        }
    }

	private void OnClick() {
		/*if (deck_build_mode_enabled) {
			//Debug.Log ("card clickded");
			DeckScrollComp.ShowCardInHighlight ((GameObject)Instantiate (CardPrefab));
		} else if (inDeckInteractions) {
			DeckScrollComp.Build_RemoveCardToDeck (this.gameObject);
			Destroy (this.gameObject);
		}*/
	}

	private bool CanBePlayed() {
		bool ReturnDecision = false;

		if (GameplayController != null) {
			if (GameplayController.myTurn == true) {
				if (CardCost <= GameplayController.Mana) {
					ReturnDecision = true;
				} else {
					GameplayController.ShowWarning ("Masz za mało punktów akcji aby zagrać tą kartę");
				}
			} else {
				GameplayController.ShowWarning ("Trwa tura przeciwnika");
			}
		}
		return ReturnDecision;
	}

	public bool SignalCanBePlayed() {
		bool rv = false;
		//Debug.Log ("SignalCanBePlayed");
		if (GameplayController != null) {
			//Debug.Log ("SignalCanBePlayed (cost: " + CardCost + " mana: " + GameplayController.Mana + ")");
			if (CardCost <= GameplayController.Mana) {
				ChangeCardSignalColor (true);
				rv = true;
			} else {
				ChangeCardSignalColor (false);
			}
		} else {
			Debug.Log ("Cannot find Gameplay component");
		}
		return rv;
	}

	public void ChangeCardSignalColor(bool setSignalColor) {
		Debug.Log ("ChangeCardSignalColor(" + setSignalColor + ")");
		if (signalStatus != setSignalColor) {
			SpriteRenderer renderer = CardBlurEffect.GetComponent<SpriteRenderer> ();
			KeepParentRenderLayer KeepRender = CardBlurEffect.GetComponent<KeepParentRenderLayer> ();
			if (CardBlurEffect != null) {
				if (setSignalColor) {
					//CardBlurEffect.GetComponent<Animation> ().Play ();
					//renderer.color = CanBePlayedColor;
					renderer.enabled = true;
					KeepRender.KeepEnable = true;
				} else {
					//CardBlurEffect.GetComponent<Animation> ().Stop ();
					//renderer.color = CanNotBePlayedColor;
					KeepRender.KeepEnable = false;
					renderer.enabled = false;
				}
			}
			if (CardHighlight != null) {
				//Destroy(CardHighlight);
				CardHighlight.GetComponent<CardInteraction> ().ChangeCardSignalColor (setSignalColor);
			}
		}
		signalStatus = setSignalColor;
	}

	void OnDestroy() {
		if (CardHighlight) {
			Destroy (CardHighlight);
		}
	}

    public void SetCardInterationsEnable(bool enable) {
        interactions_enabled = enable;
    }

	public void SetCardHandIndex(int index) {
		cardHandIndex = index;
		if (pawnComponent)
			pawnComponent.handIndex = index;
	}

    public void SetCardOrder(int order)
    {
        cardOrder = order;
        startLayer = cardOrder * 10;
        if (cardSpriteRender)
        {
			cardSpriteRender.sortingOrder = startLayer;
        }
    }

    public void SetCardCost(int value)
    {
		CardCost = value;
		if (CardCostText != null) {
			CardCostText.SetText ("" + value);
		}
    }

	public void SetName(string Name)
	{
		CardName = Name;
		if (CardNameText != null) {
			CardNameText.SetText (Name);
		}
	}

	public void SetDescription(string Desc)
	{
		CardDescription = Desc;
		if (CardDescText != null) {
			CardDescText.SetText (Desc);
		}
	}

	public void SetBelowText(string Text)
	{
		if (CardBelowText != null) {
			CardBelowText.SetText (Text);
		}
	}

	public void SetTypeDescText(string Text)
	{
		if (CardTypeDescText != null) {
			CardTypeDescText.SetText (Text);
		}
	}

    private void ReturnToStartPosision()
    {
		//Debug.Log ("return to start pos: " + this.gameObject.name);
       // transform.position = startPos;
        cardSpriteRender.sortingOrder = startLayer;
		if (HandComp) {
			HandComp.SortCardsInHand ();
		} else {
			SmoothTrComp.SmothTransformTo (pawnStartPosition, 30);
		}
      //  transform.localScale = startScale;
       // transform.localEulerAngles = startRotation;
    }

    private void OnOver()
    {
		//SmoothTrComp.smothTransformPosRunning = false;
		if (!HandComp.UseHandCanvas) {
			if (!isCardHiglightSpawned) {
				CardInteraction cardInt;
				Pawn PawnComp;
				GameObject cardPawn;
				Quaternion newRot = new Quaternion (0, 0, 0, 0);

				//Debug.Log ("Show card highlight");

				//Vector3 locCPos = transform.localPosition;
				//locCPos.y = 15.5f;
				//SmoothTrComp.smothTransformPosRunning = false;
				//transform.localPosition = locCPos;

				if (HandComp != null) {
					HandComp.SetCardFocus (cardHandIndex);
				}

				if (CardHighlight == null) {
					CardHighlight = (GameObject)Instantiate (CardPrefab, transform.position, newRot);
					if (CardHighlight.GetComponent<BoxCollider2D> () != null) {
						Destroy (CardHighlight.GetComponent<BoxCollider2D> ());
					} else {
						Destroy (CardHighlight.GetComponent<BoxCollider> ());
					}
				}
				cardInt = CardHighlight.GetComponent<CardInteraction> ();
				CardHighlight.transform.localPosition = transform.position;
				CardHighlight.transform.localRotation = newRot;
				CardHighlight.GetComponent<CardInteraction> ().SetObjectVisible (true);
				if (cardInt.pawnComponent != null) {
					cardPawn = CardHighlight.transform.Find ("Pawn").gameObject;
					Destroy (cardPawn.GetComponent<PolygonCollider2D> ());
					Vector3 locPos = transform.position;
					cardInt.SetCardOrder (50);
					cardInt.SetName (CardName);
					cardInt.SetDescription (CardDescription);
					cardInt.SetTypeDescText (CardTypeDescText.text);
					cardInt.SetCardCost (CardCost);
					PawnComp = cardPawn.GetComponent<Pawn> ();
					PawnComp.SetHealth (PawnComp.Health);
					PawnComp.SetAttack (PawnComp.Attack);

					CardHighlight.GetComponent<SmothTransform> ().SmothTransformTo (locPos, newRot, 15);
					CardHighlight.transform.localRotation = newRot;
					CardHighlight.GetComponent<SmothTransform> ().smoothTransformScaleRunning = false;
					//Debug.Log (" Set card highlight scale to: " + VectorOverMouseScale);
					//Debug.Log ("current scale: " + CardHighlight.transform.localScale);
					//Debug.Log ("current rotation: " + CardHighlight.transform.rotation);
					CardHighlight.transform.localScale = VectorOverMouseScale;

					//if (Input.touchCount == 0) {//probably not needed
					//HandComp.DisableOtherCards (this.gameObject);
					//}

					isCardHiglightSpawned = true;
				} else {
					//Destroy (CardHighlight);
					CardHighlight.GetComponent<CardInteraction> ().SetObjectVisible (false);
				}
				SetObjectVisible(false);
			}
			Vector3 newPos = transform.position;
			newPos.y = OverMouseUpPosY;
			CardHighlight.GetComponent<SmothTransform> ().SmothTransformTo (newPos, 15);
			if (HandComp != null) {
				HandComp.SetCardFocus (cardHandIndex);
			}
		}
    }

    private void OnDrag(Vector3 cursorPos)
    {
        Vector3 rotate = new Vector3(0, 0, 0);
        float rotate_max = 80;
		float rot_multiple = 100;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(cursorPos);
		SmoothTrComp.smoothTransformPosRunning = false;
		SmoothTrComp.smoothTransformRotRunning = false;

		if (cardOrder != 50)
			SetCardOrder(50);

        if (worldPos.x > transform.position.x) {
			rotate.y = (worldPos.x - transform.position.x) * rot_multiple * -1;
            if (rotate.y < -rotate_max) {
                rotate.y = -rotate_max;
            }
        } else if (worldPos.x < transform.position.x) {
			rotate.y = (transform.position.x - worldPos.x) * rot_multiple;
            if (rotate.y > rotate_max) {
                rotate.y = rotate_max;
            }
        }

        if (worldPos.y > transform.position.y) {
			rotate.x = (worldPos.y - transform.position.y) * rot_multiple;
            if (rotate.x > rotate_max) {
                rotate.x = rotate_max;
            }
        } else if (worldPos.y < transform.position.y) {
			rotate.x = (transform.position.y - worldPos.y) * rot_multiple * -1;
            if (rotate.x < -rotate_max) {
                rotate.x = -rotate_max;
            }
        }

		//Debug.Log ("rot y to :" + rotate.y);

        //transform.localEulerAngles = rotate;
		SmoothTrComp.SmothTransformTo(Quaternion.Euler(rotate), 10f);
        transform.position = worldPos;
    }

	private void OnPlay(Vector3 cursorPosition)
    {
        //Debug.Log("on play now");
        if (!isOnPlaySetup) {
			if (pawnComponent.OnPlayStart ()) {
				pawnComponent.OnPlay ();
				isOnPlayPos = true;
				HandComp.HideHand ();
			} else {
				pawnComponent.OnPlay ();//must be called at least one time, check the effect status
				pawnTransform.SetParent (transform);
				pawnTransform.GetComponent<SmothTransform> ().SmothTransformTo (pawnStartPosition, Quaternion.identity, 10f);
				pawnComponent.OnPlayCancel();
				OnPlayCancel ();
			}
        }
        int playControl = pawnComponent.OnPlay();//Maybe it should be enum?
		if (playControl == -1) {         //-1: This card cannot be played
			pawnTransform.SetParent (transform);
			pawnTransform.GetComponent<SmothTransform> ().SmothTransformTo (pawnStartPosition, Quaternion.identity, 10f);
			pawnComponent.OnPlayCancel ();
			OnPlayCancel ();
		} else if (playControl == 1) {   // 1: Do not hide the card
			OnDrag(cursorPosition);
		} else {//if (playControl == 0) {// 0: Default - Hide card background
			SetObjectVisible(false);//Its only temporary. There will be some cool visual effect :)
		}

		isOnPlaySetup = true;
    }

	private void OnPlayEnd()
	{
		//HandComp.HandShowHideModeEnabled = false;
		HandComp.Hided = false;
		HandComp.HideHand ();
		HandComp.SetCardFocus (-1);
		SetObjectVisible (true);
		pawnComponent.OnPlayEnd();
		HandComp.EnableOtherCards ();
		if (HandComp) {
			HandComp.RemoveCardFromHand (this.gameObject);
		}
		GameplayController.GetCardCost (CardCost);
		Destroy(this.gameObject);
		isOnPlaySetup = false;
	}

	private void OnPlayCancel() {
		isMouseOver = false;
		isNowDrag = false;
		isOnPlaySetup = false;
		isOnPlayPos = false;
		isNowTouched = false;
		HandComp.EnableOtherCards ();
		HandComp.SetCardFocus (-1);
		ReturnToStartPosision ();
		PlayDrawBackSound ();
		GameplayController.EnableHandShowHideMode ();
	}

	void StopAnimations()
	{
		SmoothTrComp.smoothTransformPosRunning = false;
		SmoothTrComp.smoothTransformRotRunning = false;
	}
		
    void OnMouseEnter()
    {
		//Debug.Log (" OnMouseEnter pawn handidx: " + pawnComponent.handIndex + " touch:" + Input.touchCount);
		if (isNowDrag == false && !isDisableTouchEnter) {
			if (HandComp && HandComp.Hided) {
				if (Input.touchCount == 0 && !simulateTouches) {
					//HandComp.ShowHand ();
				}
			} else {
				if (Input.touchCount == 0 && !simulateTouches) {
					isMouseOver = true;
					isNowTouched = false;
				} else {
					//isMouseOver = true;
					if (HandComp) {
						HandComp.RefreshTouchOfCardsInHand ();
					}
					dist = Camera.main.WorldToScreenPoint (transform.position);
					Debug.Log ("Set touch");
					isNowTouched = true;
				}
			}
		}
		isDisableTouchEnter = false;
	}

    void OnMouseExit()
    {
		//Debug.Log (" OnMouseExit pawn handidx: " + pawnComponent.handIndex + " touch:" + Input.touchCount);
		isMouseOver = false;
    }

    void OnMouseDown()
    {
		//if (!EventSystem.current.IsPointerOverGameObject ()) {
		if (HandComp) {
			HandComp.HideDelayedCancel = true;
			PlayClickSound ();
		}
		//Debug.Log (" OnMouseDown pawn handidx: " + pawnComponent.handIndex + " touch:" + Input.touchCount);
		if (HandComp && HandComp.Hided) {
			HandComp.HandShowHideModeAutoEnabled = false;
			HandComp.DisableTouchEnterOfCardsInHand ();
			HandComp.ShowHand ();
		} else {
			if (Input.touchCount == 0 && !simulateTouches) {
				isMouseOver = false;
				isNowDrag = true;
				isNowTouched = false;
			} else {
				Debug.Log ("Set touch");
				isNowTouched = true;
				if (HandComp) {
					HandComp.EnableTouchEnterOfCardsInHand ();
				}
			}
			dist = Camera.main.WorldToScreenPoint (transform.position);
		}
		if (CardPanelComp != null) {
			CardPanelComp.RefreshCardPosition ();
			CardPanelComp.KeepCardPosition = false; 
		}
		//} else {
		//	Debug.LogWarning ("Mouse is not over - event dropped");
		//}
	}

    void OnMouseUp()
    {
		//Debug.Log (" OnMouseUp pawn handidx: " + pawnComponent.handIndex + " touch:" + Input.touchCount);
		if (CardPanelComp != null) {
			CardPanelComp.KeepCardPosition = true; 
		}
		if (isNowTouched) {
			HandComp.DisableTouchEnterOfCardsInHand ();
		}
        isMouseOver = false;
        isNowDrag = false;
		isNowTouched = false;
		longPressDetected = false;
        if (isOnPlaySetup) {
			HandComp.EnableOtherCards ();
        }
		if (isHandDrawSetup) {
			ReturnToZeroPosition ();
			isHandDrawSetup = false;
			isHandDrawPlayed = false;
		}
		if (HandComp) {
			HandComp.HandShowHideModeAutoEnabled = true;
			HandComp.RefreshTouchOfCardsInHand ();
		}
		clickTimeCurrent = 0;
    }

	private void ReturnToZeroPosition() {
		SmoothTrComp.SmothTransformTo (new Vector3(0,0,0), 30);
	}

	public void TouchOff() {
		//Debug.Log ("turn off touch");
		isMouseOver = false;
		isNowDrag = false;
		isNowTouched = false;
		longPressDetected = false;
	}

	public void SetObjectVisible(bool visible)
    {
		cardSpriteRender = GetComponent<SpriteRenderer> ();
        if (visible) {
            cardSpriteRender.color = new Color(1f, 1f, 1f, 1f);
        } else {
            cardSpriteRender.color = new Color(1f, 1f, 1f, 0f);
        }
    }

	public void DisableCard() {
		SetCardCollider (false);
	}

	public void EnableCard() {
		SetCardCollider (true);
	}

	public void PlayPlaySound() {
		PlayAudio (PlaySound);
	}

	public void PlayMoveSound() {
		PlayAudio (MoveSound);
	}

	public void PlayClickSound() {
		PlayAudio (ClickSound);
	}

	public void PlayDrawSound() {
		PlayAudio (DrawSound);
	}

	public void PlayDrawBackSound() {
		PlayAudio (DrawBackSound);
	}

	private void PlayAudio(AudioClip clip) {
		if (clip != null) {
			if (pawnComponent != null) {
				pawnComponent.AudioSourceComponent.PlayOneShot (clip);
			} else {
				AudioSource asource = GetComponent<AudioSource> ();
				if (asource != null) {
					asource.PlayOneShot (clip);
				}
			}
		}
	}

	private void SetCardCollider(bool enable) {
		BoxCollider2D coll = this.GetComponent<BoxCollider2D> ();
		if (coll != null) {
			coll.enabled = enable;
		} else {
			this.GetComponent<BoxCollider> ().enabled = enable;
		}
	}
}
