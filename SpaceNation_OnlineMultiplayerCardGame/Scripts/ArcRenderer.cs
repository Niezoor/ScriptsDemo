using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ArcRenderer : MonoBehaviour {
	LineRenderer lineRenderer;

	public Transform StartPoint;
	public Transform EndPoint;

	public float Hight;

	public int Resolution = 5;

	float g;
	float radianAngle;

	[Header("Material Animation")]
	public float ScrollX;
	public float ScrollY;
	private Renderer rend;

	void Awake() {
		lineRenderer = GetComponent<LineRenderer> ();
		rend = GetComponent<Renderer> ();
		lineRenderer.sortingOrder = 40;
	}

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if (ScrollX != 0 || ScrollY != 0) {
			float offX = Time.time * ScrollX;
			float offY = Time.time * ScrollY;
			if (rend) {
				Vector2 newOff = rend.material.mainTextureOffset;
				newOff.x = offX;
				newOff.y = offY;
				rend.material.mainTextureOffset = newOff;
			}
		}
		if (EndPoint != null) {
			List <Vector3> points = new List<Vector3> ();
			Vector3 mid = Vector3.Lerp (StartPoint.position, EndPoint.position, 0.5f);
			mid.y = mid.y + Hight;
			for (float ratio = 0; ratio <= 1; ratio += 1.0f / Resolution) {
				var targetLineVertex1 = Vector3.Lerp (StartPoint.position, mid, ratio);
				var targetLineVertex2 = Vector3.Lerp (mid, EndPoint.position, ratio);
				var bazierPoint = Vector3.Lerp (targetLineVertex1, targetLineVertex2, ratio);
				points.Add (bazierPoint);
			}
			lineRenderer.positionCount = points.Count;
			lineRenderer.SetPositions (points.ToArray ());
		} else {
			Destroy (this.gameObject);
		}
	}

}
