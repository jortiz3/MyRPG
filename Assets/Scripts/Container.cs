using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores a list of items and displays them to the screen.
/// </summary>
[Serializable]
public class Container : Interactable {
	private static Transform containerParent;

	[SerializeField, HideInInspector]
	private List<Item> items;

	private void Awake() {
		containerParent = GameObject.Find("Inventory_Container_Other").transform;
	}

	public void Display() {
		List<Item> checkRemaining = new List<Item>();
		checkRemaining.AddRange(items);
		foreach (Transform child in containerParent) { //check each child to see if
			for (int i = 0; i < checkRemaining.Count; i++) {
				if (child.name.Equals(checkRemaining[i].ToString())) {
					child.gameObject.SetActive(true);
					checkRemaining.RemoveAt(i);
					break;
				} else {
					child.gameObject.SetActive(false);
				}
			}
		}
	}

	public void RefreshUI() {

	}
}
