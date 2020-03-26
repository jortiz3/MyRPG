using System.Collections.Generic;

namespace Items {
	public class ItemModifierDatabase {
		private static List<ItemModifier> prefixes;
		private static List<ItemModifier> suffixes;

		public static ItemModifier GetPrefix(string modName) {
			for (int i = 0; i < prefixes.Count; i++) {
				if (prefixes[i].Name.Equals(modName)) {
					return prefixes[i];
				}
			}
			return null;
		}

		public static ItemModifier GetSuffix(string modName) {
			for (int i = 0; i < suffixes.Count; i++) {
				if (suffixes[i].Name.Equals(modName)) {
					return suffixes[i];
				}
			}
			return null;
		}

		public static void Initialize() {
			prefixes = new List<ItemModifier>();
			//add prefixes

			suffixes = new List<ItemModifier>();
			//add suffixes
		}


	}
}
