using System.Collections.Generic;
using UnityEngine;

namespace NPC {
	/// <summary>
	/// Notes:
	/// </summary>
	public static class NPCDatabase {
		private static string folderPath = Application.persistentDataPath + "/data/";

		private static List<NPCType> types;
		private static string[] firstNames;
		private static string[] lastNames;
		private static string[] bgFamily;
		private static string[] bgChildhood;

		public static string GenerateName() {
			string name = "";
			if (firstNames != null && firstNames.Length > 0) {
				name += firstNames[Random.Range(0, firstNames.Length)];
			}
			if (lastNames != null && lastNames.Length > 0) {
				name += lastNames[Random.Range(0, lastNames.Length)];
			}
			return name;
		}

		public static string GenerateBackground(NPCType type) {
			string background = "";
			if (bgFamily != null && bgFamily.Length > 0) {
				background += bgFamily[Random.Range(0, bgFamily.Length)];
			}
			if (bgChildhood != null && bgChildhood.Length > 0) {
				background += bgChildhood[Random.Range(0, bgChildhood.Length)];
			}
			return background;
		}

		public static NPCType GetRandomType() {
			if (types != null && types.Count > 0) {
				return types[Random.Range(0, types.Count)];
			}
			return null;
		}

		public static NPCType GetType(string typeName) {
			if (types != null) {
				for (int i = 0; i < types.Count; i++) {
					if (types[i].Name.Equals(typeName)) {
						return types[i];
					}
				}
			}
			return null;
		}

		public static void Initialize() {
			types = new List<NPCType>();

		}
	}
}
