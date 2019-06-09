using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour {
	public delegate int GetValInt();
	public delegate string GetValString();

	[System.Serializable]
	public struct DebugModuleClass
	{
		public Text DebugText;
		public string DebugPrefix;
		public string DebugPosix;
		public bool Enable;
		public float RefreshRate;
		public GetValInt GetInt;
		public GetValString GetString;
		public Coroutine TaskContext;
	}

	public DebugModuleClass FPSDebug;
	public DebugModuleClass PingDebug;
	public DebugModuleClass NetStatusDebug;

	// Use this for initialization
	void Start () {
		FPSDebug.GetInt = GetFPS;
		PingDebug.GetInt = GetPing;
		NetStatusDebug.GetString = GetNetworkStatus;
		StartDebug (FPSDebug);
		StartDebug (PingDebug);
		StartDebug (NetStatusDebug);
	}
	
	// Update is called once per frame
	void Update () {
	}

	void OnDestroy() {
		StopDebug ();
	}

	void OnDisable() {
		StopDebug ();
	}

	private void StopDebug() {
		StopAllCoroutines ();
	}

	private void StopDebug(DebugModuleClass module) {
		if (module.TaskContext != null) {
			StopCoroutine (module.TaskContext);
		}
	}

	private Coroutine StartDebug(DebugModuleClass module) {
		if (module.Enable) {
			return StartCoroutine (DebugModuleTask(module));
		}
		return null;
	}

	private int GetFPS() {
		return (int) (1 / Time.deltaTime);
	}

	private int GetPing() {
		return PhotonNetwork.GetPing ();
	}

	private string GetNetworkStatus() {
		return PhotonNetwork.connectionStateDetailed.ToString ();
	}

	private IEnumerator DebugModuleTask(DebugModuleClass module) {
		while (module.Enable) {
			string msg = "";
			if (module.GetInt != null) {
				msg = module.GetInt ().ToString ();
			}
			if (module.GetString != null) {
				msg += module.GetString ();
			}
			module.DebugText.text = module.DebugPrefix + msg + module.DebugPosix;
			yield return new WaitForSeconds (module.RefreshRate);
		}
		module.DebugText.text = "";
		yield return null;
	}
}
