using System;
using UnityEngine;
using Newtonsoft.Json;

namespace internal_Area {

	/// <summary>
	/// Entity: Information regarding a GameObject that may be loaded into a Scene.
	/// Written by Justin Ortiz
	/// </summary>
	[Serializable, JsonObject(MemberSerialization.Fields)]
	public class Entity {
		private string type; //item, structure, scenery object, prefab name
		private string[] textures; //textures to assign
		private int itemID; //harvestItemID
		private int quantity; //harvestCount
		private string uniqueString_0; //prefix, structure dimensions
		private string uniqueString_1; //suffix, structure owner
		private string uniqueString_2; //structure preset
		private bool uniqueBool_0; //allow structure collision, instantiate furniture on first instantiate
		private float positionX;
		private float positionY;
		private int lastUpdated; // time since last update (seconds)

		public Entity() {
			type = "null";
			textures = null;
			itemID = -1;
			quantity = 0;
			uniqueString_0 = "";
			uniqueString_1 = "";
			uniqueString_2 = "";
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
			uniqueString_2 = "";
			quantity = Quantity;
			uniqueBool_0 = false;
			positionX = Position.x;
			positionY = Position.y;
			lastUpdated = (int)GameManager.instance.ElapsedGameTime;
		}

		/// <summary>
		/// Creates entity data for structures.
		/// </summary>
		public Entity(string Owner, string Preset, Vector2Int Dimensions, Vector3 Position, string[] Textures, bool InstantiateFurniture = false) {
			type = "structure";
			textures = Textures;
			itemID = -1;
			uniqueString_0 = Dimensions.ToString();
			uniqueString_1 = Owner;
			uniqueString_2 = Preset;
			quantity = 0;
			uniqueBool_0 = InstantiateFurniture;
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
			uniqueString_2 = "";
			quantity = HarvestCount;
			uniqueBool_0 = AllowStructureCollision;
			positionX = Position.x;
			positionY = Position.y;
			lastUpdated = (int)GameManager.instance.ElapsedGameTime;
		}

		/// <summary>
		/// Creates entity data for furniture.
		/// </summary>
		public Entity(Vector3 position, string Texture) {
			type = "furniture";
			textures = new string[] { Texture };
			itemID = -1;
			quantity = 0;
			uniqueString_0 = "";
			uniqueString_1 = "";
			uniqueString_2 = "";
			uniqueBool_0 = false;
			positionX = position.x;
			positionY = position.y;
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
			uniqueString_2 = "";
			uniqueBool_0 = false;
			positionX = position.x;
			positionY = position.y;
			lastUpdated = (int)GameManager.instance.ElapsedGameTime;
		}

		public bool Instantiate() {
			Vector3 position = new Vector3(positionX, positionY);
			switch (type) {
				case "item":
					Item i = AssetManager.instance.InstantiateItem(position: position, itemID: itemID, quantity: quantity, textureName: textures[0]);
					if (i != null) {
						return true;
					}
					break;
				case "structure":
					Structure s = AssetManager.instance.InstantiateStructure(position: position, dimensions: Vector2IntS.Parse(uniqueString_0).ToVector2Int(),
						owner: uniqueString_1, preset: uniqueString_2, instantiateFurniture: uniqueBool_0, textureNames: textures);
					if (s != null) { //if instantiated
						return true;
					}
					break;
				case "scenery":
					SceneryObject sc = AssetManager.instance.InstantiateSceneryObject(position: position, textureName: textures[0], harvestedItemID: itemID,
						sceneryObjectHP: quantity, allowStructureCollision: uniqueBool_0);
					if (sc != null) {
						return true;
					}
					break;
				case "furniture":
					Furniture f = AssetManager.instance.InstantiateFurniture(position, textureName: textures[0]);
					if (f != null) {
						return true;
					}
					break;
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
						temp = new Entity(s.Owner, s.Preset, s.Dimensions, transform.position, s.GetTextures());
					}
					break;
				case "scenery":
					SceneryObject scenery = transform.GetComponent<SceneryObject>(); //get scenery component
					if (scenery != null) { //if component was there
						temp = new Entity(scenery.HarvestedItemID, scenery.HarvestCount,
							scenery.AllowStructureCollision, transform.position, scenery.GetTextureName()); //create entity object
					}
					break;
				case "furniture":
					Furniture f = transform.GetComponent<Furniture>();
					if (f != null) {
						temp = new Entity(f.transform.position, f.GetTextureName());
					}
					break;
				default:
					temp = new Entity(transform.name, transform.position);
					break;
			}
			return temp;
		}

		public void SetPosition(Vector3 position) {
			positionX = position.x;
			positionY = position.y;
		}
	}
}
