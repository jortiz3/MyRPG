using UnityEngine;
using UnityEngine.UI;
using HUD_Elements;

/// <summary>
/// Modifies various text and slider objects displayed on the static HUD. Written by Justin Ortiz
/// </summary>
public class HUD : MonoBehaviour {
	public static HUD instance;

	private Text interactionText;
	private Transform hotbar;

	private void Awake() {
		if (instance != null) {
			Destroy(gameObject);
		} else {
			instance = this;

			interactionText = transform.Find("Interaction").GetComponent<Text>();
			HideInteractionText();

			hotbar = transform.Find("Hotbar");
		}
	}

	public void HideInteractionText() {
		if (interactionText != null) {
			if (interactionText.gameObject.activeSelf) {
				interactionText.gameObject.SetActive(false);
			}
		}
	}

	/// <summary>
	/// To be used when changing settings.
	/// </summary>
	/// <param name="interactable">Buttons should be clickable (True/False).</param>
	public void SetHotbarInteractable(bool interactable) {
		HotbarElement currElement;
		for (int i = 0; i < hotbar.childCount; i++) {
			currElement = hotbar.GetChild(i).GetComponent<HotbarElement>();
			if (currElement != null) {
				currElement.SetButtonInteractable(interactable);
			}
		}
	}

	public void ShowInteractionText(string text) {
		if (interactionText != null) {
			interactionText.text = text;
			if (!interactionText.gameObject.activeSelf) {
				interactionText.gameObject.SetActive(true);
			}
		}
	}

	/// <summary>
	/// Called by InputManager.cs.
	/// </summary>
	/// <param name="slotNum">The slot number as defined by keybindings. [1, 10(0)]</param>
	public void UseHotbarSlot(int slotNum) {
		if (1 <= slotNum && slotNum < hotbar.childCount + 1) {
			HotbarElement element = hotbar.Find("Hotbar_Slot_" + slotNum).GetComponent<HotbarElement>();
			if (element != null) {
				element.Select();
			}
		}
	}
}
