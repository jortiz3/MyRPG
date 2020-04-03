using System;
using UnityEngine;

[Serializable]
public class GameSave {
	[SerializeField]
	private string date;
	[SerializeField]
	private string version;
	[SerializeField]
	private string playerName;
	[SerializeField]
	private string worldName;
	[SerializeField]
	private int currDifficulty;
	[SerializeField]
	private float elapsedGameTime;
	[SerializeField]
	private ContainerSaveData inventory;

	public string Date { get { return date; } }
	public string Version { get { return version; } }
	public string PlayerName { get { return playerName; } }
	public string WorldName { get { return worldName; } }
	public int Difficulty { get { return currDifficulty; } }
	public float ElapsedGameTime { get { return elapsedGameTime; } }
	public ContainerSaveData Inventory_Player { get { return inventory; } }

	public GameSave() {
		date = DateTime.Now.Date.ToString();
		version = "v0.01";
		playerName = GameManager.instance.PlayerName;
		worldName = GameManager.instance.WorldName;
		currDifficulty = GameManager.instance.Difficulty;
		elapsedGameTime = GameManager.instance.ElapsedGameTime;
		inventory = new ContainerSaveData(Inventory.instance);
	}
}
