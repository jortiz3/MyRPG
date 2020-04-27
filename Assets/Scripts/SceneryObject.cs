using UnityEngine;
using internal_Area;
using internal_Items;

/// <summary>
/// Written by Justin Ortiz
/// </summary>
public class SceneryObject : Interactable { //convert to Interactable; store item id; if no item id or no quantity left, disable interaction
	private static string[] types = new string[] { "bush", "tree", "rock" };

	private int harvestedItemID;
	private int harvestCount;
	private bool allowStructureCollision;
	private bool loaded;

	public int HarvestCount { get { return harvestCount; } }
	public int HarvestedItemID { get { return harvestedItemID; } }
	public bool AllowStructureCollision { get { return allowStructureCollision; } }

	private int GetHarvestedItemID(string textureName) {
		switch (GetSceneryType(textureName)) { //add check for areatype?
			case "bush":
				return ItemDatabase.GetItemID("Berry");
			case "rock":
				return ItemDatabase.GetItemID("Rock");
			default: //trees
				return ItemDatabase.GetItemID("Log");
		}
	}
	
	public static string GetSceneryType(string textureName) {
		for (int i = 0; i < types.Length; i++) { //loop through types
			if (textureName.Contains(types[i])) { //if the texture contains the type
				return types[i]; //return the type
			}
		}
		return types[0]; //if not found, return base type
	}

	protected override void Initialize() { //called in Start()
		transform.SetParent(AreaManager.GetEntityParent("Scenery"));
		if (!loaded) {
			harvestCount = 0;
			harvestedItemID = -1;
			allowStructureCollision = false;
			DisableInteraction();
		} else {
			if (harvestCount < 1 || harvestedItemID < 0) {
				DisableInteraction();
			}
		}
		base.Initialize();
	}

	protected override void InteractInternal() {
		if (harvestCount > 0) {
			if (harvestedItemID > 0) {
				Inventory.instance.Add(AssetManager.instance.InstantiateItem(transform.position, itemID: harvestedItemID, quantity: 5));
				harvestCount--;
			} else {
				harvestCount = 0;
			}
		} else {
			//inform player that nothing happens
		}

		if (harvestCount <= 0) {
			DisableInteraction();
		}
		base.InteractInternal();
	}

	public void Load(Texture2D Texture, int HarvestedItemID = -int.MaxValue, int HarvestCount = 0, bool AllowCollisionWithStructures = false) {
		harvestedItemID = HarvestedItemID == -int.MaxValue ? GetHarvestedItemID(Texture.name) : HarvestedItemID;
		harvestCount = HarvestCount; //ensure not to display interaction
		allowStructureCollision = AllowCollisionWithStructures;
		SetSprite(Texture);
		loaded = true;
	}

	private void OnTriggerStay(Collider other) {
		if (!allowStructureCollision) {
			if (other.CompareTag("structure")) { //overlapping with structure
				if (StructureGridManager.instance.EditFinalizing) { //if the player is finalizing a structure
					Destroy(gameObject); //destroy this object
				} else if (!StructureGridManager.instance.EditEnabled) { //if player isn't moving the structure around in edit mode >> game is loading an area
					Vector3 newPos; //stores the potential new position
					bool newPosFound = false;
					float distanceScale = 10f;

					for (int x = -1; x < 2; x++) { //x to be used for direction
						for (int y = -1; y < 2; y++) { //y to be used for direction
							newPos = transform.position + (new Vector3(x, y, 0) * distanceScale); //get the potential new position

							if (newPos.y < AreaManager.AreaSize && -AreaManager.AreaSize < newPos.y) { //if the y is in bounds
								if (newPos.x < AreaManager.AreaSize && -AreaManager.AreaSize < newPos.x) { //if the x is in bounds
									transform.position = newPos; //set the position
									newPosFound = true; //allows us to break out of outer loop
									break; //stop processing y
								}
							}
						}

						if (newPosFound) { //if we have a new pos
							break; //stop processing x
						}
					}

					if (!newPosFound) { //if made it through both loops without new pos
						Destroy(gameObject); //just delete object
					}
				}
			}
		}
	}
}
