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
		if (rt != null) {
			if (GameManager.instance.State_Play) { //game is active
				if (!MenuScript.instance.CurrentState.Equals("")) { //menu is open
					Vector2 localMousePos = rt.InverseTransformPoint(Input.mousePosition);
					if (Input.GetKeyUp(KeyCode.Mouse0)) { //if LMB up; hardcoded
						if (rt.rect.Contains(localMousePos)) {
							ContextManager.instance.Select(transform);
						}
					}

					if (Input.GetKeyDown(KeyCode.Mouse1)) { //if RMB down; hardcoded
						if (rt.rect.Contains(localMousePos)) {
							ContextManager.instance.Show(transform);
						}
					}
				}
			}
		}
	}
}
