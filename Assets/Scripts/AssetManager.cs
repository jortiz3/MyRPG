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
	private Dictionary<string, Texture2D> furniture;

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
		yield return new WaitForEndOfFrame();
		LoadAssets<Texture2D>(out furniture, "Textures/Furniture");
	}

	public Character InstantiateCharacter() {
		return null;
	}

	public Furniture InstantiateFurniture(Vector3 position, Structure parentStructure = null, string owner = "", string textureName = "chest_default", float lastUpdated = 0) {
		string prefabKey = "furniture";
		GameObject spawnedPrefab = null;

		if (prefabs.ContainsKey(prefabKey)) { //if furniture prefab exists
			if (furniture.ContainsKey(textureName)) { //if texture exists
				GameObject curr_prefab_reference = prefabs[prefabKey]; //get the prefab

				if (curr_prefab_reference != null) { //shouldn't happen but just in case
					spawnedPrefab = Instantiate(curr_prefab_reference); //spawn copy of prefab
					Furniture spawnedFurniture = spawnedPrefab.GetComponent<Furniture>(); //get furniture component
					if (spawnedFurniture != null) { //if component retrieved
						TrimGameObjectName(spawnedPrefab); //trim off "(Clone)"
						Texture2D curr_texture_reference = furniture[textureName]; //get texture2d from dictionary
						if (curr_texture_reference != null) { //if texture retrieved
							spawnedFurniture.Load(curr_texture_reference, parentStructure, owner, lastUpdated);
						}
						if (position != null) { //if given valid position
							spawnedFurniture.transform.position = position; //set the position of the object
						}
						return spawnedFurniture;
					}
				} //endif prefab reference != null
			} //endif furniture.containskey
		} //endif prefabs.containskey

		if (spawnedPrefab != null) { //if something didn't go right, and spawnedprefab was assigned
			Destroy(spawnedPrefab); //remove object from scene
		}
		return null; //return null furniture script
	}

	public Item InstantiateItem(Vector3 position, int itemID = 0, int containerID = -1, string itemPrefix = "", string itemBaseName = "",
		string itemSuffix = "", int quantity = 1, string textureName = "log", float lastUpdated = 0) {
		string prefabKey = "item";
		GameObject spawnedPrefab = null;

		if (prefabs.ContainsKey(prefabKey)) { //should exist, but just in case
			if (items.ContainsKey(textureName)) { //if the texture exists
				GameObject curr_prefab_reference = prefabs[prefabKey]; //get the prefab from dictionary
				Texture2D curr_texture_reference = items[textureName]; //get texture from texture2d list

				if (curr_texture_reference != null) { //if the texture was retrieved
					spawnedPrefab = Instantiate(curr_prefab_reference);
					Item currItem = spawnedPrefab.GetComponent<Item>(); //instantiate copy of prefab

					if (currItem != null) { //if instantiated properly
						TrimGameObjectName(currItem.gameObject);
						currItem.transform.position = position;
						currItem.Load(ID: itemID, ContainerID: containerID, Prefix: itemPrefix, BaseName: itemBaseName, Suffix: itemSuffix,
							Quantity: quantity, Texture: curr_texture_reference, LastUpdated: lastUpdated); //pass item info and texture
						return currItem;
					}
				}
			}
		}
		if (spawnedPrefab != null) { //if something didn't go right, and spawnedprefab was assigned
			Destroy(spawnedPrefab); //remove object from scene
		}
		return null;
	}

	public GameObject InstantiatePrefab(Vector3 position, string prefabName) {
		if (prefabs.ContainsKey(prefabName)) {
			GameObject spawnedPrefab = Instantiate(prefabs[prefabName], position, Quaternion.identity);
			TrimGameObjectName(spawnedPrefab);
			return spawnedPrefab;
		}
		return null;
	}

	public SceneryObject InstantiateSceneryObject(Vector3 position, string textureName = "bush_0",
		int harvestedItemID = -int.MaxValue, int sceneryObjectHP = 3, bool allowStructureCollision = false, float lastUpdated = 0) {
		string prefabKey = "scenery_" + SceneryObject.GetSceneryType(textureName.Split('_')[0]);
		GameObject spawnedPrefab = null;
		if (prefabs.ContainsKey(prefabKey)) {
			GameObject curr_prefab_reference = prefabs[prefabKey]; //get the prefab from dictionary
			if (curr_prefab_reference != null) { //if prefab retrieved
				if (scenery.ContainsKey(textureName)) { //ensure key exists
					Texture2D curr_texture_reference = scenery[textureName]; //get texture2d from dictionary

					if (curr_texture_reference != null) { //if texture retrieved
						spawnedPrefab = Instantiate(curr_prefab_reference);
						SceneryObject currObject = spawnedPrefab.GetComponent<SceneryObject>(); //instantiate copy of prefab

						if (currObject != null) { //if successfully instantiated
							TrimGameObjectName(currObject.gameObject);
							currObject.transform.position = position; //set the position
							currObject.Load(curr_texture_reference, harvestedItemID, sceneryObjectHP, allowStructureCollision, lastUpdated); //pass info to scenery object script
							return currObject;
						}
					}
				}
			}
		}
		if (spawnedPrefab != null) { //if something didn't go right, and spawnedprefab was assigned
			Destroy(spawnedPrefab); //remove object from scene
		}
		return null;
	}

	public Structure InstantiateStructure(Vector3 position, Vector2Int dimensions, string owner = "Player", string preset = "default",
		bool instantiateFurniture = false, string[] textureNames = null, float lastUpdated = 0) {
		string prefabKey = "structure_" + Structure.GetDimensionSize(dimensions);
		GameObject spawnedPrefab = null;

		if (prefabs.ContainsKey(prefabKey)) {
			GameObject curr_prefab_reference = prefabs[prefabKey]; //get the prefab from dictionary

			if (curr_prefab_reference != null) { //if prefab retrieved
				spawnedPrefab = Instantiate(curr_prefab_reference);
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
						spawnedStructure.transform.position = position; //set the position of the object
					}
					spawnedStructure.Load(dimensions, owner, preset, instantiateFurniture, curr_texture_references, lastUpdated); //pass required info to structure
					return spawnedStructure; //return reference to spawned structure
				}
			}//endif prefab reference != null
		}//endif prefabs.containskey
		if (spawnedPrefab != null) { //if something didn't go right, and spawnedprefab was assigned
			Destroy(spawnedPrefab); //remove object from scene
		}
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
