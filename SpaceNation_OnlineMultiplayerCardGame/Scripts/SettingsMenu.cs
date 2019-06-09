using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.UI;
using Honeti;

public class SettingsMenu : MonoBehaviour {
	public string DataPath = "/gamedatalocal2.dat";
	public Canvas MenuCanvas;
	public AudioSource audioManager;
	public MainMenu MainMenuComponent;
	public Slider MusicVolSlider;
	public Slider GlobalVolSlider;
	public Dropdown QualityDropDown;
	public Dropdown LanguageDropDown;
	public Toggle DebugScreen;
	public GameObject DebugScreenPrefab;
	private GameObject DebugScreenObject;

	void Awake() {
		Application.targetFrameRate = 60;
	}

	// Use this for initialization
	void Start () {
		if (audioManager == null) {
			audioManager = GameObject.Find("AudioManager").GetComponent<AudioSource>();
		}
		if (MainMenuComponent == null && GameObject.Find("MainMenu-Canvas") != null) {
			MainMenuComponent = GameObject.Find("MainMenu-Canvas").GetComponent<MainMenu>();
		}
		MenuCanvas.worldCamera = Camera.main;
		LoadData ();
		SaveData ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SaveData() {
		BinaryFormatter BFormatter = new BinaryFormatter ();
		FileStream file = File.Create (Application.persistentDataPath + DataPath);

		SettingsData data = new SettingsData();
		data.music_volume = MusicVolSlider.value;
		data.global_volume = GlobalVolSlider.value;
		data.quality = QualitySettings.GetQualityLevel ();
		data.language = LanguageDropDown.value;
		if (DebugScreenObject != null) {
			data.debug_screen = true;
		} else {
			data.debug_screen = false;
		}

		BFormatter.Serialize (file, data);
		file.Close ();
		if (MainMenuComponent != null) {
			MainMenuComponent.exitSettingsMenu ();
		} else {
			CloseMenu ();
		}
	}

	public void LoadData() {
		if (File.Exists (Application.persistentDataPath + DataPath)) {
			BinaryFormatter BFormatter = new BinaryFormatter ();
			FileStream file = File.Open (Application.persistentDataPath + DataPath, FileMode.Open);
			if (file.Length > 0) {
				SettingsData data = (SettingsData)BFormatter.Deserialize (file);

				MusicVolSlider.value = data.music_volume;
				GlobalVolSlider.value = data.global_volume;
				ChangeMusicVolume (data.music_volume);
				ChangeVolume (data.global_volume);
				QualityDropDown.value = data.quality;
				SetLanguage (data.language);
				LanguageDropDown.value = data.language;
				DebugScreen.isOn = data.debug_screen;
				SetDebugScreen (data.debug_screen);
				//SetQuality (data.quality);
				BFormatter.Serialize (file, data);
			}
			file.Close ();
		}
	}

	private void OpenDebugScreen() {
		Debug.Log ("Open debug scren");
		if (DebugScreenPrefab != null && DebugScreenObject == null) {
			DebugScreenObject = Instantiate (DebugScreenPrefab, Camera.main.transform);
			DebugScreenObject.GetComponent<Canvas> ().worldCamera = Camera.main;
			DebugScreenObject.GetComponent<Canvas> ().planeDistance = 7;
		}
	}

	private void CloseDebugScreen() {
		Debug.Log ("Close debug scren");
		if (DebugScreenObject != null) {
			Destroy (DebugScreenObject);
		}
	}

	public void OpenMenu() {
		MenuCanvas.enabled = true;
	}

	public void CloseMenu() {
		MenuCanvas.enabled = false;
	}

	public void ChangeMusicVolume(float volumeValue) {
		audioManager.volume = volumeValue;
	}

	public void ChangeVolume(float volumeValue) {
		AudioListener.volume = volumeValue;
	}

	public void SetQuality(int quality) {
		Debug.Log ("Set quality level:" + quality);
		QualitySettings.SetQualityLevel (quality);
	}

	public void SetLanguage(int lang) {
		Debug.Log ("Set language:" + lang);
		/* do not use it yet,I18N need to be shared bettwen scenes
		if (lang == 0) {
			I18N.instance.setLanguage (LanguageCode.PL);
		} else if (lang == 1) {
			I18N.instance.setLanguage (LanguageCode.EN);
		} else {
			I18N.instance.setLanguage (LanguageCode.EN);
		}*/
	}

	public void SetDebugScreen(bool toSet) {
		if (toSet) {
			OpenDebugScreen ();
		} else {
			CloseDebugScreen ();
		}
	}
}

[Serializable]
class SettingsData
{
	public float music_volume;
	public float global_volume;
	public int quality;
	public int language;
	public bool debug_screen;
}