using System;
using internal_Items;

[Serializable]
public class ItemSaveData {
	public int id;
	public string baseName;
	public string prefix;
	public string suffix;
	public int quantity;
	public bool equipped;

	public ItemSaveData() {
		id = 0;
		baseName = "Null";
		prefix = "";
		suffix = "";
		quantity = 0;
		equipped = false;
	}

	public ItemSaveData(Item i) {
		id = i.ID;
		baseName = i.BaseName;
		prefix = i.Prefix;
		suffix = i.Suffix;
		quantity = i.Quantity;
		equipped = i.Equipped;
	}

	public ItemSaveData(int ID, int Quantity) {
		id = ID;
		ItemInfo i = ItemDatabase.GetItemInfo(id);
		baseName = i.baseName;
		prefix = i.prefix;
		suffix = i.suffix;
		quantity = Quantity;
		equipped = false;
	}

	public ItemSaveData(int ID, string Prefix, string Suffix, int Quantity, bool Equipped) {
		id = ID;
		ItemInfo i = ItemDatabase.GetItemInfo(id);
		baseName = i.baseName;
		prefix = Prefix;
		suffix = Suffix;
		quantity = Quantity;
		equipped = Equipped;
	}
}
