using System.IO;
using UnityEngine;

public class GameManager : MonoBehaviour {
	public static GameManager instance;

	private static string filePath;
	private static string fileName;

	private string playerName;
	private string worldName;
	private bool state_play;
	private bool state_paused;
	private int currDifficulty;
	[SerializeField, HideInInspector]
	private float elapsedGameTime;

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
		}
	}

	private void FixedUpdate() {
		elapsedGameTime += Time.fixedDeltaTime;
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

	/// <summary>
	/// Finds game save data and populates the Load/Save UI
	/// </summary>
	private void InitializeLoadGameUI() {
		DirectoryInfo dir = new DirectoryInfo(filePath);
		DirectoryInfo[] info = dir.GetDirectories("*");

		foreach (DirectoryInfo f in info) {
			//instantiate template;
			//display f.Name
			//display load info -- GetDifficultyName(), time of save
		}
	}

	/// <summary>
	/// Called by button(s) using unity inspector
	/// </summary>
	public void LoadGame() {
		if (!playerName.Equals("") && !worldName.Equals("")) {
			WorldManager.instance.LoadAreaData(playerName, worldName);
		}
	}

	/// <summary>
	/// Called by button(s) using unity inspector & certain in-game events
	/// </summary>
	public void SaveGame() {
		//ensure you save both player and current area to prevent item dupe
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
			}
		}
	}

	private void Update() {
		state_play = true;

		if (state_paused) {
			state_play = false;
		}

		if (StructureGridManager.instance.EditEnabled) {
			state_play = false;
		}

		if (LoadingScreen.instance.isActive) {
			state_play = false;
		}
	}
}
