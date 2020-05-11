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
	private int area_position_x;
	[SerializeField]
	private int area_position_y;
	[SerializeField]
	private float player_position_x;
	[SerializeField]
	private float player_position_y;
	[SerializeField]
	private bool setting_hudClickSelect;

	public string Date { get { return date; } }
	public string Version { get { return version; } }
	public string PlayerName { get { return playerName; } }
	public string WorldName { get { return worldName; } }
	public int Difficulty { get { return currDifficulty; } }
	public float ElapsedGameTime { get { return elapsedGameTime; } }

	public GameSave() {
		date = DateTime.Now.Date.ToString();
		version = "v0.01";
		playerName = GameManager.instance.PlayerName;
		worldName = GameManager.instance.WorldName;
		currDifficulty = GameManager.instance.Difficulty;
		elapsedGameTime = GameManager.instance.ElapsedGameTime;

		//save area position
		area_position_x = AreaManager.instance.Position.x;
		area_position_y = AreaManager.instance.Position.y;

		//save player position
		player_position_x = Player.instance.transform.position.x;
		player_position_y = Player.instance.transform.position.y;

		//save settings
		setting_hudClickSelect = HUD.Setting_ClickSelectEnabled;
	}

	public Vector2Int GetAreaPosition() {
		return new Vector2Int(area_position_x, area_position_y);
	}

	public Vector3 GetPlayerPosition() {
		return new Vector3(player_position_x, player_position_y, 0);
	}

	public void LoadSettings() {
		HUD.SetClickSelectSetting(setting_hudClickSelect);
	}
}
