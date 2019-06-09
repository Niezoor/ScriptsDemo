using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeveloperPanel : MonoBehaviour {
	public Text OutText;
	public InputField InputText;
	public GamePlay GameplayController;
	public ScrollRect ScrollRectComponent;
	private List<string> CommandList = new List<string> ();
	private string[] cmdParameters;

	public string LogText;

	private bool PanelShowed = false;

	// Use this for initialization
	void Start () {
		CommandList.Add ("draw - draw random card from deck");
		CommandList.Add ("draw <card name> - draw specified card");
		CommandList.Add ("cards - list cards");
		CommandList.Add ("points <number> - set card points value");
		CommandList.Add ("network - current photon network connection status");
		CommandList.Add ("eplay1 - <name> <start idx> <end idx> <hand idx> play enemy effect");
		CommandList.Add ("eplay2 - <name> <start idx> <end idx> <hand idx> play enemy effect (for 2 words card names)");
	}

	private void eplay1() {
		if (cmdParameters.Length > 3) {
			string cardName = cmdParameters [1];
			int start = int.Parse(cmdParameters [2]);
			int end = int.Parse(cmdParameters [3]);
			int hand = int.Parse(cmdParameters [4]);
			GameplayController.PlayEnemyEffectOnBoard (cardName, start, end, hand);
		}
	}

	private void eplay2() {
		if (cmdParameters.Length > 4) {
			string cardName = cmdParameters [1];
			cardName = cardName + " " + cmdParameters [2];
			int start = int.Parse(cmdParameters [3]);
			int end = int.Parse(cmdParameters [4]);
			int hand = int.Parse(cmdParameters [5]);
			Debug.Log ("call eplay2 for: " + cardName);
			GameplayController.PlayEnemyEffectOnBoard (cardName, start, end, hand);
		}
	}

	private void network() {
		PanelLog (PhotonNetwork.connectionStateDetailed.ToString ());
	}

	private void draw() {
		if (cmdParameters.Length > 1) {
			string cardName = cmdParameters [1];
			for (int i = 2; i < cmdParameters.Length; i++) {
				cardName = cardName + " " + cmdParameters [i];
			}
			GameplayController.Draw (cardName);
		} else {
			GameplayController.Draw ();
		}
		cmdParameters = null;
	}

	private void points() {
		if (cmdParameters.Length > 1) {
			int mana = int.Parse (cmdParameters [1]);
			if (mana > GameplayController.ManaMax)
				mana = GameplayController.ManaMax;
			GameplayController.Mana = mana;
			GameplayController.UpdateManaState ();
		} else {
			PanelLog ("ERROR: cmd recources need parameter");
		}
	}

	private void cards() {
		List<string> allCards = GameplayController.CardsComp.CardsToList ();
		foreach (string card in allCards)
			PanelLog (card);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void PanelLog(string log) {
		LogText = LogText + '\n' + log;
		OutText.text = LogText;
		Canvas.ForceUpdateCanvases ();
		ScrollRectComponent.verticalNormalizedPosition = 0;
		Canvas.ForceUpdateCanvases();
	}

	public void EnterCmd() {
		string inputString = InputText.text;
		InputText.text = "";
		PanelLog (inputString);
		if (inputString.CompareTo ("help") == 0) {
			PrintHelp ();
		}
		CallCmd (inputString);
	}

	private void CallCmd (string command) {
		cmdParameters = command.Split (' ');
		Invoke (cmdParameters [0], 0);
	}

	public void ShowPanel(Animator anim) {
		PanelShowed = !PanelShowed;
		anim.SetBool ("Slide", PanelShowed);//show panel
	}

	private void PrintHelp() {
		PanelLog ("Suported command list:");
		foreach (string cmd in CommandList) {
			PanelLog (cmd);
		}
	}
}
