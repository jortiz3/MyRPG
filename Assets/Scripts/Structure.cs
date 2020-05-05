using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Buildings the player interacts with. Written by Justin Ortiz
/// </summary>
public class Structure : MonoBehaviour {
	private static int numStructuresToPopulate; //used to track how many structures need to instantiate furniture

	private Color[] defaultColors;
	[SerializeField, Tooltip("All of the sprites that make up the structure.")]
	private SpriteRenderer[] sprites;
	[SerializeField, Tooltip("The structure's additional cell size (x,y); Default size of 1 == (0,0)")]
	private Vector2Int dimensions;
	private string owner; //convert to character? load character in load() using string?
	private string preset;
	private List<Furniture> furniture;
	private bool registeredToManager;
	private float lastUpdated;

	public static bool PopulationInProgress { get { return numStructuresToPopulate > 0; } }
	public Vector2Int Dimensions { get { return dimensions; } }
	public string Owner { get { return owner; } }
	public string Preset { get { return preset; } }
	public bool Registered { get { return registeredToManager; } set { registeredToManager = value; } }
	public float LastUpdated { get { return lastUpdated; } }

	private void Awake() {
		Initialize();
	}

	public IEnumerator GenerateFurniture() {
		numStructuresToPopulate++;

		while (!registeredToManager) {
			yield return new WaitForEndOfFrame();
		}

		string[] furniturePrefixes = null;
		switch (GetDimensionSize(dimensions)) { //different amounts of furniture based on size
			case "small":
				furniturePrefixes = new string[] { "bed_", "chest_", "rug_", "table_", "chair_", "fireplace_" };
				break;
			case "medium":
				furniturePrefixes = new string[] { "bed_", "bed_", "chest_", "rug_", "table_", "chair_", "fireplace_", "stove_", "table_" };
				break;
			case "large":
				furniturePrefixes = new string[] { "bed_", "bed_", "chest_", "rug_", "table_", "chair_", "fireplace_", "stove_", "table_" };
				break;
			default:
				furniturePrefixes = new string[] { "bed_", "chest_", "table_", "chair_" };
				break;
		}

		AssetManager.instance.InstantiateFurniture(transform.position + new Vector3(0, (dimensions.y + 1) * 5), this, "workbench"); //ensure there is always a workbench

		List<Vector3> furniturePositions = new List<Vector3>();
		for (int i = 0; i < furniturePrefixes.Length; i++) {
			furniturePositions.Add(transform.position + new Vector3(i / ((dimensions.y + 1) * 2), (i * 2.5f) % 5));
		}

		if (furniturePrefixes.Length == furniturePositions.Count) {
			for (int i = 0; i < furniturePrefixes.Length; i++) {
				AssetManager.instance.InstantiateFurniture(furniturePositions[i], this, furniturePrefixes[i] + preset);
			}
		}
		numStructuresToPopulate--;
	}

	public static string GetDimensionSize(Vector2Int dimensions) {
		if (dimensions.x == dimensions.y) {
			if (dimensions.x <= 0) {
				return "tiny";
			} else if (dimensions.x <= 1) {
				return "small";
			} else if (dimensions.x <= 2) {
				return "medium";
			} else {
				return "large";
			}
		}
		return "unique";
	}

	public string[] GetTextures() {
		if (sprites != null && sprites.Length > 0) {
			string[] textureNames = new string[sprites.Length];
			for (int i = 0; i < textureNames.Length; i++) {
				if (sprites[i] != null) {
					textureNames[i] = sprites[i].sprite.texture.name;
				}
			}
			return textureNames;
		}
		return null;
	}

	public void Initialize() {
		if (sprites != null && sprites.Length > 0) { //if there are managed sprites
			defaultColors = new Color[sprites.Length]; //instantiate the default colors array
			for (int i = 0; i < defaultColors.Length; i++) { //get the default colors from sprites
				defaultColors[i] = sprites[i].color;
			}
		}
	}

	/// <summary>
	/// Passes required information to an already-instantiated structure.
	/// </summary>
	public void Load(Vector2Int Dimensions, string Owner = "Player", string Preset = "default", bool instantiateFurniture = false, Texture2D[] textures = null, float LastUpdated = 0) {
		dimensions = Dimensions;
		owner = Owner;
		preset = Preset;
		lastUpdated = LastUpdated; //replace owner after certain time

		if (instantiateFurniture) {
			StartCoroutine(GenerateFurniture());
		}

		SetSprites(textures);
	}

	public void RegisterFurniture(Furniture f) {
		if (furniture == null) {
			furniture = new List<Furniture>();
		}
		f.SetStructureParent(this);
		furniture.Add(f);
	}

	/// <summary>
	/// Resets the sprite color to default; To be used on structure edit end.
	/// </summary>
	public void ResetColor() {
		if (sprites != null) {
			if (defaultColors != null) {
				if (defaultColors.Length == sprites.Length) {
					for (int i = 0; i < sprites.Length; i++) {
						sprites[i].color = defaultColors[i];
					}
				}
			}
		}
	}

	public void ResetFurnitureTransformParent() {
		if (furniture != null) {
			for (int i = 0; i < furniture.Count; i++) {
				furniture[i].ResetTransformParent();
			}
		}
	}

	public void SetColor(Color c) {
		if (sprites != null) {
			for (int i = 0; i < sprites.Length; i++) {
				sprites[i].color = c;
			}
		}
	}

	public void SetFurnitureAsTransformChildren() {
		if (furniture != null) {
			for (int i = 0; i < furniture.Count; i++) {
				furniture[i].SetTransformParent(transform);
			}
		}
	}

	public void SetOwner(string name) {
		owner = name;
	}

	private void SetSprites(Texture2D[] textures) {
		if (textures != null) { //if the array isn't null
			if (textures.Length == sprites.Length) { //if the array has the same amount of textures as structure manages sprites
				Vector2 defaultSpriteSize = new Vector2((dimensions.x + 1) * 5, (dimensions.y + 1) * 5); //get sizes for roof and floor
				Vector2 doorSpriteSize = defaultSpriteSize / 3f;
				for (int i = 0; i < sprites.Length; i++) { //go through all textures/sprites
					if (sprites[i] != null && textures[i] != null) { //ensure corresponding texture-sprite pair exists
						sprites[i].sprite = Sprite.Create(textures[i], new Rect(0, 0, textures[i].width, textures[i].height), new Vector2(0.5f, 0.5f), 16f); //create sprite using given texture

						if (sprites[i].gameObject.name.ToLower().Contains("door")) {
							sprites[i].size = doorSpriteSize;
						} else {
							sprites[i].size = defaultSpriteSize;
						}
					}
				}
			}
		}
	}

	private void Start() {
		transform.SetParent(AreaManager.GetEntityParent("Structure"));
		if (!StructureGridManager.instance.EditEnabled) { //if this structure was instantiated via loading, edit will not be enabled
			StructureGridManager.instance.RegisterExistingStructure(this); //register cells as occupied
		}
	}
}