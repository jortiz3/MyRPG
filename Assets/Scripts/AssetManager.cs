using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using internal_Area;

public class AssetManager : MonoBehaviour {

	public static AssetManager instance;

	private Dictionary<string, GameObject> prefabs;
	private Dictionary<string, Texture2D> items;
	private Dictionary<string, Texture2D> scenery;
	private Dictionary<string, Texture2D> structures;

	private void Awake() {
		if (instance != null) {
			Destroy(this);
		} else {
			instance = this;
			StartCoroutine(Initialize());
		}
	}

	private IEnumerator Initialize() {
		LoadAssets<GameObject>(out prefabs, "Prefabs/");
		LoadAssets<Texture2D>(out items, "Textures/Items/");
		yield return new WaitForEndOfFrame();
		LoadAssets<Texture2D>(out scenery, "Textures/Scenery");
		yield return new WaitForEndOfFrame();
		LoadAssets<Texture2D>(out structures, "Textures/Structures");
	}

	public Item InstantiateItem(Vector3 position, int itemID = 0, string itemBaseName = "", int quantity = 1, string textureName = "log") {
		GameObject curr_prefab_reference = prefabs["item"]; //get the prefab from dictionary

		if (curr_prefab_reference != null) { //should exist, but just in case
			if (items.ContainsKey(textureName)) { //if the texture exists
				Texture2D curr_texture_reference = items[textureName]; //get texture from texture2d list

				if (curr_texture_reference != null) { //if the texture was retrieved
					Item currItem = Instantiate(curr_prefab_reference).GetComponent<Item>(); //instantiate copy of prefab

					if (currItem != null) { //if instantiated properly
						currItem.transform.position = position;
						currItem.Load(itemID, itemBaseName, quantity, curr_texture_reference); //pass item info and texture
						return currItem;
					}
				}
			}
		}
		return null;
	}

	public SceneryObject InstantiateSceneryObject(Vector3 position, string sceneryType = "bush", string textureName = "bush_0",
		int harvestedItemID = 0, int sceneryObjectHP = 3, bool allowStructureCollision = false) {

		string prefabKey = "scenery_" + sceneryType;
		if (prefabs.ContainsKey(prefabKey)) {
			GameObject curr_prefab_reference = prefabs[prefabKey]; //get the prefab from dictionary
			if (curr_prefab_reference != null) { //if prefab retrieved
				if (scenery.ContainsKey(textureName)) { //ensure key exists
					Texture2D curr_texture_reference = scenery[textureName]; //get texture2d from dictionary

					if (curr_texture_reference != null) { //if texture retrieved
						SceneryObject currObject; //reference to current scenery object
						currObject = Instantiate(curr_prefab_reference).GetComponent<SceneryObject>(); //instantiate copy of prefab

						if (currObject != null) { //if successfully instantiated
							currObject.transform.position = position; //set the position
							currObject.Load(harvestedItemID, sceneryObjectHP, curr_texture_reference, allowStructureCollision); //pass info to scenery object script
							return currObject;
						}
					}
				}
			}
		}
		return null;
	}

	public Structure InstantiateStructure(Vector3 position, Vector2Int dimensions, string owner = "Player", string[] textureNames = null) {
		string structureSize = Structure.GetDimensionSize(dimensions);
		string prefabKey = "structure_" + structureSize;

		if (prefabs.ContainsKey(prefabKey)) {
			GameObject curr_prefab_reference = prefabs[prefabKey]; //get the prefab from dictionary

			if (curr_prefab_reference != null) { //if prefab retrieved
				GameObject spawnedPrefab = Instantiate(curr_prefab_reference);
				Structure spawnedStructure = spawnedPrefab.GetComponent<Structure>(); //instantiate copy of prefab
				if (spawnedStructure != null) { //if prefab has required component
					if (textureNames == null) { //if optional parameter not used
						textureNames = new string[] { "floor_default", "roof_default", "door_default"}; //set to defaults
					}

					Texture2D[] curr_texture_references = new Texture2D[textureNames.Length]; //create references for the textures
					for (int iteration = 0; iteration < curr_texture_references.Length; iteration++) { //go through all texture names
						if (structures.ContainsKey(textureNames[iteration])) { //if texture exists
							curr_texture_references[iteration] = structures[textureNames[iteration]]; //add to references
						}
					}//end for

					if (position != null) { //if given valid position
						spawnedStructure.transform.position = position; //set the position of the structure
					}
					spawnedStructure.Load(dimensions, owner, curr_texture_references); //pass required info to structure
					return spawnedStructure; //return reference to spawned structure
				} else { //necessary component not on gameobject
					Destroy(spawnedPrefab); //destroy the recently spawned gameobject
				} //endif structure component
			}//endif prefab reference != null
		}//endif prefabs.containskey
		return null; //somehow procedure failed, return null reference
	}

	private void LoadAssets<T>(out Dictionary<string, T> dictionary, string folderPath)
		where T : Object {
		dictionary = new Dictionary<string, T>();
		T[] assets = Resources.LoadAll<T>(folderPath);
		foreach (T asset in assets) {
			dictionary.Add(asset.name, asset);
		}
	}
}
