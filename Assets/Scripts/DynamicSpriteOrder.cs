using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DynamicSpriteOrder : MonoBehaviour
{
	private SpriteRenderer sr;

	private void Awake() {
		sr = GetComponent<SpriteRenderer>();
	}

	private void FixedUpdate() {
		sr.sortingOrder = Mathf.RoundToInt(transform.position.y * 100f) * -1;
	}
}
