using System.IO;
using UnityEngine;

/// <summary>
/// Detects various Mods and implements them into the game.
/// </summary>
public class ModManager : MonoBehaviour {

	//check for mod folder in persistent data path
	
	public void GenerateModFolders() {
		//generate folder paths at persistent data path
	}

	private Sprite LoadSprite(string path) {
		if (string.IsNullOrEmpty(path)) {
			return null;
		}
		if (File.Exists(path)) {
			byte[] bytes = File.ReadAllBytes(path);
			Texture2D texture = new Texture2D(1, 1);
			texture.LoadImage(bytes);
			Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
			return sprite;
		}
		return null;
	}
}
