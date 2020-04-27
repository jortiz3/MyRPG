using System;
using UnityEngine;

namespace internal_Items {
	[Serializable]
	public class ItemModifier {
		[SerializeField]
		private string name;
		[SerializeField, HideInInspector]
		private int id;
		private int magic;
		private int physical;
		private int value;
		private int quality;
		private float weight;
		private string effect_status;
		private string effect_impact;
		private string[] crafting_materials_detail; //possible detail material to use to craft this
		private Color color;

		public string Name { get { return name; } }
		public int Magic_Modifier { get { return magic; } }
		public int Physical_Modifier { get { return physical; } }
		public int Value_Modifier { get { return value; } }
		public int Quality_Modifier { get { return quality; } }
		public float Weight_Modifier { get { return weight; } }
		public Color Color_Modifier { get { return color; } }

		//add constructors
	}
}
