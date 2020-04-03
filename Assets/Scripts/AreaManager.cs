﻿using System.IO;
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
	public static AreaManager instance;

	private static string currentSaveFolder;
	private static Transform backgroundParent;
	private static Transform structureParent;
	private static Transform characterParent;
	private static Transform sceneryParent;
	private static Transform furnitureParent;
	private static Transform itemParent;
	private static NavMeshSurface navMesh;

	private Area[,] areas;
	private Vector2Int currentAreaPos;

	public static string CurrentSaveFolder { get { return currentSaveFolder; } }
	public static int AreaSize { get { return Area.Size; } }

	private void Awake() {
		if (instance != null) {
			Destroy(gameObject);
		} else {
			instance = this;
			AreaTypeManager.Initialize(); //ensure all area types are loaded before they are needed
			backgroundParent = transform.Find("Background");
			structureParent = transform.Find("Structures");
			sceneryParent = transform.Find("Scenery");
			characterParent = transform.Find("Characters");
			furnitureParent = transform.Find("Furniture");
			itemParent = transform.Find("Items");
			navMesh = transform.Find("NavMesh").GetComponent<NavMeshSurface>();
		}
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
		LoadingScreen.instance.ResetProgress();
		LoadingScreen.instance.Show();

		currentSaveFolder = Application.persistentDataPath + "/saves" + "/" + playerName + "/" + worldName + "/";

		if (!Directory.Exists(currentSaveFolder)) {
			Directory.CreateDirectory(currentSaveFolder);
		}

		areas = new Area[12, 12]; //create array

		float totalNumAreas = areas.GetLength(0) * areas.GetLength(1);
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
				LoadingScreen.instance.IncreaseProgress((areasCompleted - prevAreasCompleted) / totalNumAreas);
				prevAreasCompleted = areasCompleted;
				yield return new WaitForEndOfFrame(); //allow for time between each row
			} //x for loop
		}

		LoadingScreen.instance.SetProgress(1f);
		LoadArea(startPos, false);
		yield return new WaitForEndOfFrame();
		LoadingScreen.instance.Hide();
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
			Debug.Log("Out of Range Error: AreaManager.GenerateUniqueAreaData() => loaded types (" + (hardCodedAreaTypes.Count - 4) + ") > map size (" + (areas.GetLength(0) * areas.GetLength(1)) + ")");
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
			case "Item":
				return itemParent;
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
			default: //right as default
				LoadArea(new Vector2Int(currentAreaPos.x + 1, currentAreaPos.y), true);
				return;
		}
	}

	private void LoadArea(Vector2Int position, bool saveEntities) {
		if (position.x < areas.GetLength(0) && 0 <= position.x) { //if the x is within map bounds
			if (position.y < areas.GetLength(1) && 0 <= position.y) { //if y is within map bounds
				if (areas[position.x, position.y] != null) { //if the desired area isn't empty
					LoadingScreen.instance.ResetProgress();
					LoadingScreen.instance.SetText("Gathering save data..");
					LoadingScreen.instance.Show();

					float loadIncrement = 0.40f / (transform.childCount - 1);
					List<Entity> currEntities = new List<Entity>(); //list to store entities currently in scene
					List<Container> currContainers = new List<Container>();
					Transform child;
					bool destroyChild = false;
					foreach (Transform parent in transform) { //areamanager script is attached to parent transform of entity types
						if (!parent.name.Equals("Area Exits")) { //do not process items(yet) or area exits
							for (int index = parent.childCount - 1; index >= 0; index--) { //entity types have entities as children
								child = parent.GetChild(index);
								if (!child.CompareTag("Player") && !child.CompareTag("inventory")) { //if the object isn't the player or inventory; ensure not destroyed
									if (saveEntities) { //worry about populating entity list only if saving
										if (!child.CompareTag("background")) { //if it is not a background, then continue check for save
											destroyChild = false;

											if (child.CompareTag("container")) {
												currContainers.Add(child.GetComponent<Container>());
											} else if (child.CompareTag("item")) { //if it's an item, then we need to make sure it's not in a container
												bool accountedFor = false;
												for (int containerIndex = 0; containerIndex < currContainers.Count; containerIndex++) { //loop through all containers
													if (currContainers[containerIndex].Contains(child.name)) { //see if the container contains this item
														accountedFor = true; //flag true
														break; //stop looping
													} //end if contains
												} //end for

												if (!accountedFor) {
													currEntities.Add(Entity.Parse(child)); //convert child to entity format
													destroyChild = true;
												}
											} else {
												currEntities.Add(Entity.Parse(child)); //convert child to entity format
												destroyChild = true;
											} //end if child.comparetag
										} //end if background
									} //end if saveEntitites
									if (destroyChild) { //if the child needs to be destroyed -- not player or inventory
										Destroy(child.gameObject); //destroy gameobject from scene
									}
								} //end if player
								LoadingScreen.instance.IncreaseProgress(loadIncrement / parent.childCount);
							} //end for
						} //end if area exit
					} //end foreach

					if (saveEntities) {
						LoadingScreen.instance.SetText("Saving.."); //inform player of process
						areas[currentAreaPos.x, currentAreaPos.y].Save(currContainers, currEntities);
						LoadingScreen.instance.SetProgress(0.45f); //update load progress

						for (int i = currContainers.Count - 1; i >=0; i--) { //go through all containers once saved
							currContainers[i].SelfDestruct(); //remove containers and corresponding items from scene
						}
					}

					LoadingScreen.instance.SetText("Loading Next Area.."); //inform player of process
					UpdateAreaExits(position);

					LoadingScreen.instance.SetProgress(0.5f); //update progress once complete
					StructureGridManager.instance.ResetGridStatus(); //reset grid so any loaded structures can properly snap to it
					StartCoroutine(areas[position.x, position.y].LoadToScene(navMesh)); //initiate async load area
					currentAreaPos = position;
				} //endif area empty check
			} //endif y bounds check
		} //endif x bounds check
	}

	public IEnumerator LoadAreasFromSave(string playerName, string worldName, Vector2Int loadedPos) {
		LoadingScreen.instance.ResetProgress();
		LoadingScreen.instance.SetText("Loading..");
		LoadingScreen.instance.Show();

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
					Debug.Log("Save Data Error: AreaManager.LoadAreasFromSave(...) => filename: " + currFilePath);
					yield break; //give up on loading
				} else {
					StreamReader reader = new StreamReader(File.Open(currFilePath, FileMode.Open));
					areas[x, y] = JsonUtility.FromJson<Area>(reader.ReadToEnd());
					reader.Close();
				}
			}
			LoadingScreen.instance.IncreaseProgress((x + 1) * 12 / 144.0f);
			yield return new WaitForEndOfFrame(); //allow for time between each row
		}
		LoadArea(loadedPos, false);
		yield return new WaitForEndOfFrame();
		LoadingScreen.instance.Hide();
	}

	private void UpdateAreaExits(Vector2Int position) {
		AreaExit currExit;
		currExit = transform.Find("Area Exits").Find("Area Exit_Left").GetChild(0).GetComponent<AreaExit>(); //get left exit
		if (0 < position.x - 1) { //check bounds
			currExit.EnableInteraction(); //enable left exit
			currExit.SetExitInteractMessage(areas[position.x - 1, position.y].TypeName, position + Vector2Int.left); //pass info to area exit
		} else { //out of bounds
			currExit.DisableInteraction(); //disable left exit
		}

		currExit = transform.Find("Area Exits").Find("Area Exit_Right").GetChild(0).GetComponent<AreaExit>(); //get next exit
		if (position.x + 1 < areas.GetLength(0)) { //check bounds
			currExit.EnableInteraction(); //enable exit
			currExit.SetExitInteractMessage(areas[position.x + 1, position.y].TypeName, position + Vector2Int.right); //pass info to area exit
		} else { //out of bounds
			currExit.DisableInteraction(); //disable exit
		}

		currExit = transform.Find("Area Exits").Find("Area Exit_Up").GetChild(0).GetComponent<AreaExit>(); //get next exit
		if (0 < position.y - 1) { //check bounds
			currExit.EnableInteraction(); //enable exit
			currExit.SetExitInteractMessage(areas[position.x, position.y - 1].TypeName, position + Vector2Int.down); //pass info to area exit
		} else { //out of bounds
			currExit.DisableInteraction(); //disable exit
		}

		currExit = transform.Find("Area Exits").Find("Area Exit_Down").GetChild(0).GetComponent<AreaExit>(); //get next exit
		if (position.y + 1 < areas.GetLength(1)) { //check bounds
			currExit.EnableInteraction(); //enable exit
			currExit.SetExitInteractMessage(areas[position.x, position.y + 1].TypeName, position + Vector2Int.up); //pass info to area exit
		} else { //out of bounds
			currExit.DisableInteraction(); //disable exit
		}
	}
}
