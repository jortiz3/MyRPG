using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HUD_Elements;

/// <summary>
/// Modifies various text and slider objects displayed on the static HUD. Written by Justin Ortiz
/// </summary>
public class HUD : MonoBehaviour {
	public static HUD instance;

	private Text interactionText;
	private List<HotbarElement> hotbar;

	private void Awake() {
		if (instance != null) {
			Destroy(gameObject);
		} else {
			instance = this;

			interactionText = transform.Find("Interaction").GetComponent<Text>();
			HideInteractionText();

			hotbar = new List<HotbarElement>();
		}
	}

	public void HideInteractionText() {
		if (interactionText != null) {
			if (interactionText.gameObject.activeSelf) {
				interactionText.gameObject.SetActive(false);
			}
		}
	}

	public void RegisterHotbarElement(HotbarElement element) {
		if (hotbar != null) {
			if (element != null) {
				hotbar.Add(element);
			}
		}
	}

	public bool SelectItem(Item item) {
		if (HotbarElement.HotkeyAssignmentActive) {
			SetHotbarAssignmentActive(false);
			HotbarElement.EndHotkeyAssignment(item);
			return true;
		} else {

		}
		return false;
	}

	private void SetHotbarAssignmentActive(bool active) {
		if (active) { //if activating
			HotbarElement.BeginHotkeyAssignment(); //begin assignment detection
		}

		foreach (HotbarElement element in hotbar) {
			element.SetHighlightActive(active); //highlight all hotbar elements
		}
	}

	/// <summary>
	/// To be used when changing settings.
	/// </summary>
	/// <param name="interactable">Buttons should be clickable (True/False).</param>
	public void SetHotbarInteractable(bool interactable) {
		foreach (HotbarElement element in hotbar) {
			element.SetButtonInteractable(interactable);
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
	public void UseHotbarSlot(string slotName) {
		foreach (HotbarElement element in hotbar) {
			if (element.transform.name.Equals(slotName)) {
				element.Select();
				break;
			}
		}
	}
}
