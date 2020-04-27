using System.Collections;
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

	public override void TeleportToPos(Vector3 position) {
		StartCoroutine(Teleport(position, true));
	}
}
