using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetManager : MonoBehaviour {

	public static AssetManager instance;

	private void Awake() {
		if (instance != null) {
			Destroy(this);
		} else {
			instance = this;
		}
	}

	public IEnumerator InstantiateItem(string textureName, int quantity = 1, int itemID = -1, string itemBaseName = "") {
		GameObject curr_prefab_reference = null; //get the prefab from resources
		Texture2D curr_texture_reference = null; //get texture from texture2d list

		if (curr_prefab_reference != null) {
			Item currItem;
			for (int iteration = 0; iteration < quantity; iteration++) {
				currItem = Instantiate(curr_prefab_reference).GetComponent<Item>();

				if (currItem != null && curr_texture_reference != null) {
					currItem.Load(itemID, itemBaseName, curr_texture_reference);
				}
				yield return new WaitForEndOfFrame();
			}
		}
	}
}
