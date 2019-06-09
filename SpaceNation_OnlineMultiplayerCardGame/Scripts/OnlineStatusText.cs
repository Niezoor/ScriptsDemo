using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnlineStatusText : MonoBehaviour {
	public Text text;
	public string StartText;
	public GameObject LowOnlinePlayersPanel;
	public GameObject NoOnlinePlayersPanel;
	public int Interval = 3;
	private int intervalnow  = 0;
	private Canvas ownCanvas;

	public Text TimeText;
	public static float timer;
	public static bool timeStarted = false;

	// Use this for initialization
	void Start () {
		intervalnow = Interval+1;
		ownCanvas = GetComponent<Canvas> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (timeStarted == true) 
		{
			timer += Time.deltaTime;
			float minutes = Mathf.Floor(timer / 60);
			float seconds = timer%60;

			TimeText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
		}
		if (ownCanvas != null && ownCanvas.enabled) {
			timeStarted = true;
			if (intervalnow > Interval) {
				int count = PhotonNetwork.countOfPlayersOnMaster;
				intervalnow = 0;
				text.text = StartText + count;
				if (count < 3) {
					if (!NoOnlinePlayersPanel.GetActive ()) {
						NoOnlinePlayersPanel.SetActive (true);
					}
				} else if (count < 50) {
					if (!LowOnlinePlayersPanel.GetActive ()) {
						LowOnlinePlayersPanel.SetActive (true);
					}
				} else {
					if (LowOnlinePlayersPanel.GetActive ()) {
						LowOnlinePlayersPanel.SetActive (false);
					}
					if (NoOnlinePlayersPanel.GetActive ()) {
						NoOnlinePlayersPanel.SetActive (false);
					}
				}
			}
			intervalnow++;
		} else {
			timeStarted = false;
			timer = 0;
		}
	}
}
