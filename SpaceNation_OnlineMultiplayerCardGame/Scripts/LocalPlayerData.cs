using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class LocalPlayerData : MonoBehaviour {
	public string localDataFileName = "/gamedatalocal.dat";
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SaveData(PlayerData saveData) {
		BinaryFormatter BFormatter = new BinaryFormatter ();
		FileStream file = File.Create (Application.persistentDataPath + localDataFileName);

		PlayerData data = new PlayerData();
		data.dispName = saveData.dispName;
		data.name = saveData.name;
		data.auth = saveData.auth;
		data.deck = saveData.deck;
		data.avatar = saveData.avatar;
		//data.level = saveData.level;
		//data.exp = saveData.exp;
		//data.wins = saveData.wins;
		//data.looses = saveData.looses;
		data.logout = saveData.logout;
		BFormatter.Serialize (file, data);
		file.Close ();
	}

	public bool LoadData(PlayerData loadData) {
		bool playerDataOK = false;
		if (File.Exists (Application.persistentDataPath + localDataFileName)) {
			BinaryFormatter BFormatter = new BinaryFormatter ();
			FileStream file = File.Open (Application.persistentDataPath + localDataFileName, FileMode.Open);
			if (file.Length > 0) {
				PlayerData data = (PlayerData)BFormatter.Deserialize (file);

				loadData.dispName = data.dispName;
				loadData.name = data.name;
				loadData.auth = data.auth;
				loadData.deck = data.deck;
				loadData.avatar = data.avatar;
				//loadData.level = data.level;
				//loadData.exp = data.exp;
				//loadData.wins = data.wins;
				//loadData.looses = data.looses;
				loadData.logout = data.logout;
				BFormatter.Serialize (file, data);
				playerDataOK = true;
			}
			file.Close ();
		}
		return playerDataOK;
	}
}

[Serializable]
public class PlayerData
{
	public string dispName;
	public string name;
	public string avatar;
	public int deck;
	public int level;
	public int exp;
	public int rank;
	public int qp_wins;
	public int qp_losses;
	public int qp_looses;
	public int qp_games_guard;
	public int qp_games_pirate;
	public int tr_wins;
	public int tr_losses;
	public int tr_games_guard;
	public int tr_games_pirate;
	public int wins;
	public int looses;
	public int rk_games_guard;
	public int rk_games_pirate;
	public bool logout;
	public string auth;
}
