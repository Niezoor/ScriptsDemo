using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SNWeapon : MonoBehaviour {

	public bool useWeaponAnimations = true;
	public float parryKnockbackDuration;
	public float parryKnockbackForce;
	public float parryKnockbackDelay;
	public float parryOnAttackTimeDifference = 0.2f;

	public float blockPower;
	public float blockPowerRegen;
	public float blockPowerMax;

	[System.Serializable]
	public class AttackClass {
		public string attackName;
		public bool enabled;
		public string weaponAndPlayerAnimationName;
		public int attackDamageMin;
		public int attackDamageMax;
		[Range(0f, 1f)]
		public float criticalDamageRatio = 0.1f;
		public float attackDuration;
		[Range(0f, 3f)]
		public float attackSpeed = 1;
		public AudioClip attackSound;
		public AudioClip hitBodySound;
		public AudioClip hitMetalSound;
		public float attackMoveVelocity;
		public float attackMoveVelocityDecrease;
		public float enemyPushForce;
		public float enemyPushDuration;
		public float shakeDuration;
		public float shakeStrengh;
		public int[] bloodSplahIndexes;
		public int[] hitSparkIndexes;
		[Header("Combo")]
		public string nextAttackName;
		public float timeToNextAttackEnable;
		public float timeToNextAttackTimeout;
		public float delayBetweenCombos;
	}

	public List<AttackClass> AttackList = new List<AttackClass>();
	public string firstAttackName;

	private AttackClass currentAttack;
	private AttackClass attackToChecking;
	private List<SNCharacter> currentlyAttackedCharacters = new List<SNCharacter>();

	[HideInInspector]
	public Animator weaponAnimator;
	private SNCharacter owner;

	private Coroutine EnableNextAttackCoroutine;
	private Coroutine AttackAnimationCoroutine;

	// Use this for initialization
	void Start () {
		if (useWeaponAnimations) {
			weaponAnimator = GetComponent<Animator> ();
		}
		currentAttack = FindAttackByName (firstAttackName);
		currentAttack.enabled = true;
	}
	
	// Update is called once per frame
	void Update () {
		if (blockPower < blockPowerMax) {
			blockPower += Time.deltaTime * blockPowerRegen;
		}
	}

	private AttackClass FindAttackByName(string name) {
		if (name != null) {
			foreach (AttackClass attack in AttackList) {
				if (attack.attackName.Equals (name)) {
					return attack;
				}
			}
		}
		return null;
	}

	public void SetOwner(SNCharacter character) {
		if (character != null) {
			owner = character;
			for (int i = 0; i < this.transform.childCount; i++) {
				Transform child = this.transform.GetChild (i);
				if (child != null) {
					KeepPlayerPosition keepC = child.GetComponent<KeepPlayerPosition> ();
					if (keepC != null) {
						keepC.SetPlayerTransform (character.transform);
					}
				}
			}
		}
	}

	public void Attack() {
		if ((currentAttack != null) && (currentAttack.enabled)) {
			Attack(currentAttack);
		}
	}

	public void BreakAttack() {
		StopCoroutineSave (EnableNextAttackCoroutine);
		StopCoroutineSave (AttackAnimationCoroutine);
		StartCoroutine (AttacksLock(currentAttack.delayBetweenCombos));
		OnAnimationEnd ();
	}

	public void OnHitWall(Transform wall) {
		owner.ParryKnockback (null, owner.weapon.parryKnockbackDuration, owner.weapon.parryKnockbackForce);
	}

	public void OnHit(SNCharacter character) {
		Debug.Log ("Attack hit:" + character.name);
		if (character != null && attackToChecking != null) {
			if ((owner != null) && (!owner.tag.Equals(character.tag))) {
				if (!currentlyAttackedCharacters.Contains(character)) {
					if (character.parry) {
						if (character.weapon != null) {
							owner.ParryKnockback (character.characterTransform, character.weapon.parryKnockbackDuration, character.weapon.parryKnockbackForce);
						}
					} else if (character.blocking) {
						character.HitBlock (currentAttack, owner.characterTransform, parryKnockbackDuration, parryKnockbackForce);
					} else if ((character.attackStartTime < 5) &&
						(Mathf.Abs(owner.attackStartTime - character.attackStartTime) < parryOnAttackTimeDifference)) {
						if (character.weapon != null) {
							owner.ParryKnockback (character.characterTransform, character.weapon.parryKnockbackDuration, character.weapon.parryKnockbackForce);
						}
						character.ParryKnockback (owner.characterTransform, parryKnockbackDuration, parryKnockbackForce);
					} else {
						character.TakeDamage (attackToChecking, owner);
						currentlyAttackedCharacters.Add (character);
					}
				}
			}
		}
	}

	private IEnumerator PauseAnimation(float duration) {
		if (weaponAnimator && owner) {
			if (attackToChecking != null) {
				weaponAnimator.speed = 0;
				owner.movementController.animationController.speed = 0;
			}
			yield return new WaitForSeconds (duration);
			if (attackToChecking != null) {
				weaponAnimator.speed = attackToChecking.attackSpeed;
				owner.movementController.animationController.speed = attackToChecking.attackSpeed;
			}
		}
	}

	private void StopCoroutineSave (Coroutine cor) {
		if (cor != null) {
			StopCoroutine (cor);
		}
	}

	private void Attack(AttackClass attack) {
		StopCoroutineSave (EnableNextAttackCoroutine);
		StopCoroutineSave (AttackAnimationCoroutine);
		attack.enabled = false;

		AttackAnimationCoroutine = StartCoroutine (PlayAttackAnimations (attack));

		AttackClass nextAttack = FindAttackByName (attack.nextAttackName);
		EnableNextAttackCoroutine = StartCoroutine (EnableNextAttackTask (nextAttack, attack.timeToNextAttackEnable, attack.timeToNextAttackTimeout));
	}

	public void EnableAttack(string name) {
		AttackClass attack = FindAttackByName (name);
		if (attack != null) {
			currentlyAttackedCharacters.Clear ();
			attackToChecking = attack;
			if (owner != null) {
				owner.attackStartTime = 0;
			}
		} else {
			Debug.LogError ("Cannot find attack desc: " + name);
		}
	}

	public void DisableAttack() {
		attackToChecking = null;
	}

	public static void SetAnimationDirection(Animator animator, Vector2 direction) {
		if (animator != null) {
			animator.SetFloat ("Horizontal", direction.x);
			animator.SetFloat ("Vertical", direction.y);
		}
	}

	/// <summary>
	/// Set fixed direction to not mix blend tree animations that enable hitbox colliders.
	/// It sets fixed 4 way direction to animator based on input V2 direction.
	/// </summary>
	/// <param name="weapon">Weapon animator component</param>
	/// <param name="owner">Weapon owner animator component</param>
	/// <param name="direction">Input direction</param>
	private void SetFixedDirection(Animator weapon, Animator owner, Vector2 direction) {
		if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y)) {
			if (direction.x > 0) {
				SetAnimationDirection (weapon, Vector2.right);
				SetAnimationDirection (owner, Vector2.right);
			} else if (direction.x < 0) {
				SetAnimationDirection (weapon, Vector2.left);
				SetAnimationDirection (owner, Vector2.left);
			}
		} else {
			if (direction.y > 0) {
				SetAnimationDirection (weapon, Vector2.up);
				SetAnimationDirection (owner, Vector2.up);
			} else if (direction.y < 0) {
				SetAnimationDirection (weapon, Vector2.down);
				SetAnimationDirection (owner, Vector2.down);
			}
		}
	}

	private IEnumerator PlayAttackAnimations(AttackClass attack) {
		if (owner != null) {
			attackToChecking = attack;
			currentlyAttackedCharacters.Clear ();
			owner.movementController.UnlockAnimations ();
			owner.movementController.canMove = false;
			owner.canBlocking = false;
			owner.movementController.SetVelocity (Vector2.zero);
			SetFixedDirection (weaponAnimator, owner.movementController.animationController, owner.movementController.lastInputMove);
			owner.movementController.PlayAnimation (attack.weaponAndPlayerAnimationName, true);
			owner.movementController.SetVelocity (owner.movementController.lastInputMove * attack.attackMoveVelocity);
			owner.movementController.DecreseVelocity (attack.attackMoveVelocityDecrease);
			owner.movementController.animationController.speed = attack.attackSpeed;
			if (weaponAnimator) {
				weaponAnimator.speed = attack.attackSpeed;
				weaponAnimator.Play (attack.weaponAndPlayerAnimationName);
			}
			owner.attackStartTime = 0;
			yield return new WaitForSeconds (attack.attackDuration);
			OnAnimationEnd ();
		}
	}

	private void OnAnimationEnd() {
		if (owner != null) {
			owner.movementController.UnlockAnimations ();
			owner.movementController.canMove = true;
			owner.movementController.animationController.speed = 1;
			if (weaponAnimator) {
				weaponAnimator.Play ("Idle");
				weaponAnimator.speed = 1;
			}
			owner.canBlocking = true;
			attackToChecking = null;
		}
	}

	private IEnumerator EnableNextAttackTask(AttackClass attack, float time_to_enable, float time_to_timeout) {
		yield return new WaitForSeconds (time_to_enable);
		if (attack != null) {
			currentAttack = attack;
			currentAttack.enabled = true;
		}
		yield return new WaitForSeconds (time_to_timeout-time_to_enable);
		currentAttack.enabled = false;
		StartCoroutine (AttacksLock(currentAttack.delayBetweenCombos));
	}

	private IEnumerator AttacksLock(float time) {
		currentAttack.enabled = false;
		yield return new WaitForSeconds (time);
		currentAttack = FindAttackByName (firstAttackName);
		currentAttack.enabled = true;
	}
}
