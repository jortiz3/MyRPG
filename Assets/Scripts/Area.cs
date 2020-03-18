using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace internal_Area {

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
		private string typeName;
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
		public string TypeName { get { return typeName; } }
		public List<Entity> Entities { get { return entities; } }

		public Area() {
			discovered = false;
			position = Vector2IntS.zero;
			typeName = AreaTypeManager.GetAreaType(0).name;
			entities = new List<Entity>();
		}

		public Area(Vector2Int Position) {
			discovered = false;
			position = new Vector2IntS(Position);
			typeName = AreaTypeManager.GetAreaType(0).name;
			entities = new List<Entity>();
		}

		public Area(Vector2Int Position, string areaTypeName) {
			discovered = false;
			position = new Vector2IntS(Position);
			typeName = areaTypeName;
			entities = new List<Entity>();
		}

		public void AssignType(string TypeName) {
			typeName = TypeName;
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
			if (typeName.Contains("City")) {
				bgFileName += "City";
			} else if (typeName.Contains("Camp")) {
				bgFileName += "Camp";
			} else {
				bgFileName += typeName;
			}

			GameObject bg = Resources.Load<GameObject>(bgFileName);
			if (bg != null) {
				GameObject.Instantiate(bg, Vector3.zero, Quaternion.identity);
			} else {
				Debug.Log("File Not Found: " + bgFileName);
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
			bool isInhabited = false;
			bool isDungeon = false;

			AreaType type = AreaTypeManager.GetAreaType(typeName);

			if (type.name.Contains("City") || type.name.Contains("Camp")) {
				isInhabited = true;
			} else if (type.name.Contains("Dungeon")) {
				isDungeon = true;
			}

			string entityAreaPrefix = type.name + "_";
			string currEntityFilename;

			if (entities == null) { //shouldn't be null, but just in case
				entities = new List<Entity>();
			} else {
				entities.Clear();
			}

			Entity tempEntity; //current entity to add
			int i; //index for all 3 iterations
			int numEntities = 0; //num of entites for this area
			int currAssetCount = 0; // num of assets available for areatype
			int minAssetSpawnCount = 20;
			int assetTypeLimit = charactersOnly ? 1 : 3; //limits the loop to only do characters if true
			int currRadius;

			for (int assetType = 0; assetType < assetTypeLimit; assetType++) { //loop through each asset type and do (mostly) the same thing
				switch (assetType) { //establish the only differences
					case 1:
						currAssetCount = type.sceneryAssetCount; //set the count
						currEntityFilename = "Scenery/" + entityAreaPrefix; //set folder path
						numEntities = UnityEngine.Random.Range(minAssetSpawnCount, type.sceneryMaxSpawnCount); //set random quantity for scenery objs in this area
						currRadius = boundaryRadius;
						break;
					case 2:
						currAssetCount = type.structureAssetCount; //set the count
						currEntityFilename = "Structures/" + entityAreaPrefix; //set folder path
						minAssetSpawnCount = isInhabited ? 20 : 1;
						numEntities = UnityEngine.Random.Range(minAssetSpawnCount, type.structureMaxSpawnCount); //set random quantity for scenery objs in this area

						string tempAssetPrefix = currEntityFilename.Split('_')[0]; //i.e. "Structures/City" || "Structures/Camp" || "Structures/Dungeon"
						if (isInhabited) {
							currRadius = cityRadius;

							//add the walls surrounding the city/camp
							tempEntity = new Entity(tempAssetPrefix + "_Wall_Horizontal", -60, 60); //top
							entities.Add(tempEntity);
							tempEntity = new Entity(tempAssetPrefix + "_Wall_Horizontal", -60, -50); //bottom
							entities.Add(tempEntity);
							tempEntity = new Entity(tempAssetPrefix + "_Wall_Vertical", -60, 50); //left
							entities.Add(tempEntity);
							tempEntity = new Entity(tempAssetPrefix + "_Wall_Vertical", 55, 50); //right
							entities.Add(tempEntity);
						} else if (isDungeon) {
							currRadius = boundaryRadius;

							//add the dungeon entrance
							tempEntity = new Entity(tempAssetPrefix + "_Entrance", 0, 0);
							entities.Add(tempEntity);
						} else {
							currRadius = boundaryRadius;
						}
						break;
					default: //aka 0
						currAssetCount = type.characterAssetCount; //set the count
						currEntityFilename = "Characters/" + entityAreaPrefix; //set folder path
						numEntities = UnityEngine.Random.Range(minAssetSpawnCount, type.characterMaxSpawnCount); //set random quantity for character objs in this area
						currRadius = boundaryRadius;
						break;
				}

				if (currAssetCount > 0) { //if asset type has assets to offer
					for (i = 0; i < numEntities; i++) {
						tempEntity = new Entity(); //instantiate new entity
						tempEntity.name = currEntityFilename + (UnityEngine.Random.Range(0, currAssetCount)); //i.e. "Structures/Plains_0" -- So, it will be prepped for Resources.Load<GameObject>(path)

						//generate position for the entity
						tempEntity.positionX = UnityEngine.Random.Range(-currRadius, currRadius);
						tempEntity.positionY = UnityEngine.Random.Range(-currRadius, currRadius);
						if (assetType == 2 && isInhabited) { //if asset is a structure && is a city
							tempEntity.positionY = ((int)tempEntity.positionY / 5) * 5; //conform pos.y to grid
						}

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
