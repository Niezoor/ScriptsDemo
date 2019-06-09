using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;

public class AvatarMenu : MonoBehaviour {
	public GameObject PanelPrefab;
	public Transform Table;
	public Button ConfirmBtn;

	public AvatarPanel Selected;
	public Sprite Normal;
	public Sprite OnSelect;
	private bool req_sended = false;

	// Use this for initialization
	void Start () {
		ConfirmBtn.onClick.AddListener (Confirm);
	}

	// Update is called once per frame
	void Update () {
		
	}

	public void SetupTable(List<LocalPlayer.GameAvatar> AvatarsList) {
		foreach (LocalPlayer.GameAvatar AV in AvatarsList) {
			GameObject Gob = Instantiate (PanelPrefab, Table);
			AvatarPanel panel = Gob.GetComponent<AvatarPanel> ();
			panel.AvatarImage.sprite = AV.image;
			panel.AvatarDesc.text = AV.desc;
			panel.AvatarMenuComponent = this.GetComponent<AvatarMenu> ();
		}
	}

	public void CloseTable() {
		if (GameObject.Find ("MainMenu-Canvas") != null) {
			GameObject.Find ("MainMenu-Canvas").GetComponent<MainMenu> ().HideWaitIndicator();
		}
		if (!req_sended) {
			Destroy (this.gameObject);
		}
	}

	private void Confirm() {
		if (Selected != null && !req_sended) {
			req_sended = true;
			PlayFabClientAPI.UpdateAvatarUrl (new UpdateAvatarUrlRequest(){
				ImageUrl = Selected.AvatarImage.sprite.name
			}, OnUpdateAvatarURL, error => Debug.LogError(error.GenerateErrorReport()));
			LocalPlayer.Instace.SetAvatar (Selected.AvatarImage.sprite.name);
			if (GameObject.Find ("MainMenu-Canvas") != null) {
				GameObject.Find ("MainMenu-Canvas").GetComponent<MainMenu> ().ShowWaitIndicator();
			}
		}
	}

	private void OnUpdateAvatarURL(EmptyResult result) {
		if (GameObject.Find ("MainMenu-Canvas") != null) {
			GameObject.Find ("MainMenu-Canvas").GetComponent<MainMenu> ().HideWaitIndicator();
		}
		Destroy (this.gameObject);
	}

	public void SelectPanel(AvatarPanel panel) {
		if (Selected) {
			Selected.ButtonImage.sprite = Normal;
		}
		panel.ButtonImage.sprite = OnSelect;
		Selected = panel;
	}
}
