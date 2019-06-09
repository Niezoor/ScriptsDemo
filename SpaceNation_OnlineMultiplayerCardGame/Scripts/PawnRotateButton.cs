using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnRotateButton : MonoBehaviour {
	public bool ButtonPressed = false;
	private SpriteRenderer SpriteComponent { get { return GetComponent<SpriteRenderer> (); } }
	private CircleCollider2D ColliderComponent { get { return GetComponent<CircleCollider2D> (); } }
	private KeepParentRenderLayer KeepComponent { get { return GetComponent<KeepParentRenderLayer> (); } }

	void Start () {
		
	}

	void OnMouseDown() {
		ButtonPressed = true;
	}

	void OnMouseUp() {
		ButtonPressed = false;
	}

	public void EnableButton() {
		SpriteComponent.enabled = true;
		ColliderComponent.enabled = true;
		KeepComponent.enabled = true;
	}

	public void DisableButton() {
		SpriteComponent.enabled = false;
		ColliderComponent.enabled = false;
		KeepComponent.enabled = false;
	}
}
