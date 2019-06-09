using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

[RequireComponent(typeof(SNMovementController))]
[RequireComponent(typeof(SNCharacter))]
public class SNUserController : MonoBehaviour {
	public bool controllable = false;
	public Player Input;

	private SNMovementController movementController;
	private SNCharacter character;
	private Vector2 inputMove;
	private Vector2 inputAim;

	// Use this for initialization
	void Start () {
		Input = ReInput.players.GetPlayer (0);
		movementController = GetComponent<SNMovementController> ();
		character = GetComponent<SNCharacter> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (controllable && Input != null) {
			inputMove.x = Input.GetAxis ("MoveHorizontal");
			inputMove.y = Input.GetAxis ("MoveVertical");
			inputAim.x = Input.GetAxis ("AimHorizontal");
			inputAim.y = Input.GetAxis ("AimVertical");

			movementController.MoveUpdate (inputMove, inputAim);

			if (Input.GetButtonDown ("Attack")) {
				if (character != null) {
					character.Attack ();
				}
			}
			if (Input.GetButtonDown ("Jump")) {
				if (movementController != null) {
					movementController.Jump ();
				}
			}
			if (Input.GetButtonDown ("Shot")) {
				if (character != null) {
					character.StartShoting ();
				}
			}
			if (Input.GetButtonUp ("Shot")) {
				if (character != null) {
					character.StopShoting ();
				}
			}
			if (Input.GetButtonDown ("Defence")) {
				if (movementController != null) {
					movementController.StartBlocking ();
				}
			}
			if (Input.GetButtonUp ("Defence")) {
				if (movementController != null) {
					movementController.StopBlocking ();
				}
			}
			if (Input.GetButtonDown ("Dash")) {
				if (movementController != null) {
					movementController.Dash ();
				}
			}
		}
	}


}
