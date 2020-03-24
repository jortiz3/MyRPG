using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Written by Justin Ortiz.
/// </summary>
public class AreaExit : Interactable {
	//store all images in static array?

	private string currExitType;
	[SerializeField]
	private Image background_edge;
	[SerializeField]
	private Image background_exit;

	private void Awake() {
		SetInteractMessage("to leave this Area.");
	}

	public void SetExitInteractMessage(string adjacentAreaTypeName, Vector2Int adjacentAreaPosition) {
		SetInteractMessage("to go to " + adjacentAreaTypeName + "[" + adjacentAreaPosition.ToString() + "]");
		UpdateImages(adjacentAreaTypeName);
		currExitType = adjacentAreaTypeName;
	}

	private void UpdateImages(string typeName) {
		if (!currExitType.Equals(typeName)) {
			switch(typeName) {
				case "Mountain":
					break;
				case "Forest":
					break;
				case "Marsh":
					break;
				default:
					break;
			}
		}
	}
}
