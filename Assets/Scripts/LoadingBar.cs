using UnityEngine;
using UnityEngine.UI;

public class LoadingBar : MonoBehaviour {
	[SerializeField]
	private Text text;
	[SerializeField]
	private Slider slider;
	private GameObject background; //load background & display simultaneously

	public bool isActive { get { return gameObject.activeSelf; } }

	void Awake() {
		if (GameManager.loadingBar != null) {
			Destroy(gameObject);
		} else {
			GameManager.loadingBar = this;
		}

		if (slider == null) {
			slider = GetComponent<Slider>();
		}
		if (text == null) {
			text = GetComponent<Text>();
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
			gameObject.SetActive(true);
		}
	}
}
