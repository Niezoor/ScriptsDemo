using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SNCharacter : MonoBehaviour {
	[System.Serializable]
	public enum CharacterBodyTypes
	{
		Normal, Metal,
	}

	[Header("Statistics")]
	public int health = 100;
	public CharacterBodyTypes bodyType;

	[Header("Components")]
	public HealthBar healthBar;
	public Transform characterTransform;
	public GameObject damageIndicatorPrefab;
	public GameObject bloodSplashPrefab;
	public GameObject hitSparkPrefab;

	[Header("States")]
	public bool dead = false;
	public bool immune = false;
	public bool canAttack = true;
	public bool canBlocking = true;
	public bool blocking = false;
	public bool canAiming = true;
	public bool aiming = false;
	public bool forceAiming = false;
	public bool parry = false;
	private Coroutine parryDelayCo;

	[HideInInspector]
	public SNMovementController movementController;

	[Header("Assets")]
	public GameObject defaultWeaponPrefab;
	public SNWeapon weapon;
	public GameObject defaultGunPrefab;
	public SNGun gun;

	[Header("Settings")]
	public Color bloodColor = Color.red;
	public Color hitColor = Color.red;
	public float showHitColorFor = 0.2f;

	private Shader whiteShader;
	private Shader normalShader;
	private Coroutine showHitCo;
	private Coroutine parryKnockbackCo;

	public Vector3 localGunPosition;

	[HideInInspector]
	public SpriteRenderer spriteRenderer;

	public float attackStartTime = 10;

	private Coroutine touchShotingCoroutine;

	// Use this for initialization
	void Start () {
		attackStartTime = 10;
		movementController = GetComponent<SNMovementController> ();
		if ((weapon == null) && (defaultWeaponPrefab != null)) {
			weapon = Instantiate (defaultWeaponPrefab).GetComponent<SNWeapon> ();
			weapon.transform.SetParent (characterTransform);
			weapon.transform.localPosition = Vector3.zero;
		}
		if (weapon != null) {
			weapon.SetOwner (this);
		}

		if ((gun == null) && (defaultGunPrefab != null)) {
			gun = Instantiate (defaultGunPrefab).GetComponent<SNGun> ();
			gun.transform.SetParent (movementController.armsAnimationController.transform);
			gun.transform.localPosition = localGunPosition;
		}
		if (gun != null) {
			gun.SetOwner (this);
			gun.SetHide (true);
		}

		whiteShader = Shader.Find("GUI/Text Shader");
		normalShader = Shader.Find("Sprites/Default");

		spriteRenderer = this.transform.GetChild(0).GetComponent<SpriteRenderer> ();

		if (healthBar != null) {
			healthBar.MaxHealth = health;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (attackStartTime < 10) {
			attackStartTime += Time.deltaTime;
		}
		/*if ((gun) && (movementController)) {
			gun.SetDirection (movementController.aimDirection);
		}*/
	}

	public void StartShoting() {
		if ((gun != null) && (canAiming)) {
			gun.StartShoting ();
		}
	}

	public void StopShoting() {
		if (gun != null) {
			gun.StopShoting ();
		}
	}

	public void StartTouchShoting() {
		if (gun != null) {
			touchShotingCoroutine = StartCoroutine (TouchShoting ());
		}
	}

	public void StopTouchShoting() {
		if (gun != null) {
			if (touchShotingCoroutine != null) {
				StopCoroutine (touchShotingCoroutine);
				StopShoting ();
			}
		}
	}

	private IEnumerator TouchShoting() {
		if (gun) {
			yield return new WaitForSeconds (0.2f);
			StartShoting ();
			while (true) {
				yield return new WaitForSeconds (0.5f);
				StartShoting ();
			}
		}
	}

	public void Attack() {
		if (canAttack) {
			if (weapon) {
				weapon.Attack ();
			}
		}
	}

	public void BreakAttack() {
		if (weapon) {
			weapon.BreakAttack ();
		}
	}

	public void TakeDamage(SNWeapon.AttackClass attack, SNCharacter dealer) {
		if (!dead) {
			bool criticalHit = false;
			int damage = Random.Range (attack.attackDamageMin, attack.attackDamageMax);
			float crit = Random.Range (0.01f, 1f);

			if (crit <= attack.criticalDamageRatio) {
				criticalHit = true;
				damage *= 2;
			}
			ShowHit ();
			if ((attack.shakeDuration > 0) && (attack.shakeStrengh > 0)) {
				Camera.main.GetComponent<CameraFollow> ().Shake (attack.shakeDuration, attack.shakeStrengh);
			}
			health -= damage;
			if (healthBar != null) {
				healthBar.RefreshHealth (health);
			}
			if (damageIndicatorPrefab != null) {
				GameObject gob = Instantiate (damageIndicatorPrefab, transform.GetChild(0).position, Quaternion.identity);
				gob.GetComponent<DamageIndicator> ().ShowDamage (damage, criticalHit);
				gob.GetComponent<Animator> ().Play ("dmg_indicator_show" + Random.Range (1, 4));
				Destroy (gob, 5);
			}
			StartCoroutine(ShowBloodSplash (attack, dealer));
			ShowHitSpark (attack);
			if (!immune) {
				Vector2 dir = dealer.transform.position - this.transform.position;
				movementController.Hurt (dir, attack.enemyPushDuration);
				movementController.Knockback (-dir, attack.enemyPushForce, attack.enemyPushDuration);
			}

			if (health <= 0 && !dead) {
				Death ();
			}
		}
	}

	public void StartBlocking() {
		if (canBlocking && !forceAiming) {
			if (weapon != null) {
				if (weapon.blockPower > 0) {
					blocking = false;
					parry = true;
					canAiming = false;
					if (aiming) {
						EndAiming ();
					}
					canAttack = false;
					if (movementController != null) {
						movementController.ResetArmsPosition ();
						movementController.UnlockAnimations ();
						movementController.PlayAnimation (SNMovementController.AnimationsStates.Parry, true);
						movementController.canMove = false;
						movementController.canDash = false;
						movementController.canJump = false;
					}
					if (weapon.weaponAnimator != null) {
						weapon.weaponAnimator.Play ("Parry");
					}
					if (parryDelayCo != null) {
						StopCoroutine (parryDelayCo);
					}
					parryDelayCo = StartCoroutine (EndParryDelayed (weapon.parryKnockbackDelay));
				}
			}
		}
	}

	public void StopBlocking() {
		if (parryDelayCo != null) {
			StopCoroutine (parryDelayCo);
		}
		blocking = false;
		parry = false;
		canAiming = true;
		canAttack = true;
		if (movementController != null) {
			movementController.UnlockAnimations ();
			movementController.canMove = true;
			movementController.canDash = true;
			movementController.canJump = true;
		}
		if (weapon.weaponAnimator != null) {
			weapon.weaponAnimator.Play ("Idle");
		}
		canBlocking = false;
		StartCoroutine (EnableBlockingDelayed (0.1f));
	}

	public void StartAiming() {
		if (canAiming) {
			if (gun) {
				gun.StartAiming ();
				aiming = true;
				canAttack = false;
				if (movementController) {
					//movementController.canMove = false;
					movementController.moveVelocity = Vector2.zero;
					movementController.canDash = false;
					movementController.canJump = false;
				}
			}
		}
	}

	public void EndAiming() {
		if (gun) {
			if (!forceAiming) {
				gun.EndAiming ();
				aiming = false;
				canAttack = true;
				if (movementController) {
					//movementController.canMove = true;
					movementController.canDash = true;
					movementController.canJump = true;
					movementController.resetWeaponAnim = true;
				}
			}
		}
	}

	public void ParryKnockback(Transform dealer, float duration, float force) {
		Vector2 direction = Vector2.zero;
		if (parryKnockbackCo != null) {
			StopCoroutine (parryKnockbackCo);
		}
		if (dealer) {
			if (dealer != this) {
				ShowHitSpark (2, (dealer.position + this.characterTransform.position) / 2);
			}
			direction = dealer.position - characterTransform.position;//this.transform.position;
		} else {
			ShowHitSpark (2, this.characterTransform.position);
			if (movementController) {
				direction = movementController.lastInputMove;
			}
		}
		parryKnockbackCo = StartCoroutine(ParryKnockbackTask (direction, duration, force));
	}

	public void HitBlock(SNWeapon.AttackClass attack, Transform dealer, float duration, float force) {
		if (movementController != null) {
			Vector2 dir = dealer.position - this.characterTransform.position;
			//bool criticalHit = false;
			int damage = Random.Range (attack.attackDamageMin, attack.attackDamageMax);
			float crit = Random.Range (0.01f, 1f);

			if (crit <= attack.criticalDamageRatio) {
				//criticalHit = true;
				damage *= 2;
			}
			ShowHitSpark (2, (dealer.position + this.characterTransform.position) / 2);
			if (weapon != null) {
				weapon.blockPower -= damage;
			}
			if (weapon.blockPower > 0) {
				movementController.Knockback (-dir, attack.enemyPushForce, attack.enemyPushDuration);
			} else {
				ParryKnockback (dealer, duration, force);
			}
		}
	}

	private IEnumerator ParryKnockbackTask(Vector2 dir, float duration, float force) {
		if (movementController != null) {
			StopBlocking ();
			movementController.DashEnd ();
			movementController.SetAnimationDirection (dir);
			if (weapon != null) {
				weapon.BreakAttack ();
				if (weapon.weaponAnimator != null) {
					SNWeapon.SetAnimationDirection (weapon.weaponAnimator, dir);
					weapon.weaponAnimator.Play("Knockback");
				}
			}
			canAttack = false;
			movementController.canMove = false;
			movementController.PlayAnimation (SNMovementController.AnimationsStates.Knockback, true);
			movementController.Knockback (-dir, force, duration);
			yield return new WaitForSeconds (duration);
			movementController.UnlockAnimations ();
			if ((weapon != null) && (weapon.weaponAnimator != null)) {
				weapon.weaponAnimator.Play("Idle");
			}
			movementController.canMove = true;
			canAttack = true;
		}
	}

	private IEnumerator EndParryDelayed(float delay) {
		yield return new WaitForSeconds (delay);
		if (parry) {
			blocking = true;
			parry = false;
			if (movementController) {
				movementController.canMove = true;
				movementController.resetWeaponAnim = true;
			}
		}
	}

	private IEnumerator EnableBlockingDelayed(float delay) {
		yield return new WaitForSeconds (delay);
		canBlocking = true;
	}

	private void ShowHitSpark(SNWeapon.AttackClass attack) {
		if (hitSparkPrefab != null) {
			if (attack.hitSparkIndexes.Length > 0) {
				int index = attack.hitSparkIndexes [Random.Range (0, attack.hitSparkIndexes.Length)];
				ShowHitSpark (index, characterTransform.position);
			}
		}
	}

	private void ShowHitSpark(int index, Vector3 position) {
		Debug.Log ("Show hit spark with index " + index);
		Quaternion newRot = Quaternion.Euler (0, 0, Random.Range (0f, 180f));
		GameObject gob = Instantiate (hitSparkPrefab, position, newRot);
		gob.GetComponent<Animator> ().Play ("Hit" + index);
		Destroy (gob, 2);
	}

	private IEnumerator ShowBloodSplash(SNWeapon.AttackClass attack, SNCharacter dealer) {
		if (bloodSplashPrefab != null) {
			if (attack.bloodSplahIndexes.Length > 0) {
				int index = attack.bloodSplahIndexes [Random.Range (0, attack.bloodSplahIndexes.Length)];

				GameObject gob = Instantiate (bloodSplashPrefab, characterTransform.position, Quaternion.identity);
				if (dealer) {
					float direction = transform.position.x - dealer.transform.position.x;
					if (direction > 0) {
						gob.GetComponent<SpriteRenderer> ().flipX = true;
					}
				}
				Debug.Log ("Show blood splash with index " + index);
				gob.GetComponent<SpriteRenderer> ().color = bloodColor;
				gob.GetComponent<Animator> ().Play ("Splash" + index);
				yield return new WaitForSeconds (1);
				gob.GetComponent<SpriteRenderer> ().sortingOrder = -1;
				//yield return new WaitForSeconds (1);
				gob.GetComponent<Animator> ().Play ("Hide" + index);
				yield return new WaitForSeconds (5);
				Destroy (gob);
			}
		}
	}

	private void Death() {
		this.GetComponent<Collider2D> ().enabled = false;
		movementController.UnlockAnimations ();
		movementController.PlayAnimation (SNMovementController.AnimationsStates.Death, true);
		movementController.canMove = false;
		movementController.SetVelocity (Vector2.zero);
		if (characterTransform != null) {
			characterTransform.tag = "Dead";
		}
		dead = true;
	}

	private void ShowHit() {
		if (showHitCo != null) {
			StopCoroutine (showHitCo);
		}
		showHitCo = StartCoroutine (ShowHitTask ());
	}

	private IEnumerator ShowHitTask() {
		if (spriteRenderer) {
			spriteRenderer.material.shader = whiteShader;
			spriteRenderer.color = hitColor;
			yield return new WaitForSeconds (showHitColorFor);
			spriteRenderer.material.shader = normalShader;
			spriteRenderer.color = Color.white;
		}
		yield return null;
	}

	public void OnHit(SNCharacter character) {
		if (weapon != null) {
			weapon.OnHit (character);
		}
	}

	void OnTriggerEnter2D(Collider2D col) {
		//Debug.Log (this.transform.name + " OnTriggerEnter2D " + col.name);
	}

	void OnCollisionEnter2D(Collision2D col) {
		//Debug.Log (this.transform.name + " OnCollisionEnter2D " + col.gameObject.name);
	}
}
