using System;
using UnityEngine;
using UnityEngine.UI;

namespace HUD_Elements {
	public class HotbarElement : MonoBehaviour {
		public static HotbarElement selected;

		private Button button;
		private Text keybind_display;
		private Item item;

		public bool Assign(Item i) {
			selected = null; //remove selected on each attempt to assign

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
			button = GetComponent<Button>();
			keybind_display = transform.GetChild(0).GetComponent<Text>();
		}

		public void Select() {
			if (MenuScript.instance.CurrentState.Equals("")) {
				Use();
			} else {
				if (ContextManager.instance.Focus != null) {
					//Assign();
					selected = null;
				} else {
					selected = this;
				}
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
