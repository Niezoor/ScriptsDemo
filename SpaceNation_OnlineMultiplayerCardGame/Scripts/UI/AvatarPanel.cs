using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarPanel : MonoBehaviour {
	public Image AvatarImage;
	public Text AvatarDesc;
	public Button AvatarButton;
	public Image ButtonImage;
	public AvatarMenu AvatarMenuComponent;

	// Use this for initialization
	void Start () {
		AvatarButton.onClick.AddListener (SelectThis);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void SelectThis() {
		if (AvatarMenuComponent != null) {
			AvatarMenuComponent.SelectPanel (this.GetComponent<AvatarPanel> ());
		}
	}
}
