using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Newtonsoft.Json;

namespace Areas {

	/// <summary>
	/// Area: Class that randomly populates, manages, and displays entities that belong in an area.
	/// Written by Justin Ortiz
	/// </summary>
	[Serializable, JsonObject(MemberSerialization.OptIn)]
	public class Area {
		public static int Size { get { return boundaryRadius; } }

		private static int boundaryRadius = 100; //square boundary; distance (worldspace) from center to edge
		private static int cityRadius = 50;
		private static int numAreasToPopulate;

		[JsonProperty]
		private string typeName;
		[JsonProperty]
		private string owner;
		[JsonProperty]
		private List<Entity> entities;
		[JsonProperty]
		private Vector2IntS position;
		[JsonProperty]
		private bool discovered;
		[JsonProperty]
		private int lastUpdated;
		private bool dirty; //let's us know if this area needs to be saved

		public static bool PopulationInProgress { get { return numAreasToPopulate > 0; } }
		public Vector2IntS MapPosition { get { return position; } }
		public bool Discovered { get { return discovered; } }
		public bool Dirty { get { return dirty; } }
		public int LastUpdated { get { return lastUpdated; } }
		public string TypeName { get { return typeName; } }

		public Area() {
			discovered = false;
			position = Vector2IntS.zero;
			typeName = AreaTypeManager.GetAreaType(0).name;
			owner = "none";
			entities = new List<Entity>();
		}

		public Area(Vector2Int Position) {
			discovered = false;
			position = new Vector2IntS(Position);
			typeName = AreaTypeManager.GetAreaType(0).name;
			owner = "none";
			entities = new List<Entity>();
		}

		public Area(Vector2Int Position, string AreaTypeName, string Owner = "none") {
			discovered = false;
			position = new Vector2IntS(Position);
			typeName = AreaTypeName;
			owner = Owner;
			entities = new List<Entity>();
		}

		public void AssignType(string TypeName) {
			typeName = TypeName;
		}

		private void InstantiateEntities() {
			if (entities != null) {
				for (int i = 0; i < entities.Count; i++) {
					if (!entities[i].Instantiate()) { //try instantiate entity
						entities.RemoveAt(i); //if failed, remove from list
						i--; //counteract the shifting of elements
					}
				}
			}
		}

		public IEnumerator LoadToScene(NavMeshSurface navMesh) {
			LoadingScreen.instance.SetText("Loading prerequisites..");
			LoadingScreen.instance.Show();

			while (!StructureGridManager.instance.GridInitialized) { //in case grid manager needs more time
				yield return new WaitForEndOfFrame();
			}

			LoadingScreen.instance.SetText("Loading area..");
			float loadingIncrement = (1f - LoadingScreen.instance.GetProgress()) / 4f; //get remaining progress, divide by how many load sections

			string bgFileName = "Backgrounds/";
			if (typeName.Contains("city")) {
				bgFileName += "City";
			} else if (typeName.Contains("camp")) {
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
			yield return new WaitForSeconds(0.2f);

			

			LoadingScreen.instance.SetText("Loading entities..");
			LoadingScreen.instance.IncreaseProgress(loadingIncrement);
			yield return new WaitForEndOfFrame(); //ensure loading bar changes are rendered
			InstantiateEntities(); //begin instantiating entities
			yield return new WaitForSeconds(2f);

			LoadingScreen.instance.SetText("Populating structures..");
			LoadingScreen.instance.IncreaseProgress(loadingIncrement);
			while (StructureGridManager.instance.RegisteringStructures || Structure.PopulationInProgress) {
				yield return new WaitForEndOfFrame();
			}

			LoadingScreen.instance.SetText("Populating containers..");
			LoadingScreen.instance.IncreaseProgress(loadingIncrement);
			while (Container.PopulationInProgress) {
				yield return new WaitForEndOfFrame();
			}

			discovered = true;

			LoadingScreen.instance.SetProgress(1f);
			AreaManager.instance.SaveOrLoadInProgress = false;
			yield return new WaitForSeconds(0.5f);
			LoadingScreen.instance.Hide();
		}

		/// <summary>
		/// Populates the area with entities (Characters, Scenery, Structures).
		/// </summary>
		/// <param name="charactersOnly">To be used when repopulating dead characters</param>
		/// <returns></returns>
		public IEnumerator Populate(bool charactersOnly) {
			numAreasToPopulate++;

			bool isInhabited = false;
			bool isDungeon = false;

			AreaType type = AreaTypeManager.GetAreaType(typeName);

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

						if (assetPrefixes.Count > 0 && assetCounts.Count == assetPrefixes.Count) { //if there were assets
							numEntities = UnityEngine.Random.Range(type.scenerySpawnRange.x, type.scenerySpawnRange.y); //set random quantity for scenery objs in this area
							currRadius = boundaryRadius;

							for (i = 0; i < numEntities; i++) {
								currAsset = UnityEngine.Random.Range(0, assetPrefixes.Count);
								tempEntity = new Entity(0, 1, false, Vector3.zero,
									assetPrefixes[currAsset] + UnityEngine.Random.Range(0, assetCounts[currAsset]).ToString());

								//generate position for the entity
								tempEntity.SetPosition(new Vector3(UnityEngine.Random.Range(-currRadius, currRadius), UnityEngine.Random.Range(-currRadius, currRadius)));

								entities.Add(tempEntity); //add entity to the list
								yield return new WaitForEndOfFrame(); //add time between iterations
							}
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
							tempEntity = new Entity("City_Wall_Horizontal", new Vector3(-60, 60)); //top
							entities.Add(tempEntity);
							tempEntity = new Entity("City_Wall_Horizontal", new Vector3(-60, -50)); //bottom
							entities.Add(tempEntity);
							tempEntity = new Entity("City_Wall_Vertical", new Vector3(-60, 50)); //left
							entities.Add(tempEntity);
							tempEntity = new Entity("City_Wall_Vertical", new Vector3(55, 50)); //right
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
						Vector3 entityPos = Vector3.zero;

						for (i = 0; i < numEntities; i++) {
							tempEntity = new Entity(owner, "default", 0, Vector2Int.one, Vector3.zero, new string[] { "floor_default", "roof_default", "door_default" }, true);

							//generate position for the entity
							entityPos.x = UnityEngine.Random.Range(-currRadius, currRadius);
							if (isInhabited) { //if area is a city
								entityPos.y = (int)UnityEngine.Random.Range(-numOfRowsForRadius, numOfRowsForRadius) * heightSpacing; //get random row, then convert to world pos using cell height
							} else { //just random position
								entityPos.y = UnityEngine.Random.Range(-currRadius, currRadius);
							}
							tempEntity.SetPosition(entityPos);

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
			Save();
			numAreasToPopulate--;
		}

		public void Save() {
			GameManager.SaveObject(this, AreaManager.CurrentSaveFolder + position.x + "_" + position.y + ".json");
			dirty = false;
		}

		public void UpdateEntities(List<Entity> Entities) {
			entities = Entities; //set the new reference
			dirty = true;
		}
	}
}
