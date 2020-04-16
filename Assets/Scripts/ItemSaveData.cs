using System;

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
}
