using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginLogoAnimation : MonoBehaviour {
	public Vector3 startPos;
	public Vector3 endPos;
	public Vector3 startScale;
	public Vector3 endScale;
	private SmothTransform ST { get { return GetComponent<SmothTransform> (); } }
	// Use this for initialization
	void Start () {
		Debug.Log ("logo pos:" + transform.localPosition + " logo scale:" + transform.localScale);
		//ST = GetComponent<SmothTransform> ();
	}

	public void AnimToSmall() {
		ST.SmothTransformTo (startPos, 3);
		ST.SmoothScaleTo (startScale, 3);
	}

	public void AnimToBig() {
		ST.SmothTransformTo (endPos, 3);
		ST.SmoothScaleTo (endScale, 3);
	}

	public void JumpToSmall(bool small) {
		if (small) {
			transform.localPosition = startPos;
			transform.localScale = startScale;
		} else {
			transform.localPosition = endPos;
			transform.localScale = endScale;
		}
	}
}
