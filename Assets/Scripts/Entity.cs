using System;
using UnityEngine;
using Newtonsoft.Json;
using NPC;

namespace Areas {

	/// <summary>
	/// Entity: Information regarding a GameObject that may be loaded into a Scene.
	/// Written by Justin Ortiz
	/// </summary>
	[Serializable, JsonObject(MemberSerialization.Fields)]
	public sealed class Entity {
		private string type; //item, structure, scenery object, prefab name
		private string[] textures; //textures to assign
		private int int_0; //itemID, harvestItemID, containerID
		private int int_1; //harvestCount, npc hp
		private int int_2; //time since last update (seconds)
		private float float_0; //pos.x
		private float float_1; //pos.y
		private float float_2; //npc stamina
		private string string_0; //prefix, structure dimensions, furniture owner
		private string string_1; //suffix, structure owner
		private string string_2; //container info, structure preset
		private bool bool_0; //allow structure collision, instantiate furniture on first instantiate
		
		

		public Entity() {
			type = "null";
			textures = null;
			int_0 = 0;
			int_1 = 0;
			float_2 = 0f;
			string_0 = "";
			string_1 = "";
			string_2 = "";
			bool_0 = false;
			float_0 = 0;
			float_1 = 0;
			int_2 = 0;
		}

		/// <summary>
		/// Creates entity data for items.
		/// </summary>
		public Entity(int ItemID, string Prefix, string Suffix, int Quantity, Vector3 Position, string Texture, string containerInfo = "", float LastUpdated = 0) {
			type = "item";
			textures = new string[] { Texture };
			int_0 = ItemID;
			int_1 = Quantity;
			int_2 = (int)LastUpdated;
			float_0 = Position.x;
			float_1 = Position.y;
			float_2 = 0f;
			string_0 = Prefix;
			string_1 = Suffix;
			string_2 = containerInfo;
			bool_0 = false;
		}

		/// <summary>
		/// Creates entity data for structures.
		/// </summary>
		public Entity(string Owner, string Preset, int StructureID, Vector2Int Dimensions, Vector3 Position, string[] Textures, bool InstantiateFurniture = false, float LastUpdated = 0) {
			type = "structure";
			textures = Textures;
			int_0 = StructureID;
			int_1 = 0;
			int_2 = (int)LastUpdated;
			float_0 = Position.x;
			float_1 = Position.y;
			float_2 = 0f;
			string_0 = Dimensions.ToString();
			string_1 = Owner;
			string_2 = Preset;
			bool_0 = InstantiateFurniture;
		}

		/// <summary>
		/// Creates entity data for scenery.
		/// </summary>
		public Entity(int ItemID, int HarvestCount, bool AllowStructureCollision, Vector3 Position, string Texture, float LastUpdated = 0) {
			type = "scenery";
			textures = new string[] { Texture };
			int_0 = ItemID;
			int_1 = HarvestCount;
			int_2 = (int)LastUpdated;
			float_0 = Position.x;
			float_1 = Position.y;
			float_2 = 0f;
			string_0 = "";
			string_1 = "";
			string_2 = "";
			bool_0 = AllowStructureCollision;
		}

		/// <summary>
		/// Creates entity data for furniture
		/// </summary>
		public Entity(Vector3 position, string owner = "", string Texture = "chest_default", float LastUpdated = 0) {
			type = "furniture";
			textures = new string[] { Texture };
			int_0 = 0;
			int_1 = 0;
			int_2 = (int)LastUpdated;
			float_0 = position.x;
			float_1 = position.y;
			float_2 = 0f;
			string_0 = owner;
			string_1 = "";
			string_2 = "";
			bool_0 = false;
		}

		/// <summary>
		/// Creates entity data for containers.
		/// </summary>
		public Entity(Vector3 position, int containerID = -1, string owner = "", string Texture = "chest_default", float LastUpdated = 0) {
			type = "container";
			textures = new string[] { Texture };
			int_0 = containerID;
			int_1 = 0;
			int_2 = (int)LastUpdated;
			float_0 = position.x;
			float_1 = position.y;
			float_2 = 0f;
			string_0 = owner;
			string_1 = "";
			string_2 = "";
			bool_0 = false;
		}


		public Entity(Vector3 position, string name = "", string npcTypeName = "default", int homeID = -1, int hp = 0, float stamina = 0f) {
			type = "npc";
			textures = null;
			int_0 = homeID;
			int_1 = hp;
			int_2 = 0;
			float_0 = position.x;
			float_1 = position.y;
			float_2 = stamina;
			string_0 = name;
			string_1 = npcTypeName;
			string_2 = "";
			bool_0 = false;
		}

		/// <summary>
		/// Creates entity data for prefab/unique game objects.
		/// </summary>
		public Entity(string prefabName, Vector3 position) {
			type = prefabName;
			textures = null;
			int_0 = 0;
			int_1 = 0;
			int_2 = (int)GameManager.instance.ElapsedGameTime;
			float_0 = position.x;
			float_1 = position.y;
			float_2 = 0f;
			string_0 = "";
			string_1 = "";
			string_2 = "";
			bool_0 = false;
		}

		public bool Instantiate() {
			Vector3 position = new Vector3(float_0, float_1);
			switch (type) {
				case "item":
					int containerID = -1; //no container
					try {
						containerID = int.Parse(string_2);
					} catch {}

					Item i = AssetManager.instance.InstantiateItem(position: position, itemID: int_0, containerID: containerID, quantity: int_1,
						textureName: textures[0], lastUpdated: int_2);
					if (i != null) {
						return true;
					}
					break;
				case "structure":
					Structure s = AssetManager.instance.InstantiateStructure(position: position, dimensions: Vector2IntS.Parse(string_0).ToVector2Int(),
						instanceID: int_0, owner: string_1, preset: string_2, instantiateFurniture: bool_0, textureNames: textures, lastUpdated: int_2);
					if (s != null) { //if instantiated
						return true;
					}
					break;
				case "scenery":
					SceneryObject sc = AssetManager.instance.InstantiateSceneryObject(position: position, textureName: textures[0], harvestedItemID: int_0,
						sceneryObjectHP: int_1, allowStructureCollision: bool_0, lastUpdated: int_2);
					if (sc != null) {
						return true;
					}
					break;
				case "furniture":
					Furniture f = AssetManager.instance.InstantiateFurniture(position, owner: string_0, textureName: textures[0], lastUpdated: int_2);
					if (f != null) {
						return true;
					}
					break;
				case "container":
					Container c = AssetManager.instance.InstantiateContainer(position, containerID: int_0, owner: string_0, textureName: textures[0], lastUpdated: int_2);
					if (c != null) {
						return true;
					}
					break;
				case "npc":
					NonPlayerCharacter npc = AssetManager.instance.InstantiateNPC(position, name: string_0, npcTypeName: string_1, homeID: int_0, currentHP: int_1, currentStamina: float_2);
					break;
				default:
					GameObject spawned = AssetManager.instance.InstantiatePrefab(position, type);
					if (spawned != null) {
						return true;
					}
					break;
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
						temp = new Entity(i.ID, i.Prefix, i.Suffix, i.Quantity, transform.position, i.GetTextureName(), i.ContainerID.ToString(), i.LastUpdated); //create entity object
					}
					break;
				case "structure":
					Structure s = transform.GetComponent<Structure>(); //get structure component
					if (s != null) { //if component was there
						temp = new Entity(s.Owner, s.Preset, s.InstanceID, s.Dimensions, transform.position, s.GetTextures(), false, s.LastUpdated);
					}
					break;
				case "scenery":
					SceneryObject scenery = transform.GetComponent<SceneryObject>(); //get scenery component
					if (scenery != null) { //if component was there
						temp = new Entity(scenery.HarvestedItemID, scenery.HarvestCount,
							scenery.AllowStructureCollision, transform.position, scenery.GetTextureName(), scenery.LastUpdated); //create entity object
					}
					break;
				case "furniture":
					Furniture f = transform.GetComponent<Furniture>();
					if (f != null) {
						temp = new Entity(f.transform.position, f.Owner, f.GetTextureName(), f.LastUpdated);
					}
					break;
				case "container":
					Container c = transform.GetComponent<Container>();
					if (c != null) {
						int containerID;
						string[] splitname = c.gameObject.name.Split('_');
						int.TryParse(splitname[splitname.Length - 1], out containerID);
						temp = new Entity(c.transform.position, containerID, c.Owner, c.GetTextureName(), c.LastUpdated);
					}
					break;
				case "npc":
					NonPlayerCharacter npc = transform.GetComponent<NonPlayerCharacter>();
					if (npc != null) {
						temp = new Entity(transform.position, name: transform.name, npcTypeName: npc.Type, npc.InstanceID_Home, npc.HP, npc.Stamina);
					}
					break;
				default:
					temp = new Entity(transform.name, transform.position);
					break;
			}
			return temp;
		}

		public void SetPosition(Vector3 position) {
			float_0 = position.x;
			float_1 = position.y;
		}
	}
}
