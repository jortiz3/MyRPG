using UnityEngine;

/// <summary>
/// Used to define objects the player will interact with. Written by Justin Ortiz
/// </summary>
public class Interactable : MonoBehaviour {
	private static Interactable displayedInteractable;

	private string interactMessage;
	private bool interactable;
	private bool disabled;
	protected float lastUpdated;
	protected SpriteRenderer sprite;
	protected string owner;

	public string Owner { get { return owner; } }
	public float LastUpdated { get { return lastUpdated; } set { lastUpdated = value; } }
	

	/// <summary>
	/// Disables the capability for the player to interact with this object.
	/// </summary>
	protected virtual void DisableInteraction() {
		disabled = true;
	}

	/// <summary>
	/// Enables the capability for the player to interact with this object.
	/// </summary>
	protected virtual void EnableInteraction() {
		disabled = false;
	}

	public Sprite GetSprite() {
		if (sprite != null) {
			return sprite.sprite;
		}
		return null;
	}

	public string GetTextureName() {
		if (sprite != null) {
			if (sprite.sprite != null) {
				return sprite.sprite.texture.name;
			}
		}
		return "";
	}

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

	public static bool Interact() {
		if (displayedInteractable != null) {
			displayedInteractable.InteractInternal();
			return true;
		}
		return false;
	}

	private void OnTriggerEnter(Collider other) {
		if (!disabled) { //if enabled
			if (GameManager.instance.State_Play) { //if the player is playing -- !paused, !editing
				if (other.CompareTag("Player")) { //player enters the interact range
					if (displayedInteractable != null) { //if something is already displayed
						displayedInteractable.interactable = false; //ensure only 1 interactable is used at a time
					}
					HUD.instance.ShowInteractionText(interactMessage); //show the interaction text
					displayedInteractable = this; //flag this as shown
					interactable = true; //allow player to interact
				}
			}
		}
	}

	private void OnTriggerExit(Collider other) {
		if (GameManager.instance.State_Play) {
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
	}

	public virtual void SetActive(bool interactive = true, bool visible = true) {
		if (interactive) {
			EnableInteraction();
		} else {
			DisableInteraction();
		}

		SetSpriteActive(visible);
	}

	/// <summary>
	/// Updates the message that is displayed once the player enters the interaction range.
	/// </summary>
	/// <param name="newMessage">"Press [button]" + newMessage</param>
	protected void SetInteractMessage(string newMessage) {
		interactMessage = "Press [" + InputManager.instance.GetKeyCodeName("Interact") + "] " + newMessage;
	}

	protected void SetSprite(Texture2D texture) {
		if (texture != null) {
			if (sprite == null) { //if the sprite has not been assigned yet -- sometimes called prior to initialize
				sprite = gameObject.GetComponent<SpriteRenderer>(); //get the sprite component
			} //no else bc we still want to try to set sprite

			if (sprite != null) { //if sprite component exists
				sprite.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 16f); //create sprite using given texture
				gameObject.name = texture.name;
			}
		}
	}

	protected virtual void SetSpriteActive(bool active = true) {
		if (sprite == null) { //if the sprite has not been assigned yet -- sometimes called prior to initialize
			sprite = gameObject.GetComponent<SpriteRenderer>(); //get the sprite component
		}

		if (sprite != null) {
			sprite.enabled = active;
		}
	}

	private void Start() {
		Initialize();
	}
}
