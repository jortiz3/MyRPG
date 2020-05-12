using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HUD_Elements;

/// <summary>
/// Modifies various text and slider objects displayed on the static HUD. Written by Justin Ortiz
/// </summary>
public class HUD : MonoBehaviour {
	public static HUD instance;

	private static string setting_clickSelectKey = "HUD_Hotkey Bar Click Enabled";
	private static bool setting_clickSelectEnabled; //this determines whether the player can click hotbar elements during play to use them

	private Text interactionText;
	private List<HotbarElement> hotbar;
	public static bool Setting_ClickSelectEnabled { get { return setting_clickSelectEnabled; } }

	private void Awake() {
		if (instance != null) {
			Destroy(gameObject);
		} else {
			instance = this;

			interactionText = transform.Find("Interaction").GetComponent<Text>();
			HideInteractionText();

			hotbar = new List<HotbarElement>();

			if (PlayerPrefs.HasKey(setting_clickSelectKey)) {
				setting_clickSelectEnabled =  PlayerPrefs.GetInt(setting_clickSelectKey) == 1 ? true : false;
			} else {
				setting_clickSelectEnabled = true;
			}

			Transform settings_hud = GameObject.Find("Settings_HUD").transform; //the parent for all HUD settings
			settings_hud.Find(setting_clickSelectKey).GetComponent<Toggle>().onValueChanged.AddListener(SetClickSelectSetting); //ensure the toggle is tied to the setting
		}
	}

	public void BeginHotkeyAssignment(Item item) {
		if (item != null) {
			HotbarElement.BeginHotkeyAssignment(item); //begin assignment detection
			SetHotbarHighlight(true);
		}
	}

	public static Item GetValidAssignment(Item item) {
		if (Inventory.instance.Contains(item)) { //if inventory has memory reference to item
			return item; //return reference
		}

		Item temp = Inventory.instance.GetItem(item.ToString()); //get reference to same type of item in player's inventory
		if (temp == null) { //if no item was retrieved
			temp = Container.Transfer(item, 1); //ensure the player has at least 1 of this item, then store reference
		}
		return temp;
	}

	public void EndHotkeyAssignment() {
		HotbarElement.EndHotkeyAssignment();
		SetHotbarHighlight(false);
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

	public void RemoveHotkeyAssignment(Item item) {
		if (item != null) {
			foreach (HotbarElement element in hotbar) {
				if (element.Assigned_Item != null) {
					if (element.Assigned_Item.Equals(item)) { //if the same item in memory
						element.ClearAssignment(); //clear assignment
					}
				}
			}
		}
	}

	public static void SaveSettings() {
		PlayerPrefs.SetInt(setting_clickSelectKey, setting_clickSelectEnabled ? 1 : 0);
	}

	public bool SelectItem(Item item) {
		if (HotbarElement.HotkeySelected) {
			Item tempItem = GetValidAssignment(item);
			RemoveHotkeyAssignment(tempItem); //removes all assignments of this item
			return HotbarElement.AssignToSelectedHotkey(tempItem); //assign item to the selected hotkey
		} else {
			if (item.Slottable) {
				BeginHotkeyAssignment(item);
			} else {
				EndHotkeyAssignment();
			}
		}
		return false;
	}

	public static void SetClickSelectSetting(bool newSetting) {
		setting_clickSelectEnabled = newSetting;
	}

	private void SetHotbarHighlight(bool active) {
		foreach (HotbarElement element in hotbar) {
			element.SetHighlightActive(active); //highlight all hotbar elements
		}
	}

	/// <summary>
	/// To be used when changing settings.
	/// </summary>
	/// <param name="interactable">Buttons should be clickable (True/False).</param>
	public void RefreshSettings() {
		//hotbar settings
		bool interactable = true; //default to true
		if (MenuScript.instance.CurrentState.Equals("") || LoadingScreen.instance.isActive()) { //if no menu displayed
			interactable = setting_clickSelectEnabled; //change interactable based on setting
		}

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
				element.Select(true);
				break;
			}
		}
	}
}
