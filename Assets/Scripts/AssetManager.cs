using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetManager : MonoBehaviour {

	public static AssetManager instance;

	private Dictionary<string, GameObject> prefabs;
	private Dictionary<string, Texture2D> items;

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
		yield return new WaitForEndOfFrame();
		LoadAssets<Texture2D>(out items, "Textures/");
	}

	public IEnumerator InstantiateItem(string textureName = "log", int quantity = 1, int itemID = 0, string itemBaseName = "") {
		GameObject curr_prefab_reference = prefabs["item"]; //get the prefab from dictionary
		

		if (curr_prefab_reference != null) {
			Item currItem;
			if (items.ContainsKey(textureName)) { //if the texture exists
				Texture2D curr_texture_reference = items[textureName]; //get texture from texture2d list
				for (int iteration = 0; iteration < quantity; iteration++) { //iterate as many times as quantity
					currItem = Instantiate(curr_prefab_reference).GetComponent<Item>(); //instantiate new prefab

					if (currItem != null && curr_texture_reference != null) { //if instantiated properly & texture exists
						currItem.Load(itemID, itemBaseName, curr_texture_reference);
					}
					yield return new WaitForEndOfFrame();
				}
			}
		}
	}

	private void LoadAssets<T>(out Dictionary<string, T> dictionary, string folderPath)
		where T : Object {
		dictionary = new Dictionary<string, T>();
		T[] assets = Resources.LoadAll<T>(folderPath);
		foreach(T asset in assets) {
			dictionary.Add(asset.name, asset);
		}
	}
}
