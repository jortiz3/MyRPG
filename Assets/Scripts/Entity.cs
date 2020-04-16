using System;
using UnityEngine;

namespace internal_Area {

	/// <summary>
	/// Entity: Information regarding a GameObject that may be loaded into a Scene.
	/// Written by Justin Ortiz
	/// </summary>
	[Serializable]
	public class Entity {
		public ItemSaveData itemData;
		public StructureSaveData structureData;
		public string name_prefab;
		public float positionX;
		public float positionY;
		public int lastUpdated; // time since last update (seconds)

		public Entity() {
			itemData = null;
			structureData = null;
			name_prefab = "";
			positionX = 0;
			positionY = 0;
			lastUpdated = 0;
		}

		public Entity(Item i) {
			itemData = i.ToItemSaveData();
			structureData = null;
			positionX = i.transform.position.x;
			positionY = i.transform.position.y;
			lastUpdated = (int)GameManager.instance.ElapsedGameTime;
		}

		public Entity(Structure s) {
			itemData = null;
			structureData = s.ToStructureSaveData();
			positionX = s.transform.position.x;
			positionY = s.transform.position.y;
			lastUpdated = (int)GameManager.instance.ElapsedGameTime;
		}

		public Entity(SceneryObject s) {
			itemData = null;
			structureData = null;
			positionX = s.transform.position.x;
			positionY = s.transform.position.y;
			lastUpdated = (int)GameManager.instance.ElapsedGameTime;
		}

		public Entity(Character c) {
			itemData = null;
			structureData = null;
			positionX = c.transform.position.x;
			positionY = c.transform.position.y;
			lastUpdated = (int)GameManager.instance.ElapsedGameTime;
		}

		public Entity(string prefabName, float PositionX, float PositionY) {
			itemData = null;
			structureData = null;
			name_prefab = prefabName;
			positionX = PositionX;
			positionY = PositionY;
			lastUpdated = (int)GameManager.instance.ElapsedGameTime;
		}

		public bool Instantiate() {
			Vector3 position = new Vector3(positionX, positionY);
			if (itemData != null) {
				AssetManager.instance.InstantiateItem(position, itemData.id, itemData.baseName, itemData.quantity, itemData.baseName);
				return true;
			} else if (structureData != null) {
				AssetManager.instance.InstantiateStructure(position, structureData.dimensions.ToVector2Int(), structureData.owner, structureData.textures);
				return true;
			} else if (name_prefab != null && !name_prefab.Equals("")) {
				GameObject temp = Resources.Load<GameObject>("Prefabs/" + name_prefab);
				if (temp != null) {
					temp = GameObject.Instantiate(temp);
					temp.transform.position = position;
					return true;
				}
			}
			return false;
		}

		public static Entity Parse(Transform transform) {
			Entity temp;
			switch (transform.tag) {
				case "item":
					temp = new Entity(transform.GetComponent<Item>());
					break;
				case "structure":
					temp = new Entity(transform.GetComponent<Structure>());
					break;
				default:
					temp = new Entity(transform.name, transform.position.x, transform.position.y);
					break;
			}
			return temp;
		}
	}
}
