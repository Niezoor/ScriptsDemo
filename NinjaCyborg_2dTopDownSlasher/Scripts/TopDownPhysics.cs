using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownPhysics : MonoBehaviour {

	[Header("Settings")]
	public bool debugs = false;
	public bool destroyPhysicsAfterStop = false;
	public const float GravityMultiplier = 0.1f;
	public const float Acceleration = 15f;
	public Sprite[] shadowsList;

	public float changeShadowStep = 1f;
	public int startShadowIndex;
	private int currentShadowIndex;

	[Header("Components")]
	[HideInInspector]
	public Rigidbody2D rigidBody;
	[HideInInspector]
	public Collider2D bodyCollider;
	public Transform movingPart;
	public SpriteRenderer spriteRenderer;

	[Header("Properties")]
	public bool rising = false;
	public bool falling = false;
	public bool grounded {
		get { return (height == 0) ? true : false; }
	}
	public float height {
		get { return (movingPart.localPosition.y - startHeight); }
		set {
			Vector3 pos = movingPart.localPosition;
			pos.y = startHeight + value;
			movingPart.localPosition = pos;
		}
	}
	public int currentLevel = 1;

	private float startHeight;
	public float targetHeight;
	private float fallHeight;
	private bool RBToRemove = false;

	private bool lavelChangeLock = false;

	// Use this for initialization
	void Start () {
		if (movingPart != null) {
			startHeight = movingPart.localPosition.y;
		}
		if (rigidBody == null) {
			rigidBody = GetComponent<Rigidbody2D> ();
		}
		if (bodyCollider == null) {
			bodyCollider = GetComponent<Collider2D> ();
		}
		if (destroyPhysicsAfterStop) {
			StartCoroutine (WaitAndCheck ());
		}
		spriteRenderer = GetComponent<SpriteRenderer> ();
		ShowShadow(height);
	}

	// Update is called once per frame
	void Update () {
		if (rigidBody != null) {
			if (rigidBody.velocity.magnitude > 0f) {
				if (debugs) {
					Debug.Log (" velocity :" + rigidBody.velocity);
				}
				rigidBody.velocity = Vector2.Lerp (rigidBody.velocity, Vector2.zero, rigidBody.mass * GravityMultiplier * Time.deltaTime);
			} else if (RBToRemove) {
				Destroy (rigidBody);
				Destroy (bodyCollider);
			}
		}
		if (movingPart != null) {
			if (rising) {
				Vector3 newPos = movingPart.localPosition;
				newPos.y = startHeight + targetHeight;
				movingPart.localPosition = Vector3.Lerp (movingPart.localPosition, newPos, Acceleration * Time.deltaTime);
				//newPos.y += (Acceleration * ((targetHeight - height)%10)) * Time.deltaTime;
				if ((height >= targetHeight) || (Mathf.Abs(height - targetHeight) <= 0.01f)) {
					newPos.y = targetHeight + startHeight;
					fallHeight = height + 0.01f;
					rising = false;
					falling = true;
				}
				//movingPart.localPosition = newPos;
				ShowShadow(height);
			} else if (height > 0) {
				Vector3 newPos = movingPart.localPosition;
				falling = true;
				targetHeight = 0;
				newPos.y -= Acceleration * (((fallHeight - height) > 1f) ? 1f : (fallHeight - height)) * Time.deltaTime;
				if (newPos.y < startHeight) {
					newPos.y = startHeight;
					falling = false;
				}
				movingPart.localPosition = newPos;
				SetCollitionAirLevel ();
				ShowShadow(height);
			} else {
				SetCollitionGroundLevel ();
				falling = false;
			}
		}
	}

	private void ShowShadow(float height) {
		if (spriteRenderer) {
			int index = (int)(height / changeShadowStep);
			index += startShadowIndex;
			if (index >= shadowsList.Length) {
				index = shadowsList.Length-1;
			}
			if (currentShadowIndex != index) {
				spriteRenderer.sprite = shadowsList [index];
				currentShadowIndex = index;
			}
		} else {
			Debug.LogWarning ("SpriteRenderer component is not attached to gameobject " + this.name);
		}
	}

	/// <summary>
	/// Set object jump force
	/// </summary>
	public void Jump(float force) {
		if (movingPart != null) {
			targetHeight = height + (force * (1 / rigidBody.mass));
			rising = true;
			falling = false;
			Debug.Log (" JUMP force:" + force + " height:" + height + " target:" + targetHeight);
		}
	}

	/// <summary>
	/// Set object level (Physics2d level based collision).
	/// </summary>
	public void SetCollitionLevel(int level) {
		if (!lavelChangeLock) {
			int newLayer = LayerMask.NameToLayer ("Level" + level.ToString ());
			if (gameObject.layer != newLayer) {
				SetLayer (newLayer, gameObject.transform);
			}
		}
	}

	/// <summary>
	/// Set object level (Physics2d level based collision) based on currect 'floor'.
	/// </summary>
	public void SetCollitionGroundLevel(bool force = false) {
		if (force) {
			lavelChangeLock = false;
		}
		if (!lavelChangeLock) {
			SetCollitionLevel (currentLevel);
		}
	}

	/// <summary>
	/// Set this level (Physics2d level based collision) if object does not touch the ground.
	/// </summary>
	public void SetCollitionAirLevel() {
		if (!lavelChangeLock) {
			int newLayer = LayerMask.NameToLayer ("AirLevel1");
			if (gameObject.layer != newLayer) {
				SetLayer (newLayer, gameObject.transform);
			}
		}
	}

	/// <summary>
	/// Set this level to ignore collistions with other characters (Physics2d level based collision).
	/// </summary>
	public void SetCollitionGhostLevel() {
		int newLayer = LayerMask.NameToLayer ("GhostLevel1");
		gameObject.layer = newLayer;
		lavelChangeLock = true;
	}

	/// <summary>
	/// Recursively sets GameObject's layer to newLayer
	/// </summary>
	/// <param name="newLayer">The new layer</param>
	/// <param name="trans">Start transform</param>
	void SetLayer(int newLayer, Transform trans) {
		trans.gameObject.layer = newLayer;
		foreach (Transform child in trans) {
			child.gameObject.layer = newLayer;
			if (child.childCount > 0) {
				SetLayer(newLayer, child.transform);
			}
		}
	}

	private IEnumerator WaitAndCheck() {
		yield return new WaitForSeconds (3f);
		RBToRemove = true;
	}
}
