﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AreaManagerNS;

//loads or generates areas using areaManager
//Tracks in-game time
//updates NPCs and areas based on in-game time
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
		StartCoroutine(areaManager.GenerateWorldAreas(playerName, worldName));
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
		StartCoroutine(areaManager.LoadAreasFromWorld(playerName, worldName));
	}

	private void FixedUpdate() {
		elapsedGameTime += Time.deltaTime;
	}

	private void Start() {
		areaManager = GameObject.FindObjectOfType<AreaManager>();
	}
}
