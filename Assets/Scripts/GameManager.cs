using System.IO;
using UnityEngine;

public class GameManager : MonoBehaviour {
	public static GameManager instance;
	public static LoadingBar loadingBar;

	private string playerName;
	private string worldName;
	private bool state_play;
	private bool state_paused;
	[SerializeField, HideInInspector]
	private float elapsedGameTime;

	public bool State_Play { get { return state_play; } }
	public bool State_Paused { get { return state_paused; } }
	public float ElapsedGameTime { get { return elapsedGameTime; } }

	private void Awake() {
		if (instance != null) {
			Destroy(gameObject);
		} else {
			instance = this;
		}
	}

	/// <summary>
	/// Finds game save data and populates the Load/Save UI
	/// </summary>
	private void DetectSaveGameData() {

	}

	private void FixedUpdate() {
		elapsedGameTime += Time.fixedDeltaTime;
	}

	/// <summary>
	/// Called by button(s) using unity inspector
	/// </summary>
	public void LoadGame() {
		WorldManager.instance.LoadAreaData(playerName, worldName);
	}

	/// <summary>
	/// Called by button(s) using unity inspector
	/// </summary>
	public void SaveGame() {
		
	}

	public void SelectPlayer(string name) {
		playerName = name;
	}

	/// <summary>
	/// Called by button(s) using unity inspector
	/// </summary>
	public void SelectPlayer(Transform uiElement) {
		playerName = uiElement.name;
	}

	public void SelectWorld(string name) {
		worldName = name;
	}

	/// <summary>
	/// Called by button(s) using unity inspector
	/// </summary>
	public void SelectWorld(Transform uiElement) {
		worldName = uiElement.name;
	}

	private void Start() {
		DetectSaveGameData();

		state_paused = false;
	}

	/// <summary>
	/// Called by button(s) using unity inspector
	/// </summary>
	public void StartNewGame() {
		WorldManager.instance.GenerateWorldAreas(playerName, worldName);
	}

	private void Update() {
		state_play = true;

		if (state_paused) {
			state_play = false;
		}

		if (StructureGridManager.instance.EditEnabled) {
			state_play = false;
		}

		if (loadingBar.isActive) {
			state_play = false;
		}
	}
}
