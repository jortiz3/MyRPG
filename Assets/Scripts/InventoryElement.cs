using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI elements for either items, skills, or crafting.
/// </summary>
public class InventoryElement : MonoBehaviour {
	private RectTransform rt;

	private void Awake() {
		rt = GetComponent<RectTransform>();
	}

	private void Update() {
		if (rt != null) { //if the transform was retrieved
			if (GameManager.instance.State_Play) { //game is active
				if (!MenuScript.instance.CurrentState.Equals("")) { //menu is open
					Vector2 localMousePos = rt.InverseTransformPoint(Input.mousePosition); //ensure the mousePos is in correct coords
					if (Input.GetKeyUp(KeyCode.Mouse0)) { //if LMB up; hardcoded
						if (rt.rect.Contains(localMousePos)) { //if mouse is within rect
							ContextManager.instance.Select(transform); //select this element
						}
					}

					if (Input.GetKeyDown(KeyCode.Mouse1)) { //if RMB down; hardcoded
						if (rt.rect.Contains(localMousePos)) { //if mouse is within rect
							ContextManager.instance.Show(transform); //show the context menu for this element
						}
					}
				}
			}
		}
	}
}
