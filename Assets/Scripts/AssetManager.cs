using System.Linq;
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

	/// <summary>
	/// Gets the count of specific assets.
	/// </summary>
	public int GetAssetCount(string assetType, string assetPrefix = "") {
		string[] keys = null;
		assetType = assetType.ToLower(); //standardize asset type
		if (assetType.Contains("item")) { //if type is item
			keys = items.Keys.ToArray(); //get item dictionary keys
		} else if (assetType.Contains("scene")) {
			keys = scenery.Keys.ToArray();
		} else if (assetType.Contains("struct")) {
			keys = structures.Keys.ToArray();
		} else {
			return 0; //not an established type
		}

		int total = 0;
		for (int i = 0; i < keys.Length; i++) {
			if (keys[i].Contains(assetPrefix)) { //key must have the given prefix
				total++;
			}
		}
		return total;
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
						TrimGameObjectName(currItem.gameObject);
						currItem.transform.position = position;
						currItem.Load(itemID, itemBaseName, quantity, curr_texture_reference); //pass item info and texture
						return currItem;
					}
				}
			}
		}
		return null;
	}

	public SceneryObject InstantiateSceneryObject(Vector3 position, string textureName = "bush_0",
		int harvestedItemID = -int.MaxValue, int sceneryObjectHP = 3, bool allowStructureCollision = false) {

		string prefabKey = "scenery_" + SceneryObject.GetSceneryType(textureName);
		if (prefabs.ContainsKey(prefabKey)) {
			GameObject curr_prefab_reference = prefabs[prefabKey]; //get the prefab from dictionary
			if (curr_prefab_reference != null) { //if prefab retrieved
				if (scenery.ContainsKey(textureName)) { //ensure key exists
					Texture2D curr_texture_reference = scenery[textureName]; //get texture2d from dictionary

					if (curr_texture_reference != null) { //if texture retrieved
						SceneryObject currObject; //reference to current scenery object
						currObject = Instantiate(curr_prefab_reference).GetComponent<SceneryObject>(); //instantiate copy of prefab

						if (currObject != null) { //if successfully instantiated
							TrimGameObjectName(currObject.gameObject);
							currObject.transform.position = position; //set the position
							currObject.Load(curr_texture_reference, harvestedItemID, sceneryObjectHP, allowStructureCollision); //pass info to scenery object script
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
					TrimGameObjectName(spawnedPrefab);
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

	private void TrimGameObjectName(GameObject g) {
		g.name = g.name.Replace("(Clone)", "");
	}
}
