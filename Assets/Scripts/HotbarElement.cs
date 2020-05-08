using System;
using UnityEngine;
using UnityEngine.UI;

namespace HUD_Elements {
	public class HotbarElement : MonoBehaviour {
		public static HotbarElement selected_hotkey;

		private static Color color_default = Color.white;
		private static Color color_highlight = Color.yellow;
		private static bool hotkeyAssignmentActive;

		private Image image;
		private Button button;
		private Text keybind_display;
		private Item item;

		public static bool HotkeyAssignmentActive { get { return hotkeyAssignmentActive; } }

		private bool Assign(Item i) {
			selected_hotkey = null; //remove selected on each attempt to assign

			if (i != null) {
				if (i.Slottable) {
					i.transform.position = transform.position;
					i.transform.parent = transform;
					i.SetInteractionActive(true);
					item = i;

					//remove skill
					/*if (skill != null) {
						skill = null;
					}*/
					return true;
				}
			}
			return false;
		}

		private void Awake() {
			image = GetComponent<Image>();
			button = GetComponent<Button>();
			keybind_display = transform.GetChild(0).GetComponent<Text>();
			HUD.instance.RegisterHotbarElement(this);
		}

		public static void BeginHotkeyAssignment() {
			hotkeyAssignmentActive = true;
		}

		public static void EndHotkeyAssignment(Item item) {
			if (hotkeyAssignmentActive) {
				if (item != null) {

				}
			}
			hotkeyAssignmentActive = false;
		}

		public static void UnselectHotkey() {
			if (selected_hotkey != null) {
				selected_hotkey.SetHighlightActive(false);
				selected_hotkey = null;
			}
		}

		public void Select() {
			if (MenuScript.instance.CurrentState.Equals("")) { //if game is underway
				Use(); //use what is in the slot
			} else { //viewing inventory
				UnselectHotkey();
				SetHighlightActive(true);
				selected_hotkey = this;
			}
		}

		public void SetButtonInteractable(bool interactable) {
			if (button != null) {
				button.interactable = interactable;
			}
		}

		public void SetHighlightActive(bool active) {
			if (image != null) {
				image.color = active ? color_highlight : color_default;
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
				item.Use();
			} /*else if (skill != null) {

			}*/
		}
	}
}
