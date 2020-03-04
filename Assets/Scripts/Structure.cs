using UnityEngine;

//To do: add character ownership
//To do: add LoadCustomStructure(string baseFileName, string roofFileName, string doorFileName) { load template structure, apply filenames>sprite to sprites[] }
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
	/// Adds a custom structure to the scene.
	/// </summary>
	/// <param name="dimensions">The extra grid cells required by the structure. Default: Size of 1 cell = (0, 0)</param>
	/// <param name="worldPosition">Where the structure should be placed in the world prior to snapping to grid.</param>
	public static void LoadCustomStructure(Sprite sprite_base, Sprite sprite_roof, Sprite sprite_door, Vector2Int dimensions, Vector3 worldPosition) {
		Structure s = Resources.Load<Structure>("Structures/template_structure");
		if (s != null) { //if the template was loaded
			s.sprites[0].sprite = sprite_base; //update the sprites
			s.sprites[1].sprite = sprite_roof;
			s.sprites[2].sprite = sprite_door;

			Instantiate(s.gameObject); //add to the scene
			s.transform.localScale = new Vector3(5 + (5 * dimensions.x), 5 + (5 * dimensions.y), 1); //adjust the size of the building to conform to the grid
			s.transform.position = worldPosition; //set the position
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
		transform.SetParent(AreaManagerNS.AreaManager.GetEntityParent("Structure"));
		if (!StructureGridManager.instance.EditEnabled) { //if this structure was instantiated via loading, edit will not be enabled
			StartCoroutine(StructureGridManager.instance.RegisterExistingStructure(this)); //register cells as occupied
		}
	}
}