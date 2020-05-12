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

			if (slider != null) {
				slider.onValueChanged.AddListener(UpdateQuantity);
			}
			if (inputField != null) {
				inputField.onEndEdit.AddListener(UpdateQuantity);
			}
		} else {
			Destroy(gameObject);
		}
	}

	public void Hide() {
		gameObject.SetActive(false);
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
			inputField.text = ((int)value).ToString();
		}
	}

	public void UpdateQuantity(string value) { //called by button in inspector
		float fValue;
		float.TryParse(value, out fValue);

		if (slider != null) {
			fValue = Mathf.Clamp(fValue, slider.minValue, slider.maxValue);
			slider.value = fValue;
		}

		if (inputField != null) {
			inputField.text = ((int)fValue).ToString();
		}
	}
}
