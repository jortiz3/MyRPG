using UnityEngine;

/// <summary>
/// Written by Justin Ortiz
/// </summary>
public class SceneryObject : MonoBehaviour {

	[SerializeField]
	private bool allowStructureCollision;

	private void Start() {
		transform.SetParent(AreaManager.GetEntityParent("Scenery"));
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
