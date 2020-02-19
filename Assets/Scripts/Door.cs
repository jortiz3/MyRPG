using UnityEngine;

/// <summary>
/// Used to define specific interactions with doors. Written by Justin Ortiz
/// </summary>
public class Door : Interactable {
	[SerializeField, Tooltip("Name of the key required to open this door")]
	private string keyName;
	[SerializeField]
	private bool consumeKey;
	[SerializeField, Tooltip("The object that will be disabled once the door is opened.")]
	private GameObject disableOnOpen;

	protected override void Initialize() {
		SetInteractMessage("to open door.");
		base.Initialize();
	}

	protected override void InteractInternal() {
		//check if player has the key
		//if consume key >> display message "The key crumbles after you use it."
		if (disableOnOpen != null) {
			disableOnOpen.SetActive(false);
		}
		base.InteractInternal();
	}
}
