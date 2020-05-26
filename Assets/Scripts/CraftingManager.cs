using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the crafting of items & furniture. Written by Justin Ortiz
/// Notes:
/// -sort alphabetically in ui on display
/// -add bool to item database to mark whether item can be crafted
/// --add functionality in item.cs to be a schematic
/// ---change image to schematic icon in both inventory ui and on ground
/// </summary>

public static class CraftingManager {
	private static Toggle uiToggle;
	private static Transform uiParent;
	private static Transform prefab_schematic;

	private static List<int> schematics_item;
	//store learned furniture schematics

	public static void AddSchematic_Item(int itemID) {
		schematics_item.Add(itemID);
	}

	public static int[] GetSchematics_Item() {
		if (schematics_item != null) {
			schematics_item.Sort();
			return schematics_item.ToArray();
		}
		return null;
	}

	public static void Initialize() {
		//get ui parent, get prefab

		schematics_item = new List<int>();

		//uitoggle.onvaluechanged += onDisplay
	}

	public static void LoadSchematics(int[] items /*, int[] furniture*/) {
		if (items != null) {
			schematics_item.AddRange(items);
		}
	}

	private static void OnToggleChanged(bool value) {
		//sort elements
		//reposition scroll rect to top
	}

	private static void RefreshUIElement(int index) {
		if (uiParent != null) {

		}
	}
}
