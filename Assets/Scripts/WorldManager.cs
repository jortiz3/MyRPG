using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// WorldManager: Manages Area transitions & day/night cycle (future scope)
/// Written by Justin Ortiz
/// </summary>
public class WorldManager : MonoBehaviour {
	public static WorldManager instance;

	private void Awake() {
		if (instance != null) {
			Destroy(gameObject);
		} else {
			instance = this;
		}
	}

	public void GenerateWorldAreas(string playerName, string worldName) {
		StartCoroutine(AreaManager.instance.GenerateAllAreas(playerName, worldName, Vector2Int.zero));
	}

	public void LoadAdjacentArea(Directions direction) {
		HUD.instance.HideInteractionText(); //ensure hud is cleared as next area is loaded
		AreaManager.instance.LoadArea(direction, true);
		GameManager.instance.SaveGame();
	}

	public void LoadAreaData(string playerName, string worldName, Vector2Int currentPosition) {
		StartCoroutine(AreaManager.instance.LoadAreasFromSave(playerName, worldName, currentPosition));
	}

	public void Save() {
		AreaManager.instance.SaveCurrentArea();
	}
}
