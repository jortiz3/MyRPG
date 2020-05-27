using UnityEngine;
using UnityEngine.UI;

namespace HUD_Elements {
	/// <summary>
	/// Manages selection and assignment of hotbar ui elements. Written by Justin Ortiz
	/// </summary>
	public class HotbarElement : MonoBehaviour {
		private static HotbarElement selected_hotkey;
		private static Item selected_item;
		private static Color color_assignment = new Color(1f, 1f, 1f, 0.8f);
		private static bool hotkeyAssignmentActive;

		private Button button;
		private Image image_button;
		private Image image_assigned;
		private Text keybind_display;
		private Item item;

		public static bool HotkeySelected { get { return selected_hotkey != null ? true : false; } }
		public static bool HotkeyAssignmentActive { get { return hotkeyAssignmentActive; } }

		public Item Assigned_Item { get { return item; } }

		private bool Assign(Item i) {
			UnselectHotkey(); //remove selected on each attempt to assign

			if (i != null) {
				if (i.Slottable) {
					ClearAssignment();
					HUD.RemoveHotkeyAssignment(i); //ensure this item is not assigned to another hotkey

					if (image_assigned != null) {
						image_assigned.sprite = i.GetSprite();
						if (image_assigned.sprite != null) {
							image_assigned.color = color_assignment;
						} else {
							image_assigned.color = Color.clear;
						}
					}

					item = i; //store this item
					return true;
				}
			}
			return false;
		}

		public static bool AssignToSelectedHotkey(Item i) {
			if (HotkeySelected) {
				return selected_hotkey.Assign(i);
			}
			return false;
		}

		private void Awake() {
			button = GetComponent<Button>();
			image_button = GetComponent<Image>();
			image_assigned = transform.GetChild(0).GetComponent<Image>();
			keybind_display = transform.GetChild(1).GetComponent<Text>();

			if (image_assigned != null) {
				image_assigned.preserveAspect = true;
				image_assigned.color = Color.clear;
			}

			HUD.RegisterHotbarElement(this);
		}

		public static void BeginHotkeyAssignment(Item item) {
			UnselectHotkey();
			selected_item = item;
			hotkeyAssignmentActive = true;
		}

		public void ClearAssignment() {
			if (item != null) {
				item = null;
			}

			if (image_assigned != null) {
				image_assigned.sprite = null;
				image_assigned.color = Color.clear;
			}
		}

		public static void EndHotkeyAssignment() {
			selected_item = null; //remove reference to selected item
			hotkeyAssignmentActive = false; //stop checking for assignment
		}

		public string GetAssignmentName() {
			string assignment = "";
			if (item != null) {
				assignment = "item_" + item.ToString();
			} //if skill
			return assignment;
		}

		public void LoadAssignment(string assignmentName) {
			if (assignmentName.Contains("item")) {
				Item i = Inventory.instance.GetItem(assignmentName.Split('_')[1]); //try to get the item from player's inventory
				if (i != null) { //if item retrieved
					Assign(i); //assign to this slot
				}
			}
		}

		public static void UnselectHotkey() {
			if (selected_hotkey != null) { //if there's a selected hotkey
				selected_hotkey.SetHighlightActive(false); //remove highlight
				selected_hotkey = null; //remove reference to it
			}
		}

		public void Select(bool viaButtonPress) {
			if (MenuScript.instance.CurrentState.Equals("")) { //if game is underway
				if (viaButtonPress || HUD.Setting_ClickSelectEnabled) {
					Use(); //use what is in the slot
				}
			} else { //viewing inventory
				if (hotkeyAssignmentActive) { //player clicked slottable item, then this
					selected_item = HUD.GetValidAssignment(selected_item);
					Assign(selected_item); //assign the selected item to this hotkey
					HUD.EndHotkeyAssignment(); //inform HUD assignment should end
				} else { //hotkey was selected first
					if (selected_hotkey != this) { //if another hotkey was selected prior
						UnselectHotkey(); //remove other key as selected
						SetHighlightActive(true); //highlight this key
						selected_hotkey = this; //store reference to this hotkey
					} else { //this hotkey was selected once before
						ClearAssignment(); //clear assignment
						UnselectHotkey(); //unselect this hotkey
					}
				}
			}
		}

		public void SetButtonInteractable(bool interactable) {
			if (button != null) {
				button.interactable = interactable;
			}
		}

		public void SetHighlightActive(bool active) {
			if (image_button != null) {
				image_button.color = active ? GameManager.Color_Highlight : Color.white;
			}
		}

		private void Start() {
			if (keybind_display != null) {
				keybind_display.text = InputManager.instance.GetKeyCodeName(transform.name);
			}
		}

		/// <summary>
		/// Called by button element in ui & via keybindings.
		/// </summary>
		private void Use() {
			if (item != null) {
				if (item.Equippable) {
					Assign(Container.Item_Equip(item));
				} else {
					item.Use();
				}
			} /*else if (skill != null) {

			}*/
		}
	}
}
