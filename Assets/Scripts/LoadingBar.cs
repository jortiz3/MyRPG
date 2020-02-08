using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class LoadingBar : MonoBehaviour {
	private Slider slider;
	private GameObject background; //load background & display simultaneously
	void Awake() {
		if (GameManager.loadingBar != null) {
			Destroy(gameObject);
		} else {
			GameManager.loadingBar = this;
		}

		slider = GetComponent<Slider>();
		Hide();
	}

	public void Hide() {
		gameObject.SetActive(false);
	}

	public void IncreaseProgress(float amount) {
		slider.value += amount;
	}

	public void ResetProgress() {
		slider.value = 0;
	}

	public void SetProgress(float amount) {
		slider.value = amount;
	}

	public void Show() {
		gameObject.SetActive(true);
	}
}
