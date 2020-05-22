using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// WorldManager: Manages Area transitions & day/night cycle
/// Written by Justin Ortiz
/// </summary>
public class WorldManager : MonoBehaviour {
	public static WorldManager instance;

	private static TimeEvent onHour; //to be invoked on each hour change, passing the current hour
	private static UnityEvent onDayStart; //to be invoked at the start of each day >> hr 0 || 24
	private static float secondsPerHour; //60 seconds per hr, so 1 second = 1 minute in game
	private static int currentTime;

	public static TimeEvent OnHour { get { return onHour; } }
	public static UnityEvent OnDayStart { get { return onDayStart; } }

	private void Awake() {
		if (instance != null) {
			Destroy(gameObject);
		} else {
			instance = this;

			onHour = new TimeEvent();
			onDayStart = new UnityEvent();

			secondsPerHour = 2f; //default is 60f
		}
	}

	private void FixedUpdate() { //every frame
		if (GameManager.instance.State_Play) { //if game is running
			int newTime = GetCurrentHour(); //check time
			if (newTime != currentTime) { //if the hour has changed
				currentTime = newTime; //set the current time
				onHour.Invoke(currentTime); //invoke on hour event listeners
				if (currentTime == 0) { //if it is the first hour of the day
					onDayStart.Invoke(); //invoke on day start listeners
				}
			}
		}
	}

	/// <summary>
	/// Uses the elapsed game time to determine the current hour of day.
	/// </summary>
	public int GetCurrentHour() {
		return (int)(GameManager.instance.ElapsedGameTime / secondsPerHour) % 24; //convert minutes to hours, then mod by 24 to determine current hour of day
	}

	public void GenerateWorldAreas(string playerName, string worldName) {
		StartCoroutine(AreaManager.instance.GenerateAllAreas(playerName, worldName, Vector2Int.zero));
	}

	public void LoadAdjacentArea(Directions direction) {
		HUD.HideInteractionText(); //ensure hud is cleared as next area is loaded
		if (AreaManager.instance.LoadArea(direction, true)) {
			GameManager.instance.SaveGame();
		}
	}

	public void LoadAreaData(string playerName, string worldName, Vector2Int currentPosition) {
		StartCoroutine(AreaManager.instance.LoadAreasFromSave(playerName, worldName, currentPosition));
	}

	public void Save() {
		AreaManager.instance.Save();
	}
}
