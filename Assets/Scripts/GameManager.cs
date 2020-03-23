﻿using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
	public static GameManager instance;

	private static string filePath;
	private static string fileName;

	private string playerName;
	private string worldName;
	private bool state_gameInitialized;
	private bool state_play;
	private bool state_paused;
	private int currDifficulty;
	[SerializeField, HideInInspector]
	private float elapsedGameTime;

	private Transform loadParent;
	private GameObject loadPrefab;

	public bool State_Play { get { return state_play; } }
	public bool State_Paused { get { return state_paused; } }
	public int Difficulty { get { return currDifficulty; } }
	public float ElapsedGameTime { get { return elapsedGameTime; } }

	private void Awake() {
		if (instance != null) {
			Destroy(gameObject);
		} else {
			instance = this;
			SelectWorld("Singleplayer World Data");

			filePath = Application.persistentDataPath + "/saves/";
			fileName = "/ptsd.dat";

			loadParent = GameObject.Find("Load Game_ScrollContent").transform;
			loadPrefab = loadParent.GetChild(0).gameObject;

			InitializeLoadGameUI();

			loadPrefab.SetActive(false);
		}
	}

	private void FixedUpdate() {
		if (state_play) {
			elapsedGameTime += Time.fixedDeltaTime;
		}
	}

	private string GetDifficultyName(int difficulty) {
		if (difficulty <= 0) {
			return "Normal";
		} else if (difficulty == 1) {
			return "Hard";
		} else {
			return "Masochist";
		}
	}

	private string GetSaveDetails(string PlayerName) {
		if (File.Exists(filePath + PlayerName + fileName)) {
			string details = PlayerName + "\t\t\t\t";

			FileStream file = File.OpenRead(filePath + PlayerName + fileName);
			BinaryFormatter bf = new BinaryFormatter();
			string loadedData = bf.Deserialize(file) as string;

			string[] firstSplit = loadedData.Split(("\n").ToCharArray());
			details += firstSplit[0] + "\n";

			string[] secondSplit = firstSplit[1].Split(' ');
			details += GetDifficultyName(int.Parse(secondSplit[0])) + "\t\t\t\t";
			details += "Playtime: " + (float.Parse(secondSplit[1]) / 120f) + " hrs";

			return details;
		}
		return "Save Data Error: Unable to load save details for '" + PlayerName + "'.";
	}

	/// <summary>
	/// Finds game save data and populates the Load/Save UI
	/// </summary>
	private void InitializeLoadGameUI() {
		DirectoryInfo dir = new DirectoryInfo(filePath);
		DirectoryInfo[] info = dir.GetDirectories("*");

		Transform temp;
		Text textComponent;

		float scrollContentHeight = 0;
		float prefabHeight = loadPrefab.GetComponent<RectTransform>().sizeDelta.y;
		foreach (DirectoryInfo f in info) {
			temp = loadParent.Find(f.Name);

			if (temp == null) {
				temp = Instantiate(loadPrefab, loadParent).transform;
				temp.name = f.Name;
			}

			textComponent = temp.GetChild(0).GetComponent<Text>();
			textComponent.text = GetSaveDetails(f.Name);

			scrollContentHeight += prefabHeight;
		}
		
		RectTransform parentRect = loadParent.GetComponent<RectTransform>();
		parentRect.sizeDelta = new Vector2(parentRect.sizeDelta.x, scrollContentHeight);
	}

	private void LoadGame() {
		if (!playerName.Equals("") && !worldName.Equals("")) {
			WorldManager.instance.LoadAreaData(playerName, worldName);

			if (File.Exists(filePath + playerName + fileName)) {
				FileStream file = File.OpenRead(filePath + playerName + fileName);
				BinaryFormatter bf = new BinaryFormatter();
				string saveData = bf.Deserialize(file) as string;
				try {
					string[] splitSaveData = saveData.Split(("\n").ToCharArray())[1].Split(' ');
					currDifficulty = int.Parse(splitSaveData[0]);
					elapsedGameTime = float.Parse(splitSaveData[1]);
				} catch { }
			}

			MenuScript.instance.ChangeState("");
			state_gameInitialized = true;
		}
	}

	/// <summary>
	/// Called from button in scene using inspector.
	/// </summary>
	/// <param name="uiObj">The button utilizing the method</param>
	public void LoadGame(Transform uiObj) {
		SelectPlayer(uiObj.name);
		LoadGame();
	}

	public void PauseToggle() {
		state_paused = !state_paused;
		string menuState = state_paused ? "Pause" : "";
		MenuScript.instance.ChangeState(menuState);
	}

	/// <summary>
	/// Called during game play by player using keybind.
	/// </summary>
	public void QuickLoadGame() {
		if (state_play) {
			LoadGame();
		}
	}

	public void QuitToDesktop() {
		Application.Quit();
	}

	public void QuitToMainMenu() {
		state_gameInitialized = false;
		MenuScript.instance.ChangeState("Main Menu");
	}

	/// <summary>
	/// Called by button(s) using unity inspector & certain in-game events
	/// </summary>
	public void SaveGame() {
		string saveData = DateTime.Now.ToString() + "\n"; //add time
		saveData += currDifficulty + " ";
		saveData += elapsedGameTime;

		FileStream file = File.Create(filePath + playerName + fileName);
		BinaryFormatter bf = new BinaryFormatter();
		bf.Serialize(file, saveData);
		file.Close();

		UpdateCurrLoadElement();
	}

	public void SaveSettings() {
		InputManager.instance.SaveKeyBindings();
	}

	public void SelectPlayer(string name) {
		playerName = name;
	}

	public void SelectWorld(string name) {
		worldName = name;
	}

	private void Start() {
		InitializeLoadGameUI();

		state_paused = false;
	}

	/// <summary>
	/// Called by button(s) using unity inspector
	/// </summary>
	public void StartNewGame(int difficulty) {
		if (playerName != null && !playerName.Equals("")) {
			if (!File.Exists(filePath + playerName + fileName)) {
				WorldManager.instance.GenerateWorldAreas(playerName, worldName);
				currDifficulty = difficulty;
				MenuScript.instance.ChangeState("");
				SaveGame();
				state_gameInitialized = true;
			}
		}
	}

	private void Update() {
		if (state_gameInitialized) {
			state_play = true;
		}

		if (state_paused) {
			state_play = false;
		}

		if (StructureGridManager.instance.EditEnabled) {
			state_play = false;
		}

		if (LoadingScreen.instance.isActive()) {
			state_play = false;
		}
	}

	private void UpdateCurrLoadElement() {

	}
}
