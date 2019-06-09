using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class GameLogPanel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {//, IPointerExitHandler, IPointerEnterHandler {
	public GameObject PawnOnPanel;
	public GamePlayActionStack.ActionTypeEnum PawnAction;

	[System.Serializable]
	public class TargetClass
	{
		public string targetName;
		public int targetHealth;
		public int targetAttack;
		public int targetID;
		public bool friendly;
	}
	public List <TargetClass> Targets = new List <TargetClass> ();
	public bool pawnOnly = false;

	public GameLog gameLogComp;

	public Pawn pawnOnPanelPawnComponent;
	public int PawnID;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (!Input.GetMouseButton (0)) {
			gameLogComp.HideLogFocus ();
		}
	}

	public void OnPointerDown(PointerEventData eventData) {
		gameLogComp.ShowLogFocus (this.gameObject);
	}

	public void OnPointerUp(PointerEventData eventData) {
		gameLogComp.HideLogFocus ();
	}

	/*public void OnPointerEnter(PointerEventData eventData) {
		gameLogComp.ShowLogFocus (this.gameObject);
	}

	public void OnPointerExit(PointerEventData eventData) {
		gameLogComp.HideLogFocus ();
	}*/
}
