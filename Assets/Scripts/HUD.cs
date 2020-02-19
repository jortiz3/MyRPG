using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour {
	public static HUD instance;

	private Text interactionText;

	private void Awake() {
		if (instance != null) {
			Destroy(gameObject);
		} else {
			instance = this;

			interactionText = transform.Find("Interaction").GetComponent<Text>();
			HideInteractionText();
		}
	}

	public void HideInteractionText() {
		if (interactionText != null) {
			interactionText.gameObject.SetActive(false);
		}
	}

	public void ShowInteractionText(string text) {
		if (interactionText != null) {
			interactionText.text = text;
			interactionText.gameObject.SetActive(true);
		}
	}
}
