﻿using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using AreaManagerNS.AreaNS;

namespace AreaManagerNS {

	//loads (instantiates background, scenery, & npcs into scene) previously generated areas
	//procedurally generates areas

	//idea: list of hard-coded positions for unique areas, list of hard-coded area types -- same list.count
	//		-remove from list once used
	public class AreaManager : MonoBehaviour {
		private static string currentSaveFolder;
		private static Transform backgroundParent;
		private static Transform structureParent;
		private static Transform characterParent;
		private static Transform sceneryParent;

		private Transform canvas_area;
		private Area[,] areas;
		private Vector2Int currentAreaPos;

		public static string CurrentSaveFolder { get { return currentSaveFolder; } }

		private void Awake() {
			Area.LoadAreaTypes(); //ensure all area types are loaded before they are needed
			backgroundParent = transform.Find("Background");
			structureParent = transform.Find("Structures");
			sceneryParent = transform.Find("Scenery");
			characterParent = transform.Find("Characters");
		}

		private Area CreateArea(Vector2Int position, AreaType parentAreaType) {
			Area tempArea = new Area(position); //stores the area that will be returned
			AreaType tempType; //stores the type we are currently checking

			int cumulativeChance = Area.GenericAreaTotalSpreadChance + parentAreaType.spreadChance; //total of all possible spread chances
			int randomNum = Random.Range(0, cumulativeChance); //generate random number based on cumulative
			cumulativeChance = 0; //reset cumulative to process end result
			for (int i = 0; i < 4; i++) { //go through generic area types
				tempType = Area.GetAreaType(i); //get current type

				cumulativeChance += tempType.spreadChance; //update cumulative chance
				if (tempType.Equals(parentAreaType)) { //if same type as parent area
					cumulativeChance += tempType.spreadChance; //add chance again
				}

				if (randomNum < cumulativeChance) { //values increase as list goes on due to cumulative, so if num is < than value, the probability is maintained
					tempArea.AssignType(tempType); //assign the lucky winner
					break; //stop processing dictionary
				}
			}

			StartCoroutine(tempArea.Populate()); //populate and save file when done -- async
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
			Area currArea; //current area to set
			Vector2Int currPos; //current map position

			// hardcoded areas to 'fix' procedural generation
			currPos = new Vector2Int(11, 0);
			currArea = new Area(currPos);
			currArea.AssignType("Mountain");
			areas[currPos.x, currPos.y] = currArea;

			currPos = new Vector2Int(1, 1);
			currArea = new Area(currPos);
			currArea.AssignType("City_RAM");
			areas[currPos.x, currPos.y] = currArea;

			currPos = new Vector2Int(10, 1);
			currArea = new Area(currPos);
			currArea.AssignType("City_HoZ");
			areas[currPos.x, currPos.y] = currArea;

			currPos = new Vector2Int(1, 10);
			currArea = new Area(currPos);
			currArea.AssignType("City_CPR");
			areas[currPos.x, currPos.y] = currArea;

			currPos = new Vector2Int(10, 10);
			currArea = new Area(currPos);
			currArea.AssignType("City_DV");
			areas[currPos.x, currPos.y] = currArea;

			currPos = new Vector2Int(6, 8);
			currArea = new Area(currPos);
			currArea.AssignType("Dungeon_Minor Lich");
			areas[currPos.x, currPos.y] = currArea;

			currPos = new Vector2Int(6, 8);
			currArea = new Area(currPos);
			currArea.AssignType("Dungeon_Greater Demon");
			areas[currPos.x, currPos.y] = currArea;

			bool generationComplete = true;
			AreaType typeToSpread; //the type of area that is most likely to spread
			do {
				for (int x = 0; x < 12; x++) {
					for (int y = 0; y < 12; y++) {
						if (areas[x, y] != null) { //start with hard-coded areas
							if (areas[x,y].Type.name.Contains("City")) { //Cities are always in plains
								typeToSpread = Area.GetAreaType(0); //spread plains type
							} else if (areas[x, y].Type.name.Contains("Dungeon")) { //dungeons are alwas in marshes
								typeToSpread = Area.GetAreaType(2); //spread dungeon type
							} else {
								typeToSpread = areas[x, y].Type;
							}

							//createArea 4 adjacent areas
							if (x - 1 >= 0) { //if area adjacent left exists
								areas[x, y] = CreateArea(new Vector2Int(x - 1, y), typeToSpread); //create area adjacent left
							}
							if (x + 1 < areas.GetLength(0)) { //if area adjacent right exists
								areas[x, y] = CreateArea(new Vector2Int(x - 1, y), typeToSpread); //create area adjacent left
							}
							if (y - 1 >= 0) { //if area adjacent up exists
								areas[x, y] = CreateArea(new Vector2Int(x - 1, y), typeToSpread); //create area adjacent left
							}
							if (y + 1 < areas.GetLength(1)) { //if area adjacent down exists
								areas[x, y] = CreateArea(new Vector2Int(x - 1, y), typeToSpread); //create area adjacent left
							}
						}
					}
					//GameManager.loadingBar.IncreaseProgress((x + 1) * 12 / 144.0f);
					yield return new WaitForEndOfFrame(); //allow for time between each row
				}
			} while (!generationComplete);

			LoadArea(startPos);
			yield return new WaitForEndOfFrame();
			GameManager.loadingBar.Hide();
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
				default:
					return characterParent;
			}
		}

		public void LoadArea(Directions direction) {
			switch (direction) {
				case Directions.up:
					LoadArea(new Vector2Int(currentAreaPos.x, currentAreaPos.y - 1));
					break;
				case Directions.down:
					LoadArea(new Vector2Int(currentAreaPos.x, currentAreaPos.y + 1));
					break;
				case Directions.left:
					LoadArea(new Vector2Int(currentAreaPos.x - 1, currentAreaPos.y));
					break;
				case Directions.right:
					LoadArea(new Vector2Int(currentAreaPos.x + 1, currentAreaPos.y));
					break;
				default:
					return;
			}
		}

		public void LoadArea(Vector2Int position) {
			if (position.x < areas.GetLength(0) && 0 <= position.x) { //if the x is within map bounds
				if (position.y < areas.GetLength(1) && 0 <= position.y) { //if y is within map bounds
					if (areas[position.x, position.y] != null) { //if the desired area isn't empty
						List<Entity> currEntities = new List<Entity>(); //list to store entities currently in scene
						foreach (Transform parent in transform) { //areamanager script is attached to parent transform of entity types
							Transform child;
							for (int index = parent.childCount - 1; index >= 0; index--) { //entity types have entities as children
								child = parent.GetChild(index);
								if (!child.CompareTag("Player")) {
									currEntities.Add(Entity.Parse(child)); //convert child to entity format
									Destroy(child.gameObject); //destroy transform from scene
								}
							}
						}
						areas[currentAreaPos.x, currentAreaPos.y].SaveEntities(currEntities);
						StartCoroutine(areas[position.x, position.y].LoadToScene());
						currentAreaPos = position;
					}
				}
			}
		}

		public IEnumerator LoadAreasFromSave(string playerName, string worldName, Vector2Int loadedPos) {
			GameManager.loadingBar.ResetProgress();
			GameManager.loadingBar.Show();

			currentSaveFolder = Application.persistentDataPath + "/saves" + "/" + playerName + "/" + worldName + "/";

			if (!Directory.Exists(currentSaveFolder)) {
				StartCoroutine(GenerateAllAreas(playerName, worldName, Vector2Int.zero));
				yield break; //exit enumerator
			}

			string currFilePath;
			areas = new Area[12, 12];

			XmlSerializer xr = new XmlSerializer(typeof(Area));
			FileStream file;

			GameManager.loadingBar.IncreaseProgress(0.1f);

			for (int x = 0; x < 12; x++) {
				for (int y = 0; y < 12; y++) {
					currFilePath = currentSaveFolder + x + "_" + y + ".xml";

					if (!File.Exists(currFilePath)) {
						if (x - 1 >= 0 && areas[x - 1, y] != null) { //if there's an area left of missing one
							areas[x, y] = CreateArea(new Vector2Int(x, y), areas[x - 1, y].Type); //create area using left as parent type
						} else if (x + 1 < areas.GetLength(0) && areas[x + 1, y] != null) { //if there's an area right of missing one
							areas[x, y] = CreateArea(new Vector2Int(x, y), areas[x + 1, y].Type); //create area using right as parent type
						} else { //both are null
							//to do: prompt for regenerate world or cancel loading
							StartCoroutine(GenerateAllAreas(playerName, worldName, Vector2Int.zero)); //generate everything again
							yield break; //give up on loading
						}
					} else {
						file = new FileStream(currFilePath, FileMode.Open);
						areas[x, y] = xr.Deserialize(file) as Area;
						file.Close();
					}
				}
				GameManager.loadingBar.IncreaseProgress((x + 1) * 12 / 144.0f);
				yield return new WaitForEndOfFrame(); //allow for time between each row
			}
			LoadArea(loadedPos);
			yield return new WaitForEndOfFrame();
			GameManager.loadingBar.Hide();
		}
	}
}
