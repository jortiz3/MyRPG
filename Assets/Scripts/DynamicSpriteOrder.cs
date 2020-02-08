using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DynamicSpriteOrder : MonoBehaviour
{
	private SpriteRenderer sr;
	[SerializeField, Tooltip("Will this sprite move around? T/F")]
	private bool dynamicSprite;

	private void Awake() {
		sr = GetComponent<SpriteRenderer>();
		sr.sortingOrder = Mathf.RoundToInt(transform.position.y * 100f) * -1;
	}

	private void FixedUpdate() {
		if (dynamicSprite) {
			sr.sortingOrder = Mathf.RoundToInt(transform.position.y * 100f) * -1;
		}
	}
}
