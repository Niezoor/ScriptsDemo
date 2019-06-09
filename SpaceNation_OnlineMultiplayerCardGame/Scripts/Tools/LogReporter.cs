using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.ClientModels;
using PlayFab;

public class LogReporter : MonoBehaviour {
	public int SendedReportsLimit = 3;

	void OnEnable() {
		#if UNITY_EDITOR
		Debug.LogWarning ("Do not report log from Unity Editor");
		#else
		Application.logMessageReceivedThreaded += HandleLog;
		#endif
	}
	void OnDisable() {
		#if UNITY_EDITOR
		Debug.LogWarning ("Do not report log from Unity Editor");
		#else
		Application.logMessageReceivedThreaded -= HandleLog;
		#endif
	}
	void HandleLog(string logString, string stackTrace, LogType type) {
		if (type == LogType.Error ||
		    type == LogType.Assert ||
		    type == LogType.Exception) {
			if (SendedReportsLimit > 0) {
				Dictionary<string, object> eventReport = new Dictionary<string, object> ();
				WriteTitleEventRequest globalRequest = new WriteTitleEventRequest ();
				globalRequest.EventName = "CLIEN_LOG_" + type.ToString ();
				eventReport.Add ("Log", logString);
				eventReport.Add ("Stack", stackTrace);
				globalRequest.Body = eventReport;
				PlayFabClientAPI.WriteTitleEvent (globalRequest, OnPlaySendEventSuccess, OnPlayFabError);
				SendedReportsLimit--;
			}
		}
	}

	private void OnPlaySendEventSuccess(WriteEventResponse result)
	{
		Debug.LogWarning ("Report sended");
	}

	private void OnPlayFabError(PlayFabError error)
	{
		Debug.LogWarning ("Got an error: " + error.ErrorMessage);
	}
}
