using UnityEngine;

namespace Items {
	public class ItemModifier {
		private string name;
		private int magic;
		private int physical;
		private int value;
		private int quality;
		private float weight;
		private string magic_effect;
		private Color color;

		public string Name { get { return name; } }
		public int Magic_Modifier { get { return magic; } }
		public int Physical_Modifier { get { return physical; } }
		public int Value_Modifier { get { return value; } }
		public int Quality_Modifier { get { return quality; } }
		public float Weight_Modifier { get { return weight; } }
		public Color Color_Modifier { get { return color; } }
	}
}
