using System;

namespace internal_Area {
	/// <summary>
	/// AreaType: Information that details which entities belong within an Area.
	/// Written by Justin Ortiz
	/// </summary>
	[Serializable]
	public class AreaType {
		public string name;
		public int structureAssetCount;
		public int structureMaxSpawnCount;
		public int sceneryAssetCount;
		public int sceneryMaxSpawnCount;
		public int characterAssetCount;
		public int characterMaxSpawnCount;
		public int spreadChance; //revisit & rename

		public AreaType() {
			name = "Empty";
			structureAssetCount = 0;
			structureMaxSpawnCount = 0;

			sceneryAssetCount = 0;
			sceneryMaxSpawnCount = 0;

			characterAssetCount = 0;
			characterMaxSpawnCount = 0;

			spreadChance = 0;
		}

		public AreaType(string Name, int StructureAssetCount, int StructureMaxSpawnCount, int SceneryAssetCount,
			int SceneryMaxSpawnCount, int CharacterAssetCount, int CharacterMaxSpawnCount, int SpreadChance) {
			name = Name;
			structureAssetCount = StructureAssetCount;
			structureMaxSpawnCount = StructureMaxSpawnCount;

			sceneryAssetCount = SceneryAssetCount;
			sceneryMaxSpawnCount = SceneryMaxSpawnCount;

			characterAssetCount = CharacterAssetCount;
			characterMaxSpawnCount = CharacterMaxSpawnCount;

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
