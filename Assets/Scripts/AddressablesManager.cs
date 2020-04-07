using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

public class AddressablesManager : MonoBehaviour {
	public static AddressablesManager instance;

	[SerializeField]
	private List<AssetReference> prefabs;
	[SerializeField]
	private List<AssetReference> textures;

	private void Awake() {
		if (instance != null) {
			Destroy(this);
		} else {
			instance = this;

			StartCoroutine(Load<AssetReference>("Prefabs", prefabs));
			StartCoroutine(Load<AssetReference>("Items", prefabs));
		}
	}

	private IEnumerator Load<AssetReference>(string key, List<AssetReference> assets) { //throws exception for invalid key
		yield return Addressables.LoadAssetsAsync<AssetReference>(key, obj => {
			assets.Add(obj);
		});
	}

	public static bool AssetExists(object key) {
		if (Application.isPlaying) {
			foreach (var l in Addressables.ResourceLocators) {
				IList<IResourceLocation> locs;
				if (l.Locate(key, null, out locs))
					return true;
			}
			return false;
		} else if (Application.isEditor && !Application.isPlaying) {
#if UNITY_EDITOR
			return File.Exists(System.IO.Path.Combine(Application.dataPath, (string)key));
#endif
		}
		return false;
	}
}
