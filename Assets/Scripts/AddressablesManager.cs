using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

public class AddressablesManager : MonoBehaviour {
	public static AddressablesManager instance;

	private void Awake() {
		if (instance != null) {
			Destroy(this);
		} else {
			instance = this;

			Addressables.InitializeAsync();

			//StartCoroutine(Load("Prefabs", prefabs_common));
			//StartCoroutine(Load("Items", out textures_common));
		}
	}

	public IEnumerator InstantiateItem(string textureName, int quantity = 1, int itemID = -1, string itemBaseName = "") {
		GameObject curr_prefab_reference = Addressables.LoadAssetAsync<GameObject>("Prefabs").Result; //unable to figure out what they key should be
		Texture2D curr_texture_reference = Addressables.LoadAssetAsync<Texture2D>(textureName).Result;

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

	/*private IEnumerator Load(string key, List<AssetReference> assetReferences) { //throws exception for invalid key
		assetReferences = new List<AssetReference>();

		if (Application.isPlaying) {
			Task task = Addressables.LoadAssetAsync<IResourceLocation>(key).Task;
			while (task.Status == TaskStatus.Running) {
				yield return new WaitForEndOfFrame();
			}
			IList<IResourceLocation> locations = task as IList<IResourceLocation>;
			if (locations != null && locations.Count > 0) {
				//add references
			}
		}
	}*/

	/*public static bool AssetExists(object key, out IList<IResourceLocation> locations) {
		locations = null;
		if (Application.isPlaying) {
			foreach (var l in Addressables.ResourceLocators) {
				if (l.Locate(key, null, out locations))
					return true;
			}
			return false;
		} else if (Application.isEditor && !Application.isPlaying) {
#if UNITY_EDITOR
			return File.Exists(System.IO.Path.Combine(Application.dataPath, (string)key));
#endif
		}
		return false;
	}*/
}
