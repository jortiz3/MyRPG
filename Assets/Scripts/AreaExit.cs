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
	[SerializeField]
	private Directions direction;

	private void Awake() {
		currExitType = "";
		UpdateImages("Plains");
	}

	protected override void DisableInteraction() {
		if (background_edge != null)
			background_edge.color = Color.clear;
		if (background_exit != null)
			background_exit.color = Color.clear;
		base.DisableInteraction();
	}

	protected override void EnableInteraction() {
		if (background_edge != null)
			background_edge.color = Color.white;
		if (background_exit != null)
			background_exit.color = Color.white;
		base.EnableInteraction();
	}

	protected override void InteractInternal() {
		WorldManager.instance.LoadAdjacentArea(direction);
		base.InteractInternal();
	}

	public void SetExitInteractMessage(string adjacentAreaTypeName, Vector2Int adjacentAreaPosition) {
		SetInteractMessage("to go to " + adjacentAreaTypeName + "[" + adjacentAreaPosition.ToString() + "]");
		UpdateImages(adjacentAreaTypeName);
		currExitType = adjacentAreaTypeName;
	}

	private void Start() {
		SetInteractMessage("to leave this Area.");
	}

	private void UpdateImages(string typeName) {
		if (!currExitType.Equals(typeName)) {
			switch (typeName) {
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
