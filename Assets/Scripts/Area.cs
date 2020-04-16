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
			int minAssetSpawnCount = 20;
			int assetTypeLimit = charactersOnly ? 1 : 3; //limits the loop to only do characters if true
			int currRadius;
			float heightSpacing = 0;
			float numOfRowsForRadius = 0;

			for (int assetType = 0; assetType < assetTypeLimit; assetType++) { //loop through each asset type and do (mostly) the same thing
				switch (assetType) { //establish the only differences
					case 1:
						currEntityFilename = "Scenery/" + entityAreaPrefix; //set folder path
						numEntities = UnityEngine.Random.Range(type.scenerySpawnRange.x, type.scenerySpawnRange.y); //set random quantity for scenery objs in this area
						currRadius = boundaryRadius;
						break;
					case 2:
						currEntityFilename = "Structures/" + entityAreaPrefix; //set folder path
						minAssetSpawnCount = isInhabited ? 20 : 1;
						numEntities = UnityEngine.Random.Range(type.structureSpawnRange.x, type.structureSpawnRange.y); //set random quantity for scenery objs in this area

						string tempAssetPrefix = currEntityFilename.Split('_')[0]; //i.e. "Structures/City" || "Structures/Camp" || "Structures/Dungeon"
						if (isInhabited) {
							currRadius = cityRadius - 5; //1 cell margin for sides

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

						heightSpacing = CustomMath.GetLargestFactor(currRadius); //get largest factor to determine structure row spacing
						numOfRowsForRadius = currRadius / heightSpacing; //get num of rows depending on spacing
						break;
					default: //aka 0
						currEntityFilename = "Characters/" + entityAreaPrefix; //set folder path
						numEntities = UnityEngine.Random.Range(type.characterSpawnRange.x, type.characterSpawnRange.y); //set random quantity for character objs in this area
						currRadius = boundaryRadius;
						break;
				}

				if (currAssetCount > 0) { //if asset type has assets to offer
					for (i = 0; i < numEntities; i++) {
						tempEntity = new Entity(); //instantiate new entity
						tempEntity.name_prefab = currEntityFilename + (UnityEngine.Random.Range(0, currAssetCount)); //i.e. "Structures/Plains_0" -- So, it will be prepped for Resources.Load<GameObject>(path)

						//generate position for the entity
						tempEntity.positionX = UnityEngine.Random.Range(-currRadius, currRadius);
						if (assetType == 2 && isInhabited) { //if asset is a structure && is a city
							tempEntity.positionY = (int)UnityEngine.Random.Range(-numOfRowsForRadius, numOfRowsForRadius) * heightSpacing; //get random row, then convert to world pos using cell height
						} else { //just random position
							tempEntity.positionY = UnityEngine.Random.Range(-currRadius, currRadius);
						}

						tempEntity.lastUpdated = (int)GameManager.instance.ElapsedGameTime;
						entities.Add(tempEntity);
						yield return new WaitForEndOfFrame(); //add time between iterations
					}
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
			foreach(Container c in Containers) {
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
