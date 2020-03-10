using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace internal_Area {

	[Serializable]
	public class AreaTypeManager {
		public static int GenericAreaTotalSpreadChance { get { return totalSpreadChance; } }

		private static AreaTypeManager instance;
		private static string folderPath = Application.persistentDataPath + "/data/";
		private static string fileName = "AreaTypes.json";
		private static int totalSpreadChance;

		[SerializeField]
		private List<AreaType> areaTypes;

		public AreaTypeManager() {
			areaTypes = new List<AreaType>();
		}

		private void GenerateDefaultAreaTypes() {
			areaTypes.Add(new AreaType("Plains", 0, 0, 4, 200, 0, 0, 5000));
			areaTypes.Add(new AreaType("Forest", 0, 0, 3, 500, 0, 0, 25000));
			areaTypes.Add(new AreaType("Mountain", 0, 0, 2, 200, 0, 0, 10000));
			areaTypes.Add(new AreaType("Marsh", 0, 0, 0, 200, 0, 0, 20000));
			areaTypes.Add(new AreaType("City_RAM", 0, 0, 0, 200, 0, 0, 0));
			areaTypes.Add(new AreaType("City_CPR", 0, 0, 0, 200, 0, 0, 0));
			areaTypes.Add(new AreaType("City_HoZ", 0, 0, 0, 200, 0, 0, 0));
			areaTypes.Add(new AreaType("City_DV", 0, 0, 0, 200, 0, 0, 0));
			areaTypes.Add(new AreaType("Camp_Bandit", 0, 0, 0, 200, 0, 0, 0));
			areaTypes.Add(new AreaType("Dungeon_Minor Lich", 0, 0, 40, 200, 0, 0, 0));
			areaTypes.Add(new AreaType("Dungeon_Greater Demon", 0, 0, 0, 200, 0, 0, 0));
		}

		public static string[] GetAllAreaTypeNames() {
			string[] temp = new string[instance.areaTypes.Count];
			for (int i = 0; i < temp.Length; i++) {
				temp[i] = instance.areaTypes[i].name;
			}
			return temp;
		}

		public static AreaType GetAreaType(int index) {
			if (index >= 0 && index < instance.areaTypes.Count) {
				return instance.areaTypes[index];
			} else {
				return instance.areaTypes[0];
			}
		}

		public static AreaType GetAreaType(string typeName) {
			for (int i = 0; i < instance.areaTypes.Count; i++) {
				if (instance.areaTypes[i].name.Equals(typeName)) {
					return instance.areaTypes[i];
				}
			}
			return instance.areaTypes[0];
		}

		public static void Initialize() {
			instance = new AreaTypeManager();
			PopulateAreaTypes();
		}

		private static void PopulateAreaTypes() {
			if (!Directory.Exists(folderPath)) { //ensure the folderpath exists
				Directory.CreateDirectory(folderPath);
			}

			string json;

			if (File.Exists(folderPath + fileName)) { //if the areatypes have been created already
				StreamReader reader = new StreamReader(folderPath + fileName); //open the file
				json = reader.ReadToEnd();
				JsonUtility.FromJsonOverwrite(json, instance); //load data into instance
				reader.Close(); //close file

				totalSpreadChance = 0;
				for (int i = 0; i < 4 && i < instance.areaTypes.Count; i++) {
					totalSpreadChance += instance.areaTypes[i].spreadChance;
				}
			} else { //will need to generate
				instance.GenerateDefaultAreaTypes();
				StreamWriter writer = new StreamWriter(folderPath + fileName); //create the file
				json = JsonUtility.ToJson(instance, true); //convert to json
				writer.Write(json); //write to file using json
				writer.Close(); //close the file
			}
		}
	}
}
