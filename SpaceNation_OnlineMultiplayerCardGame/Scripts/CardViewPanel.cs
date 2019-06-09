using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardViewPanel : MonoBehaviour {
	public Transform CardPos1;
	public Transform CardPos2;
	public GameObject PanelPawn;
	public Button Btn;
	public float TableWidth = 0;
	public float PanelWidth = 0;
	public bool StartAtPositionLeft = false;
	public CardsBaseTableController CardsControllerComp;

	private DeckViewController Controller;
	// Use this for initialization
	void Start () {
		Btn.onClick.AddListener (PawnClicked);
		TableWidth = GetComponentInParent<RectTransform> ().rect.width;
	}
	
	// Update is called once per frame
	void Update () {
		//if (PanelPawn != null) {
			//SetPos 1 or 2
			//SetPawnPos();
		//}
		if (Controller != null) {
			TableWidth = Controller.TableWidth;
		} else {
			TableWidth = this.transform.parent.GetComponent<RectTransform> ().rect.width;
		}
		PanelWidth = this.GetComponent<RectTransform> ().rect.width;
	}

	void OnDestroy() {
		if (PanelPawn != null) {
			Destroy (PanelPawn);
		}
		Destroy (this.gameObject);
	}

	private void PawnClicked() {
		if (PanelPawn != null) {
			if (Controller != null) {
				Controller.PawnClicked (PanelPawn.GetComponent<Pawn> ().Name);
			}
			if (CardsControllerComp != null) {
				CardsControllerComp.RemoveCard (PanelPawn.GetComponent<Pawn> ().Name);
				Destroy (this.gameObject);
			}
		}
	}

	private IEnumerator MovePawnPosTask(bool smooth) {
		SmothTransform ST = PanelPawn.GetComponent<SmothTransform> ();
		if (ST) {
			Vector3 destPos;
			int AnimSpeed = 10;
			while (true) {
				yield return new WaitForSeconds (0.05f);
				if (Controller != null) {
					AnimSpeed = Controller.AnimationSpeed;
				}
				if (GetPositionForPawn ()) {
					destPos = CardPos1.position;
				} else {
					destPos = CardPos2.position;
				}
				ST.SmoothGlobalTransformTo (destPos, AnimSpeed);
				if (ST != null) {
					Debug.Log ("Move pawn on panel");
					while (ST.smoothTransformGlobalPosRunning) {
						if (GetPositionForPawn ()) {
							destPos = CardPos1.position;
						} else {
							destPos = CardPos2.position;
						}
						ST.SmoothGlobalTransformTo (destPos, AnimSpeed);
						yield return new WaitForSeconds (0.05f);
					}
				} else {
					Debug.LogWarning ("Missing SmothTransform component in pawn");
				}
				if (!smooth) {
					break;
				}
				yield return new WaitForSeconds (0.05f);
			}
			SetPawnPos ();
		} else {
			Debug.LogError ("Cannot get smooth transform component: " + PanelPawn);
		}
	}

	private void SetPawnPos() {
		Debug.Log ("Pawn pos reached");
		if (GetPositionForPawn ()) {
			PanelPawn.transform.SetParent (CardPos1, false);
		} else {
			PanelPawn.transform.SetParent (CardPos2, false);
		}
		PanelPawn.transform.localPosition = new Vector3 (0, 0, 0);
	}

	private bool GetPositionForPawn() {
		bool state = StartAtPositionLeft;
		if (Controller != null) {
			state = Controller.StartAtPositionLeft;
		}

		//if (TableWidth != 0) {
			/*int childIndex = this.transform.GetSiblingIndex ();
			RectTransform RectTr = GetComponent<RectTransform> ();
			float numberOfPanelsInRow = Controller.TableWidth / RectTr.rect.width;
			int rowNumber = childIndex / (int)numberOfPanelsInRow;

			//Debug.Log ("child:" + childIndex + " numberOfPanelsInRow:" + numberOfPanelsInRow + " panel width:" + RectTr.rect.width);
			if (rowNumber % 2 == 0) {
				return !Controller.StartAtPositionLeft;
			} else {
				return Controller.StartAtPositionLeft;
			}*/

			//algo 2
		int childIndex = this.transform.GetSiblingIndex ();
		int numberOfPanelsInRow = (int)(TableWidth / PanelWidth);
		if (numberOfPanelsInRow % 2 != 0) {
			int rowNumber = childIndex / numberOfPanelsInRow;
			if (rowNumber % 2 != 0) {
				state = !state;
			}
		}
		if (this.transform.GetSiblingIndex () % 2 == 0) {
			return !state;
		} else {
			return state;
		}
		//}
		//return state;
	}

	public void MovePawn(bool smooth = false) {
		StartCoroutine (MovePawnPosTask (smooth));
	}

	public void SetPawn(GameObject pawn, DeckViewController controller) {
		Controller = controller;
		PanelPawn = pawn;
	}
}
