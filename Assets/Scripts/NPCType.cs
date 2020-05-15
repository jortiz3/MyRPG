using Newtonsoft.Json;

/// <summary>
/// Stores the base attributes for various types of NPCs. Written by Justin Ortiz
/// </summary>
namespace NPC {
	[JsonObject(MemberSerialization.OptIn)]
	public class NPCType {
		[JsonProperty]
		private string name;
		[JsonProperty]
		private string bgStory;
		[JsonProperty]
		private int hp;
		[JsonProperty]
		private int stamina;
		[JsonProperty]
		private float walk;
		[JsonProperty]
		private float sprint;
		[JsonProperty]
		private int pa;
		[JsonProperty]
		private int pr;
		[JsonProperty]
		private int ma;
		[JsonProperty]
		private int mr;

		public string Name { get { return name; } }
		public string BackgroundStory { get { return bgStory; } }
		public int base_hp { get { return hp; } }
		public int base_stamina { get { return stamina; } }
		public float base_walk_speed { get { return walk; } }
		public float base_sprint_speed { get { return sprint; } }
		public int base_physical_attack { get { return pa; } }
		public int base_physical_resistance { get { return pr; } }
		public int base_magic_attack { get { return ma; } }
		public int base_magic_resistance { get { return mr; } }

		public NPCType(string Name, int baseHP, int baseStamina, float baseWalkSpeed, float baseSprintSpeed,
			int basePhysicalAttack, int basePhysicalResistance, int baseMagicAttack, int baseMagicResistance,
			string backgroundStory = "") {
			name = Name;
			bgStory = backgroundStory;
			hp = baseHP;
			stamina = baseStamina;
			walk = baseWalkSpeed;
			sprint = baseSprintSpeed;
			pa = basePhysicalAttack;
			pr = basePhysicalResistance;
			ma = baseMagicAttack;
			mr = baseMagicResistance;
		}

		public void AddBackgroundStory(string story) {
			bgStory = story;
		}
	}
}
