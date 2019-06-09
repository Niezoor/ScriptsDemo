using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class HeroesBase : MonoBehaviour {
	public GameObject SelectedHero;
	public string SelectedHeroName;

	public GameObject PawnPrefab;

	[System.Serializable]
	public class HeroDescriptionClass {
		public string Name;
		public int CardID;
		public Sprite Character;
		[TextArea]
		public string Description;
		public CardsBase.PawnConfigSet[] AttackConfig = new CardsBase.PawnConfigSet[Pawn.PawnConfigNumber];
		public Color HeroColor;
		public CardsBase.SelectedHeroCards CardsPool;
	}

	public List<HeroDescriptionClass> HeroesDescList = new List<HeroDescriptionClass>();

	public List<GameObject> HeroesBaseList = new List<GameObject>();
	private static HeroesBase Instance;

	// Use this for initialization
	void Start () {
	} 
	
	// Update is called once per frame
	void Update () {
	
	}

	private HeroDescriptionClass FindHeroDescByName(string name) {
		foreach (HeroDescriptionClass desc in HeroesDescList) {
			if (desc.Name.Equals (name)) {
				return desc;
			}
		}
		return null;
	}

	public void SetupHero(GameObject HeroObject, HeroDescriptionClass desc) {
		if (desc != null) {
			Pawn PawnComp = HeroObject.GetComponent<Pawn> ();
			Hero HeroComp = HeroObject.GetComponent<Hero> ();

			HeroComp.Name = desc.Name;
			HeroComp.HeroColor = desc.HeroColor;
			HeroComp.CardsPool = desc.CardsPool;
			HeroComp.Description = desc.Description;

			PawnComp.Name = desc.Name;
			PawnComp.Desc = desc.Description;
			PawnComp.SetConfig (desc.AttackConfig, false);
			PawnComp.Character.GetComponent<SpriteRenderer> ().sprite = desc.Character;
			PawnComp.CardID = desc.CardID;
		}
	}

	public void SetupHero(GameObject HeroObject, string name) {
		HeroDescriptionClass desc = FindHeroDescByName (name);
		if (desc != null) {
			SetupHero (HeroObject, desc);
		}
	}

	public GameObject SpawnNewHero() {
		GameObject newHero = Instantiate(PawnPrefab);
		/*Hero HeroComp = */newHero.AddComponent<Hero> ();
		Pawn PawnComp = newHero.GetComponent<Pawn> ();
		PawnComp.Health = 20;
		PawnComp.Attack = 1;
		return newHero;
	}

	/*
	public GameObject GetHeroByName(string name) {
		foreach (GameObject hero in HeroesBaseList) {
			if (string.Equals(hero.GetComponent<Hero> ().Name,  name)) {
				Debug.Log ("Found hero named " + name);
				return Instantiate(hero);
			}
		}
		Debug.Log ("Cant find hero named " + name);
		return null;
	}	
	*/
	public GameObject GetHeroByName(string name) {
		HeroDescriptionClass desc = FindHeroDescByName (name);
		if (desc != null) {
			Debug.Log ("Found hero named " + name);
			GameObject newHero = SpawnNewHero ();
			SetupHero (newHero, desc);

			return newHero;
		}
		Debug.Log ("Cant find hero named " + name);
		return null;
	}
}
