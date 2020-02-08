using System.Xml.Serialization;
using UnityEngine;

namespace AreaManagerNS.AreaNS {

	[XmlType("Entity")]
	public class Entity {
		[XmlAttribute("Name")]
		public string name;
		[XmlAttribute("Position.X")]
		public float positionX;
		[XmlAttribute("Position.Y")]
		public float positionY;
		[XmlAttribute("LastUpdatedAt")]
		public int lastUpdated; // time since last update (seconds)

		public static Entity Parse(GameObject gameObject) {
			return Parse(gameObject.transform);
		}

		public static Entity Parse(Transform transform) {
			Entity temp = new Entity();
			temp.name = transform.name;
			temp.positionX = transform.position.x;
			temp.positionY = transform.position.y;
			temp.lastUpdated = (int)WorldManager.ElapsedGameTime;
			return temp;
		}
	}
}
