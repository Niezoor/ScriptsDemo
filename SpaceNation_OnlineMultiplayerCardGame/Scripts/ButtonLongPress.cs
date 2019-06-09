using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonLongPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
	[SerializeField]
	private float holdTime = 1f;

	public UnityEvent onLongPress = new UnityEvent();

	public void OnPointerDown(PointerEventData eventData) {
		Invoke("OnLongPress", holdTime);
	}

	public void OnPointerUp(PointerEventData eventData) {
		CancelInvoke("OnLongPress");
	}

	public void OnPointerExit(PointerEventData eventData) {
		CancelInvoke("OnLongPress");
	}

	void OnLongPress() {
		onLongPress.Invoke();
	}
}