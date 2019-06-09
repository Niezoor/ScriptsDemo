using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {
	public List<Transform> Targets;
	public float speed = 0.125f;
	public Vector3 offset;

	public float CamMinSize;
	public float CamMaxSize;
	public float CamSizeOffset;

	private Vector3 center;
	private Vector2 toFit;

	private float shakeDuration = 0f;
	private float shakeAmount;

	// Use this for initialization
	void Start () {
		/*GameObject[] targets = GameObject.FindGameObjectsWithTag ("Player");
		foreach (GameObject targ in targets) {
			Targets.Add(targ.transform.GetChild(0).GetComponent<Character>());
		}
		targets = GameObject.FindGameObjectsWithTag ("Enemy");
		foreach (GameObject targ in targets) {
			Targets.Add(targ.transform.GetChild(0).GetComponent<Character>());
		}*/
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void FixedUpdate() {
		if (Targets.Count > 0) {
			float distance = 0f;
			int alive = 0;
			center = Vector2.zero;

			float minX = 0, minY = 0, maxX = 0, maxY = 0;
			for (int i = 0; i < Targets.Count; i++) {
				if (!Targets [i].tag.Equals ("Dead")) {
					alive++;

					if (alive == 1) {
						minX = Targets [i].transform.position.x;
						maxX = Targets [i].transform.position.x;
						minY = Targets [i].transform.position.y;
						maxY = Targets [i].transform.position.y;
					} else {
						if (minX > Targets [i].transform.position.x) {
							minX = Targets [i].transform.position.x;
						} else if (maxX < Targets [i].transform.position.x) {
							maxX = Targets [i].transform.position.x;
						}
						if (minY > Targets [i].transform.position.y) {
							minY = Targets [i].transform.position.y;
						} else if (maxY < Targets [i].transform.position.y) {
							maxY = Targets [i].transform.position.y;
						}
					}
				} else {
					Targets.Remove (Targets [i]);
				}
			}
			center.x = (minX + maxX) / 2f;
			center.y = (minY + maxY) / 2f;

			distance = ((maxX - minX) > (maxY - minY)) ? (maxX - minX) : (maxY - minY);

			Vector3 desire = center + offset;
			Vector3 smooth = Vector3.Lerp (transform.position, desire, speed);
			if (shakeDuration > 0) {
				shakeDuration -= Time.fixedDeltaTime;
				smooth += Random.insideUnitSphere * shakeAmount;
			}
			float newSize = (float)(distance + CamSizeOffset) * (float)Screen.height / (float)Screen.width * 0.5f;
			if (newSize > CamMaxSize) {
				newSize = CamMaxSize;
			} else if (newSize < CamMinSize) {
				newSize = CamMinSize;
			}
			if (Camera.main.orthographicSize != newSize) {
				Camera.main.orthographicSize = newSize;
			}
			transform.position = smooth;
		}
	}

	public void Shake(float duration, float amount) {
		shakeDuration = duration;
		shakeAmount = amount;
	}
}
