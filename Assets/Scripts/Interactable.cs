using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Used to define objects the player will interact with. Written by Justin Ortiz
/// </summary>
public class Interactable : MonoBehaviour {
	private static Interactable displayedInteractable;

	private string interactMessage;
	private bool interactable;

	protected virtual void Initialize() {
		if (interactMessage == null || interactMessage.Equals("")) {
			SetInteractMessage("to interact.");
		}
	}

	protected virtual void InteractInternal() {
		if (interactable) { //ensures player is within range to interact
			HUD.instance.HideInteractionText(); //hide the interact text
			displayedInteractable = null; //remove flag from this
			interactable = false; //ensure player can no longer interact
		}
	}

	public static void Interact() {
		if (displayedInteractable != null) {
			displayedInteractable.InteractInternal();
		}
	}

	private void OnTriggerEnter(Collider other) {
		if (other.CompareTag("Player")) { //player enters the interact range
			if (displayedInteractable != null) { //if something is already displayed
				displayedInteractable.interactable = false; //ensure only 1 interactable is used at a time
			}
			HUD.instance.ShowInteractionText(interactMessage); //show the interaction text
			displayedInteractable = this; //flag this as shown
			interactable = true; //allow player to interact
		}
	}

	private void OnTriggerExit(Collider other) {
		if (other.CompareTag("Player")) { //player exits the interact range
			if (displayedInteractable != null) { //if an interaction is displayed
				if (displayedInteractable.Equals(this)) { //if this is the interaction currently being displayed
					HUD.instance.HideInteractionText(); //hide the text
					displayedInteractable = null; //remove flag for this being the displayed interaction
				}
			}
			interactable = false; //ensure player can no longer interact
		}
	}

	protected void SetInteractMessage(string newMessage) {
		interactMessage = "Press '" + InputManager.instance.GetKeyCodeName("Interact") + "' " + newMessage;
	}

	private void Start() {
		Initialize();
	}
}
