using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class HeroPanel : MonoBehaviour {
	public HeroesBase HeroesBaseComp;
	public GameObject HeroPawnPrefab;
	public Canvas HeroDescCanvas;

	public List<Button> OthersHeroesPanels = new List<Button>();
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void OnChooseClickHandler() {
		if (HeroesBaseComp.SelectedHero != null) {
			Destroy (HeroesBaseComp.SelectedHero);
		}
		HeroesBaseComp.SelectedHero = Instantiate(HeroPawnPrefab);
		HeroesBaseComp.SelectedHeroName = HeroPawnPrefab.GetComponent<Hero>().Name;
		OnBackClickHandler ();
	}

	public void OnClickHandler() {
		HeroDescCanvas.enabled = true;
		foreach (Button btn in OthersHeroesPanels) {
			btn.interactable = false;
		}
	}

	public void OnBackClickHandler() {
		HeroDescCanvas.enabled = false;
		foreach (Button btn in OthersHeroesPanels) {
			btn.interactable = true;
		}
	}
}
