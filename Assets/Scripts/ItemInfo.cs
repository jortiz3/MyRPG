namespace Items {
	/// <summary>
	/// Written by Justin Ortiz
	/// </summary>
	public class ItemInfo {
		public int id;
		public string name;
		public string prefix;
		public string suffix;
		public int stat_magic;
		public int stat_physical;
		public int currency_value;
		public float weight;
		public bool equipable;
		public bool slottable;
		public bool consumable;
		public bool weapon;
		public bool armor;
		public string[] tags;

		public ItemInfo() {
			id = -1;
			name = "Null";
			prefix = "";
			suffix = "";
			stat_magic = 0;
			stat_physical = 0;
			currency_value = 0;
			weight = 0;
			equipable = false;
			slottable = false;
			consumable = false;
			weapon = false;
			armor = false;
			tags = null;
		}

		public ItemInfo(int ID, string Prefix, string Name, string Suffix, int Stat_Magic, int Stat_Physical, int Currency_Value,
			float Weight, bool Equipable, bool Slottable, bool Consumable, bool isWeapon, bool isArmor, string[] Tags) {
			id = ID;
			name = Name;
			prefix = Prefix;
			suffix = Suffix;
			stat_magic = Stat_Magic;
			stat_physical = Stat_Physical;
			currency_value = Currency_Value;
			weight = Weight;
			equipable = Equipable;
			slottable = Slottable;
			consumable = Consumable;
			weapon = isWeapon;
			armor = isArmor;
			tags = Tags;
		}
	}
}
