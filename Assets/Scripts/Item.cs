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
	[SerializeField, HideInInspector]
	private string name;
	[SerializeField, HideInInspector]
	private ItemModifier prefix;
	[SerializeField, HideInInspector]
	private ItemModifier suffix;
	private int stat_magic; //either magic damage or resistance
	private int stat_physical; //either physical damage or resistance
	private int currency_value;
	private int quality_value;
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
	public int Value { get { return currency_value; } }
	public int Quality { get { return quality_value; } }
	public int Stat_Magic { get { return stat_magic; } }
	public int Stat_Physical { get { return stat_physical; } }
	public float Weight { get { return weight; } }
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

	protected override void InteractInternal() {
		this.DisableInteraction(); //ensure the player doesn't interact w/ the object more than once
								   //add this item to player's inventory >> ItemContainer.cs?
		base.InteractInternal();
	}

	/// <summary>
	/// Load attribute info from database. (only item name, prefix, suffix, & ID are saved)
	/// </summary>
	public void LoadItemInfo() {
		ItemInfo tempInfo = ItemDatabase.GetItemInfo(id); //get from ItemDatabase.cs instead of new
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
	}

	public void SetSpriteActive(bool active) {
		if (sprite != null) {
			sprite.enabled = active;
		}
	}

	private void Start() {
		SetInteractMessage("to pick up " + this.ToString() + ".");
	}

	public override string ToString() {
		return prefix.Name + " " + name + " " + suffix.Name;
	}

	public virtual void Use() {
		if (equipable) {
			//Player.instance.Equip(this);
		} else if (consumable) {
			//Player.instance.Consume(this);
		}
	}
}
