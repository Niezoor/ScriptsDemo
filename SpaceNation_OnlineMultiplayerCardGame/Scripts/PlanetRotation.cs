using UnityEngine;
using System.Collections;

public class PlanetRotation : MonoBehaviour {
	public float Speed = 5;

	void Start() {
		StartCoroutine (RotationTask ());
	}
	// Update is called once per frame
	private IEnumerator RotationTask () {
		while (true) {
			transform.Rotate (0, 0, Time.deltaTime * Speed);
			yield return new WaitForSeconds (0.016f);//60fps
		}
	}
}
