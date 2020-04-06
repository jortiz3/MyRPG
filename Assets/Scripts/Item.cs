using System;
using UnityEngine;
using Items;

/// <summary>
/// Written by Justin Ortiz.
/// </summary>
public class Item : Interactable {
	private static GameObject itemPrefab;

	[SerializeField]
	private int id;
	[SerializeField]
	private string baseName;
	private ItemModifier prefix;
	private ItemModifier suffix;
	private int stat_magic; //either magic damage or resistance
	private int stat_physical; //either physical damage or resistance
	private int currency_value;
	private int quality_value;
	private int quantity;
	private float weight;
	private bool equippable;
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
	public string Prefix { get { return prefix != null ? prefix.Name : ""; } }
	public string Suffix { get { return suffix != null ? suffix.Name : ""; } }
	public int BaseMagicStat { get { return stat_magic; } }
	public int BasePhysicalStat { get { return stat_physical; } }
	public int BaseValue { get { return currency_value; } }
	public int Quality { get { return quality_value; } }
	public int Quantity { get { return quantity; } set { quantity = value; } }
	public float BaseWeight { get { return weight; } }
	public bool Equipable { get { return equippable; } }
	public bool Equipped { get { return equipped; } set { equipped = value; } }
	public bool Slottable { get { return slottable; } }
	public bool Consumable { get { return consumable; } }
	public bool isCraftingMaterial { get { return crafting_material; } }
	public bool isWeapon { get { return weapon; } }
	public bool isArmor { get { return armor; } }

	private void Awake() {
		if (itemPrefab == null) {
			itemPrefab = Resources.Load<GameObject>("item");
		}
	}

	public static Item Create(ItemSaveData info) {
		Item temp = GameObject.Instantiate(itemPrefab).GetComponent<Item>();
		temp.Load(info);
		return temp;
	}

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

	public override int GetHashCode() {
		return base.GetHashCode();
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
		int currQuality = quality_value;

		if (prefix != null) {
			currQuality += prefix.Quality_Modifier;
		}

		if (suffix != null) {
			currQuality += suffix.Quality_Modifier;
		}

		if (currQuality < 0) {
			return new Color(139f / 255f, 69f / 255f, 19f / 255f); //brown >> doo doo
		} else if (currQuality <= 0) {
			return Color.white;
		} else if (currQuality <= 1) {
			return new Color(255f / 255f, 192f / 255f, 203f / 255f); //pink
		} else if (currQuality <= 2) {
			return Color.yellow;
		} else if (currQuality <= 3) {
			return Color.green;
		} else if (currQuality <= 4) {
			return Color.blue;
		} else if (currQuality <= 5) {
			return new Color(150f / 255f, 70f / 255f, 170f / 255f); //purple
		} else {
			return new Color(235f / 255f, 149f / 255f, 50f / 255f); //orange
		}
	}

	public int GetMagicStat() {
		int currStat = stat_magic;

		if (prefix != null) {
			currStat += prefix.Magic_Modifier;
		}

		if (suffix != null) {
			currStat += suffix.Magic_Modifier;
		}

		return currStat;
	}

	public int GetPhysicalStat() {
		int currStat = stat_physical;

		if (prefix != null) {
			currStat += prefix.Physical_Modifier;
		}

		if (suffix != null) {
			currStat += suffix.Physical_Modifier;
		}

		return currStat;
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
		transform.parent = AreaManager.GetEntityParent("Item");
		gameObject.tag = "item";
		Load(null);
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
	public void Load(ItemSaveData providedInfo) {
		ItemInfo retrievedInfo;

		if (providedInfo != null) { //if given info
			id = providedInfo.id; //use id
			baseName = providedInfo.name; //use base name
		}

		retrievedInfo = ItemDatabase.GetItemInfo(id); //check database for info using id

		if (retrievedInfo == null) { //if not found
			retrievedInfo = ItemDatabase.GetItemInfo(baseName); //check database for info using base name
		}

		if (retrievedInfo == null) { // if still not found
			Destroy(gameObject); //remove from scene
			return; //quit
		}

		id = retrievedInfo.id; //ensure id matches database
		baseName = retrievedInfo.name; //ensure base name matches database
		stat_magic = retrievedInfo.stat_magic;
		stat_physical = retrievedInfo.stat_physical;
		currency_value = retrievedInfo.currency_value;
		quality_value = retrievedInfo.quality_value;
		weight = retrievedInfo.weight;
		equippable = retrievedInfo.equipable;
		slottable = retrievedInfo.slottable;
		consumable = retrievedInfo.consumable;
		crafting_material = retrievedInfo.crafting_material;
		weapon = retrievedInfo.weapon;
		armor = retrievedInfo.armor;
		tags = retrievedInfo.tags;

		if (providedInfo != null) {
			quantity = providedInfo.quantity;
			equipped = providedInfo.equipped; //convert to Player.instance.Equip(this);
			prefix = ItemModifierDatabase.GetPrefix(providedInfo.prefix);
			suffix = ItemModifierDatabase.GetSuffix(providedInfo.suffix);
		} else {
			quantity = 1;
			equipped = false;
			prefix = ItemModifierDatabase.GetPrefix(retrievedInfo.prefix);
			suffix = ItemModifierDatabase.GetSuffix(retrievedInfo.suffix);
		}

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

	public ItemInfo ToItemInfo() {
		ItemInfo temp = new ItemInfo(this);
		return temp;
	}

	public ItemSaveData ToItemSaveData() {
		ItemSaveData temp = new ItemSaveData(this);
		return temp;
	}

	public override string ToString() { //returns the items full name: prefix + baseName + suffix
		string temp = "";
		if (prefix != null) { //if there is a prefix modifier
			temp += prefix.Name + " ";
		}

		temp += baseName; //add the base name

		if (suffix != null) { //if there is a suffix modifier
			temp += " " + suffix.Name;
		}
		return temp; //return the result
	}

	public virtual void Use() {
		if (equippable) {
			//Player.instance.Equip(this);
		} else if (consumable) {
			//Player.instance.Consume(this);
		}
	}
}
