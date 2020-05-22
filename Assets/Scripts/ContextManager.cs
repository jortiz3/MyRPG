using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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

	public void HUD_BeginHotkeyAssignment() {
		HUD.BeginHotkeyAssignment(Container.GetDisplayedItem(currFocus.name));
	}

	public void Select(Transform newFocus) {
		currFocus = null;

		switch (newFocus.tag) {
			case "uielement_container":
				Item item = Container.GetDisplayedItem(newFocus.name);
				if (!HUD.SelectItem(item)) {
					currFocus = newFocus;
				}
				break;
			default:
				break;
		}

		Hide();
	}

	public void Show(Transform focus) {
		switch (focus.tag) {
			case "uielement_container":
				if (context_item != null) {
					Item currItem = Container.GetDisplayedItem(focus.name);
					if (currItem != null) { //if item info obtained
						GameObject use = context_item.Find("button_use").gameObject;
						GameObject equip = context_item.Find("button_equip").gameObject;

						if (currItem.Equippable) { //if equipable
							use.SetActive(false); //show 'equip' instead of 'use'
							equip.SetActive(true);
						} else { //if not equipable
							use.SetActive(true); //show 'use' instead of 'equip'
							equip.SetActive(false);
						}

						GameObject transfer = context_item.Find("button_transfer").gameObject;
						GameObject drop = context_item.Find("button_drop").gameObject;
						if (Container.GetTransferAvailable()) { //if player is interacting with container
							transfer.SetActive(true); //show 'transfer' instead of 'drop'
							drop.SetActive(false);
						} else { //player is not interacting with container
							transfer.SetActive(false); //show 'drop' instead of 'transfer'
							drop.SetActive(true);
						}

						GameObject assign = context_item.Find("button_assign").gameObject;
						RectTransform menuRect = context_item.GetComponent<RectTransform>(); //store reference to rect transform
						Vector2 menuSize = menuRect.sizeDelta; //get the initial size (mainly for width)
						menuSize.y = assign.GetComponent<RectTransform>().sizeDelta.y; //set size to size of 1 element
						if (currItem.Slottable) { //if the item is slottable
							assign.SetActive(true); //display assign button
							menuSize.y *= 4; //adjust the size of the menu
						} else { //item not slottable
							assign.SetActive(false); //hide the assign button
							menuSize.y *= 3; //adjust the size of the menu
						}
						menuRect.sizeDelta = menuSize; //set the size


						Vector3 menuPos = Input.mousePosition; //get starting pos for menu
						if (menuPos.y - menuSize.y < 0) { //if the menu will go off screen vertically
							menuPos.y += menuSize.y; //adjust pos y
						}
						if (menuPos.x + menuSize.x > Screen.width) { //if the menu will go off screen horizontally
							menuPos.x -= menuSize.x; //adjust pos x
						}
						context_item.position = menuPos; //set the position of the context menu

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
