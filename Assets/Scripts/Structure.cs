using UnityEngine;

//To do: add character ownership
//To do: manage door & roof
/// <summary>
/// Buildings the player interacts with. Written by Justin Ortiz
/// </summary>
public class Structure : MonoBehaviour {

	private Color defaultColor;
	[SerializeField, Tooltip("All of the sprites that make up the structure.")]
	private SpriteRenderer[] sprites;
	[SerializeField, Tooltip("The structure's additional cell size (x,y); Default size of 1 == (0,0)")]
	private Vector2Int dimensions;

	public Vector2Int Dimensions { get { return dimensions; } }

	private void Awake() {
		if (sprites != null && sprites.Length > 0) {
			defaultColor = sprites[0].color;
		}
	}

	/// <summary>
	/// Resets the sprite color to default; To be used on structure edit end.
	/// </summary>
	public void ResetColor() {
		SetColor(defaultColor);
	}

	public void SetColor(Color c) {
		if (sprites != null) {
			for (int i = 0; i < sprites.Length; i++) {
				sprites[i].color = c;
			}
		}
	}

	private void Start() {
		if (!StructureGridManager.instance.EditEnabled) { //if this structure was instantiated via loading, edit will not be enabled
			StartCoroutine(StructureGridManager.instance.RegisterExistingStructure(this)); //register cells as occupied
		}
	}
}