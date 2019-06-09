using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Hero : MonoBehaviour {
	public string Name;
	[TextArea]
	public string Description;
	public Color HeroColor;
	public CardsBase.SelectedHeroCards CardsPool;
	public HeroesBase HeroesBaseComp;



	// Use this for initialization
	void Start () {
		if (GameObject.Find ("HeroesBase")) {
			HeroesBaseComp = GameObject.Find ("HeroesBase").GetComponent<HeroesBase> ();
		}
		//GetComponent<Pawn> ().Name = Name;
	}
	
	// Update is called once per frame
	void Update () {

	}

	void OnMouseDown() {

	}

	void OnMouseUp() {

	}

	void OnMouseOver() {

	}

	void OnMouseExit() {

	}
}
