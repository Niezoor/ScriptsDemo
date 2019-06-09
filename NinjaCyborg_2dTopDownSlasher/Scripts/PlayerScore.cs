using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerScore : MonoBehaviour {
	public TextMeshProUGUI PlayerName;
	public Transform ScoreTable;
	public GameObject ScorePointPrefab;

	private int ScoreNow;
	// Use this for initialization
	void Start () {
		
	}

	// Update is called once per frame
	void Update () {
		
	}

	public void UpdateScore(int Score) {
		StartCoroutine (UpdateScoreTask(Score));
	}

	private IEnumerator UpdateScoreTask(int Score) {
		int toAdd = Score - ScoreNow;
		ScoreNow = Score;
		yield return new WaitForSeconds (1f);
		while (toAdd > 0) {
			Instantiate (ScorePointPrefab, ScoreTable);
			toAdd--;
			yield return new WaitForSeconds (0.5f);
		}
	}
}
