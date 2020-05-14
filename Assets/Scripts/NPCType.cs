/// <summary>
/// Stores the base attributes for various types of NPCs. Written by Justin Ortiz
/// </summary>
namespace NPC {
	public class NPCType {
		private string name;
		private string bgStory;
		private int hp;
		private int stamina;
		private int walk;
		private int sprint;
		private int pa;
		private int pr;
		private int ma;
		private int mr;

		public string Name { get { return name; } }
		public string BackgroundStory { get { return bgStory; } }
		public int base_hp { get { return hp; } }
		public int base_stamina { get { return stamina; } }
		public int base_walk_speed { get { return walk; } }
		public int base_sprint_speed { get { return sprint; } }
		public int base_physical_attack { get { return pa; } }
		public int base_physical_resistance { get { return pr; } }
		public int base_magic_attack { get { return ma; } }
		public int base_magic_resistance { get { return mr; } }

		public NPCType(string Name, int baseHP, int baseStamina, int baseWalkSpeed, int baseSprintSpeed,
			int basePhysicalAttack, int basePhysicalResistance, int baseMagicAttack, int baseMagicResistance) {
			name = Name;
			bgStory = "And now I am a " + Name + ".";
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
