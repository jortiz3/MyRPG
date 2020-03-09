using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AreaManagerNS.AreaNS {

	/// <summary>
	/// Area: Class that randomly populates, manages, and displays entities that belong in an area.
	/// Written by Justin Ortiz
	/// </summary>
	[Serializable]
	public class Area {
		public static int Size { get { return boundaryRadius; } }

		private static int boundaryRadius = 100; //square boundary; distance (worldspace) from center to edge
		private static int cityRadius = 50;

		[SerializeField]
		private AreaType type;
		[SerializeField]
		private List<Entity> entities;
		[SerializeField]
		private Vector2IntS position;
		[SerializeField]
		private bool discovered;
		[SerializeField]
		private int lastUpdated;

		
		public Vector2IntS MapPosition { get { return position; } }
		public bool Discovered { get { return discovered; } }
		public int LastUpdated { get { return lastUpdated; } }
		public AreaType Type { get { return type; } }
		public List<Entity> Entities { get { return entities; } }

		public Area() {
			discovered = false;
			position = Vector2IntS.zero;
			type = AreaTypeManager.GetAreaType(0);
			entities = new List<Entity>();
		}

		public Area(Vector2Int Position) {
			discovered = false;
			position = new Vector2IntS(Position);
			type = AreaTypeManager.GetAreaType(0);
			entities = new List<Entity>();
		}

		public Area(Vector2Int Position, AreaType areaType) {
			discovered = false;
			position = new Vector2IntS(Position);
			type = areaType;
			entities = new List<Entity>();
		}

		public void AssignType(AreaType atype) {
			if (atype != null) {
				type = atype;
			} else {
				type = AreaTypeManager.GetAreaType(0);
			}
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

		public IEnumerator LoadToScene(NavMeshSurface navMesh) {
			GameManager.loadingBar.SetText("Loading area..");
			GameManager.loadingBar.Show();

			float loadingIncrement = (1f - GameManager.loadingBar.GetProgress()) / 3f; //get remaining progress, divide by how many load sections

			string bgFileName = "Backgrounds/";
			if (type.name.Contains("City")) {
				bgFileName += "City";
			} else if (type.name.Contains("Camp")) {
				bgFileName += "Camp";
			} else {
				bgFileName += type.name;
			}

			GameObject bg = Resources.Load<GameObject>(bgFileName);
			if (bg != null) {
				GameObject.Instantiate(bg, Vector3.zero, Quaternion.identity);
			}
			//enable/disable area exits -- up, left, right, down
			yield return new WaitForSeconds(0.3f);

			GameManager.loadingBar.IncreaseProgress(loadingIncrement);
			GameManager.loadingBar.SetText("Loading entities..");
			yield return new WaitForEndOfFrame(); //ensure loading bar changes are rendered
			InstantiateEntities(); //begin instantiating entities
			yield return new WaitForSeconds(0.3f);

			GameManager.loadingBar.IncreaseProgress(loadingIncrement);
			GameManager.loadingBar.SetText("Positioning player..");
			yield return new WaitForEndOfFrame(); //ensure loading bar changes are rendered
			//set player position
			yield return new WaitForSeconds(0.3f);

			GameManager.loadingBar.IncreaseProgress(loadingIncrement);

			GameManager.loadingBar.Hide();
		}

		/// <summary>
		/// Populates the area with entities (Characters, Scenery, Structures).
		/// </summary>
		/// <param name="charactersOnly">To be used when repopulating dead characters</param>
		/// <returns></returns>
		public IEnumerator Populate(bool charactersOnly) {
			bool isCity = false;
			bool isCamp = false;

			if (type == null) {
				type = AreaTypeManager.GetAreaType(0);
			} else if (type.name.Contains("City")) {
				isCity = true;
			} else if (type.name.Contains("Camp")) {
				isCamp = true;
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
			int assetTypeLimit = charactersOnly ? 1 : 3; //limits the loop to only do characters if true
			int currRadius;

			for (int assetType = 0; assetType < assetTypeLimit; assetType++) { //loop through each asset type and do (mostly) the same thing
				switch (assetType) { //establish the only differences
					case 1:
						currAssetCount = type.sceneryAssetCount; //set the count
						currEntityFilename = "Scenery/" + entityAreaPrefix; //set folder path
						numEntities = UnityEngine.Random.Range(10, type.sceneryMaxSpawnCount); //set random quantity for scenery objs in this area
						currRadius = boundaryRadius;
						break;
					case 2:
						currAssetCount = type.structureAssetCount; //set the count
						currEntityFilename = "Structures/" + entityAreaPrefix; //set folder path
						numEntities = UnityEngine.Random.Range(1, type.structureMaxSpawnCount); //set random quantity for scenery objs in this area

						if (isCity) {
							currRadius = cityRadius;

							//instantiate walls
							tempEntity = new Entity("Structures/City_Wall_Horizontal", -60, 60); //top
							entities.Add(tempEntity);
							tempEntity = new Entity("Structures/City_Wall_Horizontal", -60, -50); //bottom
							entities.Add(tempEntity);
							tempEntity = new Entity("Structures/City_Wall_Vertical", -60, 50); //left
							entities.Add(tempEntity);
							tempEntity = new Entity("Structures/City_Wall_Vertical", 55, 50); //right
							entities.Add(tempEntity);
						} else if (isCamp) {
							currRadius = cityRadius;

							//add the appropriate walls
						} else {
							currRadius = boundaryRadius;
						}
						break;
					default: //aka 0
						currAssetCount = type.characterAssetCount; //set the count
						currEntityFilename = "Characters/" + entityAreaPrefix; //set folder path
						numEntities = UnityEngine.Random.Range(5, type.characterMaxSpawnCount); //set random quantity for character objs in this area
						currRadius = boundaryRadius;
						break;
				}

				if (currAssetCount > 0) { //if asset type has assets to offer
					for (i = 0; i < numEntities; i++) {
						tempEntity = new Entity(); //instantiate new entity
						tempEntity.name = currEntityFilename + (UnityEngine.Random.Range(0, currAssetCount)); //i.e. "Structures/Plains_0" -- So, it will be prepped for Resources.Load<GameObject>(path)
						tempEntity.positionX = UnityEngine.Random.Range(-currRadius, currRadius); //generate random position
						tempEntity.positionY = UnityEngine.Random.Range(-currRadius, currRadius);
						tempEntity.lastUpdated = (int)WorldManager.ElapsedGameTime;
						entities.Add(tempEntity);
						yield return new WaitForEndOfFrame(); //add time between iterations
					}
				}
				yield return new WaitForEndOfFrame(); //add time between iterations
			}

			Save();
		}

		private void Save() {
			string pathToSaveTo = AreaManager.CurrentSaveFolder + position.x + "_" + position.y + ".json";
			StreamWriter writer = new StreamWriter(File.Create(pathToSaveTo));//initialize writer with creating/opening filepath
			string json = JsonUtility.ToJson(this, true);
			writer.Write(json); //convert this object to json and write it
			writer.Close(); //close the file
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
