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
		LoadPrefabs(out prefabs, "Prefabs/");
		yield return new WaitForEndOfFrame();

		//LoadTextures(out items, "Textures/Items/");
	}

	public IEnumerator InstantiateItem(string textureName = "default", int quantity = 1, int itemID = -1, string itemBaseName = "") {
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

	private void LoadPrefabs(out Dictionary<string, GameObject> dictionary, string folderPath) {
		dictionary = new Dictionary<string, GameObject>();
		GameObject[] objs = Resources.LoadAll<GameObject>(folderPath);
		foreach(GameObject g in objs) {
			dictionary.Add(g.name, g);
		}
	}

	private void LoadTextures(out Dictionary<string, Texture2D> dictionary, string folderPath) {
		dictionary = new Dictionary<string, Texture2D>();
		Texture2D[] textures = Resources.LoadAll<Texture2D>(folderPath);
		foreach(Texture2D t in textures) {
			dictionary.Add(t.name, t);
		}
	}
}
