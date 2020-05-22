using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

[Serializable]
public class GameManager : MonoBehaviour {
	public static GameManager instance;

	private static string path_data;
	private static string path_save;
	private static string fileName_playerSaveInfo;

	private string playerName;
	private string worldName;
	private bool state_gameInitialized;
	private bool state_play;
	private bool state_paused;
	private int currDifficulty;
	private float elapsedGameTime;

	private Transform loadParent;
	private GameObject loadPrefab;

	public static string path_gameData { get { return path_data; } }
	public static string path_saveData { get { return path_save; } }

	public bool State_Play { get { return state_play; } }
	public bool State_Paused { get { return state_paused; } }
	public string PlayerName { get { return playerName; } }
	public string WorldName { get { return worldName; } }
	public int Difficulty { get { return currDifficulty; } }
	public float ElapsedGameTime { get { return elapsedGameTime; } }

	private Transform AddLoadElement(string PlayerName) {
		Transform temp = Instantiate(loadPrefab, loadParent).transform;
		temp.name = PlayerName;
		return temp;
	}

	private void Awake() {
		if (instance != null) {
			Destroy(gameObject);
		} else {
			instance = this;
			SelectWorld("Singleplayer World Data");

			loadParent = GameObject.Find("Load Game_ScrollContent").transform;
			loadPrefab = loadParent.GetChild(0).gameObject;

			path_data = Application.persistentDataPath + "/data/";
			path_save = Application.persistentDataPath + "/saves/";
			fileName_playerSaveInfo = "/ptsd.dat";

			CreateDirectory(path_data); //ensure required directories are established
			CreateDirectory(path_save);

			InitializeLoadGameUI();

			loadPrefab.SetActive(false);

			Items.ItemDatabase.Initialize();
			Items.ItemModifierDatabase.Initialize();
			NPC.NPCDatabase.Initialize();
			HUD.Initialize();
		}
	}

	private void CreateDirectory(string path) {
		if (!Directory.Exists(path)) {
			Directory.CreateDirectory(path);
		}
	}

	public void DeleteSaveGame(Transform uiObj) {
		if (!AreaManager.instance.SaveOrLoadInProgress) { //prevent multiple loading sequences
			if (Directory.Exists(path_save + uiObj.name)) { //if the savegame exists
				Directory.Delete(path_save + uiObj.name, true); //delete all save files
			}
			Destroy(uiObj.gameObject); //remove ui element from the scene
			ResizeLoadUI();
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
		if (File.Exists(path_save + PlayerName + fileName_playerSaveInfo)) {
			string details = PlayerName + "\t\t\t\t";

			FileStream file = File.OpenRead(path_save + PlayerName + fileName_playerSaveInfo); //open file
			BinaryFormatter bf = new BinaryFormatter(); //create formatter
			GameSave loadedData = bf.Deserialize(file) as GameSave; //deserialize
			file.Close(); //ensure the file is closed asap

			details += loadedData.Date + "\n" + GetDifficultyName(loadedData.Difficulty) + "\t\t\t\t"; //add the date and difficulty to the details
			details += "Playtime: " + (Math.Truncate((loadedData.ElapsedGameTime / 3600f) * 100f) / 100f) + " hrs"; //add the play time to the details
			return details;
		}
		return "Save Data Error: Unable to load save details for '" + PlayerName + "'.";
	}

	private void Initialize() {
		state_gameInitialized = true; //flag game as initialized
	}

	/// <summary>
	/// Finds game save data and populates the Load/Save UI
	/// </summary>
	private void InitializeLoadGameUI() {
		DirectoryInfo dir = new DirectoryInfo(path_save); //get base directory
		DirectoryInfo[] info = dir.GetDirectories("*"); //get all folders at base directory

		Transform temp; //current ui for savegame
		Text textComponent; //text component for current ui

		foreach (DirectoryInfo f in info) { //for each folder (savegame)
			temp = loadParent.Find(f.Name); //check to see if it has been checked already

			if (temp == null) { //if nothing found
				temp = AddLoadElement(f.Name); //instantiate prefab
			}

			textComponent = temp.GetChild(0).GetComponent<Text>(); //get the text component
			textComponent.text = GetSaveDetails(f.Name); //get save details and display relevant text
		}

		ResizeLoadUI();
	}

	private void LoadGame() {
		if (!AreaManager.instance.SaveOrLoadInProgress) { //prevent multiple loading sequences
			if (!playerName.Equals("") && !worldName.Equals("")) { //ensure required names are assigned
				if (File.Exists(path_save + playerName + fileName_playerSaveInfo)) { //ensure the file exists
					FileStream file = File.OpenRead(path_save + playerName + fileName_playerSaveInfo); //open the file
					BinaryFormatter bf = new BinaryFormatter(); //create formatter
					GameSave saveData = bf.Deserialize(file) as GameSave; //load the file into GameSave class
					file.Close(); //close file

					currDifficulty = saveData.Difficulty; //load difficulty
					elapsedGameTime = saveData.ElapsedGameTime; //load elapsed gametime

					WorldManager.instance.LoadAreaData(playerName, worldName, saveData.GetAreaPosition()); //load all areas & start at last saved position
					Player.instance.TeleportToPos(saveData.GetPlayerPosition()); //move the player to last saved position
					CameraManager.instance.RefocusOnTarget(); //move the camera to follow player
					HUD.LoadHotbarAssignments(saveData.HotbarAssignments);
					RefreshSettings();
					Initialize();
				}
			}
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

	public static T LoadObject<T>(string path) {
		if (File.Exists(path)) { //if the file exists
			StreamReader reader = new StreamReader(File.Open(path, FileMode.Open)); //open file
			T obj = JsonConvert.DeserializeObject<T>(reader.ReadToEnd()); //deserialize
			reader.Close(); //close file
			return obj; //return object
		}
		return default; //no file, return null
	}

	public void PauseToggle() {
		state_paused = !state_paused;
		string menuState = state_paused ? "Pause" : "";
		MenuScript.instance.ChangeState(menuState);
		RefreshSettings();
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
		state_play = false;
		state_paused = false;
		MenuScript.instance.ChangeState("Main Menu");
	}

	public void RefreshSettings() {
		HUD.RefreshSettings();
	}

	private void ResizeLoadUI() {
		float prefabHeight = loadPrefab.GetComponent<RectTransform>().sizeDelta.y; //get the height for each prefab
		RectTransform parentRect = loadParent.GetComponent<RectTransform>(); //get the parent rect once all prefabs instantiated
		parentRect.sizeDelta = new Vector2(parentRect.sizeDelta.x, parentRect.childCount * prefabHeight); //adjust the size so it fits appropriately
	}

	/// <summary>
	/// Called by button(s) using unity inspector & certain in-game events
	/// </summary>
	public void SaveGame(bool saveWorld = true) {
		if (!state_gameInitialized || !AreaManager.instance.SaveOrLoadInProgress) {
			FileStream file = File.Create(path_save + playerName + fileName_playerSaveInfo);
			BinaryFormatter bf = new BinaryFormatter();
			bf.Serialize(file, new GameSave());
			file.Close();

			UpdateCurrLoadElement();

			if (saveWorld) {
				WorldManager.instance.Save();
			}
		}
	}

	public static void SaveObject(object o, string path) {
		StreamWriter writer = new StreamWriter(File.Create(path));//initialize writer with creating/opening filepath
		string json = JsonConvert.SerializeObject(o, Formatting.Indented);
		writer.Write(json); //convert this object to json and write it
		writer.Close(); //close the file
	}

	public void SaveSettings() {
		InputManager.instance.SaveKeyBindings();
		HUD.SaveSettings();
		PlayerPrefs.Save();
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
		if (playerName != null && !playerName.Equals("")) { //if the player name has been entered
			if (!File.Exists(path_save + playerName + fileName_playerSaveInfo)) { //if the file doesn't exist, then we can create it
				WorldManager.instance.GenerateWorldAreas(playerName, worldName); //generate the world
				currDifficulty = difficulty; //set the difficulty
				Player.instance.TeleportToPos(Vector3.zero); //move the player to center
				SaveGame(false); //create a save file
				RefreshSettings();
				Initialize();
			}
		} else {
			//show pop-up stating "Enter a name for your character, then try again."
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
		Transform temp = loadParent.Find(playerName);
		if (temp == null) {
			temp = AddLoadElement(playerName);
		}
		temp.GetChild(0).GetComponent<Text>().text = GetSaveDetails(playerName);
	}
}
