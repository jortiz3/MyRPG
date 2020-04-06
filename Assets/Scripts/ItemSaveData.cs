using System;

[Serializable]
public class ItemSaveData {
	public int id;
	public string name;
	public string prefix;
	public string suffix;
	public int quantity;
	public bool equipped;

	public ItemSaveData() {
		id = 0;
		name = "Null";
		prefix = "";
		suffix = "";
		quantity = 0;
		equipped = false;
	}

	public ItemSaveData(Item i) {
		id = i.ID;
		name = i.BaseName;
		prefix = i.Prefix;
		suffix = i.Suffix;
		quantity = i.Quantity;
		equipped = i.Equipped;
	}
}
