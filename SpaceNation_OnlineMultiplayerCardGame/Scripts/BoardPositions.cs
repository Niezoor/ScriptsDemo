using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SmothTransform))]
public class BoardPositions : MonoBehaviour {
	public Vector3 normalPositionSmallScreen;
	public Vector3 startPositionSmallScreen;
	public Vector3 normalPositionBigScreen;

	public AnimationCurve AnimCurve;

	private SmothTransform SmothTransComp { get { return GetComponent<SmothTransform> (); } }
	// Use this for initialization
	void Start () {
		transform.localPosition = startPositionSmallScreen;
		SmothTransComp.SmothTransformTo (normalPositionSmallScreen, AnimCurve, 30f);
		// (normalPositionSmallScreen, 2);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
