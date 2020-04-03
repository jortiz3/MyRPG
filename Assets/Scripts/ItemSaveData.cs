using System;

[Serializable]
public class ItemSaveData
{
    public int id;
    public string name;
    public string prefix;
    public string suffix;
    public int quantity;

    public ItemSaveData() {
        id = 0;
        name = "Null";
        prefix = "";
        suffix = "";
        quantity = 0;
    }

    public ItemSaveData(Item i) {
        id = i.ID;
        name = i.BaseName;
        prefix = i.Prefix;
        suffix = i.Suffix;
        quantity = i.Quantity;
    }
}
