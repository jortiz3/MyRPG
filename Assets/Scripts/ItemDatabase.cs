using System.Collections.Generic;

namespace internal_Items {
	/// <summary>
	/// Written by Justin Ortiz
	/// </summary>
	public class ItemDatabase {
		private static bool initialized;
		private static List<ItemInfo> db;

		public static bool Initialized { get { return initialized; } }

		public static int GetItemID(string name) {
			for (int i = 0; i < db.Count; i++) {
				if (db[i].baseName.Equals(name)) {
					return i;
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
			for (int i = 0; i < db.Count; i++) {
				if (db[i].id == id) {
					return db[i];
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
			return new ItemInfo();
		}

		/// <summary>
		/// Initializes the item database with the default items.
		/// </summary>
		public static void Initialize() {
			db = new List<ItemInfo>();
			db.Add(new ItemInfo(0, "", "Log", "", 0, 0, 5, -1, 50.0f, false, false, false, true, false, false, new string[] { "material" }));
			initialized = true;
		}
	}
}
