using System.Collections.Generic;

namespace internal_Items {
	/// <summary>
	/// Written by Justin Ortiz
	/// </summary>
	public class ItemDatabase {
		private static bool initialized;
		private static List<ItemInfo> db;

		public static bool Initialized { get { return initialized; } }

		public static ItemInfo[] GetDropTable(int quality = 0, int minQuality = -int.MaxValue) {
			List<ItemInfo> items = new List<ItemInfo>();

			if (minQuality == -int.MaxValue) {
				minQuality = quality - 2;
			}

			for (int i = 0; i < db.Count; i++) {
				if (minQuality <= db[i].quality_value && db[i].quality_value <= quality) { //add variation to qualities; desired quality and
					items.Add(db[i]);
				}
			}
			return items.ToArray();
		}

		public static int GetItemID(string name) {
			if (name != null && !name.Equals("")) {
				for (int i = 0; i < db.Count; i++) {
					if (db[i].baseName.Equals(name)) {
						return i;
					}
				}
			}
			return -1;
		}
		
		/// <summary>
		/// Gets an item from the database.
		/// </summary>
		/// <param name="name">The base name for the item excluding prefix & suffix.</param>
		/// <returns>Returns base ItemInfo constructor if not found.</returns>
		public static ItemInfo GetItemInfo(int id) {
			if (0 <= id && id < db.Count) {
				for (int i = 0; i < db.Count; i++) {
					if (db[i].id == id) {
						return db[i];
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Gets an item from the database.
		/// </summary>
		/// <param name="name">The base name for the item excluding prefix & suffix.</param>
		/// <returns>Returns base ItemInfo constructor if not found.</returns>
		public static ItemInfo GetItemInfo(string name) {
			int id = GetItemID(name);
			if (0 <= id && id < db.Count) {
				return db[id];
			}
			return null;
		}

		/// <summary>
		/// Initializes the item database with the default items.
		/// </summary>
		public static void Initialize() {
			db = new List<ItemInfo>();
			db.Add(new ItemInfo(db.Count, "", "Log", "", 0, 0, 15, -1, 50.0f, false, false, false, true, false, false, new string[] { "material" }));
			initialized = true;
		}
	}
}
