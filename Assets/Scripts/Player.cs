using UnityEngine;

public class Player : Character {
	public static Player instance;

	protected override void Initialize() {
		if (instance != null) {
			Destroy(gameObject);
		} else {
			instance = this;
		}

		base.Initialize();
	}
}
