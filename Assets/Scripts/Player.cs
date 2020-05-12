using System.Collections;
using UnityEngine;

public class Player : Character {
	public static Player instance;

	private Item armor;
	private Item weapon;
	private Item ring;
	private Item necklace;

	/// <summary>
	/// Attempts to equip the given item to the player.
	/// </summary>
	/// <returns>Previously equipped item or null if not equipped.</returns>
	public Item Equip(Item item) {
		string[] itemTags = item.GetTags();
		Item prevEquipped;
		for (int i = 0; i < itemTags.Length; i++) {
			if (itemTags[i].Equals("armor")) {
				prevEquipped = armor;
				armor = item;
				return prevEquipped;
			} else if (itemTags[i].Equals("weapon")) {
				prevEquipped = weapon;
				weapon = item;
				return prevEquipped;
			} else if (itemTags[i].Equals("ring")) {
				prevEquipped = ring;
				ring = item;
				return prevEquipped;
			} else if (itemTags[i].Equals("necklace")) {
				prevEquipped = necklace;
				necklace = item;
				return prevEquipped;
			}
		}
		return null;
	}

	public override int GetMagicResistance() {
		int mr = base_resistance_magic; //start with base MR
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

	public override int GetPhysicalResistance() {
		int pr = base_resistance_physical; //start with base PR
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

		base.Initialize();
	}

	public override void TeleportToPos(Vector3 position) {
		StartCoroutine(Teleport(position, true));
	}
}
