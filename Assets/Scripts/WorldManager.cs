using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// WorldManager: Manages Area transitions, the time within the game, & day/night cycle (future scope)
/// Written by Justin Ortiz
/// </summary>
public class WorldManager : MonoBehaviour {
	public static WorldManager instance;
	private static float elapsedGameTime;

	private AreaManager areaManager;

	public static float ElapsedGameTime { get { return elapsedGameTime; } }

	private void Awake() {
		if (instance != null) {
			Destroy(gameObject);
		} else {
			instance = this;
		}
	}

	public void GenerateWorldAreas(string playerName, string worldName) {
		StartCoroutine(areaManager.GenerateAllAreas(playerName, worldName, Vector2Int.zero));
	}

	public void LoadAdjacentArea(Directions direction) {
		HUD.instance.HideInteractionText(); //ensure hud is cleared as next area is loaded
		areaManager.LoadArea(direction);
	}

	public void LoadAreaData(string playerName, string worldName) {
		StartCoroutine(areaManager.LoadAreasFromSave(playerName, worldName, Vector2Int.zero));
	}

	private void FixedUpdate() {
		elapsedGameTime += Time.deltaTime;
	}

	private void Start() {
		areaManager = GameObject.FindObjectOfType<AreaManager>();
	}
}
