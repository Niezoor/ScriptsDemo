using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SNBullet : MonoBehaviour {
	[Header("Settings")]
	public float speed = 1f;
	public bool lookDirection = false;
	[Tooltip("rotate around own axis, ex: shuriken")]
	public bool rotate = false;
	public bool destroyOnHit = true;
	public bool showHitAnimation = true;
	public float durationOfHitAnimation = 1f;
	public SNWeapon.AttackClass attackDesc;

	[Header("Components")]
	public Transform bulletTransform;
	public Animator bulletAnimator;
	//public AudioSource bulletAudioSource;

	public Vector3 direction;
	public SNCharacter owner;

	private bool destroyed = false;

	// Use this for initialization
	void Start () {
		Destroy (this.gameObject, 10f);
	}

	// Update is called once per frame
	void Update () {
		//transform.Translate (Direction * Speed * Time.deltaTime);
		transform.position += direction.normalized * speed * Time.deltaTime;
		if (lookDirection) {
			float angle = Mathf.Atan2 (direction.y, direction.x) * Mathf.Rad2Deg;
			bulletTransform.rotation = Quaternion.AngleAxis (angle, Vector3.forward);
		} else if (rotate) {
			bulletTransform.Rotate (Vector3.forward * 1200 * Time.deltaTime, Space.World);
		}
	}

	public void HandleCollision(SNCharacter characterToHit) {
		if (!destroyed) {
			bool destroy = false;

			Debug.Log ("Handle bullet hit" + characterToHit);

			if (owner) {
				if (characterToHit) {
					if (!characterToHit.Equals (owner)) {
						characterToHit.TakeDamage (attackDesc, owner);
						destroy = destroyOnHit;
					}
				} else {
					destroy = destroyOnHit;
				}
			} else {
				Debug.LogError ("Missing bullet owner");
			}

			if (destroy) {
				rotate = false;
				destroyed = true;
				if (showHitAnimation) {
					//bulletAnimator.SetTrigger ("Hit");
					if (bulletAnimator) {
						bulletAnimator.Play ("Hit");
					}
					speed = 0;
					GetComponent<Collider2D> ().enabled = false;
					Destroy (this.gameObject, durationOfHitAnimation);
				} else {
					Destroy (this.gameObject);
				}
			}
		}
	}
}
