using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DeckPanel : MonoBehaviour {
	public int index;
	public bool PanelActive = false;
	public Button ThisButton;
	public Text DeckName;
	public Text AvailableText;
	public GameObject Deck;
	public Button DelBTN;
	public Button ViewBTN;
	public Button EditBTN;
	public Image Frame;
	public Color SelectColor;
	public Color NormalColor;
	public GameObject Hero;

	public bool DeckAvailable = true;

	public DeckTableController Controller;

	void Start () {
		Controller = GameObject.Find ("DeckSelect-Canvas").GetComponent<DeckTableController> ();
		if (DelBTN != null) {
			DelBTN.GetComponent<Button> ().onClick.AddListener (DelDeck);
		}
		if (ViewBTN != null) {
			ViewBTN.GetComponent<Button> ().onClick.AddListener (ViewDeck);
		}
		if (EditBTN != null) {
			EditBTN.GetComponent<Button> ().onClick.AddListener (EditDeck);
		}
		if (ThisButton != null) {
			ThisButton.GetComponent<Button> ().onClick.AddListener (PressDeck);
		}
	}

	void LateUpdate () {
		//if (Deck != null) {
		//	Deck.gameObject.transform.position = this.transform.position;
		//}
		//Deck.gameObject.transform.SetParent(this.transform);
	}

	public int GetDeckIndex() {
		return index;
	}

	public void UncompleteDeck() {
		DeckAvailable = false;
		AvailableText.enabled = true;
		AvailableText.text = "Niekompletny";
	}

	private void PressDeck() {
		if (PanelActive) {
			if (DeckAvailable) {
				if (Controller == null)
					Controller = GameObject.Find ("DeckSelect-Canvas").GetComponent<DeckTableController> ();
				SelectDeck ();
			}
			if (DeckAvailable) {
				Controller.ChooseDeck (index);
			}
		}
	}

	public void SelectDeck() {
		Debug.Log ("Deck Selected");
		if (PanelActive) {
			if (DeckAvailable) {
				if (Controller == null)
					Controller = GameObject.Find ("DeckSelect-Canvas").GetComponent<DeckTableController> ();
				if (Controller.MainMenuComponent.DeckChooseMode) {
					Frame.color = SelectColor;
					//Deck.GetComponent<SmothTransform> ().SmoothScaleTo (new Vector3 (1.1f, 1.1f, 1.1f), 5);
					GetComponent<Animator>().SetBool("Select", true);
					ThisButton.GetComponent<SmothTransform> ().SmoothScaleTo (new Vector3 (1.1f, 1.1f, 1.1f), 5);
				}
			}
		}
	}

	public void DeSelectDeck() {
		if (PanelActive) {
			Frame.color = NormalColor;
			GetComponent<Animator>().SetBool("Select", false);
			ThisButton.GetComponent<SmothTransform> ().SmoothScaleTo (new Vector3 (1f, 1f, 1f), 5);
			//GetComponent<SmothTransform> ().SmoothScaleTo (new Vector3 (1f, 1f, 1f), 5);
		}
	}

	private void DelDeck() {
		if (Controller != null) {
			Controller.DeleteDeck (index);
		}
	}

	private void ViewDeck() {
		if (Controller != null) {
			Controller.LoadDeck (index);
		}
	}

	private void EditDeck() {
		if (Controller != null) {
			Controller.LoadDeckToEdit (index, null);
		}
	}
}