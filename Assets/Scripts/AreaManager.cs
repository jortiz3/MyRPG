using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using internal_Area;

/// <summary>
/// AreaManager: Class that procedurally generates, loads, and manages a grid of Areas.
/// Written by Justin Ortiz
/// </summary>
public class AreaManager : MonoBehaviour {
	private static string currentSaveFolder;
	private static Transform backgroundParent;
	private static Transform structureParent;
	private static Transform characterParent;
	private static Transform sceneryParent;
	private static Transform furnitureParent;
	private static NavMeshSurface navMesh;

	private Area[,] areas;
	private Vector2Int currentAreaPos;

	public static string CurrentSaveFolder { get { return currentSaveFolder; } }
	public static int AreaSize { get { return Area.Size; } }

	private void Awake() {
		AreaTypeManager.Initialize(); //ensure all area types are loaded before they are needed
		backgroundParent = transform.Find("Background");
		structureParent = transform.Find("Structures");
		sceneryParent = transform.Find("Scenery");
		characterParent = transform.Find("Characters");
		furnitureParent = transform.Find("Furniture");
		navMesh = transform.Find("NavMesh").GetComponent<NavMeshSurface>();
	}

	private Area CreateArea(Vector2Int position, AreaType parentAreaType) {
		Area tempArea = new Area(position); //stores the area that will be returned
		AreaType tempType; //stores the type we are currently checking
		int cumulativeChance = AreaTypeManager.GenericAreaTotalSpreadChance + parentAreaType.spreadChance; //total of all possible spread chances
		int randomNum = UnityEngine.Random.Range(0, cumulativeChance); //generate random number based on cumulative
		cumulativeChance = 0; //reset cumulative to process end result
		for (int i = 0; i < 4; i++) { //go through generic area types
			tempType = AreaTypeManager.GetAreaType(i); //get current type

			cumulativeChance += tempType.spreadChance; //update cumulative chance
			if (tempType.Equals(parentAreaType)) { //if same type as parent area
				cumulativeChance += tempType.spreadChance; //add chance again
			}

			if (randomNum < cumulativeChance) { //values increase as list goes on due to cumulative, so if num is < than value, the probability is maintained
				tempArea.AssignType(tempType.name); //assign the type
				break; //stop processing dictionary
			}
		}

		StartCoroutine(tempArea.Populate(false)); //populate and save file when done -- async
		return tempArea; //return the area and continue
	}

	public IEnumerator GenerateAllAreas(string playerName, string worldName, Vector2Int startPos) {
		GameManager.loadingBar.ResetProgress();
		GameManager.loadingBar.Show();

		currentSaveFolder = Application.persistentDataPath + "/saves" + "/" + playerName + "/" + worldName + "/";

		if (!Directory.Exists(currentSaveFolder)) {
			Directory.CreateDirectory(currentSaveFolder);
		}

		areas = new Area[12, 12]; //create array

		int totalNumAreas = areas.GetLength(0) * areas.GetLength(1);
		int areasCompleted = GenerateUniqueAreaData();
		int prevAreasCompleted = 0;
		string typeToSpread; //the type of area that is most likely to spread
		while (areasCompleted < totalNumAreas) {
			for (int x = 0; x < areas.GetLength(0); x++) {
				for (int y = 0; y < areas.GetLength(1); y++) {
					if (areas[x, y] != null) { //start with hard-coded areas
						if (areas[x, y].TypeName.Contains("City")) { //Cities are always in plains
							typeToSpread = AreaTypeManager.GetAreaType(0).name; //spread plains type
						} else if (areas[x, y].TypeName.Contains("Dungeon")) { //dungeons are alwas in marshes
							typeToSpread = AreaTypeManager.GetAreaType(2).name; //spread marsh type
						} else {
							typeToSpread = areas[x, y].TypeName;
						}

						//create 4 adjacent areas
						if (x - 1 >= 0) { //if area adjacent left exists
							if (areas[x - 1, y] == null) { //if area adjacent left is empty
								areas[x - 1, y] = CreateArea(new Vector2Int(x - 1, y), AreaTypeManager.GetAreaType(typeToSpread)); //create area adjacent left
								areasCompleted++;
							}
						}
						if (x + 1 < areas.GetLength(0)) { //if area adjacent right exists
							if (areas[x + 1, y] == null) { //if area adjacent right is empty
								areas[x + 1, y] = CreateArea(new Vector2Int(x + 1, y), AreaTypeManager.GetAreaType(typeToSpread)); //create area adjacent left
								areasCompleted++;
							}
						}
						if (y - 1 >= 0) { //if area adjacent up exists
							if (areas[x, y - 1] == null) { //if area adjacent up is empty
								areas[x, y - 1] = CreateArea(new Vector2Int(x, y - 1), AreaTypeManager.GetAreaType(typeToSpread)); //create area adjacent left
								areasCompleted++;
							}
						}
						if (y + 1 < areas.GetLength(1)) { //if area adjacent down exists
							if (areas[x, y + 1] == null) { //if area adjacent down is empty
								areas[x, y + 1] = CreateArea(new Vector2Int(x, y + 1), AreaTypeManager.GetAreaType(typeToSpread)); //create area adjacent left
								areasCompleted++;
							}
						}
					} //found non-null area for parent
				} //y for loop
				GameManager.loadingBar.IncreaseProgress((areasCompleted - prevAreasCompleted) / 144f);
				prevAreasCompleted = areasCompleted;
				yield return new WaitForEndOfFrame(); //allow for time between each row
			} //x for loop
		}

		GameManager.loadingBar.SetProgress(1f);
		LoadArea(startPos, false);
		yield return new WaitForEndOfFrame();
		GameManager.loadingBar.Hide();
	}

	/// <summary>
	/// Generates hard-coded, unique areas and places them into the array.
	/// </summary>
	/// <returns>The number of hard-coded, unique areas placed.</returns>
	private int GenerateUniqueAreaData() {
		List<string> hardCodedAreaTypes = new List<string>();
		hardCodedAreaTypes.AddRange(AreaTypeManager.GetAllAreaTypeNames());
		hardCodedAreaTypes.RemoveRange(0, 4); //remove the generic area types (Plains, Forest, Mountain, Marsh)
		int numUniqueAreas = hardCodedAreaTypes.Count;

		if (hardCodedAreaTypes.Count > areas.GetLength(0) * areas.GetLength(1)) { //if there are somehow more unique areas than available slots
			Debug.Log("Error: AreaManager.GenerateUniqueAreaData() did not run because there are " + (hardCodedAreaTypes.Count - 4) + " unique area types loaded!");
			return 0;
		}

		List<Vector2Int> hardCodedPositions = new List<Vector2Int>(); //instantiate list
		hardCodedPositions.Add(new Vector2Int(1, 1)); //place city locations
		hardCodedPositions.Add(new Vector2Int(10, 1));
		hardCodedPositions.Add(new Vector2Int(1, 10));
		hardCodedPositions.Add(new Vector2Int(10, 10));

		int randomIndex; //stores random number
		Vector2Int tempPos; //stores the current position
		Area currArea;
		for (int cities = 3; cities >= 0; cities--) { //next 4 in types 'should' be cities
			randomIndex = UnityEngine.Random.Range(0, hardCodedPositions.Count); //pick random position
			tempPos = hardCodedPositions[randomIndex]; //store position

			currArea = new Area(tempPos, hardCodedAreaTypes[cities]); //create area at position
			StartCoroutine(currArea.Populate(false)); //populate the area
			areas[tempPos.x, tempPos.y] = currArea; //add area to array

			hardCodedAreaTypes.RemoveAt(cities); //remove type name from list
			hardCodedPositions.RemoveAt(randomIndex); //remove position
		}

		for (int typeIndex = hardCodedAreaTypes.Count - 1; typeIndex >= 0; typeIndex--) { //go through remaining area types
			do {
				tempPos = new Vector2Int(UnityEngine.Random.Range(0, 12), UnityEngine.Random.Range(0, 12)); //get a random position
			} while (areas[tempPos.x, tempPos.y] != null); //keep trying until we get an empty slot

			currArea = new Area(tempPos, hardCodedAreaTypes[typeIndex]); //create new area at location
			StartCoroutine(currArea.Populate(false)); //populate the area
			areas[tempPos.x, tempPos.y] = currArea; //add next area type to the randomly generated position

			hardCodedAreaTypes.RemoveAt(typeIndex); //remove the area type from the list
		}

		return numUniqueAreas;
	}

	public static Transform GetEntityParent(string entityTypeName) {
		switch (entityTypeName) {
			case "Scenery":
				return sceneryParent;
			case "Structure":
				return structureParent;
			case "Character":
				return characterParent;
			case "Background":
				return backgroundParent;
			case "Furniture":
				return furnitureParent;
			default:
				return characterParent;
		}
	}

	public void LoadArea(Directions direction) {
		switch (direction) {
			case Directions.up:
				LoadArea(new Vector2Int(currentAreaPos.x, currentAreaPos.y - 1), true);
				break;
			case Directions.down:
				LoadArea(new Vector2Int(currentAreaPos.x, currentAreaPos.y + 1), true);
				break;
			case Directions.left:
				LoadArea(new Vector2Int(currentAreaPos.x - 1, currentAreaPos.y), true);
				break;
			case Directions.right:
				LoadArea(new Vector2Int(currentAreaPos.x + 1, currentAreaPos.y), true);
				break;
			default:
				return;
		}
	}

	private void LoadArea(Vector2Int position, bool saveEntities) {
		if (position.x < areas.GetLength(0) && 0 <= position.x) { //if the x is within map bounds
			if (position.y < areas.GetLength(1) && 0 <= position.y) { //if y is within map bounds
				if (areas[position.x, position.y] != null) { //if the desired area isn't empty
					GameManager.loadingBar.ResetProgress();
					GameManager.loadingBar.SetText("Gathering save data..");
					GameManager.loadingBar.Show();

					float loadIncrement = 0.45f / transform.childCount;
					List<Entity> currEntities = new List<Entity>(); //list to store entities currently in scene
					foreach (Transform parent in transform) { //areamanager script is attached to parent transform of entity types
						Transform child;
						for (int index = parent.childCount - 1; index >= 0; index--) { //entity types have entities as children
							child = parent.GetChild(index);
							if (!child.CompareTag("Player")) { //if the object isn't the player
								if (saveEntities) { //worry about populating entity list only if saving
									if (!child.CompareTag("background")) { //if it is not a background
										currEntities.Add(Entity.Parse(child)); //convert child to entity format
									}
								}
								Destroy(child.gameObject); //destroy transform from scene
							}
							GameManager.loadingBar.IncreaseProgress(loadIncrement / parent.childCount);
						}
					}

					if (saveEntities) {
						GameManager.loadingBar.SetProgress(0.45f); //update load progress
						GameManager.loadingBar.SetText("Saving.."); //inform player of process
						areas[currentAreaPos.x, currentAreaPos.y].SaveEntities(currEntities); //save to file
					}

					GameManager.loadingBar.SetProgress(0.5f); //update progress once complete
					StructureGridManager.instance.ResetGridStatus(); //reset grid so any loaded structures can properly snap to it
					StartCoroutine(areas[position.x, position.y].LoadToScene(navMesh)); //initiate async load area
					currentAreaPos = position;
				} //endif area empty check
			} //endif y bounds check
		} //endif x bounds check
	}

	public IEnumerator LoadAreasFromSave(string playerName, string worldName, Vector2Int loadedPos) {
		GameManager.loadingBar.ResetProgress();
		GameManager.loadingBar.SetText("Loading..");
		GameManager.loadingBar.Show();

		currentSaveFolder = Application.persistentDataPath + "/saves" + "/" + playerName + "/" + worldName + "/";

		if (!Directory.Exists(currentSaveFolder)) {
			StartCoroutine(GenerateAllAreas(playerName, worldName, Vector2Int.zero));
			yield break; //exit enumerator
		}

		areas = new Area[12, 12]; //instantiate array
		string currFilePath; //for storing current area filename
		for (int x = 0; x < 12; x++) {
			for (int y = 0; y < 12; y++) {
				currFilePath = currentSaveFolder + x + "_" + y + ".json";

				if (!File.Exists(currFilePath)) {
					//to do: prompt to regenerate entire world or cancel
					//StartCoroutine(GenerateAllAreas(playerName, worldName, Vector2Int.zero)); //generate everything again
					Debug.Log("File Not Found: " + currFilePath);
					yield break; //give up on loading
				} else {
					StreamReader reader = new StreamReader(File.Open(currFilePath, FileMode.Open));
					areas[x, y] = JsonUtility.FromJson<Area>(reader.ReadToEnd());
					reader.Close();
				}
			}
			GameManager.loadingBar.IncreaseProgress((x + 1) * 12 / 144.0f);
			yield return new WaitForEndOfFrame(); //allow for time between each row
		}
		LoadArea(loadedPos, false);
		yield return new WaitForEndOfFrame();
		GameManager.loadingBar.Hide();
	}
}
