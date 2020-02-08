using System.Xml.Serialization;

namespace AreaManagerNS.AreaNS {
	/// <summary>
	/// AreaType: Information that details which entities belong within an Area.
	/// Written by Justin Ortiz
	/// </summary>
	[XmlType("AreaType")]
	public class AreaType {
		[XmlAttribute("Name")]
		public string name;

		[XmlAttribute("StructureAssetCount")]
		public int structureAssetCount;
		[XmlAttribute("StructureMaxSpawnCount")]
		public int structureMaxSpawnCount;

		[XmlAttribute("SceneryAssetCount")]
		public int sceneryAssetCount;
		[XmlAttribute("SceneryMaxSpawnCount")]
		public int sceneryMaxSpawnCount;

		[XmlAttribute("CharacterAssetCount")]
		public int characterAssetCount;
		[XmlAttribute("CharacterMaxSpawnCount")]
		public int characterMaxSpawnCount;

		[XmlAttribute("SpreadChance")]
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
