using System.Collections;
using UnityEngine;
using AreaManagerNS;


public class SceneryObject : MonoBehaviour
{
	private void Start() {
		transform.SetParent(AreaManager.GetEntityParent("Scenery"));
	}
}
