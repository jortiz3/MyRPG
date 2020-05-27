using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tracks whether it is occupied; Detects mouse input while in edit mode. Written by Justin Ortiz
/// </summary>
public class StructureCell : MonoBehaviour {
	private int childIndex;
	private bool occupied;
	private Image image;

	public bool Occupied { get { return occupied; } }

	private void Awake() {
		image = GetComponent<Image>();
		SetColor(GameManager.Color_Default);
	}
	
	/// <summary>
	/// Converts the child index into a 2D index in the grid.
	/// </summary>
	/// <param name="columns">Number of columns in the grid.</param>
	/// <param name="rows">Number of rows in the grid.</param>
	/// <returns>Vector2Int grid index/position.</returns>
	public Vector2Int GetGridIndex(int columns) {
		return new Vector2Int(childIndex % columns, childIndex / columns);
	}

	/// <summary>
	/// Called by event trigger script in inspector; Moves edit structure & updates this cell's color
	/// </summary>
	public void OnPointerEnter() {
		if (StructureGridManager.instance.EditEnabled) { //only trigger while edit is enabled
			StructureGridManager.instance.MoveStructureEdit(this); //move the structure to this pos
		}
	}

	public void Reset() {
		occupied = false;
		SetColor(GameManager.Color_Default);
	}

	/// <summary>
	/// Sets the cell's index; To be used on instantiation.
	/// </summary>
	/// <param name="Index"></param>
	public void SetChildIndex(int Index) {
		childIndex = Index;
	}

	private void SetColor(Color c) {
		if (image != null) {
			image.color = c;
		}
	}

	/// <summary>
	/// Flags this cell as occupied; To be used on structure placement
	/// </summary>
	public void SetOccupied() {
		SetColor(GameManager.Color_Unavailable);
		occupied = true;
	}
}//end of class
