using UnityEngine;

/// <summary>
/// Notes:
/// -store list of references to NPCs currently pursuing player to determing whether in combat?
/// Written by Justin Ortiz
/// </summary>
public class Player : Character {
	public static Player instance;

	private Item armor;
	private Item weapon;
	private Item ring;
	private Item necklace;

	/// <summary>
	/// Attempts to equip the given item to the player.
	/// </summary>
	/// <returns>Previously equipped item (can be null).</returns>
	public Item Equip(Item item) {
		string[] itemTags = item.GetTags(); //get the item's tags
		Item prevEquipped = null; //create reference to previously equipped -- initialized to null
		for (int i = 0; i < itemTags.Length; i++) { //go through all tags
			if (itemTags[i].Equals("armor")) { //see if the tag matches equipment type
				prevEquipped = armor; //store reference to unequipped item
				armor = item; //set the new reference
				//to do: update player animations to reflect the new armor
				armor.Equipped = true; //flag as equipped
				break; //stop processing tags
			} else if (itemTags[i].Equals("weapon")) {
				prevEquipped = weapon;
				weapon = item;
				weapon.Equipped = true;
				break;
			} else if (itemTags[i].Equals("ring")) {
				prevEquipped = ring;
				ring = item;
				ring.Equipped = true;
				break;
			} else if (itemTags[i].Equals("necklace")) {
				prevEquipped = necklace;
				necklace = item;
				necklace.Equipped = true;
				break;
			}
		}

		if (prevEquipped != null) { //if something was unequipped
			prevEquipped.Equipped = false; //flag as unequipped
		}

		return prevEquipped; //return reference to unequipped item -- to be used for updating ui
	}

	public override int GetStat_MagicResistance() {
		int mr = base_magic_resistance; //start with base MR
		if (armor != null) { //add bonuses from equipped items
			mr += armor.GetMagicStat();
		}
		if (ring != null) {
			mr += ring.GetMagicStat();
		}
		if (necklace != null) {
			mr += necklace.GetMagicStat();
		}
		return mr;
	}

	public override int GetStat_PhysicalResistance() {
		int pr = base_physical_resistance; //start with base PR
		if (armor != null) { //add bonuses from equipped items
			pr += armor.GetPhysicalStat();
		}
		if (ring != null) {
			pr += ring.GetPhysicalStat();
		}
		if (necklace != null) {
			pr += necklace.GetPhysicalStat();
		}
		return pr;
	}

	protected override void Initialize() {
		if (instance != null) {
			Destroy(gameObject);
		} else {
			instance = this;
		}

		walkSpeed = 1.5f;
		sprintSpeed = 2.5f;
		base.Initialize();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="physicalDamage"></param>
	/// <param name="magicalDamage"></param>
	/// <param name="trueDamage">If true, all damage ignores resistances</param>
	protected override void ReceiveHit(int physicalDamage, int magicalDamage, bool trueDamage = false) {
		if (GameManager.instance.Difficulty >= 2) { //masochist
			physicalDamage = maxHp; //ensure the hit kills player
			trueDamage = true; //force true damage
		} else if (GameManager.instance.Difficulty >= 1) { //hard
			physicalDamage *= 2;
			magicalDamage *= 2;
		}
		base.ReceiveHit(physicalDamage, magicalDamage, trueDamage);
	}

	public override void TeleportToPos(Vector3 position) {
		StartCoroutine(Teleport(position, true));
	}

	public void Unequip(Item item) {
		if (item.Equals(armor)) { //if the memory reference is the same
			//to do: clear animation
			armor = null; //remove reference
		} else if (item.Equals(weapon)) {
			weapon = null;
		} else if (item.Equals(ring)) {
			ring = null;
		} else if (item.Equals(necklace)) {
			necklace = null;
		}
		item.Equipped = false;
	}
}
