using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ContextMenu : MonoBehaviour, IPointerDownHandler {
	private Transform context;
	private Transform prefab_button; //prefab/template to use when creating buttons
	private GraphicRaycaster grc;

	private void Awake() {
		grc = MenuScript.instance.gameObject.GetComponent<GraphicRaycaster>();
		prefab_button = transform.Find("template_button");

		if (prefab_button != null) {
			prefab_button.gameObject.SetActive(false);
		}
	}

	public void OnPointerDown(PointerEventData eventData) {
		if (eventData.button == PointerEventData.InputButton.Right) { //only display context menu when RMB is pressed
			if (GameManager.instance.State_Play) { //game is active
				if (!MenuScript.instance.CurrentState.Equals("")) {
					List<RaycastResult> results = new List<RaycastResult>();
					grc.Raycast(eventData, results);
				} else {
					//do raycast
					//get gameobject
					//Display(gameobject)
				}
			}
		}
	}

	public void Show(GameObject newContext) {
		if (!newContext.transform.Equals(context)) {
			switch (newContext.tag) {
				case "uielement_container":
					break;
				default:
					break;
			}
			context = newContext.transform;
		}
	}
}
