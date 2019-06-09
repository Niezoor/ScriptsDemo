using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour {
	public string ToLoadSceneName;

	// Use this for initialization
	void Start () {
		SceneManager.LoadSceneAsync (ToLoadSceneName);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
