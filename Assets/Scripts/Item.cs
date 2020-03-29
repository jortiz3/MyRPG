using System;
using UnityEngine;
using Items;

/// <summary>
/// Written by Justin Ortiz.
/// </summary>
[Serializable]
public class Item : Interactable {
	[SerializeField, HideInInspector]
	private int id;
	private string baseName;
	[SerializeField, HideInInspector]
	private ItemModifier prefix;
	[SerializeField, HideInInspector]
	private ItemModifier suffix;
	private int stat_magic; //either magic damage or resistance
	private int stat_physical; //either physical damage or resistance
	private int currency_value;
	private int quality_value;
	private int quantity;
	private float weight;
	private bool equipable;
	private bool equipped;
	private bool slottable;
	private bool consumable;
	private bool crafting_material;
	private bool weapon;
	private bool armor;
	private string[] tags; //used for sorting the item in the inventory screen

	private SpriteRenderer sprite;

	public int ID { get { return id; } }
	public string BaseName { get { return baseName; } }
	public string Prefix { get { return prefix.Name; } }
	public string Suffix { get { return suffix.Name; } }
	public int BaseMagicStat { get { return stat_magic; } }
	public int BasePhysicalStat { get { return stat_physical; } }
	public int BaseValue { get { return currency_value; } }
	public int Quality { get { return quality_value; } }
	public int Quantity { get { return quantity; } set { quantity = value; } }
	public float BaseWeight { get { return weight; } }
	public bool Equipable { get { return equipable; } }
	public bool Equipped { get { return equipped; } set { equipped = value; } }
	public bool Slottable { get { return slottable; } }
	public bool Consumable { get { return consumable; } }
	public bool isCraftingMaterial { get { return crafting_material; } }
	public bool isWeapon { get { return weapon; } }
	public bool isArmor { get { return armor; } }

	public override void DisableInteraction() {
		SetSpriteActive(false); //hide the sprite
		base.DisableInteraction();
	}

	public override void EnableInteraction() {
		SetSpriteActive(true); //show the sprite
		base.EnableInteraction();
	}

	public override bool Equals(object other) {
		if (other.GetType() == typeof(Item)) {
			Item temp = (Item)other;
			if (baseName.Equals(temp.baseName)) {
				if (id == temp.id) {
					return true;
				}
			}
		}
		return base.Equals(other); //returns whether it's the same object in memory
	}

	public string GetItemType() {
		string temp = "";
		if (weapon) {
			temp = "W";
		} else if (armor) {
			temp = "A";
		} else if (consumable) {
			temp = "C";
		} else if (crafting_material) {
			temp = "M";
		}
		return temp;
	}

	public Color GetQualityColor() {
		int currQuality = quality_value + prefix.Quality_Modifier + suffix.Quality_Modifier;
		if (currQuality < 0) {
			return new Color(139, 69, 19); //brown >> doo doo
		} else if(currQuality <= 0) {
			return Color.white;
		} else if (currQuality <= 1) {
			return Color.yellow;
		} else if (currQuality <= 2) {
			return new Color(255, 192, 203); //pink
		} else if (currQuality <= 3) {
			return Color.green;
		} else if (currQuality <= 4) {
			return Color.blue;
		} else if (currQuality <= 5) {
			return new Color(142, 68, 173); //purple
		} else {
			return new Color(235, 149, 50); //orange
		}
	}

	public int GetMagicStat() {
		return stat_magic + prefix.Magic_Modifier + suffix.Physical_Modifier;
	}

	public int GetPhysicalStat() {
		return stat_physical + prefix.Physical_Modifier + suffix.Physical_Modifier;
	}

	public string[] GetTags() {
		string[] tempTags = new string[tags.Length];
		for (int i = 0; i < tempTags.Length; i++) {
			tempTags[i] = tags[i];
		}
		return tempTags;
	}

	public int GetValue() {
		return currency_value + prefix.Value_Modifier + suffix.Value_Modifier;
	}

	public float GetWeight() {
		float currWeight = weight * quantity;
		currWeight = (float)Math.Truncate(currWeight * 100f) / 100f;
		return currWeight;
	}

	public bool HasTag(string tag) {
		if (tags != null) {
			if (tags.Length > 0) {
				for (int i = 0; i < tags.Length; i++) {
					if (tags[i].Equals(tag)) {
						return true;
					}
				}
			}
		}
		return false;
	}

	protected override void Initialize() {
		SetInteractMessage("to pick up " + ToString() + ".");
		base.Initialize();
	}

	protected override void InteractInternal() {
		DisableInteraction(); //ensure the player doesn't interact w/ the object more than once
		Inventory.instance.Add(this); //add this item to player's inventory >>
		base.InteractInternal();
	}

	/// <summary>
	/// Load attribute info from database. (only item name, prefix, suffix, & ID are saved)
	/// </summary>
	public void LoadItemInfo() {
		ItemInfo tempInfo = ItemDatabase.GetItemInfo(id);
		baseName = tempInfo.name;
		stat_magic = tempInfo.stat_magic;
		stat_physical = tempInfo.stat_physical;
		currency_value = tempInfo.currency_value;
		quality_value = tempInfo.quality_value;
		weight = tempInfo.weight;
		equipable = tempInfo.equipable;
		equipped = false;
		slottable = tempInfo.slottable;
		consumable = tempInfo.consumable;
		crafting_material = tempInfo.crafting_material;
		weapon = tempInfo.weapon;
		armor = tempInfo.armor;
		tags = tempInfo.tags;

		prefix = ItemModifierDatabase.GetPrefix(tempInfo.prefix);
		suffix = ItemModifierDatabase.GetSuffix(tempInfo.suffix);
		gameObject.name = ToString();
	}

	/// <summary>
	/// Assigns a prefix item modifier. (To be called during crafting)
	/// </summary>
	public void SetPrefix(string prefixName) {
		prefix = ItemModifierDatabase.GetPrefix(prefixName);
	}

	public void SetSpriteActive(bool active) {
		if (sprite != null) {
			sprite.enabled = active;
		}
	}

	public void SetSuffix(string suffixName) {
		suffix = ItemModifierDatabase.GetSuffix(suffixName);
	}

	public override string ToString() {
		return prefix.Name + " " + baseName + " " + suffix.Name;
	}

	public virtual void Use() {
		if (equipable) {
			//Player.instance.Equip(this);
		} else if (consumable) {
			//Player.instance.Consume(this);
		}
	}
}
