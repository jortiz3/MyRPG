using System;
using UnityEngine;
using Items;

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
	public bool Equippable { get { return equippable; } }
	public bool Equipped { get { return equipped; } set { equipped = value; } }
	public bool Slottable { get { return slottable; } }
	public bool Consumable { get { return consumable; } }

	private void Awake() {
		if (itemPrefab == null) {
			itemPrefab = Resources.Load<GameObject>("item");
		}
	}

	public bool Equals(Item other, bool reqDiffContainerID) {
		if (ToString().Equals(other.ToString())) { //both items have same prefix, base name, & suffix
			if (id == other.id) { //both items have the same item ID
				if (!reqDiffContainerID || other.ContainerID != containerID) { //if difference not needed OR they have different containers
					return true;
				}
			}
		}
		return false;
	}

	public string GetItemType() {
		string temp = "";
		/*if (weapon) {
			temp = "W";
		} else if (armor) {
			temp = "A";
		} else*/
		if (consumable) {
			temp = "C";
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
		int totalValue = currency_value;
		if (prefix != null) {
			totalValue += prefix.Value_Modifier;
		}
		if (suffix != null) {
			totalValue += suffix.Value_Modifier;
		}
		return totalValue;
	}

	/// <summary>
	/// Returns to total weight of this item.
	/// </summary>
	/// <returns>weight * quantity</returns>
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
		transform.SetParent(AreaManager.GetEntityParent("Item"), true);
		gameObject.tag = "item";

		if (containerID != 0) { //if a container is supposed to track this item
			Container c = Container.GetContainer(containerID); //try to find container
			if (c != null) { //if container found
				if (c.Add(this)) { //add to container
					SetActive(false, false);
				}
			} else { //if container not found
				containerID = 0; //reset the id
			}
		}

		base.Initialize();
	}

	protected override void InteractInternal() {
		if (Inventory.instance.Add(this)) { //if this item is added to player inventory
			SetActive(false, false); //hide item and disable interaction
		}
		base.InteractInternal();
	}

	/// <summary>
	/// Load attribute info from database. (only item name, prefix, suffix, & ID are saved)
	/// </summary>
	private void LoadDBInfo(int ID = 0, string Prefix = "", string BaseName = "", string Suffix = "", int Quantity = 1) {
		id = ID;
		baseName = BaseName;

		ItemInfo retrievedInfo;
		retrievedInfo = ItemDatabase.GetItemInfo(id); //check database for info using id

		if (retrievedInfo == null) { //if not found
			retrievedInfo = ItemDatabase.GetItemInfo(baseName); //check database for info using base name
		}

		if (retrievedInfo == null) { // if still no info to use
			Destroy(gameObject); //remove from scene
			return; //quit
		}

		id = retrievedInfo.id; //ensure id matches database
		baseName = retrievedInfo.baseName; //ensure base name matches database
		stat_magic = retrievedInfo.stat_magic;
		stat_physical = retrievedInfo.stat_physical;
		currency_value = retrievedInfo.currency_value;
		quality_value = retrievedInfo.quality_value;
		quantity = Quantity;
		weight = retrievedInfo.weight;
		equippable = retrievedInfo.equipable;
		slottable = retrievedInfo.slottable;
		consumable = retrievedInfo.consumable;
		tags = retrievedInfo.tags;

		string prefixToObtain = !Prefix.Equals("") ? Prefix : retrievedInfo.prefix;
		prefix = ItemModifierDatabase.GetPrefix(prefixToObtain);

		string suffixToObtain = !Suffix.Equals("") ? Suffix : retrievedInfo.suffix;
		suffix = ItemModifierDatabase.GetSuffix(suffixToObtain);

		gameObject.name = ToString();
	}

	public void Load(int ID = -1, int ContainerID = -1, string Prefix = "", string BaseName = "", string Suffix = "",
		int Quantity = 1, Texture2D Texture = null, float LastUpdated = 0) {
		containerID = ContainerID;
		lastUpdated = LastUpdated;

		if (containerID < 1 && containerID != Inventory.instance.InstanceID) { //not in container
			if (GameManager.instance.ElapsedGameTime - lastUpdated > 300) { //if item has been laying on ground for 5 mins
				Destroy(gameObject); //remove from scene
			}
		}

		LoadDBInfo(ID: ID, Prefix: Prefix, BaseName: BaseName, Suffix: Suffix, Quantity: Quantity);
		SetSprite(Texture);
	}

	/// <summary>
	/// Assigns a prefix item modifier. (To be called during crafting)
	/// </summary>
	public void SetPrefix(string prefixName) {
		prefix = ItemModifierDatabase.GetPrefix(prefixName);
	}

	public void SetSuffix(string suffixName) {
		suffix = ItemModifierDatabase.GetSuffix(suffixName);
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
		if (consumable) {
			//Player.instance.Consume(this);
		}
	}
}
