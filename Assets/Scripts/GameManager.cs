﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using Facebook.Unity;
using LitJson;
using System.IO;

public class GameManager : MonoBehaviour {

	public static GameManager instance;
	private Scene activeScene;

	public int supply, knife_for_pickup, club_for_pickup, ammo_for_pickup, gun_for_pickup, active_survivor_for_pickup, inactive_survivors;
	public string userId;
	public string userFirstName, userLastName;
	public string lastLogin_ts;
	public string locationJsonText, clearedBldgJsonText;
	public float homebase_lat, homebase_lon;
	public bool dataIsInitialized;

	public List <GameObject> survivorCardList = new List<GameObject>();

	public static string serverURL = "http://www.argzombie.com/ARGZ_DEV_SERVER";
	private string fetchSurvivorDataURL = GameManager.serverURL+"/FetchSurvivorData.php";


	private static SurvivorPlayCard survivorPlayCardPrefab;

	void Awake () {
		MakeSingleton();
		dataIsInitialized = false;
		survivorPlayCardPrefab = Resources.Load<SurvivorPlayCard>("Prefabs/SurvivorPlayCard");
	}

	void OnLevelWasLoaded () {
		//this is a catch all to slave the long term memory to the active GameManager.instance object- each load will update long term memory.


		activeScene = SceneManager.GetActiveScene();
		if (activeScene.name.ToString() == "02a Homebase"){
			
		} else if (activeScene.name.ToString() == "01a Login") {
			LoginManager loginMgr = FindObjectOfType<LoginManager>();
			if (FB.IsLoggedIn == true) {
				loginMgr.loginFailedPanel.SetActive(false);
				loginMgr.returnToGameButton.SetActive(true);
			}
		}
	}

	void MakeSingleton() {
		if (instance != null) {
			Destroy (gameObject);
		} else {
			instance = this;
			DontDestroyOnLoad (gameObject);
		}
	}




	IEnumerator FetchSurvivorData () {
		//construct form
		WWWForm form = new WWWForm();
		if (FB.IsLoggedIn == true) {
			form.AddField("id", GameManager.instance.userId);
		} else {
			;
			GameManager.instance.userId = "10154194346243929";
			form.AddField("id", GameManager.instance.userId);
		}
		form.AddField("login_ts", GameManager.instance.lastLogin_ts);
		form.AddField("client", "web");

		//make www call
		WWW www = new WWW(fetchSurvivorDataURL, form);
		yield return www;
		Debug.Log(www.text);

		if (www.error == null) {
			//encode json return
			string survivorJsonString = www.text;
			JsonData survivorJson = JsonMapper.ToObject(survivorJsonString);

			if (survivorJson[0].ToString() != "Failed") {
				//parse through json creating "player cards" within gamemanager for each player found on the server.
				for (int i = 0; i < survivorJson.Count; i++) {
					SurvivorPlayCard instance = Instantiate(survivorPlayCardPrefab);
					instance.survivor.name = survivorJson[i]["name"].ToString();
					instance.gameObject.name = survivorJson[i]["name"].ToString();
					//instance.survivor.weaponEquipped.name = survivorJson[i]["weapon_equipped"].ToString();
					instance.survivor.baseAttack = (int)survivorJson[i]["base_attack"];
					instance.survivor.baseStamina = (int)survivorJson[i]["base_stam"];
					instance.survivor.curStamina = (int)survivorJson[i]["curr_stam"];
					instance.entry_id = (int)survivorJson[i]["entry_id"];
					instance.survivor_id = (int)survivorJson[i]["survivor_id"];

					instance.transform.SetParent(GameManager.instance.transform);
				}
				survivorCardList.AddRange (GameObject.FindGameObjectsWithTag("survivorcard"));
			} else {
				//server has returned a failure
				Debug.Log("Survivor Query failed: "+survivorJson[1].ToString());
			}


			if (SceneManager.GetActiveScene().buildIndex != 2 ) {
				SceneManager.LoadScene("02a Homebase");
			}

		} else {
			Debug.LogWarning(www.error);
		}
	}

}
