using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StructureSaveData {
	public string owner;
	public string[] textures;
	public Vector2IntS dimensions;

	public StructureSaveData() {
		owner = "";
		textures = null;
		dimensions = new Vector2IntS(Vector2Int.zero);
	}

	public StructureSaveData(Structure s) {
		owner = s.Owner;
		textures = s.GetTextures();
		dimensions = new Vector2IntS(s.Dimensions);
	}
}
