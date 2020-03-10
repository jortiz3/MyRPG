using UnityEngine;

public class Background : MonoBehaviour
{
	private void Start() {
		transform.SetParent(AreaManager.GetEntityParent("Background"));
	}
}
