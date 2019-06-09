using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIHelpScreen : MonoBehaviour {
	public List<Canvas> CardsSlides = new List <Canvas> ();
	public List<Canvas> DeckSlides = new List <Canvas> ();
	public List<Canvas> GameplaySlides = new List <Canvas> ();
	public Button TutorialBtn;

	public Transform CardPagesTable;
	public Transform DeckPagesTable;
	public Transform GameplayPagesTable;
	public GameObject PageIndicatorPrefab;
	public Color ActivePageIndicatorColor;
	public Color InactivePageIndicatorColor;

	public int SlideCurrent = 0;

	public int currentSlidesList = 0;
	public int prevSlide = 0;
	public int prevList = 0;
	public Canvas currCanvas = null;

	// Use this for initialization
	void Start () {
		SlideCurrent = 0;
		prevSlide = 0;
		currentSlidesList = 0;
		FillPageIndicators (CardPagesTable, CardsSlides.Count);
		FillPageIndicators (DeckPagesTable, DeckSlides.Count);
		FillPageIndicators (GameplayPagesTable, GameplaySlides.Count);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void FillPageIndicators(Transform table, int pages) {
		int i = 0;
		GameObject gob;

		for (i = 0; i < pages; i++) {
			gob = Instantiate (PageIndicatorPrefab, table);
			gob.GetComponent<Image> ().color = InactivePageIndicatorColor;
		}
	}

	private void ActivatePageIndicator(Transform table, int page) {
		if (page >= 0 && page < table.childCount) {
			Transform indicator = table.GetChild (page);
			if (indicator != null) {
				indicator.GetComponent<Image> ().color = ActivePageIndicatorColor;
			}
		}
	}

	private void DezactivatePageIndicator(Transform table, int page) {
		if (page >= 0 && page < table.childCount) {
			Transform indicator = table.GetChild (page);
			if (indicator != null) {
				indicator.GetComponent<Image> ().color = InactivePageIndicatorColor;
			}
		}
	}

	public void StartTutorial() {
		if (SceneManager.GetActiveScene ().name.Equals ("Main_Menu")) {
			DisableHelpMenu ();
			GameObject.Find ("NetworkManager").GetComponent<MyNetworkManager> ().SetGameModeTutorial ();
			GameObject.Find ("NetworkManager").GetComponent<MyNetworkManager> ().StartGame ();
		}
	}

	public void EnableHelpMenu() {
		GetComponent<Canvas> ().enabled = true;
		if (SceneManager.GetActiveScene ().name.Equals ("Main_Menu")) {
			TutorialBtn.interactable = true;
		} else {
			TutorialBtn.interactable = false;
		}
	}

	public void DisableHelpMenu() {
		DisableCurrentSlide ();
		GetComponent<Canvas> ().enabled = false;
	}

	public void GotoCardSlides() {
		currentSlidesList = 2;
		SlideCurrent = 0;
		RefreshSlides ();
	}

	public void GotoDeckSlides() {
		currentSlidesList = 1;
		SlideCurrent = 0;
		RefreshSlides ();
	}

	public void GotoGameplaySlides() {
		currentSlidesList = 0;
		SlideCurrent = 0;
		RefreshSlides ();
	}

	public void GotoNextSlide() {
		prevSlide = SlideCurrent;
		prevList = currentSlidesList;
		SlideCurrent++;
		RefreshSlides ();
	}

	public void GotoPrevSlide() {
		prevSlide = SlideCurrent;
		prevList = currentSlidesList;
		if (SlideCurrent > 0) {
			SlideCurrent--;
			RefreshSlides ();
		} else if (currentSlidesList > 0) {
			currentSlidesList--;
			if (currentSlidesList == 0) {
				SlideCurrent = GameplaySlides.Count - 1;
			} else if (currentSlidesList == 1) {
				SlideCurrent = DeckSlides.Count - 1;
			} else if (currentSlidesList == 2) {
				SlideCurrent = CardsSlides.Count - 1;
			} else {
				SlideCurrent = 0;
			}
			RefreshSlides ();
		}
	}

	public void DisableCurrentSlide() {
		if (currCanvas != null) {
			if (currCanvas.GetComponent<UICardSpawner> () != null) {
				currCanvas.GetComponent<UICardSpawner> ().DestroySpawns ();
			}
			currCanvas.enabled = false;
			currCanvas = null;
		}
	}

	public void RefreshSlides() {
		if (prevList == 0) {
			DezactivatePageIndicator (GameplayPagesTable, prevSlide);
		} else if (prevList == 1) {
			DezactivatePageIndicator (DeckPagesTable, prevSlide);
		} else if (prevList == 2) {
			DezactivatePageIndicator (CardPagesTable, prevSlide);
		}
		if (currentSlidesList == 0) {
			if (SlideCurrent < GameplaySlides.Count) {
				DisableCurrentSlide ();
				currCanvas = GameplaySlides [SlideCurrent];
				ActivatePageIndicator (GameplayPagesTable, SlideCurrent);
			} else {
				SlideCurrent = 0;
				currentSlidesList = 1;
				RefreshSlides ();
			}
		} else if (currentSlidesList == 1) {
			if (SlideCurrent < DeckSlides.Count) {
				DisableCurrentSlide ();
				currCanvas = DeckSlides [SlideCurrent];
				ActivatePageIndicator (DeckPagesTable, SlideCurrent);
			} else {
				SlideCurrent = 0;
				currentSlidesList = 2;
				RefreshSlides ();
			}
		} else {
			if (SlideCurrent < CardsSlides.Count) {
				DisableCurrentSlide ();
				currCanvas = CardsSlides [SlideCurrent];
				ActivatePageIndicator (CardPagesTable, SlideCurrent);
			} else {
				SlideCurrent = 0;
				currentSlidesList = 0;
				RefreshSlides ();
			}
		}
		if (currCanvas != null) {
			currCanvas.enabled = true;
		}
	}
}
