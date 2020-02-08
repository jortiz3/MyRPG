using System.Collections;
using UnityEngine;
using AreaManagerNS;


public class SceneryObject : MonoBehaviour
{
	private void Awake() {
		transform.SetParent(AreaManager.GetEntityParent("Scenery"));
	}
}
