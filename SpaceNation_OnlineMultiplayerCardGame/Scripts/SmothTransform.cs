using UnityEngine;
using System.Collections;

public class SmothTransform : MonoBehaviour {

	public Vector3 destPos       = new Vector3(0,0,0);
	public Vector3 destGlobalPos = new Vector3(0,0,0);
	public Vector3 destScale     = new Vector3(0,0,0);
	public Quaternion destRot       = new Quaternion(0,0,0,0);
	public Quaternion destGlobalRot = new Quaternion(0,0,0,0);

	public float smoothTransformSpeed;
	public float smoothTransformRotSpeed;
	public float smoothScaleSpeed;

	public bool smoothTransformPosRunning = false;
	public bool smoothTransformRotRunning = false;
	public bool smoothTransformGlobalPosRunning = false;
	public bool smoothTransformGlobalRotRunning = false;
	public bool smoothTransformScaleRunning = false;
	public bool curvedTransformPosRunning = false;
	public bool curvedTransformRosRunning = false;
	public bool SmoothTransformDisabled = false;

	public bool TrackPosition = false;

	public AnimationCurve AnimCurve;
	public float animTime;

    // Use this for initialization
    void Start () {
		/*smothTransformPosRunning = false;
		smothTransformRotRunning = false;
		smothTransformScaleRunning = false;*/
	}

	// Update is called once per frame
	void Update () {
		/* curve animation time */
		if (curvedTransformPosRunning || curvedTransformRosRunning) {
			animTime += Time.deltaTime;
		}
		/* local position */
		if (smoothTransformPosRunning) {
			if (curvedTransformPosRunning) {
				float s = animTime / smoothTransformSpeed;
				transform.localPosition = Vector3.Lerp (transform.localPosition, destPos, AnimCurve.Evaluate(s));
			} else {
				transform.localPosition = Vector3.Lerp (transform.localPosition, destPos, smoothTransformSpeed * Time.deltaTime);
			}
		}
		/* local scale */
		if (smoothTransformScaleRunning) {
			transform.localScale = Vector3.Lerp (transform.localScale, destScale, smoothScaleSpeed * Time.deltaTime);
		}
		/* local rotation */
		if (smoothTransformRotRunning) {
			if (curvedTransformRosRunning) {
				float s = animTime / smoothTransformRotSpeed;
				transform.localRotation = Quaternion.Slerp (transform.localRotation, destRot, AnimCurve.Evaluate(s));
			} else {
				transform.localRotation = Quaternion.Slerp (transform.localRotation, destRot, smoothTransformRotSpeed * Time.deltaTime);
			}
		}
		/* global transform */
		if (smoothTransformGlobalPosRunning) {
			transform.position = Vector3.Lerp (transform.position, destGlobalPos, smoothTransformSpeed * Time.deltaTime);
		}
		if (smoothTransformGlobalRotRunning) {
			transform.localRotation = Quaternion.Lerp (transform.localRotation, destGlobalRot, smoothTransformSpeed * Time.deltaTime);
		}

		if (!TrackPosition && transform.localPosition == destPos) {
			smoothTransformPosRunning = false;
			curvedTransformPosRunning = false;
			curvedTransformRosRunning = false;
			transform.localPosition = destPos;
        }
		if (transform.localScale == destScale) {
			smoothTransformScaleRunning = false;
			transform.localScale = destScale;
		}
		if (smoothTransformRotRunning) {
			if (transform.localRotation == destRot) {
				//Debug.Log ("Local Rot reached" + transform.localRotation + " = " + destRot);
				smoothTransformRotRunning = false;
				transform.localRotation = destRot;
			}
		}
		if (smoothTransformGlobalPosRunning) {
			if (transform.position == destGlobalPos) {
				smoothTransformGlobalPosRunning = false;
				transform.position = destGlobalPos;
			}
		}
		if (smoothTransformGlobalRotRunning) {
			if (transform.rotation == destGlobalRot) {
				//Debug.Log ("Global Rot reached" + transform.rotation + " = " + destGlobalRot);
				smoothTransformGlobalRotRunning = false;
				transform.rotation = destGlobalRot;
			}
		}
	}

    public void SmothTransformTo(Vector3 destinationPosition, Quaternion destinationRotation, float speed)
    {
		if (!SmoothTransformDisabled) {
			animTime = 0.0f;
			destPos = destinationPosition;
			destRot = destinationRotation;
			smoothTransformSpeed = speed;
			smoothTransformRotSpeed = speed;
			smoothTransformPosRunning = true;
			smoothTransformRotRunning = true;
			smoothTransformGlobalPosRunning = false;
		}
    }

	public void SmothTransformTo(Vector3 destinationPosition, Quaternion destinationRotation, AnimationCurve curve, float duration)
	{
		if (!SmoothTransformDisabled) {
			animTime = 0.0f;
			destPos = destinationPosition;
			destRot = destinationRotation;
			AnimCurve = curve;
			smoothTransformSpeed = duration;
			smoothTransformRotSpeed = duration;
			curvedTransformPosRunning = true;
			curvedTransformRosRunning = true;
			smoothTransformPosRunning = true;
			smoothTransformRotRunning = true;
			smoothTransformGlobalPosRunning = false;
		}
	}

	public void SmothTransformTo(Vector3 destinationPosition, AnimationCurve curve, float duration)
	{
		if (!SmoothTransformDisabled) {
			animTime = 0.0f;
			destPos = destinationPosition;
			AnimCurve = curve;
			smoothTransformSpeed = duration;
			smoothTransformRotSpeed = duration;
			curvedTransformPosRunning = true;
			smoothTransformPosRunning = true;
			smoothTransformGlobalPosRunning = false;
		}
	}

	public void SmothTransformTo(Vector3 destinationPosition, float speed)
	{
		if (!SmoothTransformDisabled) {
			destPos = destinationPosition;
			smoothTransformSpeed = speed;
			smoothTransformPosRunning = true;
			smoothTransformGlobalPosRunning = false;
		}
	}

	public void SmothTransformTo(Quaternion destinationRotation, float speed)
	{
		if (!SmoothTransformDisabled) {
			destRot = destinationRotation;
			//Debug.Log ("start transform rotation with speed = " + smothTransformSpeed);
			smoothTransformRotSpeed = speed;
			smoothTransformRotRunning = true;
		}
	}

	public void SmoothGlobalTransformTo(Vector3 destinationGlobalPosition, float speed) {
		if (!SmoothTransformDisabled) {
			destGlobalPos = destinationGlobalPosition;
			smoothTransformSpeed = speed;
			smoothTransformGlobalPosRunning = true;
			smoothTransformPosRunning = false;
		}
	}

	public void SmoothScaleTo(Vector3 destinationScale, float speed) {
		//if (destScale != transform.localScale) {
		if (!SmoothTransformDisabled) {
			destScale = destinationScale;
			smoothScaleSpeed = speed;
			smoothTransformScaleRunning = true;
		}
		//}
	}

	public void SmoothGlobalRotation(Quaternion destinationRotation, float speed)
	{
		if (!SmoothTransformDisabled) {
			destGlobalRot = destinationRotation;
			//Debug.Log ("start transform rotation with speed = " + smothTransformSpeed);
			smoothTransformRotSpeed = speed;
			smoothTransformGlobalRotRunning = true;
		}
	}

	public void StopAll() {
		smoothTransformPosRunning = false;
		smoothTransformRotRunning = false;
		smoothTransformGlobalPosRunning = false;
		smoothTransformGlobalRotRunning = false;
		smoothTransformScaleRunning = false;
		curvedTransformPosRunning = false;
		curvedTransformRosRunning = false;
		SmoothTransformDisabled = false;
	}
}
