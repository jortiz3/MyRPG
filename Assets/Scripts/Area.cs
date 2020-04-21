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
		private List<ContainerSaveData> containers;
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
			containers = new List<ContainerSaveData>();
		}

		public Area(Vector2Int Position) {
			discovered = false;
			position = new Vector2IntS(Position);
			typeName = AreaTypeManager.GetAreaType(0).name;
			entities = new List<Entity>();
			containers = new List<ContainerSaveData>();
		}

		public Area(Vector2Int Position, string areaTypeName) {
			discovered = false;
			position = new Vector2IntS(Position);
			typeName = areaTypeName;
			entities = new List<Entity>();
			containers = new List<ContainerSaveData>();
		}

		public void AssignType(string TypeName) {
			typeName = TypeName;
		}

		private void InstantiateEntities() {
			if (entities != null) {
				for (int i = 0; i < entities.Count; i++) {
					if (!entities[i].Instantiate()) { //try instantiate entity
						entities.RemoveAt(i); //if failed, remove from list
					}
				}
			}
		}

		public IEnumerator LoadToScene(NavMeshSurface navMesh) {
			LoadingScreen.instance.SetText("Loading area..");
			LoadingScreen.instance.Show();

			float loadingIncrement = (1f - LoadingScreen.instance.GetProgress()) / 3f; //get remaining progress, divide by how many load sections

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
				Debug.Log("Asset Load Error: Area.LoadToScene(...) => background filename: " + bgFileName);
			}
			//enable/disable area exits -- up, left, right, down
			yield return new WaitForSeconds(0.3f);

			LoadingScreen.instance.IncreaseProgress(loadingIncrement);
			LoadingScreen.instance.SetText("Loading entities..");
			yield return new WaitForEndOfFrame(); //ensure loading bar changes are rendered
			InstantiateEntities(); //begin instantiating entities
			yield return new WaitForSeconds(0.3f);

			LoadingScreen.instance.IncreaseProgress(loadingIncrement);
			LoadingScreen.instance.SetText("Positioning player..");
			yield return new WaitForEndOfFrame(); //ensure loading bar changes are rendered
												  //set player position
			yield return new WaitForSeconds(0.3f);

			LoadingScreen.instance.IncreaseProgress(loadingIncrement);
			LoadingScreen.instance.Hide();
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
			type.name = type.name.ToLower();

			if (type.name.Contains("city") || type.name.Contains("camp")) {
				isInhabited = true;
			} else if (type.name.Contains("dungeon")) {
				isDungeon = true;
			}
			
			if (entities == null) { //shouldn't be null, but just in case
				entities = new List<Entity>();
			} else {
				entities.Clear();
			}

			List<string> assetPrefixes = new List<string>();
			List<int> assetCounts = new List<int>();
			Entity tempEntity = null; //current entity to add
			int i; //index for all 3 iterations
			int currAsset;
			int currAssetCount;
			int numEntities = 0; //num of entites for this area
			int assetTypeLimit = charactersOnly ? 1 : 3; //limits the loop to only do characters if true
			int currRadius;


			for (int assetType = 0; assetType < assetTypeLimit; assetType++) { //loop through each asset type and do (mostly) the same thing
				switch (assetType) { //establish the only differences
					case 1: //scenery
						assetPrefixes.Clear();
						assetCounts.Clear();

						assetPrefixes.Add(type.name + "_bush_");
						assetPrefixes.Add(type.name + "_tree_");
						assetPrefixes.Add(type.name + "_rock_");

						for (i = assetPrefixes.Count - 1; i >= 0; i--) {
							currAssetCount = AssetManager.instance.GetAssetCount("scenery", assetPrefixes[i]); //get the count for current prefix
							if (currAssetCount > 0) { //if there are assets
								assetCounts.Insert(0, currAssetCount); //add the count
							} else { //if no assets with prefix
								assetPrefixes.RemoveAt(i); //remove prefix
							}
						}

						numEntities = UnityEngine.Random.Range(type.scenerySpawnRange.x, type.scenerySpawnRange.y); //set random quantity for scenery objs in this area
						currRadius = boundaryRadius;

						for (i = 0; i < numEntities; i++) {
							currAsset = UnityEngine.Random.Range(0, assetPrefixes.Count);
							tempEntity = new Entity(0, 1, false, Vector3.zero,
								assetPrefixes[currAsset] + UnityEngine.Random.Range(0, assetCounts[currAsset]).ToString());

							//generate position for the entity
							tempEntity.positionX = UnityEngine.Random.Range(-currRadius, currRadius);
							tempEntity.positionY = UnityEngine.Random.Range(-currRadius, currRadius);

							entities.Add(tempEntity); //add entity to the list
							yield return new WaitForEndOfFrame(); //add time between iterations
						}
						break;
					case 2:
						assetPrefixes.Clear();
						assetCounts.Clear();

						numEntities = UnityEngine.Random.Range(type.structureSpawnRange.x, type.structureSpawnRange.y); //set random quantity for scenery objs in this area

						string tempAssetPrefix = type.name;
						if (isInhabited) {
							currRadius = cityRadius - 5; //1 cell margin for sides

							//add the walls surrounding the city/camp
							tempEntity = new Entity(tempAssetPrefix + "_Wall_Horizontal", new Vector3(-60, 60)); //top
							entities.Add(tempEntity);
							tempEntity = new Entity(tempAssetPrefix + "_Wall_Horizontal", new Vector3(-60, -50)); //bottom
							entities.Add(tempEntity);
							tempEntity = new Entity(tempAssetPrefix + "_Wall_Vertical", new Vector3(-60, 50)); //left
							entities.Add(tempEntity);
							tempEntity = new Entity(tempAssetPrefix + "_Wall_Vertical", new Vector3(55, 50)); //right
							entities.Add(tempEntity);
						} else if (isDungeon) {
							currRadius = boundaryRadius;

							//add the dungeon entrance
							tempEntity = new Entity(tempAssetPrefix + "_Entrance", new Vector3(0, 0));
							entities.Add(tempEntity);
						} else {
							currRadius = boundaryRadius;
						}

						float heightSpacing = CustomMath.GetLargestFactor(currRadius); //get largest factor to determine structure row spacing
						float numOfRowsForRadius = currRadius / heightSpacing; //get num of rows depending on spacing

						for (i = 0; i < numEntities; i++) {
							tempEntity = new Entity("", Vector2Int.one, Vector3.zero, new string[] { "floor_default", "roof_default", "door_default" });

							//generate position for the entity
							tempEntity.positionX = UnityEngine.Random.Range(-currRadius, currRadius);
							if (isInhabited) { //if area is a city
								tempEntity.positionY = (int)UnityEngine.Random.Range(-numOfRowsForRadius, numOfRowsForRadius) * heightSpacing; //get random row, then convert to world pos using cell height
							} else { //just random position
								tempEntity.positionY = UnityEngine.Random.Range(-currRadius, currRadius);
							}

							entities.Add(tempEntity); //add entity to the list
							yield return new WaitForEndOfFrame(); //add time between iterations
						}
						break;
					default: //aka 0
						assetPrefixes.Clear();
						assetCounts.Clear();

						numEntities = UnityEngine.Random.Range(type.characterSpawnRange.x, type.characterSpawnRange.y); //set random quantity for character objs in this area
						currRadius = boundaryRadius;
						break;
				}
				yield return new WaitForEndOfFrame(); //add time between iterations
			}

			Save(entities);
		}

		private void Save() {
			string pathToSaveTo = AreaManager.CurrentSaveFolder + position.x + "_" + position.y + ".json";
			StreamWriter writer = new StreamWriter(File.Create(pathToSaveTo));//initialize writer with creating/opening filepath
			string json = JsonUtility.ToJson(this, true);
			writer.Write(json); //convert this object to json and write it
			writer.Close(); //close the file
		}

		private void Save(List<Entity> Entities) {
			UpdateEntities(Entities);
			Save();
		}

		public void Save(List<Container> Containers, List<Entity> Entities) {
			UpdateContainers(Containers);
			UpdateEntities(Entities);
			Save();
		}

		public void SetPosition(Vector2Int newPosition) {
			position = new Vector2IntS(newPosition);
		}

		private void UpdateContainers(List<Container> Containers) {
			containers.Clear();
			foreach (Container c in Containers) {
				containers.Add(new ContainerSaveData(c));
			}
		}

		private void UpdateContainers(List<ContainerSaveData> Containers) {
			containers.Clear();
			if (Containers.Count > 0) {
				containers.AddRange(Containers);
			}
		}

		private void UpdateEntities(List<Entity> Entities) {
			entities.Clear();
			if (Entities.Count > 0) {
				entities.AddRange(Entities);
			}
		}
	}
}
