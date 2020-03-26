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
	private string prefix; //convert to class? ItemModifier.cs >> effects stats and/or color
	[SerializeField, HideInInspector]
	private string suffix;
	private int stat_magic; //either magic damage or resistance
	private int stat_physical; //either physical damage or resistance
	private int currency_value;
	private float weight;
	private bool equipable;
	private bool equipped;
	private bool slottable;
	private bool consumable;
	private bool weapon;
	private bool armor;
	private string[] tags; //used for sorting the item in the inventory screen

	private SpriteRenderer sprite;

	public int ID { get { return id; } }
	public int Value { get { return currency_value; } }
	public int Stat_Magic { get { return stat_magic; } }
	public int Stat_Physical { get { return stat_physical; } }
	public float Weight { get { return weight; } }
	public bool Equipable { get { return equipable; } }
	public bool Equipped { get { return equipped; } set { equipped = value; } }
	public bool Slottable { get { return slottable; } }
	public bool Consumable { get { return consumable; } }
	public bool isWeapon { get { return weapon; } }
	public bool isArmor { get { return armor; } }

	public override void Disable() {
		SetSpriteActive(false); //hide the sprite
		base.Disable();
	}

	public override void Enable() {
		SetSpriteActive(true); //show the sprite
		base.Enable();
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
		this.Disable(); //ensure the player doesn't interact w/ the object more than once
		//add this item to player's inventory >> ItemContainer.cs?
		base.InteractInternal();
	}

	/// <summary>
	/// Load attribute info from database. (only item name, prefix, suffix, & ID are saved)
	/// </summary>
	public void LoadItemInfoFromDB() {
		ItemInfo temp = new ItemInfo(); //get from ItemDatabase.cs instead of new
		stat_magic = temp.stat_magic;
		stat_physical = temp.stat_physical;
		currency_value = temp.currency_value;
		weight = temp.weight;
		equipable = temp.equipable;
		equipped = false;
		slottable = temp.slottable;
		consumable = temp.consumable;
		weapon = temp.weapon;
		armor = temp.armor;
		tags = temp.tags;
	}

	public void SetSpriteActive(bool active) {
		if (sprite != null) {
			if (active) {
				sprite.enabled = true;
			} else {
				sprite.enabled = false;
			}
		}
	}

	private void Start() {
		SetInteractMessage("to pick up " + this.ToString() + ".");
	}

	public override string ToString() {
		return prefix + name + suffix;
	}

	public virtual void Use() {
		if (equipable) {
			//Player.instance.Equip(this);
		} else if (consumable) {
			//Player.instance.Consume(this);
		}
	}
}
