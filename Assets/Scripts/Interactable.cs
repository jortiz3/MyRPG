using UnityEngine;

/// <summary>
/// Used to define objects the player will interact with. Written by Justin Ortiz
/// </summary>
public class Interactable : MonoBehaviour {
	private static Interactable displayedInteractable;

	private string interactMessage;
	private bool interactable;

	public static Interactable DisplayedInteractable { get { return displayedInteractable; } }

	private void FixedUpdate() {
		if (interactable) {
			//if not displayed
			//display message
		}
	}

	protected virtual void Initialize() {
		if (interactMessage == null || interactMessage.Equals("")) {
			SetInteractMessage("to interact.");
		}
	}

	protected virtual void Interact() {
		Debug.Log("Player interacted with " + gameObject.name);
	}

	private void OnTriggerEnter(Collider collision) {
		if (collision.CompareTag("Player")) {
			displayedInteractable.interactable = false;
			displayedInteractable = this;
			interactable = true;
		}
	}

	protected void SetInteractMessage(string newMessage) {
		interactMessage = "Press " + InputManager.instance.GetKeyCodeName("Interact") + " " + newMessage;
	}
}
