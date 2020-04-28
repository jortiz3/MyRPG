using System;
using UnityEngine;
using internal_Items;

/// <summary>
/// Written by Justin Ortiz.
/// </summary>
public class Item : Interactable {
	private static GameObject itemPrefab;

	private int id;
	private int containerID;
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

	public int ID { get { return id; } }
	public int ContainerID { get { return containerID; } set { containerID = value; } }
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
		if (other.GetType() == typeof(Item)) { //if the other object is an item
			Item temp = (Item)other; //store/cast to item to access attributes
			if (ToString().Equals(temp.ToString())) { //both items have same prefix, base name, & suffix
				if (id == temp.id) { //both items have the same item ID
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

		if (containerID == Inventory.instance.InstanceID) {
			Inventory.instance.Add(this);
		} else if (0 < containerID) {
			Container c = Container.GetContainer(containerID);
			if (c != null) {
				c.Add(this);
			}
		}

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
			baseName = providedInfo.baseName; //use base name
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
		baseName = retrievedInfo.baseName; //ensure base name matches database
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
			quantity = quantity <= 0 ? 1 : quantity;
			equipped = false;
			if (prefix == null)
				prefix = ItemModifierDatabase.GetPrefix(retrievedInfo.prefix);
			if (suffix == null)
				suffix = ItemModifierDatabase.GetSuffix(retrievedInfo.suffix);
		}

		gameObject.name = ToString();
	}

	public void Load(int ID = -1, int ContainerID = -1, string ItemBaseName = "", int Quantity = 1, Texture2D Texture = null) {
		id = ID;
		containerID = ContainerID;
		baseName = ItemBaseName;
		quantity = Quantity;
		Load(null);
		SetSprite(Texture);
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
