using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AreaManagerNS;

/// <summary>
/// WorldManager: Manages Area transitions & Entity update information.
/// Written by Justin Ortiz
/// </summary>
public class WorldManager : MonoBehaviour {
	private static float elapsedGameTime;

	private AreaManager areaManager;

	public static float ElapsedGameTime { get { return elapsedGameTime; } }

	private void Awake() {
		if (GameManager.worldManager != null) {
			Destroy(gameObject);
		} else {
			GameManager.worldManager = this;
		}
	}

	public void GenerateWorldAreas(string playerName, string worldName) {
		StartCoroutine(areaManager.GenerateAllAreas(playerName, worldName, Vector2Int.zero));
	}

	public void LoadAdjacentArea(Directions direction) {
		switch (direction) {
			case Directions.up:
				areaManager.LoadArea(direction);
				break;
			case Directions.down:
				areaManager.LoadArea(direction);
				break;
			case Directions.left:
				areaManager.LoadArea(direction);
				break;
			case Directions.right:
				areaManager.LoadArea(direction);
				break;
			default:
				LoadAdjacentArea(Directions.right);
				break;
		}
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
