using System;
using System.Collections.Generic;

namespace Areas {

	[Serializable]
	public static class AreaTypeManager {
		public static int GenericAreaTotalSpreadChance { get { return totalSpreadChance; } }
		private static string filePath_areaTypes = GameManager.path_gameData + "areaTypes.json";
		private static int totalSpreadChance;
		private static List<AreaType> areaTypes;

		private static void GenerateDefaultAreaTypes() {
			areaTypes = new List<AreaType>();
			areaTypes.Add(new AreaType("plains", new Vector2IntS(0, 1), new Vector2IntS(50, 500), new Vector2IntS(0, 0), 5000));
			areaTypes.Add(new AreaType("forest", new Vector2IntS(0, 1), new Vector2IntS(50, 500), new Vector2IntS(0, 0), 25000));
			areaTypes.Add(new AreaType("mountain", new Vector2IntS(0, 1), new Vector2IntS(50, 500), new Vector2IntS(0, 0), 10000));
			areaTypes.Add(new AreaType("marsh", new Vector2IntS(0, 1), new Vector2IntS(50, 500), new Vector2IntS(0, 0), 20000));
			areaTypes.Add(new AreaType("city", new Vector2IntS(10, 20), new Vector2IntS(50, 300), new Vector2IntS(0, 0), 0));
			areaTypes.Add(new AreaType("camp", new Vector2IntS(3, 10), new Vector2IntS(50, 500), new Vector2IntS(0, 0), 0));
			areaTypes.Add(new AreaType("dungeon", new Vector2IntS(0, 0), new Vector2IntS(50, 500), new Vector2IntS(0, 0), 0));
			GameManager.SaveObject(areaTypes, filePath_areaTypes);
		}

		public static string[] GetAllAreaTypeNames() {
			string[] temp = new string[areaTypes.Count];
			for (int i = 0; i < temp.Length; i++) {
				temp[i] = areaTypes[i].name;
			}
			return temp;
		}

		public static AreaType GetAreaType(int index) {
			if (index >= 0 && index < areaTypes.Count) {
				return areaTypes[index];
			} else {
				return areaTypes[0];
			}
		}

		public static AreaType GetAreaType(string typeName) {
			for (int i = 0; i < areaTypes.Count; i++) {
				if (areaTypes[i].name.Equals(typeName)) {
					return areaTypes[i];
				}
			}
			return areaTypes[0];
		}

		public static void Initialize() {
			PopulateAreaTypes();
		}

		private static void PopulateAreaTypes() {
			areaTypes = GameManager.LoadObject<List<AreaType>>(filePath_areaTypes);

			if (areaTypes == null) { //if the areatypes have been created already
				GenerateDefaultAreaTypes();
			}

			totalSpreadChance = 0;
			for (int i = 0; i < 4 && i < areaTypes.Count; i++) {
				totalSpreadChance += areaTypes[i].spreadChance;
			}
		}
	}
}
