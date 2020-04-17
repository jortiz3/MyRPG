using System;

[Serializable]
public class SceneryObjectSaveData {
	public string name;
	public int harvestCount;
	public int harvestedItemID;
	public bool allowStructureCollision;

	public SceneryObjectSaveData() {
		harvestCount = 0;
		harvestedItemID = 0;
		allowStructureCollision = false;
	}

	public SceneryObjectSaveData(SceneryObject s) {
		name = s.name;
		harvestCount = s.HarvestCount;
		harvestedItemID = s.HarvestedItemID;
		allowStructureCollision = s.AllowStructureCollision;
	}

	public SceneryObjectSaveData(int HarvestCount, int HarvestedItemID, bool AllowStructureCollision) {
		harvestCount = HarvestCount;
		harvestedItemID = HarvestedItemID;
		allowStructureCollision = AllowStructureCollision;
	}

	public string GetSceneryType() {
		return name.Split('_')[0];
	}
}
