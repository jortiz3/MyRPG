using System;
using UnityEngine;

namespace AreaManagerNS.AreaNS {

	/// <summary>
	/// Entity: Information regarding a GameObject that may be loaded into a Scene.
	/// Written by Justin Ortiz
	/// </summary>
	[Serializable]
	public class Entity {
		public string name;
		public float positionX;
		public float positionY;
		public int lastUpdated; // time since last update (seconds)

		public static Entity Parse(GameObject gameObject) {
			return Parse(gameObject.transform);
		}

		public static Entity Parse(Transform transform) {
			Entity temp = new Entity();
			temp.name = transform.parent.name + "/" + transform.name.Replace("(Clone)", "");
			temp.positionX = transform.position.x;
			temp.positionY = transform.position.y;
			temp.lastUpdated = (int)WorldManager.ElapsedGameTime;
			return temp;
		}
	}
}
