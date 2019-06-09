using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class NewTurnButton : MonoBehaviour {
	public Color MyTurnColor;
	public Color EnemyTurnColor;
	public Color AlarmTurnColor;
	public Color InsideColor;
	public Color AlphaZeroColor;

	public string MyTurnText;
	public string EnemyTurnText;

	public TextMeshPro ButtonTextComponent;
	public GamePlay GamePlayComponent;

	public SpriteRenderer ButtonFrame;
	public SpriteRenderer ButtonInside;
	public SpriteRenderer ButtonOutline;

	public bool interactable = false;
	private bool alarmMode = false;
	private Vector2 startMousePos;
	private float maxClickDelta = 0.02f;

	// Use this for initialization
	void Start () {
		
	}

	public void SignalTurnReady() {
		if (!alarmMode) {
			ButtonOutline.color = MyTurnColor;
			GetComponent<Animation> ().Play ();
		}
	}

	public void ChangeButtonToEnemyTurn () {
		GetComponent<Animation> ().Stop ();
		ButtonFrame.color = EnemyTurnColor;
		ButtonInside.color = EnemyTurnColor;
		ButtonOutline.color = AlphaZeroColor;
		ButtonTextComponent.SetText(EnemyTurnText);
	}

	public void ChangeButtonToMyTurn () {
		GetComponent<Animation> ().Stop ();
		ButtonFrame.color = MyTurnColor;
		ButtonInside.color = InsideColor;
		ButtonOutline.color = AlphaZeroColor;
		ButtonTextComponent.SetText(MyTurnText);
	}

	public void ChangeButtonToAlarmMode () {
		if (!alarmMode) {
			alarmMode = true;
			ButtonFrame.color = AlarmTurnColor;
			ButtonInside.color = AlarmTurnColor;
			ButtonOutline.color = AlarmTurnColor;
			GetComponent<Animation> ().Play ();
		}
	}

	public void OnMouseDown() {
		if (!EventSystem.current.IsPointerOverGameObject ()) {
			Debug.Log ("End turn pressed");
			startMousePos = Camera.main.ScreenToViewportPoint (Input.mousePosition);
		}
	}

	public void OnMouseUp() {
		if (!EventSystem.current.IsPointerOverGameObject ()) {
			if (maxClickDelta > Vector3.Distance (startMousePos,
				    Camera.main.ScreenToViewportPoint (Input.mousePosition)))
			{
				Debug.Log ("End turn released");
				if (interactable && GamePlayComponent.myTurn) {
					alarmMode = false;
					GamePlayComponent.GiveTurn ();
				}
			}
		}
	}

}
