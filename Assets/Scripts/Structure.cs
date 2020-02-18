using UnityEngine;

//To do: add character ownership
//To do: manage door & roof
/// <summary>
/// Buildings the player interacts with. Written by Justin Ortiz
/// </summary>
public class Structure : MonoBehaviour {

	private Color defaultColor;
	private SpriteRenderer sprite;
	[SerializeField, Tooltip("The structure's cell size (x,y)")]
	private Vector2Int dimensions;

	public Vector2Int Dimensions { get { return dimensions; } }

	private void Awake() {
		sprite = GetComponent<SpriteRenderer>();
		defaultColor = sprite.color;
	}

	/// <summary>
	/// Resets the sprite color to default; To be used on structure edit end.
	/// </summary>
	public void ResetColor() {
		if (sprite != null) {
			sprite.color = defaultColor;
		}
	}

	public void SetColor(Color c) {
		if (sprite != null) {
			sprite.color = c;
		}
	}

	private void Start() {
		if (!StructureGridManager.instance.EditEnabled) { //if this structure was instantiated via loading, edit will not be enabled
			StructureGridManager.instance.RegisterExistingStructure(this); //register cells as occupied
		}
	}
}