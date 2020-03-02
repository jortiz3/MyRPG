using System.Collections;
using UnityEngine;
using AreaManagerNS;

/// <summary>
/// Written by Justin Ortiz
/// </summary>
public class SceneryObject : MonoBehaviour
{
	private void Start() {
		transform.SetParent(AreaManager.GetEntityParent("Scenery"));
	}

	private void OnTriggerEnter(Collider other) {
		if (other.CompareTag("structure")) { //overlapping with structure
			if (StructureGridManager.instance.EditFinalizing) { //if the player is finalizing a structure
				Destroy(gameObject); //destroy this object
			} else if (!StructureGridManager.instance.EditEnabled) { //if player isn't moving the structure around in edit mode >> game is loading an area
				Vector3 newPos = transform.position + (Vector3.up * 3f); //try to move object up
				bool newPosFound = false;
				if (newPos.y < AreaManager.AreaSize) { //if new position is in bounds
					transform.position = newPos; //set the position
					newPosFound = true; //stop checking
				}

				if (!newPosFound) { //if prev check didn't work
					newPos = transform.position + (Vector3.left * 3f); //try left
					if (newPos.x > -AreaManager.AreaSize) { //if in bounds
						transform.position = newPos; //set the position
						newPosFound = true; //stop checking
					}

					if (!newPosFound) { //if prev check didn't work
						newPos = transform.position + (Vector3.right * 3f); //try right
						if (newPos.x < AreaManager.AreaSize) { //if in bounds
							transform.position = newPos; //set the position
							newPosFound = true; //stop checking
						}

						if (!newPosFound) { //if prev check didn't work
							newPos = transform.position + (Vector3.down * 3f); //try down
							if (newPos.y > -AreaManager.AreaSize) { //if in bounds
								transform.position = newPos; //set the position
							} else { //no positions suitable; technically not possible, but just in case
								Destroy(gameObject);
							}
						}
					}
				}
			}
		}
	}
}
