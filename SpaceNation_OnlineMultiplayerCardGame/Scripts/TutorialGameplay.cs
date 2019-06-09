using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialGameplay : MonoBehaviour {
	public GamePlay GameplayComponent;
	public GameObject TutorialEnemyDeckPrefab;
	public GameObject MyTutorialDeckPrefab;
	public GameObject MarkPrefab;
	public GameObject markObject;

	public GameObject AttackMarkPrefab;
	public GameObject attackMarkObject;

	public Canvas NotifyCanvas;
	public Text NotifyText;

	private Deck EnemyTutorialDeck;
	private Hero TutorialEnemyHero;
	private CardsBase CardsBaseComponent;
	public int AIPawnIDNext = 1000;
	private GameObject fakePawn;
	public enum TutorialSteps
	{
		step1,
		step2,
		step3,
		step4,
		step5,
		step6,
		step7,
		step8
	}

	public TutorialSteps TutorialCurrentStep = TutorialSteps.step1;

	public string StartHandCard = "Przyboczna Strażniczka";
	public string EnemyStep2Card = "Przyboczna Strażniczka";
	public string MyStep2Card = "Przyboczna Strażniczka";
	public string EnemyStep3Card = "Cierpliwy Wojownik";
	public string MyStep3Card = "Tarcza";
	public string EnemyStep4Card = "Krzepiciel";
	public string MyStep4Card = "Nalot";
	public string EnemyStep5Card = "Behemot";
	public string MyStep5Card = "Bohaterska Strażniczka";
	public string EnemyStep6Card = "";
	public string MyStep6Card = "Shotgun";
	public string EnemyStep7Card = "Paskudny Niszczyciel";
	public string MyStep7Card = "Elitarny Strażnik";
	public string EnemyStep8Card = "Lekki Czołg Kroczący";
	public string MyStep8Card = "Plecak Rakietowy";
	[Header("Turial slides")]
	public Canvas Slide1;
	public Canvas Slide2;
	public Canvas Slide3;
	public Canvas Slide4;
	public Canvas Slide5;
	public Canvas CardHelp;
	public Canvas EffectCardHelp;
	public Canvas ItemCardHelp;
	public Canvas PawnHelp;
	public Canvas DistanceHelp;
	public Canvas MeleeHelp;
	public Canvas BlockHelp;


	[Header("Turial texts")]
	[TextArea()]
	public string MyHeroText;
	[TextArea()]
	public string EnemyHeroText;
	[TextArea()]
	public string ActionPointsText;
	[TextArea()]
	public string EndTurnString;
	[TextArea()]
	public string MyCardString;
	[TextArea()]
	public string EnemyCardString;
	[TextArea()]
	public string MyDeckString;
	[TextArea()]
	public string EnemyDeckString;
	public Canvas NextMessageCanvas;
	public Text NextMessageText;
	private Vector3 OriginalScale;
	private Vector3 OriginalPosition;
	public float handUpScale = 0.1f;
	public float deckUpScale = 0.1f;
	public float heroUpScale = 2f;
	public float buttonUpScale = 0.2f;

	public GameObject currentPawn;
	public Pawn currentEnemyPawn;
	private GameObject currentWeapon;

	// Use this for initialization
	void Start () {
		Slide1.enabled = false;
		Slide2.enabled = false;
		Slide3.enabled = false;
		Slide4.enabled = false;
		NextMessageCanvas.enabled = false;
		HideNotify ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void TutorialInit(GamePlay GPComponent) {
		GameplayComponent = GPComponent;
		//GameplayComponent.EnemyInitCardDraw ();
		GameplayComponent.EnemyIsReady = true;
		GameplayComponent.IAmReady = true;
		//GameplayComponent.MatchStart ();
		GameplayComponent.myTurn = false;
		CardsBaseComponent = GameplayComponent.CardsComp;
		GameObject card = CardsBaseComponent.SpawnCardByName (StartHandCard);
		Transform pawnTransform = card.transform.Find ("Pawn");
		pawnTransform.SetParent (this.transform, false);
		pawnTransform.GetComponent<KeepParentRenderLayer>().KeepingActive = false;
		pawnTransform.gameObject.GetComponent<SpriteRenderer> ().color = new Color (0, 0, 0, 0);
		fakePawn = pawnTransform.gameObject;
		fakePawn.GetComponent<Pawn> ().Friendly = false;
		fakePawn.GetComponent<Pawn> ().Fake = true;
		GameplayComponent.PutPawnOnPosisionImpl (fakePawn, 4);
		GameplayComponent.PutPawnOnPosisionImpl (fakePawn, 1);
		GameplayComponent.PutPawnOnPosisionImpl (fakePawn, 10);
		GameplayComponent.PutPawnOnPosisionImpl (fakePawn, 6);

		GameObject DeckGO = (GameObject)Instantiate(TutorialEnemyDeckPrefab);
		EnemyTutorialDeck = DeckGO.GetComponent<Deck> ();
		TutorialEnemyHero = EnemyTutorialDeck.Hero.GetComponent<Hero> ();
		string heroName = TutorialEnemyHero.Name;
		Destroy (TutorialEnemyHero);
		Destroy (DeckGO);

		//GameplayComponent.enemyDeck = DeckGO;
		GameplayComponent.SetEnemyHero (heroName);
		GameplayComponent.enemyDeck.GetComponent<Deck> ().SetHero (GameplayComponent.enemyDeck);
		GameplayComponent.enemyHero.GetComponent<Pawn> ().SetHealth (4);
		GameplayComponent.Mana = 3;
		GameplayComponent.UpdateManaState ();

		Destroy (card);
	}

	public void StartTutorial() {
		//GameplayComponent.KeepStartHand ();
		GameplayComponent.HandComp.SetupNormalHand ();
		GameplayComponent.EnableHandShowHideMode ();
		//GameplayComponent.HandComp.ShowHand ();

		GameplayComponent.EnemyInitCardDraw ();
		GameplayComponent.EnemyIsReady = true;
		//GameplayComponent.MatchStart ();

		GameplayComponent.EndTurnButton.interactable = false;

		StartCoroutine( TutorialStep1 ());
	}

	private IEnumerator TutorialStep1() {
		yield return new WaitForSeconds (1f);
		while (GameplayComponent.myHero.GetComponent<SmothTransform> ().smoothTransformPosRunning == true) {
			yield return new WaitForSeconds (0.1f);
		}
		GameplayComponent.HandComp.HideHand ();
		GameplayComponent.Draw (StartHandCard);
		GameplayComponent.myTurn = false;
		GameplayComponent.HandComp.DisableOtherCards (null);
		yield return new WaitForSeconds (2f);
		GameplayComponent.HandComp.DisableOtherCards (null);
		GameplayComponent.HandComp.HideHand ();
		Slide1.enabled = true;
	}

	public void HideCardHelp() {
		CardHelp.enabled = false;
		TutorialEndStep1 ();
	}

	public void HidePawnHelp() {
		PawnHelp.enabled = false;
		SetupEndTurn ();
	}

	public void TutorialEndStep1() {
		Slide1.enabled = false;
		//GameplayComponent.ShowTargetNotification ("Przytrzymaj kartę z ręki, aby ją powiększyć.\nPrzesuń na planszę, aby ją zagrać");
		ShowNotify ("Przytrzymaj kartę z ręki, aby ją powiększyć.\nPrzesuń na planszę, aby ją zagrać.");
		GameplayComponent.HandComp.EnableOtherCards ();
		GameplayComponent.myTurn = true;
		ShowCardToPlay ();
		if (currentPawn != null) {
			currentPawn.GetComponent<Pawn>().RegisterPlayConfirmCallback(Step1CardPlayed);
			currentPawn.GetComponent<Pawn>().RotationDisabled = true;
		}
	}

	private void Step1CardPlayed(int id) {
		PawnHelp.enabled = true;
		HideNotify ();
	}

	private IEnumerator TutorialStep2() {
		StartCoroutine (playCard (EnemyStep2Card, 17));
		yield return new WaitForSeconds (3f);
		GameplayComponent.NewTurnAnimation.SetTrigger ("ShowNotification");
		GameplayComponent.EndTurnButton.ChangeButtonToMyTurn();
		GameplayComponent.EndTurnButton.interactable = false;
		GameplayComponent.Mana = 2;
		GameplayComponent.UpdateManaState ();
		yield return new WaitForSeconds (2f);
		GameplayComponent.Draw (MyStep2Card);
		GameplayComponent.HandComp.HideHand ();
		yield return new WaitForSeconds (1.1f);
		GameplayComponent.HandComp.HideHand ();
		GameplayComponent.HandComp.DisableOtherCards (null);
		ShowDistanceAttackHelp ();
		//markObject.transform.localScale = new Vector3 (3, 3, 3);
		//StartCoroutine(ShowMoveMark (currentPawn.gameObject, currentPawn.gameObject, 5));
	}

	private void ShowDistanceAttackHelp() {
		DistanceHelp.enabled = true;
	}

	public void HideDistanceAttackHelp() {
		DistanceHelp.enabled = false;
		Pawn pawnComp = currentPawn.GetComponent<Pawn> ();
		pawnComp.AttackRulesOverrideCallback = TutorialStep2PawnAttackOverrideCallback;
		pawnComp.RegisterOnAttackCallback (TutorialStep2PawnDeathCallback);
		pawnComp.EnablePawn ();
		pawnComp.ResetState ();
		pawnComp.SetAttackOnlyMode ();
		//GameplayComponent.ShowTargetNotification ("Kliknij w pionka na planszy, aby zobaczyć możliwe ruchy");
		ShowNotify ("Kliknij w pionka na planszy, aby zobaczyć możliwe ruchy.");
		ShowMark (currentPawn.transform, 5);
	}

	private void TutorialStep2PawnAttackOverrideCallback(int id) {
		Slide2.enabled = true;
		GameplayComponent.HandComp.HideHand ();
		GameplayComponent.HandComp.DisableOtherCards (null);
		GameplayComponent.HandComp.HandCards [0].GetComponent<CardInteraction> ().DisableCard ();
		//GameplayComponent.ShowTargetNotification ("Kliknij w pionka przeciwnika, aby go zaatakować\nŻółte punkty oznaczają ilość zadawanych obrażeń, a czerwone ilość życia");
		ShowNotify ("Kliknij w pionka przeciwnika, aby go zaatakować\n" +
			"Żółte cyfry to punkty obrażeń, a czerwone ilość życia.");
		currentPawn.GetComponent<PolygonCollider2D> ().enabled = false;
		ShowMark (GameplayComponent.Board [17].Pawn.transform, 5);
		//markObject.transform.localScale = new Vector3 (3, 3, 3);
	}

	private void TutorialStep2PawnDeathCallback(int id) {
		Slide2.enabled = false;
		Destroy (markObject);
		//GameplayComponent.ShowTargetNotification ("Jeśli atakowany pionek ma atakującego w swoim zasięgu to on równierz zadaje obrażenia");
		//ShowNotify ("Jeśli atakowany pionek ma atakującego w swoim zasięgu to on równierz zadaje obrażenia");
		StartCoroutine (TutorialStep2End());
	}

	private IEnumerator TutorialStep2End() {
		yield return new WaitForSeconds (2f);
		ShowCardToPlay ();
		if (currentPawn != null) {
			currentPawn.GetComponent<Pawn>().RegisterPlayConfirmCallback(TutorialStep2EndCardPlay);
		}
		//GameplayComponent.ShowTargetNotification ("Posiadasz 2 punkty akcji, możesz za nie wyłożyć nową kartę na planszę");
		HideNotify();
		GameplayComponent.HandComp.EnableOtherCards ();
		GameplayComponent.myTurn = true;
		ShowNotify ("Posiadasz 2 punkty akcji, możesz za nie wyłożyć nową kartę na planszę.");
	}

	private void TutorialStep2EndCardPlay(int id) {
		GameplayComponent.Mana = 0;
		GameplayComponent.UpdateManaState ();
		SetupEndTurn ();
	}

	private IEnumerator TutorialStep3() {
		StartCoroutine (playCard (EnemyStep3Card, 17));
		yield return new WaitForSeconds (3f);
		GameplayComponent.NewTurnAnimation.SetTrigger ("ShowNotification");
		GameplayComponent.EndTurnButton.ChangeButtonToMyTurn();
		GameplayComponent.EndTurnButton.interactable = false;
		GameplayComponent.Mana = 3;
		GameplayComponent.UpdateManaState ();
		yield return new WaitForSeconds (2f);
		GameplayComponent.Draw (MyStep3Card);
		GameplayComponent.HandComp.HideHand ();
		yield return new WaitForSeconds (3f);
		//GameplayComponent.HandComp.HideHand ();
		GameplayComponent.HandComp.DisableOtherCards (null);
		ShowMelleAttackHelp ();
	}

	private void ShowMelleAttackHelp() {
		MeleeHelp.enabled = true;
	}

	public void HideMelleAttackHelp() {
		MeleeHelp.enabled = false;
		ShowNotify ("Zasięg ataku twojego pionka to jedno pole.\n" +
			"Pionek przeciwnika ma Cie w swoim zasięgu.\n" +
			"Daj swojemu pionkowi tarcze, aby uniemożliwić przeciwnikowi atak.");
		ShowCardToPlay ();
		if (currentWeapon != null) {
			currentWeapon.GetComponent<Pawn>().RegisterPlayConfirmCallback(TutorialStep3EndCardPlay);
		}
		Slide3.enabled = true;
	}

	private void TutorialStep3EndCardPlay(int id) {
		Slide3.enabled = false;
		//Destroy(markObject);
		currentWeapon.GetComponent<Pawn>().AddWeaponToPawn (id);
		Pawn pawnComp = currentPawn.GetComponent<Pawn> ();
		pawnComp.RotationDisabled = true;
		ShowBlockHelp ();
		//GameplayComponent.Mana = 0;
		//GameplayComponent.UpdateManaState ();
		//SetupEndTurn ();
	}

	private void ShowBlockHelp() {
		HideNotify ();
		BlockHelp.enabled = true;
	}

	public void HideBlockHelp() {
		BlockHelp.enabled = false;
		Pawn pawnComp = currentPawn.GetComponent<Pawn> ();
		pawnComp.AttackRulesOverrideCallback = TutorialStep3PawnClicked;
		pawnComp.RegisterOnAttackCallback (TutorialStep3PawnAttaked);
		pawnComp.OnDeselectCallback = TutorialStep3PawnUnselect;
		pawnComp.EnablePawn ();
		pawnComp.ResetState ();
		TutorialStep3PawnUnselect (0);
		ShowMark (currentPawn.transform, 5);
		Slide4.enabled = true;
	}

	private void TutorialStep3PawnUnselect(int unused) {
		ShowNotify ("Klinknij swojego pionka, aby zobaczyć możliwe ruchy");
		ShowMark (currentPawn.transform, 5);
		markObject.transform.localRotation = new Quaternion (0, 0, 0, 0);
	}

	private void TutorialStep3PawnClicked(int id) {
		Debug.Log ("TutorialStep3PawnClicked");
		if (currentPawn.GetComponent<Pawn> ().boardPosisionIndex == 5) {
			ShowNotify ("Przesuń swojego pionkaaby znalazał się w zasięgu twojego ataku.\n" +
			"Przemieszczanie się pionkami zużywa punkty akcji.");
			ShowMark (GameplayComponent.Board [11].BoardPiece.transform, 5);
			markObject.transform.localPosition = new Vector3 (0, 0, 0.01f);
			markObject.transform.localScale = new Vector3 (0.005f, 0.005f, 0.005f);
		} else {
			Debug.Log ("TutorialStep3PawnMoved");
			currentPawn.GetComponent<PolygonCollider2D> ().enabled = false;
			ShowNotify ("Kliknij w pionka przeciwnika, aby go zaatakować\n" +
			"Tarcza uchroni Cię przed jego atakiem dystansowym");
			ShowMark (GameplayComponent.Board [17].Pawn.transform, 5);
		}
	}

	private void TutorialStep3PawnAttaked(int id) {
		currentPawn.GetComponent<Pawn> ().AttackRulesOverrideCallback = null;
		HideNotify ();
		SetupEndTurn ();
		Slide4.enabled = false;
	}

	private IEnumerator TutorialStep4() {
		GameplayComponent.ChangeEnemyPawnPos (AIPawnIDNext - 1, 4, 12);
		GameplayComponent.ConfirmEnemyPawnPos ("", AIPawnIDNext - 1, 4, 12);
		yield return new WaitForSeconds (1f);
		GameplayComponent.DoAttackImpl (12, 11, GamePlay.attackDirections.DownLeft, 1);
		yield return new WaitForSeconds (1f);
		StartCoroutine (playCard (EnemyStep4Card, 17));
		yield return new WaitForSeconds (1f);
		GameplayComponent.NewTurnAnimation.SetTrigger ("ShowNotification");
		GameplayComponent.EndTurnButton.ChangeButtonToMyTurn();
		GameplayComponent.EndTurnButton.interactable = false;
		GameplayComponent.Board [12].Pawn.GetComponent<Pawn> ().RegisterDeathCallback (TutorialStep4End);
		yield return new WaitForSeconds (2f);
		ShowEffectCardHelp ();
	}

	private void TutorialStep4CardPlayAgain() {
		StartCoroutine(TutorialStep4CardPlay ("Spróbuj ponownie.\nUżyj karty efektu \"Nalot\", aby zniszczyć pionki przeciwnika"));
	}

	private IEnumerator TutorialStep4CardPlay(string msg) {
		GameplayComponent.Draw (MyStep4Card);
		GameplayComponent.HandComp.HideHand ();
		yield return new WaitForSeconds (2f);
		GameplayComponent.Mana = 3;
		GameplayComponent.UpdateManaState ();
		//GameplayComponent.HandComp.HideHand ();
		GameplayComponent.HandComp.DisableOtherCards (null);
		if (GameplayComponent.HandComp.HandCards [0] != null) {
			GameObject cardInHand = GameplayComponent.HandComp.HandCards [0].gameObject;
			Transform pawnTransform = cardInHand.transform.Find ("Pawn");
			if (pawnTransform.GetComponent<Pawn> ().CardType == CardsBase.CardTypesEnum.Pawn) {
				currentPawn = pawnTransform.gameObject;
			}
			VerticalLineAttack VLA = pawnTransform.GetComponent<VerticalLineAttack> ();
			VLA.TutorialModeBadPlayCallback = TutorialStep4CardPlayAgain;
			VLA.TutorialModeStartIndex = 3;
			VLA.TutorialMode = true;
			ShowMark (cardInHand.transform, 14);
			StartCoroutine (ShowMoveMark (cardInHand,
				GameplayComponent.Board [12].BoardPiece,
				GameplayComponent.Board [21].BoardPiece,
				100));
			GameplayComponent.HandComp.EnableOtherCards ();
			GameplayComponent.myTurn = true;
		}
		ShowNotify (msg);
	}

	private void ShowEffectCardHelp() {
		HideNotify ();
		EffectCardHelp.enabled = true;
	}

	public void HideEffectCardHelp() {
		EffectCardHelp.enabled = false;
		StartCoroutine(TutorialStep4CardPlay ("Użyj karty efektu \"Nalot\", aby zniszczyć pionki przeciwnika"));
	}

	private void TutorialStep4End(int unsused) {
		SetupEndTurn ();
	}

	private IEnumerator TutorialStep5() {
		StartCoroutine (playCard (EnemyStep5Card, 17));
		yield return new WaitForSeconds (1f);
		GameplayComponent.NewTurnAnimation.SetTrigger ("ShowNotification");
		GameplayComponent.EndTurnButton.ChangeButtonToMyTurn();
		GameplayComponent.EndTurnButton.interactable = false;
		GameplayComponent.Mana = 3;
		GameplayComponent.UpdateManaState ();
		yield return new WaitForSeconds (2f);
		GameplayComponent.Draw (MyStep5Card);
		GameplayComponent.HandComp.HideHand ();
		yield return new WaitForSeconds (3f);
		//GameplayComponent.HandComp.HideHand ();
		GameplayComponent.HandComp.DisableOtherCards (null);
		ShowNotify ("Zagraj kartę na planszę");
		ShowCardToPlay ();
		if (currentPawn != null) {
			currentPawn.GetComponent<Pawn>().RegisterPlayConfirmCallback(TutorialStep5EndCardPlay);
			//currentPawn.GetComponent<Pawn>().RotationDisabled = true;
		}
	}

	private void TutorialStep5EndCardPlay(int unused) {
		SetupEndTurn ();
		ShowNotify ("Swoim pionkiem możesz zaatakować dopiero w nastepnej turze.\n" +
			"Aby zakończyć swoją turę wciśnji przycisk \nKONIEC TURY.");
	}

	private IEnumerator TutorialStep6() {
		GameplayComponent.ChangeEnemyPawnPos (AIPawnIDNext - 1, 3, 11);
		GameplayComponent.ConfirmEnemyPawnPos ("", AIPawnIDNext - 1, 3, 11);
		yield return new WaitForSeconds (1f);
		GameplayComponent.DoAttackImpl (11, 5, GamePlay.attackDirections.Down, 1);
		yield return new WaitForSeconds (1f);
		yield return new WaitForSeconds (1f);
		GameplayComponent.NewTurnAnimation.SetTrigger ("ShowNotification");
		GameplayComponent.EndTurnButton.ChangeButtonToMyTurn();
		GameplayComponent.EndTurnButton.interactable = false;
		GameplayComponent.Mana = 3;
		GameplayComponent.UpdateManaState ();
		yield return new WaitForSeconds (2f);
		GameplayComponent.Draw (MyStep6Card);
		GameplayComponent.HandComp.HideHand ();
		yield return new WaitForSeconds (3f);
		//GameplayComponent.HandComp.HideHand ();
		GameplayComponent.HandComp.DisableOtherCards (null);
		ShowItemCardHelp ();
	}

	private void ShowItemCardHelp() {
		HideNotify ();
		ItemCardHelp.enabled = true;
	}

	public void HideItemCardHelp() {
		ItemCardHelp.enabled = false;
		ShowNotify ("Daj broń swojemu pionkowi, zwiększy ona liczbę jego punktów ataku.");
		ShowCardToPlay ();
		if (currentWeapon != null) {
			currentWeapon.GetComponent<Pawn>().RegisterPlayConfirmCallback(TutorialStep6CardPlay);
			currentWeapon.GetComponent<Pawn>().OnApplyItemCallback = TutorialStep6WeaponApplied;
		}
	}

	private void TutorialStep6CardPlay(int unused) {
		ShowNotify ("Możesz obrócić broń zmieniając kierunek jej ataku, \nkliknij w broń aby zatwierdzić wybrany kierunek.");
		ShowRotateHelp (currentWeapon, "Kliknij w broń aby zatwierdzić wybrany kierunek.");
	}

	private void TutorialStep6WeaponApplied(int unused) {
		ShowNotify ("Klinknij swojego pionka, aby zobaczyć możliwe ruchy");
		Pawn pawnComp = currentPawn.GetComponent<Pawn> ();
		pawnComp.AttackRulesOverrideCallback = TutorialStep6PawnClicked;
		pawnComp.RegisterDeathCallback(TutorialStep6PawnDeath);
		//pawnComp.RegisterOnAttackCallback (TutorialStep3PawnAttaked);
		//pawnComp.OnDeselectCallback = TutorialStep3PawnUnselect;
		//pawnComp.RotationDisabled = true;
		pawnComp.EnablePawn ();
		pawnComp.ResetState ();
		ShowMark (currentPawn.transform, 5);
	}

	private void TutorialStep6PawnClicked(int unused) {
		currentPawn.GetComponent<Pawn> ().AttackRulesOverrideCallback = TutorialStep6PawnReset;
		ShowNotify ("Możesz też obrócić swojego pionka zmieniając kierunek jego ataku.");
		ShowRotateHelp(currentPawn, "Zaatakuj pionka przeciwnika.");
	}

	private void TutorialStep6PawnReset(int unused) {
		Pawn pawnComp = currentPawn.GetComponent<Pawn> ();
		pawnComp.AttackAlready = false;
		pawnComp.AttackOnly = false;
		pawnComp.ManaConsumed = false;
		pawnComp.RotationDisabled = false;
	}

	private void TutorialStep6PawnDeath(int unused) {
		SetupEndTurn ();
	}

	private IEnumerator TutorialStep7() {
		StartCoroutine (playCard (EnemyStep7Card, 17));
		yield return new WaitForSeconds (1f);
		GameplayComponent.NewTurnAnimation.SetTrigger ("ShowNotification");
		GameplayComponent.EndTurnButton.ChangeButtonToMyTurn();
		GameplayComponent.EndTurnButton.interactable = false;
		GameplayComponent.Mana = 4;
		GameplayComponent.UpdateManaState ();
		yield return new WaitForSeconds (2f);
		GameplayComponent.Draw (MyStep7Card);
		//GameplayComponent.HandComp.HideHand ();
		yield return new WaitForSeconds (3f);
		//GameplayComponent.HandComp.HideHand ();
		GameplayComponent.HandComp.DisableOtherCards (null);
		currentEnemyPawn = GameplayComponent.Board [17].Pawn.GetComponent<Pawn> ();
		currentEnemyPawn.RegisterDeathCallback (TutorialStep7PawnKilled);
		ShowNotify ("Zagraj kartę na planszę.");
		ShowCardToPlay ();
		if (currentPawn != null) {
			currentPawn.GetComponent<Pawn>().RegisterPlayConfirmCallback(TutorialStep7EndCardPlay);
			//currentPawn.GetComponent<Pawn>().RotationDisabled = true;
			currentPawn.GetComponent<DealDamageOnPlay> ().tutorialMode = true;
		}
	}

	private void TutorialStep7EndCardPlay(int unused) {
		ShowNotify ("Użyj zdolności swojego pionka.");
	}

	private void TutorialStep7PawnKilled(int unused) {
		//ShowNotify ("Użyj zdolności swojego pionka.");
		SetupEndTurn ();
	}

	private IEnumerator TutorialStep8() {
		StartCoroutine (playCard (EnemyStep8Card, 17));
		yield return new WaitForSeconds (1f);
		GameplayComponent.NewTurnAnimation.SetTrigger ("ShowNotification");
		GameplayComponent.EndTurnButton.ChangeButtonToMyTurn();
		GameplayComponent.EndTurnButton.interactable = false;
		GameplayComponent.Mana = 4;
		GameplayComponent.UpdateManaState ();
		yield return new WaitForSeconds (2f);
		GameplayComponent.Draw (MyStep8Card);
		//GameplayComponent.HandComp.HideHand ();
		yield return new WaitForSeconds (3f);
		//GameplayComponent.HandComp.HideHand ();
		GameplayComponent.HandComp.DisableOtherCards (null);
		ShowNotify ("Aby użyć karty efektu przeciągnij ją i upuść na planszy");
		ShowCardToPlay (11);
		if (currentWeapon != null) {
			currentWeapon.GetComponent<Pawn>().RegisterPlayConfirmCallback(TutorialStep8EndCardPlay);
		}
	}

	private void TutorialStep8EndCardPlay(int unused) {
		ShowNotify ("Wybierz cel efektu. U góry wyświetlany jest opis efektu. Klinknij swojego pionka.");
		Pawn pawnComp = currentPawn.GetComponent<Pawn> ();
		pawnComp.AttackRulesOverrideCallback = TutorialStep8PawnClicked;
		//pawnComp.RegisterOnAttackCallback (TutorialStep3PawnAttaked);
		pawnComp.OnDeselectCallback = TutorialStep8PawnUnClicked;
		pawnComp.RegisterOnMoveCallback (TutorialStep8PawnMoved);
		//pawnComp.RotationDisabled = true;
		GameplayComponent.RemovePawnFromPosisionImpl(10);
		pawnComp.EnablePawn ();
		pawnComp.ResetState ();
		ShowMark (currentPawn.transform, 5);
		Slide5.enabled = true;
	}

	private void TutorialStep8PawnClicked(int unused) {
		if (currentPawn.GetComponent<Pawn> ().boardPosisionIndex == 19) {
			//ShowNotify ("Zaatakuj wrogiego bohatera i wygraj gre.");
			TutorialStep8PawnMoved (19);
		} else {
			ShowNotify ("Przemieść tu pionka i obróć go w strone wrogiego bohatera.");
			currentPawn.GetComponent<Pawn> ().RotationDisabled = false;
			ShowMark (GameplayComponent.Board [19].BoardPiece.transform, 5);
			markObject.transform.localPosition = new Vector3 (0, 0.005f, 0.01f);
			markObject.transform.localScale = new Vector3 (0.005f, 0.005f, 0.005f);
		}
		Slide5.enabled = false;
	}

	private void TutorialStep8PawnUnClicked(int unused) {
		ShowNotify ("Klinknij swojego pionka, aby zobaczyć możliwe ruchy.");
		Pawn pawnComp = currentPawn.GetComponent<Pawn> ();
		pawnComp.AttackAlready = false;
		pawnComp.AttackOnly = false;
		pawnComp.ManaConsumed = false;
		pawnComp.RotationDisabled = false;
		ShowMark (currentPawn.transform, 5);
	}

	private void TutorialStep8PawnMoved(int idx) {
		if (idx == 19) {
			HideNotify ();
			//GameplayComponent.PutPawnOnPosisionImpl (fakePawn, 15);
			currentPawn.GetComponent<PolygonCollider2D> ().enabled = false;
			currentPawn.GetComponent<Pawn> ().AttackRulesOverrideCallback = TutorialStep8PawnReset;
			TutorialStep8PawnEndTutorial ();
		}
	}

	private void TutorialStep8PawnEndTutorial() {
		ShowNotify ("Zaatakuj wrogiego bohatera i wygraj gre.");
		GameplayComponent.enemyHero.GetComponent<Pawn> ().RegisterDeathCallback (TutorialStep8Won);;
		ShowMark (GameplayComponent.Board [22].BoardPiece.transform, 5);
		markObject.transform.localPosition = new Vector3 (0, 0, 0.01f);
		markObject.transform.localScale = new Vector3 (0.005f, 0.005f, 0.005f);
		TutorialStep8PawnReset(0);
	}

	private void TutorialStep8PawnReset(int unused) {
		Pawn pawnComp = currentPawn.GetComponent<Pawn> ();
		pawnComp.AttackAlready = false;
		pawnComp.AttackOnly = false;
		pawnComp.ManaConsumed = true;
		pawnComp.pawnState = Pawn.pawnStates.playable;
	}

	private void TutorialStep8Won(int unused) {
		HideNotify ();
	}

	#region UTILS
	private void ShowNextMessage(string msg) {
		NextMessageText.text = msg;
		Canvas.ForceUpdateCanvases ();
		NextMessageCanvas.enabled = true;
		NextMessageText.text = msg;
		Canvas.ForceUpdateCanvases ();
	}

	private void HideNextMessage() {
		NextMessageCanvas.enabled = false;
	}

	public void GotoNextMessage() {
		Slide1.enabled = false;
		//HideNextMessage ();
		if (NextMessageText.text.Equals("start")) {
			OriginalScale = GameplayComponent.myHero.transform.localScale;
			GameplayComponent.myHero.GetComponent<SmothTransform> ().SmoothTransformDisabled = false;
			GameplayComponent.myHero.GetComponent<SmothTransform> ().SmoothScaleTo (
				new Vector3 (OriginalScale.x + heroUpScale, OriginalScale.y + heroUpScale, OriginalScale.z + heroUpScale), 5);
			ShowNextMessage (MyHeroText);
		} else if (NextMessageText.text.Equals(MyHeroText)) {
			GameplayComponent.myHero.GetComponent<SmothTransform> ().SmoothScaleTo (
				OriginalScale, 5);
			OriginalScale = GameplayComponent.enemyHero.transform.localScale;
			GameplayComponent.enemyHero.GetComponent<SmothTransform> ().SmoothTransformDisabled = false;
			GameplayComponent.enemyHero.GetComponent<SmothTransform> ().SmoothScaleTo (
				new Vector3 (OriginalScale.x + heroUpScale, OriginalScale.y + heroUpScale, OriginalScale.z + heroUpScale), 5);
			ShowNextMessage (EnemyHeroText);
		} else if (NextMessageText.text.Equals(EnemyHeroText)) {
			GameplayComponent.enemyHero.GetComponent<SmothTransform> ().SmoothScaleTo (
				OriginalScale, 5);
			OriginalScale = GameplayComponent.ActualManaStateText.transform.parent.localScale;
			GameplayComponent.ActualManaStateText.transform.parent.GetComponent<SmothTransform> ().SmoothScaleTo (
				new Vector3 (OriginalScale.x + buttonUpScale, OriginalScale.y + buttonUpScale, OriginalScale.z + buttonUpScale), 5);
			ShowNextMessage (ActionPointsText);
		} else if (NextMessageText.text.Equals(ActionPointsText)) {
			GameplayComponent.ActualManaStateText.transform.parent.GetComponent<SmothTransform> ().SmoothScaleTo (
				OriginalScale, 5);
			GameplayComponent.EndTurnButton.SignalTurnReady ();
			ShowNextMessage (EndTurnString);
		} else if (NextMessageText.text.Equals(EndTurnString)) {
			GameplayComponent.EndTurnButton.ChangeButtonToMyTurn ();
			OriginalScale = GameplayComponent.HandComp.gameObject.transform.localScale;
			GameplayComponent.HandComp.gameObject.GetComponent<SmothTransform> ().SmoothScaleTo (
				new Vector3 (OriginalScale.x + handUpScale, OriginalScale.y + handUpScale, OriginalScale.z + handUpScale), 5);
			ShowNextMessage (MyCardString);
		} else if (NextMessageText.text.Equals(MyCardString)) {
			GameplayComponent.HandComp.gameObject.GetComponent<SmothTransform> ().SmoothScaleTo (
				OriginalScale, 5);
			OriginalScale = GameplayComponent.EnemyHandComp.gameObject.transform.localScale;
			GameplayComponent.EnemyHandComp.gameObject.GetComponent<SmothTransform> ().SmoothScaleTo (
				new Vector3 (OriginalScale.x + handUpScale, OriginalScale.y + handUpScale, OriginalScale.z + handUpScale), 5);
			ShowNextMessage (EnemyCardString);
		} else if (NextMessageText.text.Equals(EnemyCardString)) {
			GameplayComponent.EnemyHandComp.gameObject.GetComponent<SmothTransform> ().SmoothScaleTo (
				OriginalScale, 5);
			OriginalScale = GameplayComponent.myDeck.transform.localScale;
			GameplayComponent.myDeck.GetComponent<SmothTransform> ().SmoothScaleTo (
				new Vector3 (OriginalScale.x + deckUpScale, OriginalScale.y + deckUpScale, OriginalScale.z + deckUpScale), 5);
			ShowNextMessage (MyDeckString);
		} else if (NextMessageText.text.Equals(MyDeckString)) {
			GameplayComponent.myDeck.GetComponent<SmothTransform> ().SmoothScaleTo (
				OriginalScale, 5);
			OriginalScale = GameplayComponent.enemyDeck.transform.localScale;
			GameplayComponent.enemyDeck.GetComponent<SmothTransform> ().SmoothScaleTo (
				new Vector3 (OriginalScale.x + deckUpScale, OriginalScale.y + deckUpScale, OriginalScale.z + deckUpScale), 5);
			ShowNextMessage (EnemyDeckString);
		} else if (NextMessageText.text.Equals(EnemyDeckString)) {
			GameplayComponent.enemyDeck.GetComponent<SmothTransform> ().SmoothScaleTo (
				OriginalScale, 5);
			HideNextMessage ();
			CardHelp.enabled = true;
		} else {
			HideNextMessage ();
			CardHelp.enabled = true;
		}
		NextMessageText.RecalculateClipping ();
		Canvas.ForceUpdateCanvases ();
	}

	private void ShowRotateHelp(GameObject RotObj, string EndMessage) {
		StartCoroutine (ShowRotateHelpTask(RotObj, EndMessage));
	}

	private IEnumerator ShowRotateHelpTask(GameObject RotObj, string endMessage) {
		Debug.Log ("Show rotate help");
		if (RotObj != null) {
			GameObject MarkObject = (GameObject)Instantiate (AttackMarkPrefab);
			Pawn pawnComp = RotObj.GetComponent<Pawn> ();
			MarkObject.transform.SetParent (pawnComp.PawnConfigPosHandler);
			MarkObject.transform.localPosition = new Vector3 (0, -9 -2);
			MarkObject.transform.localScale = new Vector3 (4, 4, 4);
			yield return new WaitForSeconds (0.5f);
			while (!Input.GetMouseButton (0)) {
				if (pawnComp == null) {
					break;
				}
				pawnComp.SetPawnRotation (1);
				for (int i = 0; i < 10; i++) {
					if (Input.GetMouseButton (0)) {
						break;
					}
					yield return new WaitForSeconds (0.1f);
				}
				if (pawnComp == null) {
					break;
				}
				pawnComp.SetPawnRotation (0);
				for (int i = 0; i < 10; i++) {
					if (Input.GetMouseButton (0)) {
						break;
					}
					yield return new WaitForSeconds (0.1f);
				}
			}
			Destroy(MarkObject);
		}
		ShowNotify (endMessage);
		if (RotObj == currentPawn) {
			if (GameplayComponent.Board [11].Pawn != null) {
				ShowMark (GameplayComponent.Board [11].Pawn.transform, 5);
			}
		}
		yield return null;
	}

	private void ShowCardToPlay() {
		ShowCardToPlay (5);
	}

	private void ShowCardToPlay(int targetIdx) {
		if (GameplayComponent.HandComp.HandCards [0] != null) {
			GameObject cardInHand = GameplayComponent.HandComp.HandCards [0].gameObject;
			Transform pawnTransform = cardInHand.transform.Find ("Pawn");
			if (pawnTransform.GetComponent<Pawn> ().CardType == CardsBase.CardTypesEnum.Pawn) {
				currentPawn = pawnTransform.gameObject;
			} else {// if (pawnTransform.GetComponent<Pawn> ().CardType == CardsBase.CardTypesEnum.Weapon) {
				currentWeapon = pawnTransform.gameObject;
			}
			ShowMark (cardInHand.transform, 14);
			//StartCoroutine (ShowMoveMark (cardInHand, GameplayComponent.Board [targetIdx].BoardPiece, 100));
			GameplayComponent.HandComp.EnableOtherCards ();
			GameplayComponent.myTurn = true;
		} else {
			Debug.LogError ("No card in hand");
		}
	}

	private void ShowNotify(string message) {
		NotifyCanvas.enabled = true;
		NotifyText.text = message;
	}

	private void HideNotify() {
		NotifyCanvas.enabled = false;
	}

	public void EndTurn() {
		GameplayComponent.EndTurnButton.interactable = false;
		HideNotify ();
		if (markObject) {
			Destroy (markObject);
		}
		if (TutorialCurrentStep == TutorialSteps.step1) {
			TutorialCurrentStep = TutorialSteps.step2;
			StartCoroutine (TutorialStep2 ());
		} else if (TutorialCurrentStep == TutorialSteps.step2) {
			TutorialCurrentStep = TutorialSteps.step3;
			StartCoroutine (TutorialStep3 ());
		} else if (TutorialCurrentStep == TutorialSteps.step3) {
			TutorialCurrentStep = TutorialSteps.step4;
			StartCoroutine (TutorialStep4 ());
		} else if (TutorialCurrentStep == TutorialSteps.step4) {
			TutorialCurrentStep = TutorialSteps.step5;
			StartCoroutine (TutorialStep5 ());
		} else if (TutorialCurrentStep == TutorialSteps.step5) {
			TutorialCurrentStep = TutorialSteps.step6;
			StartCoroutine (TutorialStep6());
		} else if (TutorialCurrentStep == TutorialSteps.step6) {
			TutorialCurrentStep = TutorialSteps.step7;
			StartCoroutine (TutorialStep7());
		} else if (TutorialCurrentStep == TutorialSteps.step7) {
			TutorialCurrentStep = TutorialSteps.step8;
			StartCoroutine (TutorialStep8());
		}
		//GameplayComponent.EnemyCardDraw ();
	}

	private void SetupEndTurn() {
		if (currentPawn != null) {
			currentPawn.GetComponent<Pawn> ().ConfirmPosition ();
			currentPawn.GetComponent<Pawn> ().SetAttackOnlyMode ();
		}
		//GameplayComponent.ShowTargetNotification ("Aby zakończyć swoją turę wciśnji przycisk 'KONIEC TURY'");
		ShowNotify ("Aby zakończyć swoją turę wciśnji przycisk\nKONIEC TURY.");
		//markObject = (GameObject)Instantiate(MarkPrefab);
		ShowMark (Camera.main.transform, 5);
		Vector3 newPos = GameplayComponent.EndTurnButton.transform.position;
		markObject.transform.localScale = new Vector3 (1, 1, 1);
		markObject.transform.localRotation = new Quaternion (0, 0, 0, 0);
		newPos.y += 1;
		markObject.transform.position = newPos;
		GameplayComponent.EndTurnButton.interactable = true;
	}

	private void ShowMark(Transform pos, float high) {
		if (markObject == null) {
			markObject = (GameObject)Instantiate (MarkPrefab);
		}
		markObject.transform.SetParent (pos);
		markObject.transform.localPosition = new Vector3 (0, high, 0);
		markObject.transform.localScale = new Vector3 (3, 3, 3);
	}

	private IEnumerator ShowMoveMark(GameObject startPosOb, GameObject endPosOb, int count) {
		attackMarkObject = (GameObject)Instantiate(AttackMarkPrefab);
		Transform start = startPosOb.transform;
		Transform end = endPosOb.transform;
		Vector3 endPos = end.localPosition;
		endPos.z += -1;
		attackMarkObject.transform.SetParent (GameplayComponent.transform);

		for (int i = 0; i <= count; i++) {
			if (startPosOb == null || attackMarkObject == null) {
				break;
			}
			attackMarkObject.transform.position = start.position;
			attackMarkObject.GetComponent<SmothTransform> ().SmothTransformTo (endPos, 5);
			yield return new WaitForSeconds (1f);
		}
		Destroy (attackMarkObject);
	}

	private IEnumerator ShowMoveMark(GameObject startPosOb, GameObject midPosOb, GameObject endPosOb, int count) {
		attackMarkObject = (GameObject)Instantiate(AttackMarkPrefab);
		Transform start = startPosOb.transform;
		Transform end = endPosOb.transform;
		Vector3 endPos = end.localPosition;
		Vector3 midPos = midPosOb.transform.localPosition;
		endPos.z += -1;
		midPos.z += -1;
		attackMarkObject.transform.SetParent (GameplayComponent.transform);

		for (int i = 0; i <= count; i++) {
			if (startPosOb == null || attackMarkObject == null) {
				break;
			}
			attackMarkObject.transform.position = start.position;
			attackMarkObject.GetComponent<SmothTransform> ().SmothTransformTo (midPos, 5);
			yield return new WaitForSeconds (1f);
			attackMarkObject.GetComponent<SmothTransform> ().SmothTransformTo (endPos, 5);
			yield return new WaitForSeconds (1f);
		}
		Destroy (attackMarkObject);
	}

	private IEnumerator playCard(string cardToPlayName, int boardPosIndex) {
		yield return new WaitForSeconds (1f);
		GameplayComponent.PutEnemyPawnOnBoard (cardToPlayName, AIPawnIDNext, 3, 0, boardPosIndex);
		yield return new WaitForSeconds (1f);
		GameplayComponent.ConfirmEnemyPawnPos (cardToPlayName, AIPawnIDNext, 3, boardPosIndex);
		AIPawnIDNext++;
	}
	#endregion
}
