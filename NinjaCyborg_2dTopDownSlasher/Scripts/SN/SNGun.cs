using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SNGun : MonoBehaviour {
	[Header("Settings")]
	public float shotDuration;
	public float fireRate;
	public float amount;
	public float spread;
	[Tooltip("If not, regular spread")]
	public bool randomSpread = true;
	public bool fullAuto = true;

	[Header("Components")]
	public Animator animator;
	public GameObject bulletPrefab;
	public Transform gunTransform;
	public Transform bulletStartPosition;

	private Coroutine shotCo;
	private SNCharacter owner;
	private bool shoting = false;
	private bool shotProceed = false;
	public bool aiming = false;
	public bool horizontal = false;

	public float shotDelay = 0f;

	private string currentAnimation = "";
	private Vector2 direction;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if (shotDelay > 0f) {
			shotDelay -= Time.deltaTime;
		}
		if (shoting) {
			if (shotDelay <= 0f) {
				for (int i = 0; i < amount; i++) {
					float angle = SNMovementController.Angle (direction);

					if (randomSpread) {
						angle += Random.Range (-(spread/2f), (spread/2f));
					} else {
						if (amount > 1) {
							float anglePiece = spread / (amount - 1);
							angle = angle - (spread / 2f) + (anglePiece * i);
						}
					}
					float radian = angle * Mathf.Deg2Rad;
					Vector3 bDir = new Vector3 (Mathf.Sin (radian), Mathf.Cos (radian), 0);
					Shot (bDir);
					shotDelay = fireRate;
				}
				if (!fullAuto) {
					StopShoting ();
				}
			}
		}
	}

	public void SetDirection(Vector2 dir) {
		if (aiming && animator) {
			if (owner.movementController.armsAnimationController) {
				float angle = SNMovementController.Angle (dir);

				direction = dir;
				/*if (animator != null) {
				animator.SetFloat ("Horizontal", dir.x);
				animator.SetFloat ("Vertical", dir.y);
				}*/
				//Debug.Log ("angle " + angle);
				//angle = -(angle - 90);
				Quaternion newRot = Quaternion.Euler (0, 0, -(angle - 90));
				SpriteRenderer rendrer = animator.GetComponent<SpriteRenderer> ();
				SpriteRenderer rendrer2 = rendrer;

				//Debug.Log ("angle2 " + angle);

				rendrer2 = owner.movementController.armsAnimationController.GetComponent<SpriteRenderer> ();
				owner.movementController.armsAnimationController.transform.parent.localRotation = newRot;
				if (angle >= 45 && angle < 135) {
					//right
					horizontal = true;
					rendrer.flipY = false;
					rendrer2.flipY = false;
					gunTransform.localPosition = owner.movementController.gunPositionR;
				} else if (angle >= 135 && angle < 225) {
					//down
					horizontal = false;
					rendrer.flipY = false;
					rendrer2.flipY = false;
					gunTransform.localPosition = owner.movementController.gunPositionD;
				} else if (angle >= 225 && angle < 315) {
					//left
					horizontal = true;
					rendrer.flipY = true;
					rendrer2.flipY = true;
					gunTransform.localPosition = owner.movementController.gunPositionL;
				} else if (angle >= 315) {
					//up
					horizontal = false;
					rendrer.flipY = false;
					rendrer2.flipY = false;
					gunTransform.localPosition = owner.movementController.gunPositionU;
				} else { //&& angle < 45)
					//up
					horizontal = false;
					rendrer.flipY = false;
					rendrer2.flipY = false;
					gunTransform.localPosition = owner.movementController.gunPositionU;
				}
				//transform.localRotation = newRot;

			
				owner.movementController.armsAnimationController.Play ("Aim" + GetAnimationMode ());
					//SNWeapon.SetAnimationDirection (owner.movementController.animationController, dir);
					//animator.Play ("Hold" + GetAnimationMode ());
				if (!shotProceed) {
					PlayAnimation ("Hold" + GetAnimationMode ());
				}
			}
		}
	}

	public void StartShoting() {
		SetHide (false);
		owner.forceAiming = true;
		if (!aiming) {
			if (owner) {
				if (owner.movementController) {
					//start aim in last direction
					owner.movementController.MoveUpdate (Vector2.zero, owner.movementController.lastInputMove);
				}
			}
		}
		shoting = true;
	}

	public void StopShoting() {
		shoting = false;
		if (owner) {
			owner.forceAiming = false;
		}
	}

	public void Shot(Vector3 bulletDirection) {
		Debug.Log ("Shot from gun: " + this.gameObject.name);
		owner.forceAiming = true;
		if (!aiming) {
			if (owner) {
				if (owner.movementController) {
					//start aim in last direction
					owner.movementController.MoveUpdate (Vector2.zero, owner.movementController.lastInputMove);
				}
			}
		}
		if (shotCo != null) {
			StopCoroutine (shotCo);
		}
		shotCo = StartCoroutine (ShotTask (bulletDirection));
	}

	public void SetOwner(SNCharacter character) {
		owner = character;
	}

	public void StartAiming() {
		SetHide (false);
		aiming = true;
	}

	public void EndAiming() {
		SetHide (true);
		if ((owner) && (owner.movementController)) {
			owner.movementController.UnlockAnimations ();
		}
		aiming = false;
	}

	private void PlayAnimation(string anim) {
		if ((animator) && (!anim.Equals(currentAnimation))) {
			animator.Play (anim);
			currentAnimation = anim;
		}
	}

	private string GetAnimationMode() {
		if (horizontal) {
			return "H";
		} else {
			return "V";
		}
	}

	public void SetHide(bool hide) {
		Debug.Log ("Set gun hide: " + hide);
		if (animator) {
			if (hide) {
				if (owner && owner.movementController && owner.movementController.armsAnimationController) {
					owner.movementController.armsAnimationController.gameObject.SetActive (false);
				}
				animator.gameObject.SetActive(false);
			} else {
				if (owner && owner.movementController && owner.movementController.armsAnimationController) {
					owner.movementController.armsAnimationController.gameObject.SetActive (true);
				}
				animator.gameObject.SetActive(true);
			}
		}
	}

	private IEnumerator ShotTask(Vector3 bulletDirection) {
		//animator.Play ("Shot" + GetAnimationMode ());
		shotProceed = true;
		PlayAnimation ("Hold" + GetAnimationMode ());
		PlayAnimation ("Shot" + GetAnimationMode ());
		if (bulletPrefab != null) {
			GameObject ob = Instantiate (bulletPrefab, bulletStartPosition.position, this.transform.rotation);
			SNBullet bullet = ob.GetComponent<SNBullet> ();
			if (bullet != null) {
				bullet.owner = owner;
				bullet.direction = bulletDirection;
			}
		}
		yield return new WaitForSeconds (shotDuration);
		shotProceed = false;
	}
}
