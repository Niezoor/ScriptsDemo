using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;

public class WinScreen : MonoBehaviour {
	[Header("configuration")]
	public bool UpdateStatsViaCloud;
	public string CloudScriptName;
	[Header("component setup")]
	public bool GameWon = false;
	public GamePlay GamePlayComponent;
	public AudioClip WinClip;
	public AudioClip LooseClip;
	public AudioClip ExpIncresePick;
	public AudioSource AudioManager;

	public Animator AnimationController;
	public Canvas WinScreenCanvas;
	public Canvas LogoCanvas;
	public ProgressBar ExperienceBar;
	public Text CurrentLvlText;
	public Text NextLvlText;
	public Text ExpDescText;
	public Text DamageValText;
	public Text RestHealthValText;
	public TextMeshProUGUI WinText;
	public Image WinImage;
	public string WinString;
	public Sprite WinSprite;
	public string LooseString;
	public Sprite LooseSprite;

	public GameObject ParticleEffectPrefab;
	public Color WinEffectColor;
	public Color LooseEffectColor;

	public bool UserEndGame = false;
	public bool GameEnd = false;
	public bool DataReady = false;

	public int ExpPerLevel = 1000;

	public float ExpFactor;
	public int ExpForWin;
	public int ExpForLoose;
	public int ExpForDmgFactor;
	public int ExpForHealthFactor;

	public int ExpForGame;
	public int ExpForDmg;
	public int ExpForHealth;

	public int CurrentXP;
	public int CurrentLVL;
	public double NextXP;
	public int NextLVL;

	public int currentLvl;
	public double currLvlExp;
	public double nextLvlExp;
	public double fullLvlPieceExp;
	public double currLvlPieceExp;

	private static bool newXPcalculated = false;
	private static bool animationStarted = false;
	private bool TitleDataLoaded = false;
	public GameplayMenu GameplayMenuComponent;

	[Header("rewards")]
	public Sprite PackageRewardImage;
	public string PackageRewardName;
	public Sprite CreditCurrencyRewardImage;
	public string CreditCurrencyRewardName;
	public GameObject ShopMenuPrefab;
	private ShopMenu ShopMenuComponent;
	private NewItemNotify NewItemNotifyComponent;
	private Dictionary<String, UserDataRecord> Rewards;
	public bool RewardsReady = false;

	//[Header("auto assigned")]
	//public LocalPlayer Player;

	public GameObject netManager;

	// Use this for initialization
	void Start () {
		GameEnd = false;
		DataReady = false;
		animationStarted = false;
		newXPcalculated = false;
		ExpDescText.text = "";
		netManager = GameObject.Find ("NetworkManager");
		if (netManager != null) {
			if (LocalPlayer.Instace != null) {
				if (netManager.GetComponent<MyNetworkManager> ().GameMode == MyNetworkManager.gameModeEnum.tutorial) {
					LocalPlayer.Instace.SavePlayerFlag (LocalPlayer.TutorialFlag, 1);//lets allow to skip the tutorial now
				}
				LocalPlayer.Instace.DataLoaded = false;
				LocalPlayer.Instace.GetPlayerProfile ();
			}
		}
		PlayFabClientAPI.GetTitleData( new GetTitleDataRequest(), LoadTitleData,
			error => Debug.LogError(error.GenerateErrorReport()));
	}
	
	// Update is called once per frame
	void Update () {
		//CurrentXP = (int)CalculateExp (CurrentLVL);
		if (DataReady && UserEndGame && !LocalPlayer.Instace.DataSaveing && LocalPlayer.Instace.DataLoaded && TitleDataLoaded) {
			if (netManager) {
				netManager.GetComponent<MyNetworkManager> ().GotoMainMenuScene ();
			} else {
				SceneManager.LoadScene("LoginScene");
			}
		}
	}

	private void LoadTitleData(GetTitleDataResult result) {
		if (result != null) {
			ExpPerLevel = Int32.Parse(result.Data ["ExpPerLevel"]);
			ExpForWin = Int32.Parse(result.Data ["ExpForWin"]);
			ExpForLoose = Int32.Parse(result.Data ["ExpForLoose"]);
		}
		TitleDataLoaded = true;
	}

	/// <summary>
	/// Called by leave game button - give up / leave game
	/// </summary>
	public void LeaveThisGame() {
		Debug.Log ("LeaveThisGame:" + animationStarted);
		if (!GameEnd) {
			DataReady = false;
			GameWon = false;
			//UpdateEndGameStats ();//Leave game before finish count as lost
			GamePlayComponent.GiveUp();
		} else {
			UserEndGame = true;
		}
	}

	public void ShowWinscreen(bool win) {
		GameWon = win;
		GameEnd = true;
		GameplayMenuComponent.CloseSubmenu ();
		if (GamePlayComponent.NetManager != null) {
			GamePlayComponent.NetManager.GetComponent<MyNetworkManager>().HideAllMenus ();
			GamePlayComponent.NetManager.GetComponent<MyNetworkManager> ().SaveRoomName ("");
		}
		StartCoroutine (PrepareStats());
	}

	private IEnumerator PrepareStats() {
		while (!LocalPlayer.Instace.DataLoaded && TitleDataLoaded) {
			yield return new WaitForSeconds (0.1f);
		}

		if (GameWon) {
			ExpForGame = ExpForWin;
			if (GamePlayComponent.myHero.GetComponent<Pawn> ().Health < 0) {
				RestHealthValText.text = "0";
				ExpForHealth = 0;
			} else {
				RestHealthValText.text = GamePlayComponent.myHero.GetComponent<Pawn> ().Health.ToString ();
				ExpForHealth = (ExpForHealthFactor * GamePlayComponent.myHero.GetComponent<Pawn> ().Health);
			}
		} else {
			ExpForGame = ExpForLoose;
			ExpForHealth = 0;
		}
		ExpForDmg = (ExpForDmgFactor * GamePlayComponent.DamageDone);
		if (UpdateStatsViaCloud) {
			int ExpBonus = ExpForHealth + ExpForDmg;
			string gameModeName = "none";
			string ExpBonusStr = ExpBonus.ToString ();
			string gameWon = "won";
			string HeroId = GamePlayComponent.myHero.GetComponent<Pawn> ().CardID.ToString();
			if (!GameWon) {
				gameWon = "lost";
			}
			if (netManager != null) {
				gameModeName = netManager.GetComponent<MyNetworkManager> ().GameMode.ToString ();

				PlayFabClientAPI.ExecuteCloudScript (new ExecuteCloudScriptRequest () {
					FunctionName = CloudScriptName, // Arbitrary function name (must exist in your uploaded cloud.js file)
					FunctionParameter = new {
						game_mode = gameModeName,
						game_won = gameWon,
						exp_bonus = ExpBonusStr,
						hero_id = HeroId
					}, // The parameter provided to your function
					GeneratePlayStreamEvent = true, // Optional - Shows this event in PlayStream
				}, OnCloudWinGame, error => Debug.LogError (error.GenerateErrorReport ()));
			}
		}
		if (animationStarted == false) {
			animationStarted = true;
			AnimationController.SetBool ("Show", true);
		}
	}

	private NewItemNotify GetNewIntemNotifyComponent() {
		if (NewItemNotifyComponent == null) {
			GameObject obj = GameObject.Find ("NewItemNotify-Canvas");
			if (obj != null) {
				NewItemNotifyComponent = obj.GetComponent<NewItemNotify> ();
			} else {
				Debug.LogWarning ("Cannot find item notify canvas");
			}
		}
		return NewItemNotifyComponent;
	}

	private void OnCloudWinGame(ExecuteCloudScriptResult result) {
		// Cloud Script returns arbitrary results, so you have to evaluate them one step and one parameter at a time
		Debug.Log("Cloud stats updated" + result.APIRequestsIssued + " error:" + result.Error
			+ " log:" + result.Logs);
		PlayFabClientAPI.ExecuteCloudScript (new ExecuteCloudScriptRequest () {
			FunctionName = "CheckRewards", // Arbitrary function name (must exist in your uploaded cloud.js file)
			FunctionParameter = null, // The parameter provided to your function
			GeneratePlayStreamEvent = true, // Optional - Shows this event in PlayStream
		}, OnCloudCheckRewards, error => Debug.LogError (error.GenerateErrorReport ()));
	}

	private void OnCloudCheckRewards(ExecuteCloudScriptResult result) {
		Debug.Log("Rewards checked" + result.FunctionResult + " error:" + result.Error
			+ " log:" + result.Logs);
		
		Rewards = PlayFab.Json.JsonWrapper.DeserializeObject<Dictionary<String, UserDataRecord>>(PlayFab.Json.JsonWrapper.SerializeObject(result.FunctionResult));
		Debug.Log("Rewards checked data:" + Rewards);
		RewardsReady = true;
	}

	public void AnimationStart() {
		GameEnd = true;
		DataReady = false;
		GameObject effect = Instantiate (ParticleEffectPrefab);
		effect.transform.SetParent (WinImage.gameObject.transform, false);
		ParticleSystem.MainModule part1, part2;
		part1 = effect.GetComponent<ParticleSystem> ().main;
		part2 = effect.transform.GetChild(1).GetComponent<ParticleSystem> ().main;

		AudioManager.Stop();
		if (GameWon) {
			//AudioManager.clip = WinClip;
			GamePlayComponent.GetComponent<AudioSource> ().PlayOneShot (WinClip);
			WinText.SetText (WinString);
			WinText.fontMaterial.SetColor (ShaderUtilities.ID_GlowColor, WinEffectColor);
			WinImage.sprite = WinSprite;
			part1.startColor = new ParticleSystem.MinMaxGradient (WinEffectColor, part1.startColor.colorMax);
			part2.startColor = new ParticleSystem.MinMaxGradient (WinEffectColor, part2.startColor.colorMax);
		} else {
			//AudioManager.clip = LooseClip;
			GamePlayComponent.GetComponent<AudioSource> ().PlayOneShot (LooseClip);
			WinText.SetText (LooseString);
			WinText.fontMaterial.SetColor (ShaderUtilities.ID_GlowColor, LooseEffectColor);
			WinImage.sprite = LooseSprite;
			part1.startColor = new ParticleSystem.MinMaxGradient (LooseEffectColor, part1.startColor.colorMax);
			part2.startColor = new ParticleSystem.MinMaxGradient (LooseEffectColor, part2.startColor.colorMax);
		}

		UpdateEndGameStats ();
		WinScreenCanvas.enabled = true;
		LogoCanvas.enabled = true;
		//AnimationController.SetBool ("Show", true);
		//AudioManager.loop = false;
		//AudioManager.Play();
	}

	private void CalculateNewExp(double currXp, int currLvl) {
		if (!newXPcalculated) {
			newXPcalculated = true;

			DamageValText.text = GamePlayComponent.DamageDone.ToString ();
			RestHealthValText.text = ExpForHealth.ToString();

			int sum = ExpForGame;
			sum += ExpForDmg;
			sum += ExpForHealth;
			Debug.Log ("Exp for game:" + ExpForGame);
			Debug.Log ("Exp for damage:" + ExpForDmg);
			Debug.Log ("Exp for health:" + ExpForHealth);
			double fullXP = CalculateExp (currLvl);
			fullXP += currXp;
			fullXP += sum;
			NextLVL = (int)CalculateLevel (fullXP);
			NextXP = fullXP - CalculateExp (NextLVL);
		}
	}

	private double CalculateLevel(double Experience) {
		/*double rv;
		if (Experience > ExpConstAt) {
			Experience -= ExpConstAt;
			rv = ExpFactor * Math.Sqrt (ExpConstAt) + (Experience/ExpConstAt);
		} else {
			rv = ExpFactor * Math.Sqrt (Experience);
		}*/
		return ((double)Experience / ExpPerLevel);
	}

	private double CalculateExp(int Level) {
		/*double rv;
		int ConstLVL = (int)CalculateLevel (ExpConstAt);
		if (Level > ConstLVL) {
			rv = (ExpConstAt * (Level - ConstLVL)) + ExpConstAt;
		} else {
			rv = Math.Pow (Level / ExpFactor, 2);
		}*/
		return ((double)ExpPerLevel * Level);
	}

	private void UpdateLevelText(int lvl) {
		CurrentLvlText.text = lvl.ToString ();
		lvl++;
		NextLvlText.text = lvl.ToString ();
	}

	private void ExpBarSetup(double currExp, int currLvl) {
		UpdateLevelText (currLvl);
		currLvlExp = (int)CalculateExp (currLvl);
		nextLvlExp = (int)CalculateExp (currLvl + 1);
		fullLvlPieceExp = nextLvlExp - currLvlExp;
		currLvlPieceExp = currExp;
		currentLvl = currLvl;
		ExperienceBar.SetMax ((int)fullLvlPieceExp);
		ExperienceBar.SetProgress ((int)currLvlPieceExp);
		ExperienceBar.SetAfterText ("/" + (int)fullLvlPieceExp);
	}

	private IEnumerator ShowRewards() {
		if (RewardsReady) {
			GameObject gob = Instantiate (ShopMenuPrefab, Camera.main.transform);
			ShopMenuComponent = gob.GetComponent<ShopMenu> ();
			UserDataRecord data;
			string curr = "";
			int amount = 0;

			if (GetNewIntemNotifyComponent () != null) {
				if (Rewards.TryGetValue ("rewardItems", out data)) {
					if (data != null && data.Value != null && data.Value.Length > 0) {
						ShopMenu.MyShopItemClass item = ShopMenuComponent.FindItem (data.Value);
						NewItemNotifyComponent.ShowNotify (item.Picture, null, item.DisplayName);
						yield return new WaitForSeconds (1.5f);
					}
				}
				if (Rewards.TryGetValue ("rewardCurr", out data)) {
					if (data != null && data.Value != null && data.Value.Length > 0) {
						curr = data.Value;
					}
				}
				if (Rewards.TryGetValue ("rewardCurrAmount", out data)) {
					if (data != null && data.Value != null && data.Value.Length > 0) {
						amount = Int32.Parse(data.Value);
					}
				}
				if (amount > 0) {
					if (curr.Equals ("CR") && amount > 0) {
						NewItemNotifyComponent.ShowNotify (
							CreditCurrencyRewardImage, null, amount + " " + CreditCurrencyRewardName
						);
					}
				}
			}
		}
		yield return null;
	}

	private IEnumerator ExpBarUpdate(double currExp, int currLvl) {
		ExpBarSetup (currExp, currLvl);
		//Debug.Log("currLvlExp:" + currLvlExp + "nextLvlExp:" + nextLvlExp);
		AudioSource aSource = this.GetComponent<AudioSource>();
		yield return new WaitForSeconds (5f);
		aSource.volume = 0.3f;

		ExpDescText.text = "+" + ExpForGame + " za mecz";
		for (int i = 0; i < ExpForGame; i++) {
			currLvlPieceExp++;
			ExperienceBar.SetProgress ((int)currLvlPieceExp);
			aSource.PlayOneShot (ExpIncresePick);
			float p = 1.5f + (i * 0.003f);
			aSource.pitch = p > 2 ? 2 : p;
			if (currLvlPieceExp >= fullLvlPieceExp) {
				ExpBarSetup (0, currentLvl+1);
			}
			yield return new WaitForSeconds (0.1f/(ExpForGame-i));
			//Debug.Log ("currLvlExp:" + 0.1f / (ExpForGame - i));
			//Debug.Log ("pitch:" + aSource.pitch);
		}
		yield return new WaitForSeconds (1.5f);

		ExpDescText.text = "+" + ExpForDmg + " za obrażenia";
		for (int i = 0; i < ExpForDmg; i++) {
			currLvlPieceExp++;
			ExperienceBar.SetProgress ((int)currLvlPieceExp);
			aSource.PlayOneShot (ExpIncresePick);
			float p = 1.5f + (i * 0.003f);
			aSource.pitch = p > 2 ? 2 : p;
			if (currLvlPieceExp >= fullLvlPieceExp) {
				ExpBarSetup (0, currentLvl+1);
			}
			yield return new WaitForSeconds (0.1f/(ExpForDmg-i));
		}
		yield return new WaitForSeconds (1.5f);

		ExpDescText.text = "+" + ExpForHealth + " za pozostałe życie";
		for (int i = 0; i < ExpForHealth; i++) {
			currLvlPieceExp++;
			ExperienceBar.SetProgress ((int)currLvlPieceExp);
			aSource.PlayOneShot (ExpIncresePick);
			float p = 1.5f + (i * 0.003f);
			aSource.pitch = p > 2 ? 2 : p;
			if (currLvlPieceExp >= fullLvlPieceExp) {
				ExpBarSetup (0, currentLvl+1);
			}
			yield return new WaitForSeconds (0.1f/(ExpForHealth-i));
		}
		yield return new WaitForSeconds (1.5f);
		ExpDescText.text = "+" + (ExpForGame + ExpForDmg + ExpForHealth);
		yield return new WaitForSeconds (0.5f);
		int timeout = 50;
		while(!RewardsReady && timeout > 0) {
			yield return new WaitForSeconds (0.1f);
			timeout--;
		}
		StartCoroutine( ShowRewards ());
	}

	public void ShowXPBarAnimation() {
		//CalculateNewExp (CurrentXP, CurrentLVL);//for test only
		StartCoroutine( ExpBarUpdate(CurrentXP, CurrentLVL));
	}

	public void UpdateEndGameStats() {
		bool updateQPStats = false;
		bool updateRankedStats = false;
		bool updateTrainingStats = false;

		CurrentXP = LocalPlayer.Instace.localPlayerData.exp;
		CurrentLVL = LocalPlayer.Instace.localPlayerData.level;
		ExpBarSetup (LocalPlayer.Instace.localPlayerData.exp, LocalPlayer.Instace.localPlayerData.level);
		CalculateNewExp (LocalPlayer.Instace.localPlayerData.exp, LocalPlayer.Instace.localPlayerData.level);

		LocalPlayer.Instace.localPlayerData.exp = (int)NextXP;
		LocalPlayer.Instace.localPlayerData.level = NextLVL;
		if (netManager != null) {
			if (netManager.GetComponent<MyNetworkManager> ().GameMode ==
				MyNetworkManager.gameModeEnum.quickPlay) {
				updateQPStats = true;
				if (GameWon) {
					LocalPlayer.Instace.localPlayerData.qp_wins++;
				} else {
					LocalPlayer.Instace.localPlayerData.qp_losses++;
				}
			} else if (netManager.GetComponent<MyNetworkManager> ().GameMode ==
				MyNetworkManager.gameModeEnum.ranked) {
				updateRankedStats = true;
				if (GameWon) {
					LocalPlayer.Instace.localPlayerData.wins++;
				} else {
					LocalPlayer.Instace.localPlayerData.looses++;
				}
			} else if (netManager.GetComponent<MyNetworkManager> ().GameMode ==
				MyNetworkManager.gameModeEnum.training) {
				updateTrainingStats = true;
				if (GameWon) {
					LocalPlayer.Instace.localPlayerData.tr_wins++;
				} else {
					LocalPlayer.Instace.localPlayerData.tr_losses++;
				}
			}
			if (LocalPlayer.Instace != null) {
				if (!UpdateStatsViaCloud) { 
					LocalPlayer.Instace.StorePlayerStats (true, updateQPStats, updateRankedStats, updateTrainingStats);
				}
				//Player.SavePlayerData ();
				if (netManager.GetComponent<MyNetworkManager> ().GameMode == MyNetworkManager.gameModeEnum.tutorial) {
					LocalPlayer.Instace.SavePlayerFlag (LocalPlayer.TutorialFlag, 1);
				}
			}
		}
		DataReady = true;
	}
}
