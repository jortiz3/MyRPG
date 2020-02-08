using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace AreaManagerNS.AreaNS {

	/// <summary>
	/// Area: Class that randomly populates, manages, and displays entities that belong in an area.
	/// Written by Justin Ortiz
	/// </summary>
	[XmlRoot(ElementName = "Area"), XmlInclude(typeof(AreaType)), XmlInclude(typeof(Entity))]
	public class Area {

		public static int GenericAreaTotalSpreadChance { get { return genericAreaTotalSpreadChance; } }
		public static int Size { get { return boundaryRadius; } }

		private static List<AreaType> areaTypes;
		private static int genericAreaTotalSpreadChance;
		private static int boundaryRadius = 50; //square boundary; distance (worldspace) from center to edge

		private AreaType type;
		private List<Entity> entities;
		private Vector2IntS position;
		private bool discovered;
		private string pathToSaveTo;

		[XmlElement("MapPosition")]
		public Vector2IntS MapPosition { get { return position; } set { position = value; } }

		[XmlElement("DiscoveredByPlayer")]
		public bool Discovered { get { return discovered; } set { discovered = value; } }

		[XmlElement("AreaType")]
		public AreaType Type { get { return type; } set { type = value; } }

		[XmlArray("EntityList"), XmlArrayItem(ElementName = "Entity", Type = typeof(Entity))]
		public List<Entity> Entities { get { return entities; } set { entities = value; } }

		public Area() {
			discovered = false;
			position = Vector2IntS.zero;
			pathToSaveTo = AreaManager.CurrentSaveFolder + position.x + "_" + position.y + ".xml";
			type = areaTypes[0];
			entities = new List<Entity>();
		}

		public Area(AreaType areaType, Vector2Int Position) {
			discovered = false;
			position = new Vector2IntS(Position);
			pathToSaveTo = AreaManager.CurrentSaveFolder + position.x + "_" + position.y + ".xml";
			type = areaType;
			entities = new List<Entity>();
		}

		public void AssignType(int typeID) {
			if (typeID >= 0 && typeID < areaTypes.Count) {
				if (areaTypes != null) {
					type = areaTypes[typeID];
				}
			}
		}

		public void AssignType(string areaName) {
			for (int i = 0; i < areaTypes.Count; i++) { //search through list
				if (areaTypes[i].name.Equals(areaName)) { //if name matches
					type = areaTypes[i]; //return the type
					return; //leave method
				}
			} //if loop completes, area name was not found
			type = areaTypes[0]; //assign default type
		}

		public void AssignType(AreaType atype) {
			if (atype != null) {
				type = atype;
			} else {
				type = areaTypes[0];
			}
		}

		public static string[] GetAllAreaTypeNames() {
			string[] temp = new string[areaTypes.Count];
			for (int i = 0; i < temp.Length; i++) {
				temp[i] = areaTypes[i].name;
			}
			return temp;
		}

		public static AreaType GetAreaType(int index) {
			if (index >= 0 && index < areaTypes.Count) {
				return areaTypes[index];
			} else {
				return areaTypes[0];
			}
		}

		public static AreaType GetAreaType(string typeName) {
			for (int i = 0; i < areaTypes.Count; i++) {
				if (areaTypes[i].name.Equals(typeName)) {
					return areaTypes[i];
				}
			}
			return areaTypes[0];
		}

		private void InstantiateEntities() {
			if (entities != null) {
				for (int i = entities.Count - 1; i >= 0; i--) {
					GameObject temp = Resources.Load<GameObject>(entities[i].name);
					if (temp != null) {
						temp = GameObject.Instantiate(temp, new Vector3(entities[i].positionX, entities[i].positionY, 0), Quaternion.identity);
						entities[i].lastUpdated = (int)WorldManager.ElapsedGameTime; //replace 10 with gamemanager time
					} else { //asset not found
						entities.RemoveAt(i); //remove from entity list
					}
				}
			}
		}

		public static void LoadAreaTypes() {
			string folderPath = Application.persistentDataPath + "/data/";
			string fileName = "AreaTypes.xml";

			if (!Directory.Exists(folderPath)) { //ensure the folderpath exists
				Directory.CreateDirectory(folderPath);
			}

			XmlSerializer xr = new XmlSerializer(typeof(List<AreaType>)); //create serializer necessities
			FileStream file;

			if (File.Exists(folderPath + fileName)) { //if the areatypes have been created already
				file = File.OpenRead(folderPath + fileName); //open the file
				areaTypes = xr.Deserialize(file) as List<AreaType>; //load data into list
				file.Close(); //close file

				genericAreaTotalSpreadChance = 0;
				for (int i = 0; i < 4 && i < areaTypes.Count; i++) {
					genericAreaTotalSpreadChance += areaTypes[i].spreadChance;
				}
			} else {
				AreaType generic = new AreaType();
				generic.name = "Plains";
				generic.structureAssetCount = 0;
				generic.sceneryAssetCount = 1;
				generic.characterAssetCount = 0;
				generic.spreadChance = 40;

				areaTypes = new List<AreaType>(); //instantiate the array
				areaTypes.Add(generic); //fill with basic data

				file = File.Create(folderPath + fileName); //create the file
				xr.Serialize(file, areaTypes); //save data to file
				file.Close(); //close the file
			}
		}

		public IEnumerator LoadToScene() {
			GameManager.loadingBar.ResetProgress();
			GameManager.loadingBar.Show();
			//"Loading Area"

			GameObject bg = Resources.Load<GameObject>("Backgrounds/" + type.name);
			if (bg != null) {
				GameObject.Instantiate(bg, Vector3.zero, Quaternion.identity);
			}
			//enable/disable area exits -- up, left, right, down
			GameManager.loadingBar.IncreaseProgress(0.33f);
			yield return new WaitForSeconds(0.2f);

			//"Loading Entities"
			InstantiateEntities();
			GameManager.loadingBar.IncreaseProgress(0.33f);
			yield return new WaitForSeconds(0.2f);

			//position player?
			GameManager.loadingBar.IncreaseProgress(0.34f);
			yield return new WaitForSeconds(0.2f);
			GameManager.loadingBar.Hide();
		}

		public IEnumerator Populate() {
			if (type == null) {
				if (areaTypes != null && areaTypes[0] != null) {
					type = areaTypes[0];
				}
			}

			string entityAreaPrefix = type.name + "_";
			string currEntityFilename;

			if (entities == null) { //shouldn't be null, but just in case
				entities = new List<Entity>();
			} else {
				entities.Clear();
			}

			Entity tempEntity; //current entity to add
			int i; //iteration index for all 3 loops
			int numEntities = 0; //num of entites for this area
			int currAssetCount = 0; // num of assets available for areatype

			for (int assetType = 0; assetType < 3; assetType++) { //loop through each asset type and do (mostly) the same thing
				switch (assetType) { //establish the only differences
					case 1:
						currAssetCount = type.sceneryAssetCount; //set the count
						currEntityFilename = "Scenery/" + entityAreaPrefix; //set folder path
						numEntities = Random.Range(10, type.sceneryMaxSpawnCount); //set random quantity for scenery objs in this area
						break;
					case 2:
						currAssetCount = type.characterAssetCount; //set the count
						currEntityFilename = "Characters/" + entityAreaPrefix; //set folder path
						numEntities = Random.Range(5, type.characterMaxSpawnCount); //set random quantity for character objs in this area
						break;
					default:
						currAssetCount = type.structureAssetCount; //set the count
						currEntityFilename = "Structures/" + entityAreaPrefix; //set folder path
						numEntities = Random.Range(1, type.structureMaxSpawnCount); //set random quantity for scenery objs in this area
						break;
				}

				if (currAssetCount > 0) { //if asset type has assets to offer
					for (i = 0; i < numEntities; i++) {
						tempEntity = new Entity(); //instantiate new entity
						tempEntity.name = currEntityFilename + (Random.Range(0, currAssetCount)); //i.e. "Structures/Plains_0" -- So, it will be prepped for Resources.Load<GameObject>(path)
						tempEntity.positionX = Random.Range(-boundaryRadius, boundaryRadius); //generate random position
						tempEntity.positionY = Random.Range(-boundaryRadius, boundaryRadius);
						tempEntity.lastUpdated = (int)WorldManager.ElapsedGameTime;
						entities.Add(tempEntity);
					}
				}
				yield return new WaitForEndOfFrame(); //add time between iterations
			}

			Save();
		}

		private void Save() {
			XmlSerializer xr = new XmlSerializer(typeof(Area));
			FileStream file = new FileStream(pathToSaveTo, FileMode.Create);
			xr.Serialize(file, this);
			file.Close();
		}

		public void SaveEntities(List<Entity> Entities) {
			entities.Clear();
			if (Entities.Count > 0) {
				entities.AddRange(Entities);
			}
			Save();
		}

		public void SetPosition(Vector2Int newPosition) {
			position = new Vector2IntS(newPosition);
		}
	}
}
