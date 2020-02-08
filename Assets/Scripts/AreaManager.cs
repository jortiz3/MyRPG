using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using AreaManagerNS.AreaNS;

namespace AreaManagerNS {

	//loads (instantiates background, scenery, & npcs into scene) previously generated areas
	//procedurally generates areas
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

		private Area CreateArea(Vector2Int position) {
			Area tempArea = new Area(position);

			Dictionary<AreaType, int> chances = new Dictionary<AreaType, int>();
			AreaType tempType;
			if (position.x - 1 >= 0 && areas[position.x - 1, position.y] != null) { //if the area is within bounds of the array
				tempType = areas[position.x - 1, position.y].GetAreaType(); //try to get the type
				if (tempType != null) { //if areatype has value
					chances.Add(tempType, 1); //set spreadchance in dictionary to cumulative so far
				}
			}
			if (position.x + 1 < areas.GetLength(0) && areas[position.x + 1, position.y] != null) {
				tempType = areas[position.x + 1, position.y].GetAreaType();
				if (tempType != null) {
					if (!chances.ContainsKey(tempType)) {
						chances.Add(tempType, 1);
					} else {
						chances[tempType] += 1;
					}
				}
			}
			if (position.y - 1 >= 0 && areas[position.x, position.y - 1] != null) {
				tempType = areas[position.x, position.y - 1].GetAreaType();
				if (tempType != null) {
					if (!chances.ContainsKey(tempType)) {
						chances.Add(tempType, 1);
					} else {
						chances[tempType] += 1;
					}
				}
			}
			if (position.y + 1 < areas.GetLength(1) && areas[position.x, position.y + 1] != null) {
				tempType = areas[position.x, position.y + 1].GetAreaType();
				if (tempType != null) {
					if (!chances.ContainsKey(tempType)) {
						chances.Add(tempType, 1);
					} else {
						chances[tempType] += 1;
					}
				}
			}

			if (chances.Count < 4) { //none of surrounding areas have been instantiated
				for (int i = 0; i < 4; i++) { //get the base generic area types
					tempType = Area.GetAreaType(i);
					if (!chances.ContainsKey(tempType)) {
						chances.Add(tempType, 1);
					}
				}
			}

			int cumulativeChance = 0;
			foreach (KeyValuePair<AreaType, int> kvp in chances) { //go through and update appropriate values
				cumulativeChance += chances[kvp.Key] * kvp.Key.spreadChance; //update cumulative chance
			}

			int randomNum = Random.Range(0, cumulativeChance); //generate random number based on cumulative
			cumulativeChance = 0;
			foreach (KeyValuePair<AreaType, int> kvp in chances) { //go through and update appropriate values
				cumulativeChance += chances[kvp.Key] * kvp.Key.spreadChance; //update cumulative chance
				if (randomNum < cumulativeChance) { //values increase as list goes on due to cumulative, so if num is < than value, the probability is maintained
					tempArea.AssignType(kvp.Key); //assign the lucky winner
					break; //stop processing dictionary
				}
			}

			tempArea.AssignType(0);

			StartCoroutine(tempArea.Populate()); //populate and save file when done -- async
			return tempArea; //return the area and continue
		}

		public IEnumerator GenerateWorldAreas(string playerName, string worldName) {
			GameManager.loadingBar.ResetProgress();
			GameManager.loadingBar.Show();

			currentSaveFolder = Application.persistentDataPath + "/saves" + "/" + playerName + "/" + worldName + "/";

			if (!Directory.Exists(currentSaveFolder)) {
				Directory.CreateDirectory(currentSaveFolder);
			}

			areas = new Area[12, 12];
			Area currArea;
			Vector2Int currPos;

			currPos = new Vector2Int(1, 1);
			currArea = new Area(currPos);
			currArea.AssignType("City_RAM");
			areas[currPos.x, currPos.y] = currArea;

			currArea = new Area(currPos);
			currArea.AssignType("City_HoZ");
			areas[10, 10] = currArea;

			currArea = new Area(currPos);
			currArea.AssignType("City_CPR");
			areas[1, 10] = currArea;

			currArea = new Area(currPos);
			currArea.AssignType("City_DV");
			areas[10, 1] = currArea;

			for (int x = 0; x < 12; x++) {
				for (int y = 0; y < 12; y++) {
					if (areas[x, y] == null) {
						areas[x, y] = CreateArea(new Vector2Int(x, y)); //store reference
					}
				}
				GameManager.loadingBar.IncreaseProgress((x + 1) * 12 / 144.0f);
				yield return new WaitForEndOfFrame(); //allow for time between each row
			}

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

		public IEnumerator LoadAreasFromWorld(string playerName, string worldName) {
			GameManager.loadingBar.ResetProgress();
			GameManager.loadingBar.Show();

			currentSaveFolder = Application.persistentDataPath + "/saves" + "/" + playerName + "/" + worldName + "/";

			if (!Directory.Exists(currentSaveFolder)) {
				GenerateWorldAreas(playerName, worldName);
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
						areas[x, y] = CreateArea(new Vector2Int(x, y));
					} else {
						file = new FileStream(currFilePath, FileMode.Open);
						areas[x, y] = xr.Deserialize(file) as Area;
						file.Close();
					}
				}
				GameManager.loadingBar.IncreaseProgress((x + 1) * 12 / 144.0f);
				yield return new WaitForEndOfFrame(); //allow for time between each row
			}

			GameManager.loadingBar.Hide();
		}
	}
}
