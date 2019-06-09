using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class SpriteClick : MonoBehaviour {
	public UnityEvent OnClickedDown;
	public UnityEvent OnClickedUp;
	private bool clicked = false;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnMouseDown()
	{
		clicked = true;
		if (clicked && OnClickedDown != null) {
			OnClickedDown.Invoke();
		}
	}

	public void OnMouseUp()
	{
		Debug.Log ("u " + clicked);
		if (clicked && OnClickedUp != null) {
			OnClickedUp.Invoke();
		}
		clicked = false;
	}
}
