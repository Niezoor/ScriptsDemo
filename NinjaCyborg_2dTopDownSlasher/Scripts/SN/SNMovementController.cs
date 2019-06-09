using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SNMovementController : MonoBehaviour {
	[Header("Components")]
	public Animator animationController;
	public Animator armsAnimationController;
	public Transform armsTransform;
	public Vector3 armsPosition;
	public Vector3 armsPositionU;
	public Vector3 armsPositionD;
	public Vector3 armsPositionL;
	public Vector3 armsPositionR;
	public Vector3 gunPositionU;
	public Vector3 gunPositionD;
	public Vector3 gunPositionL;
	public Vector3 gunPositionR;

	[Header("Moving")]
	public float runSpeed = 1.5f;
	public float walkSpeed = 1f;
	public float jumpPower = 40;
	public bool doubleJump;
	private bool doubleJumpMade = false;
	private float currentSpeed;

	[Header("Dash")]
	public float dashSpeed;
	public float dashDuration;
	public float dashEnergy;
	public float dashEnergyMax;
	public float dashEnergyCost;
	public float dashEnergyRegen;
	public float dashInterval;
	public bool dashDealDamage;
	public bool dashImmuneDamage;
	public bool dashIgnoreCollisions;
	private Vector2 dashDirection;
	public bool canDash = true;

	public GameObject hurtbox;

	public GameObject dashEffectObject;
	public GameObject dashEffectPrefab;

	[System.Serializable]
	public enum AnimationsStates
	{
		//remember to add new enum field at the end
		Run, Idle, Jump, Fall, AttackP1, AttackP2,
		Dash, Hurt, Death, Prepare, Custom, Parry,
		Block, BlockWalkDown, BlockWalkLeft, BlockWalkRight, BlockWalkUp,
		Knockback, Hold, Shot, Aim,
	}
	private AnimationsStates currentAnimationState;
	private AnimationsStates currentArmsAnimationState;
	private bool animationChangeLock = false;

	[HideInInspector]
	public Rigidbody2D rigidBody;
	[HideInInspector]
	public SNCharacter character;
	[HideInInspector]
	public TopDownPhysics topDownPhysics;

	public Vector2 moveVelocity;
	public Vector2 aimDirection;

	public Vector2 lastInputMove;
	public bool canMove = true;
	public bool canJump = true;
	public bool simulateMove = false;

	private float decreaseVelocity;

	private bool dashing = false;
	private bool afterDash = false;
	private float dashTime;

	private Coroutine hurtCo;
	private Coroutine knockbackCo;
	private bool useKinematic;
	private Vector2 V2Zero = Vector2.zero;

	public bool resetWeaponAnim = true;

	// Use this for initialization
	void Start () {
		currentSpeed = runSpeed;
		rigidBody = GetComponent<Rigidbody2D> ();
		character = GetComponent<SNCharacter> ();
		topDownPhysics = GetComponent<TopDownPhysics> ();
		currentAnimationState = AnimationsStates.Custom;
		if (rigidBody.isKinematic) {
			useKinematic = true;
		} else {
			useKinematic = false;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (!canMove) {
			if (decreaseVelocity > 0) {
				moveVelocity -= moveVelocity * decreaseVelocity * Time.deltaTime;
			}
		}
		if (dashing) {
			dashTime -= Time.deltaTime;
			if (dashTime <= 0) {
				DashEnd ();
			} else {
				moveVelocity = dashDirection.normalized * dashSpeed;
			}
		}
		if (dashEnergy < dashEnergyMax) {
			dashEnergy += Time.deltaTime * dashEnergyRegen;
		}
		if (simulateMove) {
			MoveUpdate (Vector2.zero);
		}
	}

	void FixedUpdate()
	{
		if (moveVelocity.magnitude > 0f) {
			rigidBody.MovePosition (rigidBody.position + (moveVelocity  * Time.fixedDeltaTime));
			//RBody.AddForce (moveVelocity, ForceMode2D.Force);
		}
	}

	public void ResetArmsPosition() {
		if (armsTransform && armsAnimationController) {
			armsTransform.localPosition = armsPosition;
			armsAnimationController.transform.parent.localRotation = Quaternion.identity;
			SpriteRenderer rendrer = armsAnimationController.GetComponent<SpriteRenderer> ();
			rendrer.flipY = false;
			resetWeaponAnim = true;
		}
	}

	private IEnumerator UnlockAnimationsDelayed(float delay) {
		yield return new WaitForSeconds (delay);
		UnlockAnimations ();
	}

	private IEnumerator EndDashAnim(float delay) {
		yield return new WaitForSeconds (delay);
		EndDashAnim ();
	}

	private void EndDashAnim() {
		if ((afterDash) && (character != null) && (character.weapon != null)) {
			if (!animationChangeLock) {
				character.weapon.weaponAnimator.Play ("Idle");
			}
			afterDash = false;
		}
	}

	private IEnumerator JumpTask(float power, bool delayed) {
		if (delayed) {
			yield return new WaitForSeconds (0.1f);
		}
		topDownPhysics.Jump (power);
	}

	/// <summary>
	/// 2d top down jumping simulation
	/// </summary>
	/// <param name="power">Jump power</param>
	/// <param name="delayed">Delay before jump for animation, Default true</param>
	public void Jump(bool delayed = false) {
		if (canMove && canJump) {
			if (topDownPhysics.grounded) {
				topDownPhysics.Jump (jumpPower);
				doubleJumpMade = false;
			} else if (!doubleJumpMade) {
				topDownPhysics.Jump (jumpPower);
				doubleJumpMade = true;
			}
		}
	}

	public void StartBlocking() {
		if (character != null) {
			character.StartBlocking ();
		}
	}

	public void StopBlocking() {
		if (character.blocking) {
			if (character != null) {
				character.StopBlocking ();
			}
			if (armsAnimationController) {
				armsAnimationController.gameObject.SetActive (false);
				currentArmsAnimationState = AnimationsStates.Custom;
			}
		}
	}

	public void MoveUpdate (Vector2 move) {
		MoveUpdate (move, V2Zero);
	}

	public void MoveUpdate (Vector2 move, Vector2 aim) {
		if (move.magnitude > 0.001f) {
			lastInputMove = move.normalized;
		} else {
			if (aim.magnitude > 0.001f) {
				lastInputMove = aim.normalized;
			}
		}
		if (canMove) {
			if (character != null) {
				if (aim.magnitude > 0.001f) {
					if (!character.aiming) {
						if (character != null) {
							if (!character.blocking) {
								character.StartAiming ();
							}
						}
					}
				} else {
					if ((character) && (character.aiming) && (!character.forceAiming)) {
						character.EndAiming ();
						if (armsAnimationController) {
							armsAnimationController.gameObject.SetActive(false);
							currentArmsAnimationState = AnimationsStates.Custom;
						}
					}
				}
				if (aim.magnitude <= 0.001f) {
					aim = lastInputMove;
				}
				aimDirection = aim;
				if (character.blocking || character.aiming) {
					currentSpeed = walkSpeed;
					if (armsAnimationController) {
						armsAnimationController.gameObject.SetActive(true);
					}
					if (moveVelocity.magnitude > 0.001f) {
						AnimationsStates newAnimState = AnimationsStates.Block;
						Vector3 newArmsPos;
						Vector3 newAnimationFixedDirection;

						if (Mathf.Abs (aim.x) > Mathf.Abs (aim.y)) {
							if (aim.x > 0) {
								newAnimState = AnimationsStates.BlockWalkRight;
								newArmsPos = armsPositionR;
								newAnimationFixedDirection = Vector2.right;
							} else {
								newAnimState = AnimationsStates.BlockWalkLeft;
								newArmsPos = armsPositionL;
								newAnimationFixedDirection = Vector2.left;
							}
						} else {
							if (aim.y > 0) {
								newAnimState = AnimationsStates.BlockWalkDown;
								newArmsPos = armsPositionD;
								newAnimationFixedDirection = Vector2.down;
							} else {
								newAnimState = AnimationsStates.BlockWalkUp;
								newArmsPos = armsPositionU;
								newAnimationFixedDirection = Vector2.up;
							}
						}
						UnlockAnimations ();
						PlayAnimation (newAnimState, true);
						SetArmsAnimationDirection (newAnimationFixedDirection);
						if (character.blocking) {
							PlayArmsAnimation (newAnimState);
							if ((character.weapon) && (character.weapon.weaponAnimator)) {
								//character.weapon.weaponAnimator.Play (newAnimState.ToString());
								if ((character.weapon.weaponAnimator.speed == 1f) ||
									(resetWeaponAnim)) {
									character.weapon.weaponAnimator.Play ("");
									character.weapon.weaponAnimator.Play (AnimationsStates.Block.ToString ());
									//idle and block is the same animation, only speed is different
									character.weapon.weaponAnimator.speed = 5f;
									resetWeaponAnim = false;
								}
							}
						} else if (character.aiming) {
							armsTransform.localPosition = newArmsPos;
							if (character.gun) {
								armsTransform.localPosition = newArmsPos;
								character.gun.SetDirection (aim);
							}
						}
					} else {
						UnlockAnimations ();
						PlayAnimation (AnimationsStates.Block, true);
						if (character.blocking) {
							PlayArmsAnimation (AnimationsStates.Block);
							if ((character.weapon) && (character.weapon.weaponAnimator)) {
								if ((character.weapon.weaponAnimator.speed == 5f) ||
									(resetWeaponAnim)) {
									character.weapon.weaponAnimator.Play ("");
									character.weapon.weaponAnimator.Play (AnimationsStates.Block.ToString ());
									//idle and block is the same animation, only speed is different
									character.weapon.weaponAnimator.speed = 1f;
									resetWeaponAnim = false;
								}
							}
							SetArmsAnimationDirection (lastInputMove);
						} else if (character.aiming) {
							if (character.gun) {
								character.gun.SetDirection (aim);
							}
							if (Mathf.Abs (lastInputMove.x) > Mathf.Abs (lastInputMove.y)) {
								if (lastInputMove.x > 0) {
									SetArmsAnimationDirection (Vector2.right);
									armsTransform.localPosition = armsPositionR;
								} else {
									SetArmsAnimationDirection (Vector2.left);
									armsTransform.localPosition = armsPositionL;
								}
							} else {
								if (lastInputMove.y > 0) {
									SetArmsAnimationDirection (Vector2.down);
									armsTransform.localPosition = armsPositionD;
								} else {
									SetArmsAnimationDirection (Vector2.up);
									armsTransform.localPosition = armsPositionU;
								}
							}
						}
					}
				} else {
					
					if ((character.weapon) && (character.weapon.weaponAnimator)) {
						character.weapon.weaponAnimator.speed = 1f;
						resetWeaponAnim = true;
					}
					currentSpeed = runSpeed;
				}
			}
			if (topDownPhysics.rising) {
				PlayAnimation (AnimationsStates.Jump);
			} else if (topDownPhysics.falling) {
				PlayAnimation (AnimationsStates.Fall);
			} else if (move.magnitude > 0.01f) {
				PlayAnimation (AnimationsStates.Run);
				if (afterDash) {
					EndDashAnim ();
				}
			} else {
				if (!afterDash) {
					PlayAnimation (AnimationsStates.Idle);
				}
			}
			SetAnimationDirection (lastInputMove);
			if (decreaseVelocity > 0) {
				decreaseVelocity = 0;
			}
			moveVelocity = move.normalized * currentSpeed;
		}
	}

	public void SetVelocity(Vector2 velocity) {
		moveVelocity = velocity;
	}

	public void DecreseVelocity(float amount) {
		decreaseVelocity = amount;
	}

	public void Dash() {
		if (canMove && canDash) {
			if (dashEnergy > dashEnergyCost) {
				dashEnergy -= dashEnergyCost;
				dashing = true;
				afterDash = false;
				canMove = false;
				dashTime = dashDuration;
				dashDirection = lastInputMove;
				PlayAnimation (AnimationsStates.Dash, true);
				if (character != null) { 
					character.canAttack = false;
					if (character.weapon != null) {
						SNWeapon.SetAnimationDirection (character.weapon.weaponAnimator, dashDirection);
						character.weapon.weaponAnimator.Play ("Idle");
						character.weapon.weaponAnimator.Play (AnimationsStates.Dash.ToString ());
						if (dashDealDamage) {
							character.weapon.EnableAttack ("Dash");
						}
					}
				}
				if (dashEffectObject != null) {
					DashTrail trail = dashEffectObject.GetComponent<DashTrail> ();
					if (trail != null) {
						trail.Show (Angle (dashDirection));
						if ((character != null) && (character.spriteRenderer != null)) {
							character.spriteRenderer.enabled = false;
						}
					}
				}
				if (dashEffectPrefab != null) {
					GameObject dashEffectAnimObject = Instantiate (dashEffectPrefab);
					dashEffectAnimObject.transform.position = character.characterTransform.position;
					Destroy (dashEffectAnimObject, 1);
				}
				if (dashIgnoreCollisions) {
					topDownPhysics.SetCollitionGhostLevel ();
				}
				if (dashImmuneDamage) {
					if (hurtbox != null) {
						hurtbox.SetActive (false);
					}
				}
			}
		}
	}

	public void DashEnd() {
		if (dashing) {
			dashing = false;
			canMove = true;
			afterDash = true;
			if (character != null) {
				character.canAttack = true;
			}
			if (dashEffectObject != null) {
				DashTrail trail = dashEffectObject.GetComponent<DashTrail> ();
				if (trail != null) {
					trail.Hide ();
					if ((character != null) && (character.spriteRenderer != null)) {
						character.spriteRenderer.enabled = true;
					}
				}
			}
			if (dashIgnoreCollisions) {
				topDownPhysics.SetCollitionGroundLevel (true);
			}
			if (dashDealDamage) {
				if ((character != null) && (character.weapon != null)) {
					character.weapon.DisableAttack ();
				}
			}
			if (dashImmuneDamage) {
				if (hurtbox != null) {
					hurtbox.SetActive (true);
				}
			}
			StartCoroutine (EndDashAnim (0.3f));
			UnlockAnimations ();
			canDash = false;
			StartCoroutine (DashInterval ());
		}
	}

	private IEnumerator DashInterval() {
		yield return new WaitForSeconds (dashInterval);
		canDash = true;
	}

	public void SetAnimationDirection(Animator animator, Vector2 direction) {
		if (animator != null) {
			animator.SetFloat ("Horizontal", direction.x);
			animator.SetFloat ("Vertical", direction.y);
		}
	}

	public void SetAnimationDirection(Vector2 direction) {
		if (animationController != null) {
			SetAnimationDirection (animationController, direction);
		}
	}

	public void SetArmsAnimationDirection(Vector2 direction) {
		if (armsAnimationController != null) {
			SetAnimationDirection (armsAnimationController, direction);
		}
		if (character.weapon) {
			SetAnimationDirection (character.weapon.weaponAnimator, direction);
		}
	}

	private IEnumerator HurtTask(Vector2 direction, float duration) {
		if (character) {
			character.BreakAttack ();
			character.canAttack = false;
		}
		UnlockAnimations ();
		canMove = false;
		moveVelocity = Vector2.zero;
		PlayAnimation (AnimationsStates.Idle);
		SetAnimationDirection (direction);
		PlayAnimation (AnimationsStates.Hurt, true);
		yield return new WaitForSeconds (duration);
		canMove = true;
		if (character) {
			character.canAttack = true;
		}
		UnlockAnimations ();
		//SetAnimationDirection (lastInputMove);
	}

	/// <summary>
	/// Show hurt animation
	/// </summary>
	/// <param name="direction">Direction of received damage</param>
	/// <param name="duration">Duration of showing hurt animation</param>
	public void Hurt(Vector2 direction, float duration) {
		if (hurtCo != null) {
			StopCoroutine (hurtCo);
		}
		hurtCo = StartCoroutine(HurtTask(direction, duration));
	}

	private IEnumerator KnockbackTask(Vector2 direction, float power, float duration) {
		if (useKinematic) {
			rigidBody.isKinematic = false;
		}
		Debug.LogWarning ("set knockback with force:" + power);
		SetVelocity (Vector2.zero);
		decreaseVelocity = 0;
		rigidBody.AddForce (direction.normalized * power, ForceMode2D.Impulse);
		yield return new WaitForSeconds (duration);
		rigidBody.velocity = Vector2.zero;
		if (useKinematic) {
			rigidBody.isKinematic = true;
		}
	}

	public void Knockback(Vector2 direction, float power, float duration) {
		if (knockbackCo != null) {
			StopCoroutine (knockbackCo);
		}
		knockbackCo = StartCoroutine(KnockbackTask(direction, power, duration));
	}

	public void PlayAnimation(AnimationsStates state, bool lockAnimation = false) {
		//TODO get rid of character.dead checking
		if (!animationChangeLock && !character.dead) {
			animationChangeLock = lockAnimation;
			if (currentAnimationState != state) {
				//Debug.Log (this.name + " play animation: " + state.ToString () + " lock:" + lockAnimation);
				animationController.Play (state.ToString());
				currentAnimationState = state;
			}
		}
	}

	public void PlayArmsAnimation(AnimationsStates state) {
		if (armsAnimationController) {
			if (currentArmsAnimationState != state) {
				Debug.Log ("Play arms animation:" + state);
				armsAnimationController.Play (state.ToString ());
				currentArmsAnimationState = state;
			}
		}
	}

	public void PlayAnimation(string animation, bool lockAnimation = false) {
		if (!animationChangeLock && !character.dead) {
			animationChangeLock = lockAnimation;
			Debug.Log (this.name + " play animation: " + animation + " lock:" + lockAnimation);
			animationController.Play (animation);
			currentAnimationState = AnimationsStates.Custom;
		}
	}

	public void UnlockAnimations() {
		animationChangeLock = false;
	}

	public void UnlockAnimations(float delay) {
		StartCoroutine(UnlockAnimationsDelayed (delay));
	}

	public static float Angle(Vector2 p_vector2)
	{
		if (p_vector2.x < 0)
		{
			return 360 - (Mathf.Atan2(p_vector2.x, p_vector2.y) * Mathf.Rad2Deg * -1);
		}
		else
		{
			return Mathf.Atan2(p_vector2.x, p_vector2.y) * Mathf.Rad2Deg;
		}
	}
}
