using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LoadingScreen : MonoBehaviour {
	public static LoadingScreen instance;

	[SerializeField]
	private Text text;
	[SerializeField]
	private Slider slider;
	[SerializeField]
	private Image image;
	[SerializeField]
	private List<Sprite> loadScreenSprites;

	public bool isActive { get { return gameObject.activeSelf; } }

	void Awake() {
		if (instance != null) {
			Destroy(gameObject);
		} else {
			instance = this;
		}

		if (slider == null) {
			slider = GetComponent<Slider>();
		}
		if (text == null) {
			text = GetComponent<Text>();
		}
		if (image == null) {
			image = GetComponent<Image>();
		}

		Hide();
	}

	public float GetProgress() {
		if (slider != null) {
			return slider.value;
		}
		return 0f;
	}

	public void Hide() {
		if (gameObject.activeSelf) {
			gameObject.SetActive(false);
		}
	}

	public void IncreaseProgress(float amount) {
		if (slider != null) {
			slider.value += amount;
		}
	}

	public void ResetProgress() {
		if (slider != null) {
			slider.value = 0;
		}
		if (text != null) {
			text.text = "";
		}
	}

	public void SetProgress(float amount) {
		if (slider != null) {
			slider.value = amount;
		}
	}

	public void SetText(string textToDisplay) {
		if (text != null) {
			text.text = textToDisplay;
		}
	}

	public void Show() {
		if (!gameObject.activeSelf) {
			if (image != null) {
				if (loadScreenSprites != null) {
					if (loadScreenSprites.Count > 0) {
						image.sprite = loadScreenSprites[UnityEngine.Random.Range(0, loadScreenSprites.Count)];
					} //end if sprites count
				} //end if sprites null
			} //end if image null

			gameObject.SetActive(true);
		}
	}
}
