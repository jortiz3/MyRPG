using System.Collections;
using UnityEngine;
using AreaManagerNS;

public class Background : MonoBehaviour
{
	private void Awake() {
		transform.SetParent(AreaManager.GetEntityParent("Background"));
	}
}
