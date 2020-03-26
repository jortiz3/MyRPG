using System.Collections.Generic;

namespace Items {
	/// <summary>
	/// Written by Justin Ortiz
	/// </summary>
	public class ItemDatabase {
		private static bool initialized;
		private static List<ItemInfo> db;

		public static bool Initialized { get { return initialized; } }

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
			return new ItemInfo();
		}

		/// <summary>
		/// Gets an item from the database.
		/// </summary>
		/// <param name="name">The base name for the item excluding prefix & suffix.</param>
		/// <returns>Returns base ItemInfo constructor if not found.</returns>
		public static ItemInfo GetItemInfo(string name) {
			for (int i = 0; i < db.Count; i++) {
				if (db[i].name.Equals(name)) {
					return db[i];
				}
			}
			return new ItemInfo();
		}

		/// <summary>
		/// Initializes the item database with the default items.
		/// </summary>
		public static void Initialize() {
			db = new List<ItemInfo>();
			//db.Add(new ItemInfo());
			initialized = true;
		}
	}
}
