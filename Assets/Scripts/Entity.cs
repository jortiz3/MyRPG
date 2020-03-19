using System;
using UnityEngine;

namespace internal_Area {

	/// <summary>
	/// Entity: Information regarding a GameObject that may be loaded into a Scene.
	/// Written by Justin Ortiz
	/// </summary>
	[Serializable]
	public class Entity {
		public string name;
		public string uniqueData;
		public float positionX;
		public float positionY;
		public int lastUpdated; // time since last update (seconds)

		public Entity() {
			name = "";
			uniqueData = "";
			positionX = 0;
			positionY = 0;
			lastUpdated = 0;
		}

		public Entity(string Name, float PositionX, float PositionY) {
			name = Name;
			uniqueData = "";
			positionX = PositionX;
			positionY = PositionY;
			lastUpdated = (int)GameManager.instance.ElapsedGameTime;
		}

		public Entity(string Name, string UniqueData, float PositionX, float PositionY) {
			name = Name;
			uniqueData = UniqueData;
			positionX = PositionX;
			positionY = PositionY;
			lastUpdated = (int)GameManager.instance.ElapsedGameTime;
		}

		private void GetUniqueData(GameObject g) {
			//search components; call methods
		}

		public bool Instantiate() {
			GameObject temp = Resources.Load<GameObject>(name);
			if (temp != null) {
				temp = GameObject.Instantiate(temp, new Vector3(positionX, positionY, 0), Quaternion.identity);
				PassUniqueData(temp);
				return true;
			}
			return false;
		}

		public static Entity Parse(Transform transform) {
			Entity temp = new Entity();
			temp.name = transform.parent.name + "/" + transform.name.Replace("(Clone)", "");
			temp.positionX = transform.position.x;
			temp.positionY = transform.position.y;
			temp.lastUpdated = (int)GameManager.instance.ElapsedGameTime;
			temp.GetUniqueData(transform.gameObject);
			return temp;
		}

		private void PassUniqueData(GameObject g) {

		}
	}
}
