using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour {
	public SNWeapon weapon;
	public SNBullet bullet;

	// Use this for initialization
	void Start () {
		if (transform.parent != null) {
			weapon = transform.parent.GetComponent<SNWeapon> ();
			if (weapon == null) {
				if (transform.parent.parent != null) {
					weapon = transform.parent.parent.GetComponent<SNWeapon> ();
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter2D(Collider2D col) {
		Debug.Log (this.transform.name + " hitbox OnTriggerEnter2D " + col.name);
		if (col.name.Equals ("Hurtbox")) {
			SNCharacter charToHit = null;

			if ((col.transform.parent != null) && (col.transform.parent.parent != null)) {
				charToHit = col.transform.parent.parent.GetComponent<SNCharacter> ();
			} else {
				Debug.LogWarning ("no character found");
			}
			if (weapon != null) {
				weapon.OnHit (charToHit);
			} else if (bullet) {
				bullet.HandleCollision (charToHit);
			} else {
				Debug.LogWarning("no weapon found");
			}
		} else if (col.gameObject.layer == LayerMask.NameToLayer("WallLevel1")) {
			if (weapon != null) {
				weapon.OnHitWall (col.transform);
			} else if (bullet) {
				bullet.HandleCollision (null);
			}
		}
	}
}
