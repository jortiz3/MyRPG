using UnityEngine;
using UnityEngine.UI;

public class QuantityManager : MonoBehaviour {
	public static QuantityManager instance;

	[SerializeField]
	private Slider slider;
	[SerializeField]
	private InputField inputField;

	private void Awake() {
		if (instance == null) {
			instance = this;
			gameObject.SetActive(false);
		} else {
			Destroy(gameObject);
		}
	}

	public void SetQuantityRange(float maxValue, float minValue = 1f) {
		if (slider != null) {
			slider.minValue = minValue;
			slider.maxValue = maxValue;

			slider.value = slider.maxValue;
		}
	}

	public void UpdateQuantity(float value) { //called by button in inspector
		if (slider != null) {
			slider.value = value;
		}

		if (inputField != null) {
			inputField.text = value.ToString();
		}
	}

	public void UpdateQuantity(string value) { //called by button in inspector
		if (slider != null) {
			slider.value = float.Parse(value);
		}

		if (inputField != null) {
			inputField.text = value;
		}
	}
}
