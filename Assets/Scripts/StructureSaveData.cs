using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StructureSaveData {
	public string owner;
	public string[] textures;
	public Vector2IntS dimensions;

	public StructureSaveData() {
		owner = "NPC";
		textures = new string[] { "floor_default", "roof_default", "door_default" };
		dimensions = new Vector2IntS(Vector2Int.one);
	}

	public StructureSaveData(Structure s) {
		owner = s.Owner;
		textures = s.GetTextures();
		dimensions = new Vector2IntS(s.Dimensions);
	}

	public StructureSaveData(string Owner, string[] Textures, Vector2IntS Dimensions) {
		owner = Owner;
		textures = Textures;
		dimensions = Dimensions;
	}
}
