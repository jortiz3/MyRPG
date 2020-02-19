using UnityEngine;

/// <summary>
/// Used to define specific interactions with doors. Written by Justin Ortiz
/// </summary>
public class Door : Interactable {
	[SerializeField, Tooltip("Name of the key required to open this door")]
	private string keyName;
	[SerializeField]
	private bool consumeKey;
	[SerializeField, Tooltip("The object that will be disabled once the door is opened; typically it will be the door itself.")]
	private GameObject disableOnOpen;
	[SerializeField, Tooltip("The roof to enable once the door is opened. (Enables transparency)")]
	private Roof enableRoofOnOpen;

	protected override void Initialize() {
		SetInteractMessage("to open door.");
		base.Initialize();
	}

	protected override void InteractInternal() {
		if (keyName != null && !keyName.Equals("")) { //if there is a key name
			//check if player has the key
			if (consumeKey) {
				//remove key from inventory
				//display message: "[keyName] crumbled into dust.."
			}
		}
		if (disableOnOpen != null) {
			disableOnOpen.SetActive(false);
		}
		if (enableRoofOnOpen != null) {
			enableRoofOnOpen.Enable();
		}
		base.InteractInternal();
	}
}
