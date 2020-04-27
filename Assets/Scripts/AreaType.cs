using System;

namespace internal_Area {
	/// <summary>
	/// AreaType: Information that details which entities belong within an Area.
	/// Written by Justin Ortiz
	/// </summary>
	[Serializable]
	public class AreaType {
		public string name;
		public Vector2IntS structureSpawnRange;
		public Vector2IntS scenerySpawnRange;
		public Vector2IntS characterSpawnRange;
		public int spreadChance; //revisit & rename

		public AreaType() {
			name = "Empty";
			spreadChance = 0;
		}

		public AreaType(string Name, Vector2IntS StructureSpawnRange, Vector2IntS ScenerySpawnRange,
			Vector2IntS CharacterSpawnRange, int SpreadChance) {
			name = Name;
			structureSpawnRange = StructureSpawnRange;
			scenerySpawnRange = ScenerySpawnRange;
			characterSpawnRange = CharacterSpawnRange;
			spreadChance = SpreadChance;
		}

		public override bool Equals(object obj) {
			if (obj.GetType().Equals(typeof(AreaType))) {
				AreaType cast = (AreaType)obj;
				if (cast.name.Equals(name)) {
					return true;
				}
			}
			return false;
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}

		public override string ToString() {
			return name;
		}
	}
}
