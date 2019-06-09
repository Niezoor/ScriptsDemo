using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageIndicator : MonoBehaviour {
	public TextMeshPro textMesh;
	public float critSize;
	public Color critColor;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ShowDamage(int damage, bool crit) {
		if (textMesh != null) {
			textMesh.text = "-" + damage.ToString ();
			if (crit) {
				textMesh.color = critColor;
				textMesh.fontSize = critSize;
			}
		} else {
			Debug.LogWarning ("Cannot find TextMeshPro component in child(0)");
		}
	}
}
