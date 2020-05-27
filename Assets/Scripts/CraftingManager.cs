using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the crafting of items & furniture. Written by Justin Ortiz
/// Notes:
/// 
/// -add sort functions
/// --sort alphabetically
/// --sort by schematic type, then by alphabetically within type
/// -add remove .slottable from itemInfo >> determine item slottable based on .equipable || .consumable
/// --add functionality in item.cs to be a schematic
/// ---change image to schematic icon in both inventory ui and on ground
/// -show/hide tab on inventory open/close if crafting table utilized; hide parent_component on inventory close
/// --use unity event on menuchange? possibly simplify update method in GameManager.cs
/// </summary>

public class CraftingManager : MonoBehaviour {
	public static CraftingManager instance;

	private static Toggle uiToggle;
	private static Transform parent_component; //the parent component selection ui
	private static Transform parent_schematic; //the parent for schematic ui
	private static Transform prefab_schematic;
	private static List<int> schematics_item;
	//store learned furniture schematics
	private static Item component_main;
	private static Item component_detail;

	public static void AddSchematic_Item(int itemID) {
		schematics_item.Add(itemID);
	}

	void Awake() {
		if (instance == null) {
			uiToggle = GameObject.Find("Toggle_Inventory_Other").GetComponent<Toggle>();
			uiToggle.onValueChanged.AddListener(OnToggleChanged);

			parent_component = transform.parent.Find("Inventory_Crafting_Component_Selection");
			parent_component.Find("Button_Crafting_Submit").GetComponent<Button>().onClick.AddListener(OnCraftingSubmit);

			parent_schematic = transform.Find("ScrollRect_Inventory_Crafting").Find("Inventory_Crafting_Content");
			prefab_schematic = parent_schematic.GetChild(0);

			schematics_item = new List<int>();

			instance = this;
		} else {
			Destroy(this); //destroy only this component
		}
	}

	public static int[] GetSchematics_Item() {
		if (schematics_item != null) {
			schematics_item.Sort();
			return schematics_item.ToArray();
		}
		return null;
	}

	public static void LoadSchematics(int[] items /*, int[] furniture*/) {
		if (items != null) {
			schematics_item.AddRange(items);
		}
	}

	private static void OnCraftingSubmit() {
		if (component_main != null) {


			component_main = null;
			component_detail = null;
			parent_component.gameObject.SetActive(false);
		}
	}

	private static void OnToggleChanged(bool value) {
		//sort elements
		//reposition scroll rect to top
	}

	private static void RefreshUIElement() {
		if (parent_schematic != null) {

		}
	}
}
