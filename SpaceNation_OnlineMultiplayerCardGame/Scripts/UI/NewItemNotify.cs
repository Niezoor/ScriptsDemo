using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewItemNotify : MonoBehaviour {
	public GameObject PanelPrefab;
	public Transform Table;
	public Animator AnimController;
	private bool BGshowed = false;

	// Use this for initialization
	void Start () {
		//ShowNotify (null, null, null, 0);
		//ShowNotify (null, null, null, 2);
	}
	
	// Update is called once per frame
	void Update () {
		if (Table.childCount > 0 && !BGshowed) {
			AnimController.SetTrigger ("Show");
			BGshowed = true;
		} else if (Table.childCount <= 0 && BGshowed) {
			AnimController.SetTrigger ("Hide");
			BGshowed = false;
		}
	}

	public void ShowNotify(Sprite image, string title, string desc, float delay = 0f) {
		GameObject obj = Instantiate (PanelPrefab, Table);
		obj.GetComponent<NewItemNotifyPanel> ().ShowNotify (image, title, desc, delay);
	}
}
