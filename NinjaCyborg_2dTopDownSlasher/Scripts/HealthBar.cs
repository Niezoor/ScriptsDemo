using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour {
	public Image HealthImage;
	public int MaxHealth;

	public float maximizeScale;
	public float maximizeTime;
	public float maximizeSpeed;
	private float maximizeTimeCurrent;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (maximizeTimeCurrent <= 0f) {
			if (transform.localScale.x != 1f) {
				Vector3 targetScale = new Vector3 (1f, 1f, 1f);
				transform.localScale = Vector3.Lerp (transform.localScale, targetScale, maximizeSpeed);
			}
		} else {
			if (transform.localScale.x != maximizeScale) {
				Vector3 targetScale = new Vector3 (maximizeScale, maximizeScale, maximizeScale);
				transform.localScale = Vector3.Lerp (transform.localScale, targetScale, maximizeSpeed);
			}
			maximizeTimeCurrent -= Time.deltaTime;
		}
	}

	public void RefreshHealth(int health) {
		if (HealthImage != null) {
			HealthImage.fillAmount = (float)health / MaxHealth;
		}
		maximizeTimeCurrent = maximizeTime;
	}
}
