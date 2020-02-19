using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Modifies various text and slider objects displayed on the static HUD. Written by Justin Ortiz
/// </summary>
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
