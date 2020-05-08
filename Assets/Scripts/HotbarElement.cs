using System;
using UnityEngine;
using UnityEngine.UI;

namespace HUD_Elements {
	public class HotbarElement : MonoBehaviour {
		public static HotbarElement selected_hotkey;

		private static Item selected_item;
		private static Color color_default = Color.white;
		private static Color color_highlight = Color.yellow;

		private Image image;
		private Button button;
		private Text keybind_display;
		private Item item;

		public bool Assign(Item i) {
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
		}

		private void SetHighlightActive(bool active) {
			if (image != null) {
				image.color = active ? color_highlight : color_default;
			}
		}

		public void Select() {
			if (MenuScript.instance.CurrentState.Equals("")) {
				Use();
			} else {
				if (selected_hotkey != null) {
					selected_hotkey.SetHighlightActive(false);
				}
				SetHighlightActive(true);
				selected_hotkey = this;
			}
		}

		public void SetButtonInteractable(bool interactable) {
			if (button != null) {
				button.interactable = interactable;
			}
		}

		private void Start() {
			if (keybind_display != null) {
				keybind_display.text = InputManager.instance.GetKeyCodeName(transform.name.Replace("Hotbar_", ""));
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
