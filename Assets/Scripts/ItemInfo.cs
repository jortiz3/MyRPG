using System;

namespace Items {
	/// <summary>
	/// Written by Justin Ortiz
	/// </summary>
	[Serializable]
	public class ItemInfo {
		public int id;
		public string name;
		public string prefix;
		public string suffix;
		public int stat_magic;
		public int stat_physical;
		public int currency_value;
		public int quality_value;
		public float weight;
		public bool equipable;
		public bool slottable;
		public bool consumable;
		public bool crafting_material;
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
			quality_value = 0;
			weight = 0;
			equipable = false;
			slottable = false;
			consumable = false;
			crafting_material = false;
			weapon = false;
			armor = false;
			tags = null;
		}

		public ItemInfo(ItemInfo copy) {
			id = copy.id;
			name = copy.name;
			prefix = copy.prefix;
			suffix = copy.suffix;
			stat_magic = copy.stat_magic;
			stat_physical = copy.stat_physical;
			currency_value = copy.currency_value;
			quality_value = copy.quality_value;
			weight = copy.weight;
			equipable = copy.equipable;
			slottable = copy.slottable;
			consumable = copy.consumable;
			crafting_material = copy.crafting_material;
			weapon = copy.weapon;
			armor = copy.armor;
			tags = copy.tags;
		}

		public ItemInfo(Item item) {
			id = item.ID;
			name = item.BaseName;
			prefix = item.Prefix;
			suffix = item.Suffix;
			stat_magic = item.BaseMagicStat;
			stat_physical = item.BasePhysicalStat;
			currency_value = item.BaseValue;
			quality_value = item.Quality;
			weight = item.BaseWeight;
			equipable = item.Equipable;
			slottable = item.Slottable;
			consumable = item.Consumable;
			crafting_material = item.isCraftingMaterial;
			weapon = item.isWeapon;
			armor = item.isArmor;
			tags = item.GetTags();
		}

		public ItemInfo(int ID, string Prefix, string Name, string Suffix, int Stat_Magic, int Stat_Physical, int Currency_Value,
			int Quality_Value, float Weight, bool Equipable, bool Slottable, bool Consumable, bool isCraftingMaterial, bool isWeapon,
			bool isArmor, string[] Tags) {
			id = ID;
			name = Name;
			prefix = Prefix;
			suffix = Suffix;
			stat_magic = Stat_Magic;
			stat_physical = Stat_Physical;
			currency_value = Currency_Value;
			quality_value = Quality_Value;
			weight = Weight;
			equipable = Equipable;
			slottable = Slottable;
			consumable = Consumable;
			crafting_material = isCraftingMaterial;
			weapon = isWeapon;
			armor = isArmor;
			tags = Tags;
		}

		public override bool Equals(object obj) {
			return base.Equals(obj);
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}

		public override string ToString() {
			return prefix + " " + name + " " + suffix;
		}
	}
}
