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
		public SceneryObjectSaveData sceneryData;
		public string name_prefab;
		public float positionX;
		public float positionY;
		public int lastUpdated; // time since last update (seconds)

		public Entity() {
			itemData = null;
			structureData = null;
			sceneryData = null;
			name_prefab = "";
			positionX = 0;
			positionY = 0;
			lastUpdated = 0;
		}

		//for items
		public Entity(int itemID, string prefix, string suffix, int quantity, bool equipped, Vector3 position) {
			itemData = new ItemSaveData(itemID, prefix, suffix, quantity, equipped);
			structureData = null;
			sceneryData = null;
			positionX = position.x;
			positionY = position.y;
			lastUpdated = (int)GameManager.instance.ElapsedGameTime;
		}

		//for structures
		public Entity(string structureOwner, string[] textures, Vector2IntS dimensions, Vector3 position) {
			itemData = null;
			structureData = new StructureSaveData(structureOwner, textures, dimensions);
			sceneryData = null;
			positionX = position.x;
			positionY = position.y;
			lastUpdated = (int)GameManager.instance.ElapsedGameTime;
		}

		//for scenery data
		public Entity(int HarvestCount, int HarvestedItemID, bool AllowStructureCollision, Vector3 position) {
			itemData = null;
			structureData = null;
			sceneryData = new SceneryObjectSaveData(HarvestCount, HarvestedItemID, AllowStructureCollision);
			positionX = position.x;
			positionY = position.y;
			lastUpdated = (int)GameManager.instance.ElapsedGameTime;
		}

		//for default prefabs
		public Entity(string prefabName, Vector3 position) {
			itemData = null;
			structureData = null;
			sceneryData = null;
			name_prefab = prefabName;
			positionX = position.x;
			positionY = position.y;
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
			} else if (sceneryData != null) {
				AssetManager.instance.InstantiateSceneryObject(position, sceneryData.GetSceneryType(), sceneryData.name, sceneryData.harvestedItemID,
					sceneryData.harvestCount, sceneryData.allowStructureCollision);
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
			Entity temp = null;
			switch (transform.tag) {
				case "item":
					Item i = transform.GetComponent<Item>();
					temp = new Entity(i.ID, i.Prefix, i.Suffix, i.Quantity, i.Equipped, transform.position);
					break;
				case "structure":
					Structure s = transform.GetComponent<Structure>();
					temp = new Entity(s.Owner, s.GetTextures(), new Vector2IntS(s.Dimensions), transform.position);
					break;
				case "scenery":
					SceneryObject scenery = transform.GetComponent<SceneryObject>();
					temp = new Entity(scenery.HarvestCount, scenery.HarvestedItemID, scenery.AllowStructureCollision, transform.position);
					break;
				default:
					temp = new Entity(transform.name, transform.position);
					break;
			}
			return temp;
		}
	}
}
