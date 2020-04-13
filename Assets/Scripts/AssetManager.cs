using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetManager : MonoBehaviour {

	public static AssetManager instance;

	private Dictionary<string, GameObject> prefabs;
	private Dictionary<string, Texture2D> items;
	private Dictionary<string, Texture2D> scenery;

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
	}

	public Item InstantiateItem(int itemID = 0, string itemBaseName = "", int quantity = 1, string textureName = "log") {
		GameObject curr_prefab_reference = prefabs["item"]; //get the prefab from dictionary

		if (curr_prefab_reference != null) { //should exist, but just in case
			if (items.ContainsKey(textureName)) { //if the texture exists
				Texture2D curr_texture_reference = items[textureName]; //get texture from texture2d list

				if (curr_texture_reference != null) { //if the texture was retrieved
					Item currItem = Instantiate(curr_prefab_reference).GetComponent<Item>(); //instantiate copy of prefab

					if (currItem != null) { //if instantiated properly
						currItem.Load(itemID, itemBaseName, quantity, curr_texture_reference); //pass item info and texture
						return currItem;
					}
				}
			}
		}
		return null;
	}

	public SceneryObject InstantiateSceneryObject(Vector3 position, string sceneryType = "bush", string textureName = "",
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
							return currObject;
						}
					}
				}
			}
		}
		return null;
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
