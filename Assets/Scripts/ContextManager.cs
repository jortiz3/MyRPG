using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using HUD_Elements;

/// <summary>
/// Displays the context menu with the appropriate buttons/info, and works with HotbarElement.cs to assign things to the hotbar. Written by Justin Ortiz
/// </summary>
public class ContextManager : MonoBehaviour {
	public static ContextManager instance;

	private Transform currFocus;
	private Transform context_item;

	public Transform Focus { get { return currFocus; } }

	private void Awake() {
		if (instance == null) {
			context_item = transform.Find("context_container");

			if (context_item != null) {
				context_item.gameObject.SetActive(false);
			}

			instance = this;
		} else {
			Destroy(gameObject);
		}
	}

	public void Container_Drop(Slider slider_quantity) { //display slider, input field, and button to submit result
		Container.Drop(currFocus.name, (int)slider_quantity.value);
	}

	public void Container_Equip() {
		Container.Item_Equip(currFocus.name);
	}

	public void Container_Transfer(Slider slider_quantity) {
		Container.Transfer(currFocus.name, (int)slider_quantity.value);
	}

	public void Container_Use() {
		Container.Item_Use(currFocus.name);
	}

	public void Hide() {
		context_item.gameObject.SetActive(false);
		QuantityManager.instance.Hide();
	}

	public void Select(Transform newFocus) {
		currFocus = null; //start focus as null

		bool used = false;
		switch (newFocus.tag) {
			case "uielement_container":
				if (HotbarElement.selected != null) {
					if (HotbarElement.selected.Assign(Container.GetDisplayedItem(newFocus.name))) {
						used = true;
					}
				}
				break;
			default:
				break;
		}

		if (!used) {
			currFocus = newFocus;
		}

		Hide();
	}

	public void Show(Transform focus) {
		switch (focus.tag) {
			case "uielement_container":
				if (context_item != null) {
					Vector3 menuPos = Input.mousePosition; //get starting pos for menu
					Vector2 menuSize = context_item.GetComponent<RectTransform>().sizeDelta; //get size of context menu
					if (menuPos.y - menuSize.y < 0) { //if the menu will go off screen vertically
						menuPos.y += menuSize.y; //adjust pos y
					}
					if (menuPos.x + menuSize.x > Screen.width) { //if the menu will go off screen horizontally
						menuPos.x -= menuSize.x; //adjust pos x
					}

					context_item.position = menuPos; //set the position of the context menu

					GameObject use = context_item.Find("button_use").gameObject;
					GameObject equip = context_item.Find("button_equip").gameObject;

					Item currItem = Container.GetDisplayedItem(focus.name);
					if (currItem != null) {
						if (currItem.Equipable) {
							use.SetActive(false);
							equip.SetActive(true);
						} else {
							use.SetActive(true);
							equip.SetActive(false);
						}

						GameObject transfer = context_item.Find("button_transfer").gameObject;
						GameObject drop = context_item.Find("button_drop").gameObject;
						if (Container.GetTransferAvailable()) {
							transfer.SetActive(true);
							drop.SetActive(false);
						} else {
							transfer.SetActive(false);
							drop.SetActive(true);
						}
						QuantityManager.instance.SetQuantityRange(currItem.Quantity);
						context_item.Find("text_header_context_container").GetComponent<Text>().text = currItem.ToString();
						currFocus = focus;
						context_item.gameObject.SetActive(true);
					} else {
						currFocus = null;
					}
				}
				break;
			default:
				Hide();
				break;
		}
	}
}
