using System;
using UnityEngine;

namespace internal_Area {

	/// <summary>
	/// Entity: Information regarding a GameObject that may be loaded into a Scene.
	/// Written by Justin Ortiz
	/// </summary>
	[Serializable]
	public class Entity {
		public string type;
		public string[] textures;
		public int itemID; //harvestItemID
		public int quantity; //harvestCount
		public string uniqueString_0; //prefix, structure dimensions
		public string uniqueString_1; //suffix, structure owner
		public bool uniqueBool_0; //allow structure collision,
		public float positionX;
		public float positionY;
		public int lastUpdated; // time since last update (seconds)

		public Entity() {
			type = "null";
			textures = null;
			itemID = -1;
			quantity = 0;
			uniqueString_0 = "";
			uniqueString_1 = "";
			uniqueBool_0 = false;
			positionX = 0;
			positionY = 0;
			lastUpdated = 0;
		}

		/// <summary>
		/// Creates entity data for items.
		/// </summary>
		public Entity(int ItemID, string Prefix, string Suffix, int Quantity, Vector3 Position, string Texture) {
			type = "item";
			textures = new string[] { Texture };
			itemID = ItemID;
			uniqueString_0 = Prefix;
			uniqueString_1 = Suffix;
			quantity = Quantity;
			uniqueBool_0 = false;
			positionX = Position.x;
			positionY = Position.y;
			lastUpdated = (int)GameManager.instance.ElapsedGameTime;
		}

		/// <summary>
		/// Creates entity data for structures.
		/// </summary>
		public Entity(string Owner, Vector2Int Dimensions, Vector3 Position, string[] Textures) {
			type = "structure";
			textures = Textures;
			itemID = -1;
			uniqueString_0 = Dimensions.ToString();
			uniqueString_1 = Owner;
			quantity = 0;
			uniqueBool_0 = false;
			positionX = Position.x;
			positionY = Position.y;
			lastUpdated = (int)GameManager.instance.ElapsedGameTime;
		}

		/// <summary>
		/// Creates entity data for scenery.
		/// </summary>
		public Entity(int ItemID, int HarvestCount, bool AllowStructureCollision, Vector3 Position, string Texture) {
			type = "structure";
			textures = new string[] { Texture };
			itemID = ItemID;
			uniqueString_0 = "";
			uniqueString_1 = "";
			quantity = HarvestCount;
			uniqueBool_0 = AllowStructureCollision;
			positionX = Position.x;
			positionY = Position.y;
			lastUpdated = (int)GameManager.instance.ElapsedGameTime;
		}

		/// <summary>
		/// Creates entity data for prefab/unique game objects.
		/// </summary>
		public Entity(string prefabName, Vector3 position) {
			type = prefabName;
			textures = null;
			itemID = -1;
			quantity = 0;
			uniqueString_0 = "";
			uniqueString_1 = "";
			uniqueBool_0 = false;
			positionX = position.x;
			positionY = position.y;
			lastUpdated = (int)GameManager.instance.ElapsedGameTime;
		}

		public bool Instantiate() { //rethink method; with .json, values are not null
			Vector3 position = new Vector3(positionX, positionY);
			switch (type) {
				case "item":
					AssetManager.instance.InstantiateItem(position: position, itemID: itemID, quantity: quantity, textureName: textures[0]);
					return true;
				case "structure":
					AssetManager.instance.InstantiateStructure(position: position, dimensions: Vector2IntS.Parse(uniqueString_0).ToVector2Int(), owner: uniqueString_1, textureNames: textures);
					return true;
				case "scenery":
					AssetManager.instance.InstantiateSceneryObject(position: position, textureName: textures[0], harvestedItemID: itemID,
						sceneryObjectHP: quantity, allowStructureCollision: uniqueBool_0);
					return true;
				default:
					GameObject temp = Resources.Load<GameObject>("Prefabs/" + type);
					if (temp != null) { //if the prefab is found
						temp = GameObject.Instantiate(temp); //instantiate
						temp.transform.position = position; //set position
						return true; //return entity was instantiated
					} else { //prefab not found
						break; //exit switch
					}
			}
			return false; //entity not instantiated
		}

		/// <summary>
		/// Finds the necessary component, and stores references to the necessary data in an Entity.
		/// </summary>
		/// <returns>Entity object.</returns>
		public static Entity Parse(Transform transform) {
			Entity temp = null; //the obtained data
			switch (transform.tag) { //check the tag
				case "item":
					Item i = transform.GetComponent<Item>(); //get item component
					if (i != null) { //if component was there
						temp = new Entity(i.ID, i.Prefix, i.Suffix, i.Quantity, transform.position, i.GetTextureName()); //create entity object
					}
					break;
				case "structure":
					Structure s = transform.GetComponent<Structure>(); //get structure component
					if (s != null) { //if component was there
						temp = new Entity(s.Owner, s.Dimensions, transform.position, s.GetTextures());
					}
					break;
				case "scenery":
					SceneryObject scenery = transform.GetComponent<SceneryObject>(); //get scenery component
					if (scenery != null) { //if component was there
						temp = new Entity(scenery.HarvestedItemID, scenery.HarvestCount,
							scenery.AllowStructureCollision, transform.position, scenery.GetTextureName()); //create entity object
					}
					break;
				default:
					temp = new Entity(transform.name, transform.position);
					break;
			}
			return temp;
		}
	}
}
