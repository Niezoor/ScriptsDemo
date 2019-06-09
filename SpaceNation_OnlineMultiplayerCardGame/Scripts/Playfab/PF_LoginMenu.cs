using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.SceneManagement;

public class PF_LoginMenu : MonoBehaviour {
	[Header("Settings")]
	public bool AutoLogin = true;
	public LoginLogoAnimation LogoAnim;

	[Header("Menu Canvases")]
	public Canvas LoginCanvas;
	public Canvas RegisterCanvas;
	public Canvas ErrorPOPUPCanvas;
	public Canvas LoadingCanvas;
	public Canvas HaveAccountCanvas;
	public Canvas RegisterEmailCanvas;
	public Canvas SetNameCanvas;
	public Canvas LoginMethodCanvas;
	public Canvas LocalAccountExistsCanvas;
	public GameObject RODO_ConfirmPrefab;
	private GameObject RODO_Confirm;
	private Canvas CurrentCanvas;

	[Header("Login Input fields")]
	public InputField LoginUserName;
	public InputField LoginPassword;

	/*[Header("Register Input fields")]
	public InputField RegisterUserName;
	public InputField RegisterEmail;
	public InputField RegisterPassword;
	public InputField RegisterConfirmPassword;
	public InputField RegisterPlayerName;*/

	[Header("Error text")]
	public Text ErrorTitle;
	public Text ErrorString;

	public string SceneToLoadAfterLogin;
	public GameObject localPlayerDataObject;

	public string UserName;
	public string Password;

	public string Email;
	public string ConfirmnEmail;
	public string ConfirmnPassword;

	public string PlayerName;

	private string _playFabPlayerIdCache;
	private bool proceed = false;
	public bool DontUpdateDisplayName = false;
	public bool NewAccountCreation = false;
	public bool OnlyUpdateName = false;
	private LoginResult loginResultCache;
	private string AuthMethod = "dev";//email
	// Use this for initialization
	void Start () {
		LoadingCanvas.enabled = false;
		DontDestroyOnLoad (localPlayerDataObject);
		ZPlayerPrefs.Initialize("spacequak132", "spacesaltysalt132");
		LoginCanvas.enabled = false;
		RegisterCanvas.enabled = false;
		HaveAccountCanvas.enabled = false;
		RegisterEmailCanvas.enabled = false;
		CurrentCanvas = LoginCanvas;
		Email = "";
		UserName = "";
		Password = "";
		LogoAnim.JumpToSmall (true);
		if (AutoLogin && !PlayerLogout ()) {
			Email = ZPlayerPrefs.GetString ("username");
			Password = ZPlayerPrefs.GetString ("passd");
			LogoAnim.JumpToSmall (false);
			if ((Email.Length > 0) && (Password.Length > 0)) {
				LogIn ();
			} else {
				LogInWithDeviceID (false);
			}
		} else {
			ShowCanvas (HaveAccountCanvas);
		}
	}

	private void ShowCanvas(Canvas canv) {
		if (CurrentCanvas != null) {
			CurrentCanvas.enabled = false;
		}
		if (canv != null) {
			canv.enabled = true;
		}
		CurrentCanvas = canv; 
	}

	public void ExitGame() {
		Application.Quit();
	}

	private bool PlayerLogout() {
		bool rv = false;
		PlayerData data = new PlayerData ();
		if (localPlayerDataObject.GetComponent<LocalPlayerData> ().LoadData (data)) {
			if (data.logout) {
				rv = true;
			}
		} else {
			rv = true;
		}
		return rv;
	}

	#region DialogsFlow
	public void IHaveAccount() {
		ShowCanvas(LoginMethodCanvas);
	}

	public void ChooseLoginWithEmail() {
		ShowCanvas(LoginCanvas);
	}

	public void ChooseLoginWithoutEmail() {
		LogInWithDeviceID (false);
	}

	public void IDontHaveAccount() {
		RODO_Confirm = Instantiate (RODO_ConfirmPrefab, Camera.main.transform);
		RODO_Confirm.GetComponent<Canvas> ().worldCamera = Camera.main;
		RODO_Confirm.GetComponent<Canvas> ().planeDistance = 20;
		RODO_Confirm.GetComponent<RODO_Info> ().ConfirmBtn.onClick.AddListener (RODO_Confirmed);
	}

	public void RODO_Confirmed() {
		Destroy (RODO_Confirm);
		LogInWithDeviceID (true);
	}

	public void IWantToRegisterEMail() {
		NewAccountCreation = false;
		ShowCanvas (RegisterCanvas);
	}

	public void IDONTWantToRegisterEMail() {
		ShowLoading ();
		RequestPhotonToken (loginResultCache);
	}

	private void NameUpdatedWithCustomID(UpdateUserTitleDisplayNameResult result)
	{
		HideLoading ();
		ShowCanvas (RegisterEmailCanvas);
	}

	private void NameUpdatedForNewAccount(UpdateUserTitleDisplayNameResult result)
	{
		RegisterSuccess (null);
	}

	private void NameUpdatedOnly(UpdateUserTitleDisplayNameResult result)
	{
		RequestPhotonToken (loginResultCache);
	}

	public void SetDisplayName() {
		if (PlayerName != null && PlayerName.Length > 0) {
			ShowLoading();
			UpdateUserTitleDisplayNameRequest request = new UpdateUserTitleDisplayNameRequest ();
			if (PlayerName.Length > 0) {
				request.DisplayName = PlayerName;
			} else {
				request.DisplayName = UserName;
			}
			if (NewAccountCreation) {
				PlayFabClientAPI.UpdateUserTitleDisplayName (request, NameUpdatedForNewAccount, OnPlayFabError);
			} else if (OnlyUpdateName) {
				PlayFabClientAPI.UpdateUserTitleDisplayName (request, NameUpdatedOnly, OnPlayFabError);
			} else {
				PlayFabClientAPI.UpdateUserTitleDisplayName (request, NameUpdatedWithCustomID, OnPlayFabError);
			}
		}
	}

	public void StartMainDialogAgain() {
		NewAccountCreation = false;
		ShowCanvas (HaveAccountCanvas);
	}

	#endregion

	#region InputData

	public void SetInputLogin(string userName) {
		UserName = userName;
	}

	public void SetPassword(string pass) {
		Password = pass;
	}

	public void SetEmail(string mail) {
		Email = mail;
	}

	public void SetConfirmEmail(string mail) {
		ConfirmnEmail = mail;
	}

	public void SetConfirmPassword(string pass) {
		ConfirmnPassword = pass;
	}

	public void SetDisplayPlayerName(string name) {
		PlayerName = name;
	}

	#endregion

	#region CreateAccount
	public void ShowRegisterMenu() {
		if (!proceed) {
			//LoginCanvas.enabled = false;
			//RegisterCanvas.enabled = true;
			//CurrentCanvas = RegisterCanvas;
			ShowCanvas(HaveAccountCanvas);
		}
	}

	public void ShowRegisterNewAccount() {
		NewAccountCreation = true;
		ShowCanvas (RegisterCanvas);
	}

	public void RegisterAccount() {
		if (NewAccountCreation) {
			RegisterNewAccount ();
		} else {
			UpdateLocalAccount ();
		}
	}

	public void RegisterNewAccount() {
		Debug.Log ("Register new account");
		if (!proceed && validateData ()) {
			Debug.Log ("Register player");
			proceed = true;
			//LoadingCanvas.enabled = true;
			ShowLoading ();
			RegisterCanvas.enabled = false;
			RegisterPlayFabUserRequest request = new RegisterPlayFabUserRequest();
			request.TitleId = PlayFabSettings.TitleId;
			request.Email = Email;
			request.Password = Password;
			//UserName = request.Username;
			request.RequireBothUsernameAndEmail = false;
			//Debug.Log("TitleId : "+request.TitleId);
			PlayFabClientAPI.RegisterPlayFabUser(request,UpdatePlayerName,OnPlayFabError);
		} else {
			ShowErrorPOPUP ("Błąd", "Sprawdź czy poprawnie podałeś e-mail i hasło");
		}
	}

	private void UpdatePlayerName(RegisterPlayFabUserResult result) {
		proceed = false;
		HideLoading ();
		ShowCanvas (SetNameCanvas);
	}

	public void UpdateLocalAccount() {
		Debug.Log ("Register local account");
		if (!proceed && validateData ()) {
			Debug.Log ("Register player");
			proceed = true;
			//LoadingCanvas.enabled = true;
			ShowLoading ();
			RegisterCanvas.enabled = false;
			//RegisterPlayFabUserRequest request = new RegisterPlayFabUserRequest();
			AddUsernamePasswordRequest request = new AddUsernamePasswordRequest ();
			//request.TitleId = PlayFabSettings.TitleId;
			request.Username = _playFabPlayerIdCache;//UserName;//beetle.gamedev@gmail.com kamcioniezo@gmail.com
			request.Email = Email;
			request.Password = Password;
			UserName = request.Username;
			//request.RequireBothUsernameAndEmail = false;
			//Debug.Log("TitleId : "+request.TitleId);
			//PlayFabClientAPI.RegisterPlayFabUser(request,UpdatePlayerName,OnPlayFabError);
			PlayFabClientAPI.AddUsernamePassword (request, RegisterSuccess, OnPlayFabError);
		} else {
			ShowErrorPOPUP ("Błąd", "Sprawdź czy poprawnie podałeś e-mail i hasło");
		}
	}

	public void RegisterSuccess(AddUsernamePasswordResult result) {
		Debug.Log ("Register Sucess!");
		proceed = false;
		/*UpdateUserTitleDisplayNameRequest request = new UpdateUserTitleDisplayNameRequest ();
		if (PlayerName.Length > 0) {
			request.DisplayName = PlayerName;
		} else {
			request.DisplayName = UserName;
		}
		//use link PlayFabClientAPI.UpdateUserTitleDisplayName (request, LinkAccount, OnPlayFabError);
		PlayFabClientAPI.UpdateUserTitleDisplayName (request, NameUpdated, OnPlayFabError);
		//LoadingCanvas.enabled = true;*/
		NameUpdated (null);
		ShowLoading ();
	}

	private void LinkAccount(UpdateUserTitleDisplayNameResult result) {
		Debug.Log ("Create account success - link with device ID");
		#if UNITY_ANDROID
		LinkAndroidID();
		#else
		#if UNITY_IOS
		LinkIOSID();
		#else
		LinkCustomID();
		#endif
		#endif
	}

	private void LinkAndroidID() {
		LinkAndroidDeviceIDRequest request = new LinkAndroidDeviceIDRequest ();
		request.AndroidDeviceId = SystemInfo.deviceUniqueIdentifier;

		PlayFabClientAPI.LinkAndroidDeviceID (request, AndroidLinked, OnPlayFabError);
	}

	private void AndroidLinked(LinkAndroidDeviceIDResult result) {
		NameUpdated (null);
	}

	private void LinkIOSID() {
		LinkIOSDeviceIDRequest request = new LinkIOSDeviceIDRequest ();
		request.DeviceId = SystemInfo.deviceUniqueIdentifier;

		PlayFabClientAPI.LinkIOSDeviceID (request, IOSLinked, OnPlayFabError);
	}

	private void IOSLinked(LinkIOSDeviceIDResult result) {
		NameUpdated (null);
	}

	private void LinkCustomID() {
		LinkCustomIDRequest request = new LinkCustomIDRequest ();
		request.CustomId = SystemInfo.deviceUniqueIdentifier;

		PlayFabClientAPI.LinkCustomID (request, CustomLinked, OnPlayFabError);
	}

	private void CustomLinked(LinkCustomIDResult result) {
		NameUpdated (null);
	}

	private void NameUpdated(UpdateUserTitleDisplayNameResult result)
	{
		//LoadingCanvas.enabled = false;
		HideLoading ();
		if (result != null) {
			Debug.Log ("Name " + result.DisplayName + " updated");
		}
		ShowErrorPOPUP ("Konto utworzone!", "Gratulacje, teraz możesz zalogować się używając podanych danych");
		proceed = false;
		//ExitRegisterMenu ();
		ShowCanvas (LoginCanvas);
	}

	private bool validateData() {
		if ((Password.Length > 0) && (ConfirmnPassword.Length > 0)) {
			return ((Password == ConfirmnPassword) && (Email == ConfirmnEmail)); 
		} else {
			return false;
		}
	}
	#endregion

	#region LoginWithDeviceID
	private void LogInWithDeviceID(bool create) {
		#if UNITY_ANDROID
		LogInWithAndroidID(create);
		#else
		#if UNITY_IOS
		LogInWithIOSID(create);
		#else
		LogInWithCustomID(create);
		#endif
		#endif
	}

	private void LogInWithCustomID(bool create) {
		Debug.Log ("Login with Custom ID");
		if (!proceed) {
			proceed = true;
			//LoadingCanvas.enabled = true;
			ShowLoading ();
			ShowCanvas (null);
			LoginWithCustomIDRequest request = new LoginWithCustomIDRequest ();
			GetPlayerCombinedInfoRequestParams playerRequest = new GetPlayerCombinedInfoRequestParams ();
			playerRequest.GetPlayerProfile = true;
			request.InfoRequestParameters = playerRequest;
			request.CreateAccount = create;
			request.CustomId = SystemInfo.deviceUniqueIdentifier;
			request.TitleId = PlayFabSettings.TitleId;
			if (create) {
				PlayFabClientAPI.LoginWithCustomID (request, LoginWithAccountCreated, OnPlayFabError);
			} else {
				PlayFabClientAPI.LoginWithCustomID (request, OnLoginSuccess, OnPlayFabError);
			}
		}
	}

	private void LogInWithAndroidID(bool create) {
		Debug.Log ("Login with Adroid ID");
		if (!proceed) {
			proceed = true;
			//LoadingCanvas.enabled = true;
			ShowLoading ();
			ShowCanvas (null);
			LoginWithAndroidDeviceIDRequest request = new LoginWithAndroidDeviceIDRequest ();
			GetPlayerCombinedInfoRequestParams playerRequest = new GetPlayerCombinedInfoRequestParams ();
			playerRequest.GetPlayerProfile = true;
			request.InfoRequestParameters = playerRequest;
			request.CreateAccount = create;
			request.AndroidDeviceId = SystemInfo.deviceUniqueIdentifier;
			request.TitleId = PlayFabSettings.TitleId;
			//add to request OS and model
			if (create) {
				PlayFabClientAPI.LoginWithAndroidDeviceID (request, LoginWithAccountCreated, OnPlayFabError);
			} else {
				PlayFabClientAPI.LoginWithAndroidDeviceID (request, OnLoginSuccess, OnPlayFabError);
			}
		}
	}

	private void LogInWithIOSID(bool create) {
		Debug.Log ("Login with IOS ID");
		if (!proceed) {
			proceed = true;
			//LoadingCanvas.enabled = true;
			ShowLoading ();
			ShowCanvas (null);
			LoginWithIOSDeviceIDRequest request = new LoginWithIOSDeviceIDRequest ();
			GetPlayerCombinedInfoRequestParams playerRequest = new GetPlayerCombinedInfoRequestParams ();
			playerRequest.GetPlayerProfile = true;
			request.InfoRequestParameters = playerRequest;
			request.CreateAccount = create;
			request.DeviceId = SystemInfo.deviceUniqueIdentifier;
			request.TitleId = PlayFabSettings.TitleId;
			//add to request OS and model
			if (create) {
				PlayFabClientAPI.LoginWithIOSDeviceID (request, LoginWithAccountCreated, OnPlayFabError);
			} else {
				PlayFabClientAPI.LoginWithIOSDeviceID (request, OnLoginSuccess, OnPlayFabError);
			}
		}
	}

	private void LoginWithAccountCreated(LoginResult obj) {
		proceed = false;
		loginResultCache = obj;
		_playFabPlayerIdCache = obj.PlayFabId;
		HideLoading ();
		if (!obj.NewlyCreated) {
			DontUpdateDisplayName = true;
			//PlayerName = obj.InfoResultPayload.PlayerProfile.DisplayName;
			Debug.Log (" Login with device ID - name:" + PlayerName);
			ShowCanvas (LocalAccountExistsCanvas);
		} else {
			Debug.Log (" Register with device ID - name:" + PlayerName);
			ShowCanvas (SetNameCanvas);
		}
		//LoadingCanvas.enabled = false;
	}

	#endregion

	#region LoginWithPhoton
	public void LogIn()  {
		if (!proceed) {
			if (Email.Length > 0 && Password.Length > 0) {
				proceed = true;
				//LoadingCanvas.enabled = true;
				ShowLoading ();
				LoginCanvas.enabled = false;
				LoginWithEmailAddressRequest request = new LoginWithEmailAddressRequest ();
				GetPlayerCombinedInfoRequestParams playerRequest = new GetPlayerCombinedInfoRequestParams ();
				playerRequest.GetPlayerProfile = true;
				request.InfoRequestParameters = playerRequest;
				request.Email = Email;
				request.Password = Password;
				request.TitleId = PlayFabSettings.TitleId;
				PlayFabClientAPI.LoginWithEmailAddress (request, OnLoginSuccess, OnPlayFabError);
				AuthMethod = "email";
			}
		}
	}

	private void OnLoginSuccess(LoginResult obj) {
		Debug.Log (" Login with email - new account?:" + obj.NewlyCreated);
		proceed = false;
		_playFabPlayerIdCache = obj.PlayFabId;
		loginResultCache = obj;
		if (!obj.NewlyCreated) {
			if (obj != null && obj.InfoResultPayload != null && obj.InfoResultPayload.PlayerProfile != null) {
				PlayerName = obj.InfoResultPayload.PlayerProfile.DisplayName;
				if (PlayerName == null || PlayerName.Length == 0) {
					NewAccountCreation = false;
					OnlyUpdateName = true;
					ShowCanvas (SetNameCanvas);
					HideLoading ();
					return;
				}
			} else {
				NewAccountCreation = false;
				OnlyUpdateName = true;
				ShowCanvas (SetNameCanvas);
				HideLoading ();
				return;
			}
			Debug.Log (" Login with email - name:" + PlayerName);
		}
		RequestPhotonToken(obj);
	}

	private void RequestPhotonToken(LoginResult obj) {
		Debug.Log("PlayFab authenticated. Requesting photon token...");

		//We can player PlayFabId. This will come in handy during next step
		_playFabPlayerIdCache = obj.PlayFabId;

		PlayFabClientAPI.GetPhotonAuthenticationToken(new GetPhotonAuthenticationTokenRequest()
			{
				PhotonApplicationId = PhotonNetwork.PhotonServerSettings.AppID
			}, AuthenticateWithPhoton, OnPlayFabError);
	}

	private void AuthenticateWithPhoton(GetPhotonAuthenticationTokenResult obj) {
		string gamever = MainMenu.GameVersion;
		gamever = gamever.Replace (".", "_");
		gamever = gamever.Replace (" ", "_");
		Debug.Log ("Photon token acquired: " + obj.PhotonCustomAuthenticationToken);
		Debug.Log ("Authentication complete playfabID " + _playFabPlayerIdCache);

		//We set AuthType to custom, meaning we bring our own, PlayFab authentication procedure.
		var customAuth = new AuthenticationValues { AuthType = CustomAuthenticationType.Custom };

		//We add "username" parameter. Do not let it confuse you: PlayFab is expecting this parameter to contain player PlayFab ID (!) and not username.
		customAuth.AddAuthParameter("username", _playFabPlayerIdCache);    // expected by PlayFab custom auth service
		customAuth.UserId = PlayerName;
		//We add "token" parameter. PlayFab expects it to contain Photon Authentication Token issues to your during previous step.
		customAuth.AddAuthParameter("token", obj.PhotonCustomAuthenticationToken);

		//We finally tell Photon to use this authentication parameters throughout the entire application.
		PhotonNetwork.AuthValues = customAuth;

		ZPlayerPrefs.SetString ("playfabId", _playFabPlayerIdCache);
		ZPlayerPrefs.SetString ("token", obj.PhotonCustomAuthenticationToken);
		ZPlayerPrefs.SetString ("username", Email);
		ZPlayerPrefs.SetString ("passd", Password);

		PlayerData data = new PlayerData ();
		localPlayerDataObject.GetComponent<LocalPlayerData> ().LoadData (data);
		data.name = Email;
		data.logout = false;
		data.auth = AuthMethod;
		localPlayerDataObject.GetComponent<LocalPlayerData> ().SaveData (data);

		PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
			{
				FunctionName = "ClientLogin",
				FunctionParameter = new {
					game_version = gamever
				},
				GeneratePlayStreamEvent = true,
			}, OnCloudScriptSuccess, OnCloudScriptFail
		);
	}

	private void OnCloudScriptSuccess(ExecuteCloudScriptResult result) {
		Debug.Log("Checked initial inventory");
		LoadNextScene();
	}

	private void OnCloudScriptFail(PlayFabError obj) {
		Debug.LogWarning("Check initial fail");
		LoadNextScene();
	}

	private void LoadNextScene() {
		SceneManager.LoadSceneAsync (SceneToLoadAfterLogin);
	}

	#endregion

	private void ShowLoading() {
		CurrentCanvas.enabled = false;
		LoadingCanvas.enabled = true;
		LogoAnim.AnimToBig ();
	}

	private void HideLoading() {
		LoadingCanvas.enabled = false;
		LogoAnim.AnimToSmall ();
	}

	#region ErrorShowing
	private void OnPlayFabError(PlayFabError obj) {
		proceed = false;
		ShowErrorPOPUP ("ERROR!!!", obj.ErrorMessage);
	}

	private void ShowErrorPOPUP(string title, string ErrorText) {
		ErrorPOPUPCanvas.enabled = true;
		ErrorString.text = ErrorText;
		ErrorTitle.text = title;
		HideLoading ();//LoadingCanvas.enabled = false;
	}

	public void HideErrorPOPUP() {
		ErrorPOPUPCanvas.enabled = false;
		if (CurrentCanvas != null) {
			//if (CurrentCanvas == RegisterEmailCanvas) {
			//	CurrentCanvas = SetNameCanvas;
			//}
			CurrentCanvas.enabled = true;
		} else {
			ShowCanvas (HaveAccountCanvas);
		}
	}
	#endregion
}
