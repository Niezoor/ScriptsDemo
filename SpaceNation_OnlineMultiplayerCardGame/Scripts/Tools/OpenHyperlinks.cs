using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class OpenHyperlinks : MonoBehaviour, IPointerClickHandler {
	private TextMeshProUGUI pTextMeshPro;

	void Awake() {
		pTextMeshPro = this.GetComponent<TextMeshProUGUI> ();
	}

	public void OnPointerClick(PointerEventData eventData) {
		int linkIndex = TMP_TextUtilities.FindIntersectingLink(pTextMeshPro, Input.mousePosition, Camera.main);
		Debug.Log ("link clicked");
		if( linkIndex != -1 ) { // was a link clicked?
			TMP_LinkInfo linkInfo = pTextMeshPro.textInfo.linkInfo[linkIndex];
			Debug.Log ("link clicked - open:" + linkInfo.GetLinkID());

			// open the link id as a url, which is the metadata we added in the text field
			Application.OpenURL(linkInfo.GetLinkID());
		}
	}
}
