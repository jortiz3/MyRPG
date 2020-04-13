using UnityEngine;

/// <summary>
/// Written by Justin Ortiz
/// </summary>
public class SceneryObject : Interactable { //convert to Interactable; store item id; if no item id or no quantity left, disable interaction
	private int harvestCount;
	private int harvestedItemID;
	private bool allowStructureCollision;
	private bool loaded;

	protected override void Initialize() { //called in Start()
		transform.SetParent(AreaManager.GetEntityParent("Scenery"));
		if (!loaded) {
			harvestCount = 0;
			harvestedItemID = -1;
			allowStructureCollision = false;
			DisableInteraction();
		}
		base.Initialize();
	}

	protected override void InteractInternal() {
		if (harvestCount > 0) {
			Inventory.instance.Add(AssetManager.instance.InstantiateItem(quantity:5, itemBaseName:"Log"));
			harvestCount--;
		}

		if (harvestCount <= 0) {
			DisableInteraction();
		}
		base.InteractInternal();
	}

	public void Load(int HarvestCount, int HarvestedItemID, Texture2D Texture, bool AllowCollisionWithStructures = false) {
		harvestCount = HarvestCount;
		harvestedItemID = HarvestedItemID;
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
