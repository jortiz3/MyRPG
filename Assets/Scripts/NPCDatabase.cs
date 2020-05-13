using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NPC.Database {
	public class NPCDatabase : MonoBehaviour {
		public static NPCDatabase instance;

		private void Awake() {
			if (instance == null) {
				instance = this;
			} else {
				Destroy(this);
			}
		}
	}
}
