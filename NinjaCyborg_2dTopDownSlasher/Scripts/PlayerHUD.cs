using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour {
	private SNCharacter Player;
	public Transform HealthBar;
	public GameObject HealthBarFullPrefab;
	public Color FullColor;
	public Color EmptyColor;
	public Image EnergyBar;

	private Color playerColor;
	private List<GameObject> healthBars = new List<GameObject> ();
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		UpdateEnergyBar ();
	}

	public void InitHUD(SNCharacter player, Color color) {
		//this.gameObject.SetActive(true);
		if (player) {
			playerColor = color;
			Player = player;
			EnergyBar.color = color;
			for (int i = 0; i < healthBars.Count; i++) {
				Destroy (healthBars[i]);
			}
			healthBars.Clear ();
			for (int i = 0; i < player.health; i++) {
				GameObject gob = Instantiate (HealthBarFullPrefab, HealthBar);
				gob.GetComponent<Image> ().color = FullColor * playerColor;
				healthBars.Add (gob);
			}
		}
	}

	public void UpdateHealthBar() {
		if (Player != null) {
			for (int i = 0; i < healthBars.Count; i++) {
				if (i >= Player.health) {
					Image image = healthBars [i].GetComponent<Image> ();
					if (!image.color.Equals(EmptyColor * playerColor)) {
						image.color = EmptyColor * playerColor;
					}
				}
			}
		}
	}

	private void UpdateEnergyBar() {
		if (Player != null && Player.movementController != null) {
			//nergyBar.fillAmount = (1f / Player.playerController.DashEnergyMax) * Player.playerController.DashEnergy;
		}
	}
}
