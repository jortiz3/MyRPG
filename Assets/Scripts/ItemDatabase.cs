using System;
using System.Collections.Generic;

namespace internal_Items {
	/// <summary>
	/// Written by Justin Ortiz
	/// </summary>
	public class ItemDatabase {
		private static bool initialized;
		private static List<ItemInfo> db;

		public static bool Initialized { get { return initialized; } }

		/// <summary>
		/// Uses the provided tags to return an array of items.
		/// </summary>
		/// <param name="Tags">Each item must have at least one of these.</param>
		/// <returns>Array of type ItemInfo.</returns>
		public static ItemInfo[] GetDropTable(string[] Tags) {
			List<ItemInfo> items = new List<ItemInfo>(); //the items to be returned

			for (int dbIndex = 0; dbIndex < db.Count; dbIndex++) { //for all items in db
				for (int paramTagsIndex = 0; paramTagsIndex < Tags.Length; paramTagsIndex++) { //for all tags included in parameter
					if (Array.Exists(db[dbIndex].tags, element => element.Equals(Tags[paramTagsIndex]))) { //if one of the tags is met
						items.Add(db[dbIndex]); //add to the list
						break; //exit param tags loop
					}
				}
			} //end for db index
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
			db.Add(new ItemInfo(db.Count, "", "Log", "", 0, 0, 15, -1, 50.0f, false, false, false, new string[] { "material", "chest" }, Texture: "log"));
			initialized = true;
		}
	}
}
