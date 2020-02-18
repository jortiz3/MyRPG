using UnityEngine;

/// <summary>
/// Used to define specific interactions with doors. Written by Justin Ortiz
/// </summary>
public class Door : Interactable {
	[SerializeField, Tooltip("Name of the key required to open this door")]
	private string keyName;
	[SerializeField]
	private bool consumeKey;

	protected override void Initialize() {
		SetInteractMessage("open door.");
		base.Initialize();
	}

	protected override void Interact() {
		//check if player has the key
		//if consume key >> display message "The key crumbles after you use it."
	}
}
