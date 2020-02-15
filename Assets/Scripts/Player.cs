using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character {
	protected override void Initialize() {
		if (GameManager.player != null) {
			Destroy(gameObject);
		} else {
			GameManager.player = this;
		}

		base.Initialize();
	}
}
