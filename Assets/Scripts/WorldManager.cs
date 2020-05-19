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

	private bool thread_running;
	private bool thread_cancel;

	public static TimeEvent OnHour { get { return onHour; } }
	public static UnityEvent OnDayStart { get { return onDayStart; } }

	private void Awake() {
		if (instance != null) {
			Destroy(gameObject);
		} else {
			instance = this;

			onHour = new TimeEvent();
			onDayStart = new UnityEvent();

			secondsPerHour = 60f;

			GameManager.OnGamePlay.AddListener(OnGamePlay); //invoked each time play is started
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
		HUD.instance.HideInteractionText(); //ensure hud is cleared as next area is loaded
		if (AreaManager.instance.LoadArea(direction, true)) {
			GameManager.instance.SaveGame();
		}
	}

	public void LoadAreaData(string playerName, string worldName, Vector2Int currentPosition) {
		StartCoroutine(AreaManager.instance.LoadAreasFromSave(playerName, worldName, currentPosition));
	}

	private void OnGamePlay() {
		StartCoroutine(UpdateTime());
	}

	public void Save() {
		AreaManager.instance.SaveCurrentArea();
	}

	private IEnumerator UpdateTime() {
		if (thread_running) { //if this type of thread is running already
			thread_cancel = true; //cancel previous thread because it will be out of sync
		}

		while (thread_running) { //wait until prev thread is done
			yield return new WaitForEndOfFrame();
		}

		thread_running = true; //flag this thread as running
		float secondsToNextHour = secondsPerHour - (GameManager.instance.ElapsedGameTime % (int)secondsPerHour); //subtract remainder of current hour from seconds per hour
		int currentTime;
		while (true) { //infinite loop
			yield return new WaitForSeconds(secondsToNextHour); //wait for next check
			if (GameManager.instance.State_Play && !thread_cancel) {
				currentTime = GetCurrentHour();
				if (currentTime == 0) {
					onDayStart.Invoke();
				} else {
					onHour.Invoke(currentTime);
				}
				secondsToNextHour = 60f;
			} else {
				break;
			}
		}
		thread_cancel = false;
		thread_running = false;
	}
}
