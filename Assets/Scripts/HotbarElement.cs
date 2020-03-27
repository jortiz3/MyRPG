using System;
using UnityEngine;
using UnityEngine.UI;

namespace HUD_Elements {
	public class HotbarElement : MonoBehaviour {
		private Button button;
		private Text keybind_display;
		private Item item;
		private string skill;

		public void Assign(Item i) {
			if (i.Slottable) {
				i.transform.position = transform.position;
				i.transform.parent = transform;
				i.SetSpriteActive(true);
				item = i;

				//remove skill
				if (skill != null) {
					skill = null;
				}
			}
		}

		private void Awake() {
			button = GetComponent<Button>();
			keybind_display = transform.GetChild(0).GetComponent<Text>();
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
		public void Use() {
			if (item != null) {
				item.Use();
			} else if (skill != null) {

			}
		}
	}
}
