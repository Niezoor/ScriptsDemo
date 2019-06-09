using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonToogleCanvas : MonoBehaviour {
	public Canvas CanvasToToogle;
	public bool StartCanvasState = false;
	private Button btn { get { return GetComponent<Button> (); } }
	// Use this for initialization
	void Start () {
		btn.onClick.AddListener(() =>toogleCanvas());
		if (CanvasToToogle != null) {
			CanvasToToogle.enabled = StartCanvasState;
		}
	}
	
	// Update is called once per frame
	void toogleCanvas () {
		if (CanvasToToogle != null) {
			CanvasToToogle.enabled = !CanvasToToogle.enabled;
		}
	}

	public void DisableCanvas() {
		CanvasToToogle.enabled = false;
	}
}
