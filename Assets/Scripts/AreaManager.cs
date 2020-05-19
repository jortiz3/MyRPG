using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Areas;

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
	private bool save_load;

	public static string CurrentSaveFolder { get { return currentSaveFolder; } }
	public static int AreaSize { get { return Area.Size; } }

	public Vector2Int Position { get { return currentAreaPos; } }
	public bool SaveOrLoadInProgress { get { return save_load; }  set { save_load = value; } }

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
		LoadingScreen.instance.SetText("Generating world areas..");
		LoadingScreen.instance.Show();
		save_load = true;

		currentSaveFolder = Application.persistentDataPath + "/saves" + "/" + playerName + "/" + worldName + "/";

		if (!Directory.Exists(currentSaveFolder)) {
			Directory.CreateDirectory(currentSaveFolder);
		}

		areas = new Area[12, 12]; //create array
		int areasCompleted = GenerateUniqueAreaData();

		if (areasCompleted < 0) { //if something went wrong
			yield break; //abort
		}

		float totalNumAreas = areas.GetLength(0) * areas.GetLength(1); //get total num of areas based off array just created -- only calculate 1 time
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
				LoadingScreen.instance.IncreaseProgress((areasCompleted - prevAreasCompleted) / totalNumAreas * 0.30f);
				prevAreasCompleted = areasCompleted;
				yield return new WaitForEndOfFrame(); //allow for time between each row
			} //x for loop
		}

		LoadingScreen.instance.SetText("Populating world areas..");

		while (Area.PopulationInProgress) {
			yield return new WaitForEndOfFrame();
		}

		LoadingScreen.instance.SetProgress(0.5f);

		currentAreaPos = new Vector2Int(-1, -1); //reset the currentArea in case a game was loaded previously
		LoadArea(position: startPos, saveEntities: false, resetLoadingScreen: false); //loading screen updated/hidden elsewhere
	}

	/// <summary>
	/// Generates hard-coded, unique areas and places them into the array.
	/// </summary>
	/// <returns>The number of hard-coded, unique areas placed.</returns>
	private int GenerateUniqueAreaData() {
		List<string> hardCodedAreaTypes = new List<string>();
		hardCodedAreaTypes.AddRange(AreaTypeManager.GetAllAreaTypeNames());

		if (hardCodedAreaTypes.Count > areas.GetLength(0) * areas.GetLength(1)) { //if there are somehow more unique areas than available slots
			Debug.Log("Out of Range Error: AreaManager.GenerateUniqueAreaData() => loaded types (" + (hardCodedAreaTypes.Count) + ") > map size (" + (areas.GetLength(0) * areas.GetLength(1)) + ")");
			return -1;
		}

		hardCodedAreaTypes.RemoveRange(0, 7); //remove default area types
		int numUniqueAreas = 0;

		List<Vector2Int> cityPositions = new List<Vector2Int>(); //instantiate list
		cityPositions.Add(new Vector2Int(1, 1)); //place city locations
		cityPositions.Add(new Vector2Int(10, 1));
		cityPositions.Add(new Vector2Int(1, 10));
		cityPositions.Add(new Vector2Int(10, 10));

		int randomIndex; //stores random number
		Vector2Int tempPos; //stores the current position
		Area currArea;
		string[] cityOwners = new string[] { "CPR", "HoZ", "RAM", "DV" };
		for (int cities = 3; cities >= 0; cities--) { //next 4 in types 'should' be cities
			randomIndex = UnityEngine.Random.Range(0, cityPositions.Count); //pick random position
			tempPos = cityPositions[randomIndex]; //store position

			currArea = new Area(tempPos, "city", cityOwners[cities]); //create area at position
			StartCoroutine(currArea.Populate(false)); //populate the area
			areas[tempPos.x, tempPos.y] = currArea; //add area to array
			cityPositions.RemoveAt(randomIndex); //remove position from list
			numUniqueAreas++;
		}

		for (int typeIndex = hardCodedAreaTypes.Count - 1; typeIndex >= 0; typeIndex--) { //go through remaining area types
			do {
				tempPos = new Vector2Int(UnityEngine.Random.Range(0, 12), UnityEngine.Random.Range(0, 12)); //get a random position
			} while (areas[tempPos.x, tempPos.y] != null); //keep trying until we get an empty slot

			currArea = new Area(tempPos, hardCodedAreaTypes[typeIndex]); //create new area at location
			StartCoroutine(currArea.Populate(false)); //populate the area
			areas[tempPos.x, tempPos.y] = currArea; //add next area type to the randomly generated position

			hardCodedAreaTypes.RemoveAt(typeIndex); //remove the area type from the list
			numUniqueAreas++;
		}

		return numUniqueAreas;
	}

	public static Transform GetEntityParent(string entityTypeName) {
		switch (entityTypeName.ToLower()) {
			case "scenery":
				return sceneryParent;
			case "structure":
				return structureParent;
			case "character":
				return characterParent;
			case "background":
				return backgroundParent;
			case "furniture":
				return furnitureParent;
			case "item":
				return itemParent;
			default:
				return characterParent;
		}
	}

	public bool LoadArea(Directions direction, bool teleportPlayer = false) {
		Vector3 teleportPos = Player.instance.transform.position; //get current player position
		bool loaded = false; //store whether next area was loaded
		switch (direction) { //based on direction
			case Directions.up:
				loaded = LoadArea(new Vector2Int(currentAreaPos.x, currentAreaPos.y - 1), saveEntities: true); //load the area north of current
				teleportPos.y = -Mathf.Abs(teleportPos.y); //always set position to bottom of screen
				break;
			case Directions.down:
				loaded = LoadArea(new Vector2Int(currentAreaPos.x, currentAreaPos.y + 1), saveEntities: true); //load area south of current
				teleportPos.y = Mathf.Abs(teleportPos.y); //always set position to top
				break;
			case Directions.left:
				loaded = LoadArea(new Vector2Int(currentAreaPos.x - 1, currentAreaPos.y), saveEntities: true); //load area west of current
				teleportPos.x = Mathf.Abs(teleportPos.x); //always set position to right
				break;
			default: //right as default
				loaded = LoadArea(new Vector2Int(currentAreaPos.x + 1, currentAreaPos.y), saveEntities: true); //load area east of current
				teleportPos.x = -Mathf.Abs(teleportPos.x); //always set position to left
				break;
		}

		if (teleportPlayer) {
			if (loaded) {
				Player.instance.TeleportToPos(teleportPos);
			}
		}
		return loaded;
	}

	private bool LoadArea(Vector2Int position, bool saveEntities = false, bool resetLoadingScreen = true) {
		if (position.x < areas.GetLength(0) && 0 <= position.x) { //if the x is within map bounds
			if (position.y < areas.GetLength(1) && 0 <= position.y) { //if y is within map bounds
				if (areas[position.x, position.y] != null) { //if the desired area isn't empty
					save_load = true;

					if (resetLoadingScreen) {
						LoadingScreen.instance.ResetProgress();
						LoadingScreen.instance.Show();
					}
					LoadingScreen.instance.SetText("Gathering Current Area Data..");

					bool loadingNewArea = currentAreaPos.Equals(position) ? false : true; //if the given area position is the same as current, different steps will be taken
					float numIncrements = saveEntities ? transform.childCount + 1 : transform.childCount;
					float loadIncrement = ((1f - LoadingScreen.instance.GetProgress()) / 2f) / numIncrements;
					List<Entity> currEntities = new List<Entity>(); //list to store entities currently in scene
					Transform parent;
					Transform child;
					bool destroyChild = false;
					for (int i = 0; i < transform.childCount; i++) { //this script is attached to parent of all entities
						parent = transform.GetChild(i); //get the current parent transform
						if (!parent.name.Equals("Area Exits")) { //do not process items(yet) or area exits
							for (int index = parent.childCount - 1; index >= 0; index--) { //entity types have entities as children
								child = parent.GetChild(index);
								if (!child.CompareTag("Player")) { //if the object isn't the player; ensure not destroyed
									if (!child.CompareTag("background")) { //if it is not a background, then continue check for save
										destroyChild = false;

										if (child.CompareTag("item")) { //if it's an item, then we need to make sure it's not in a container
											Item currItem = child.GetComponent<Item>();
											bool belongsToPlayer = currItem.ContainerID == Inventory.instance.InstanceID ? true : false;

											if (!belongsToPlayer || !loadingNewArea) { //if reloading area, intent is to save player inventory
												if (saveEntities) {
													currEntities.Add(Entity.Parse(child)); //convert child to entity format
												}
												destroyChild = true;
											}
										} else {
											if (saveEntities) { //only add to list if saving entities
												currEntities.Add(Entity.Parse(child)); //convert child to entity format
											}
											destroyChild = true;
										} //end if child.comparetag
									} else {
										destroyChild = true;
									} //end if background
									if (loadingNewArea) { //if loading new area
										if (destroyChild) { //if the child needs to be destroyed -- not player or inventory
											Destroy(child.gameObject); //destroy gameobject from scene
										}
									}
								} //end if player
							} //end for child.child
						} //end if area exit
						LoadingScreen.instance.IncreaseProgress(loadIncrement);
					} //end for parent

					if (saveEntities) {
						LoadingScreen.instance.SetText("Saving.."); //inform player of process
						areas[currentAreaPos.x, currentAreaPos.y].Save(currEntities);
						GameManager.instance.SaveGame();
						LoadingScreen.instance.IncreaseProgress(loadIncrement);
					}

					if (loadingNewArea) { //if loading a new area
						UpdateAreaExits(position); //hide/display exits when appropriate
						Container.ResetInstanceIDs(); //reset the ids for the next area
						StructureGridManager.instance.ResetGridStatus(); //reset grid so any loaded structures can properly snap to it
						StartCoroutine(areas[position.x, position.y].LoadToScene(navMesh)); //initiate async load area
						currentAreaPos = position;
					}
					return true;
				} //endif area empty check
			} //endif y bounds check
		} //endif x bounds check
		return false;
	}

	public IEnumerator LoadAreasFromSave(string playerName, string worldName, Vector2Int loadedPos) {
		LoadingScreen.instance.ResetProgress();
		LoadingScreen.instance.SetText("Loading Save Data..");
		LoadingScreen.instance.Show();

		currentSaveFolder = GameManager.path_saveData + playerName + "/" + worldName + "/";

		if (!Directory.Exists(currentSaveFolder)) {
			StartCoroutine(GenerateAllAreas(playerName, worldName, Vector2Int.zero));
			yield break; //exit enumerator
		}

		areas = new Area[12, 12]; //instantiate array
		string currFilePath; //for storing current area filename
		float loadingIncrement = (1f - LoadingScreen.instance.GetProgress()) * (1 / 12f) * 0.3f;
		for (int x = 0; x < 12; x++) {
			for (int y = 0; y < 12; y++) {
				currFilePath = currentSaveFolder + x + "_" + y + ".json";

				if (!File.Exists(currFilePath)) {
					//to do: prompt to regenerate entire world or cancel
					//StartCoroutine(GenerateAllAreas(playerName, worldName, Vector2Int.zero)); //generate everything again
					Debug.Log("Save Data Error: AreaManager.LoadAreasFromSave(...) => filename: " + currFilePath);
					LoadingScreen.instance.Hide();
					GameManager.instance.QuitToMainMenu();
					yield break; //give up on loading
				} else {
					areas[x, y] = GameManager.LoadObject<Area>(currFilePath);
				}
			}
			LoadingScreen.instance.IncreaseProgress(loadingIncrement);
			yield return new WaitForEndOfFrame(); //allow for time between each row
		}
		currentAreaPos = new Vector2Int(-1, -1); //reset current pos
		LoadArea(loadedPos, resetLoadingScreen: false); //loading screen updated/hidden elsewhere
	}

	public void SaveCurrentArea() {
		LoadArea(position: currentAreaPos, saveEntities: true, resetLoadingScreen: false);
	}

	private void UpdateAreaExits(Vector2Int position) {
		AreaExit currExit;
		currExit = transform.Find("Area Exits").Find("Area Exit_Left").GetChild(0).GetComponent<AreaExit>(); //get left exit
		if (0 < position.x - 1) { //check bounds
			currExit.SetActive(); //enable exit
			currExit.SetExitInteractMessage(areas[position.x - 1, position.y].TypeName, position + Vector2Int.left); //pass info to area exit
		} else { //out of bounds
			currExit.SetActive(false, false); //disable exit
		}

		currExit = transform.Find("Area Exits").Find("Area Exit_Right").GetChild(0).GetComponent<AreaExit>(); //get next exit
		if (position.x + 1 < areas.GetLength(0)) { //check bounds
			currExit.SetActive(); //enable exit
			currExit.SetExitInteractMessage(areas[position.x + 1, position.y].TypeName, position + Vector2Int.right); //pass info to area exit
		} else { //out of bounds
			currExit.SetActive(false, false); //disable exit
		}

		currExit = transform.Find("Area Exits").Find("Area Exit_Up").GetChild(0).GetComponent<AreaExit>(); //get next exit
		if (0 < position.y - 1) { //check bounds
			currExit.SetActive(); //enable exit
			currExit.SetExitInteractMessage(areas[position.x, position.y - 1].TypeName, position + Vector2Int.down); //pass info to area exit
		} else { //out of bounds
			currExit.SetActive(false, false); //disable exit
		}

		currExit = transform.Find("Area Exits").Find("Area Exit_Down").GetChild(0).GetComponent<AreaExit>(); //get next exit
		if (position.y + 1 < areas.GetLength(1)) { //check bounds
			currExit.SetActive(); //enable exit
			currExit.SetExitInteractMessage(areas[position.x, position.y + 1].TypeName, position + Vector2Int.up); //pass info to area exit
		} else { //out of bounds
			currExit.SetActive(false, false); //disable exit
		}
	}
}
