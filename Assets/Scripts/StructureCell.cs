using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Tracks whether it is occupied; Detects mouse input while in edit mode. Written by Justin Ortiz
/// </summary>
public class StructureCell : MonoBehaviour {
	private static Color color_default = new Color(1f, 1f, 1f, 0.5f);
	private static Color color_occupied = new Color(1f, 0f, 0f, 0.5f);
	private static Color color_unoccupied = new Color(0.2f, 1f, 0.2f, 0.5f);

	private int childIndex;
	private bool occupied;
	private SpriteRenderer sprite;

	public static Color Color_Occupied { get { return color_occupied; } }
	public static Color Color_Unoccupied { get { return color_unoccupied; } }

	public bool Occupied { get { return occupied; } }

	private void Awake() {
		sprite = GetComponent<SpriteRenderer>();
		SetColor(color_default);
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
	/// Called by event trigger script in inspector; Finalizes edit mode
	/// </summary>
	public void OnPointerClick() {
		if (StructureGridManager.instance.EditEnabled) { //only trigger while edit is enabled
			StructureGridManager.instance.FinalizeStructureEdit(this);
		}
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
		SetColor(color_default);
	}

	/// <summary>
	/// Sets the cell's index; To be used on instantiation.
	/// </summary>
	/// <param name="Index"></param>
	public void SetChildIndex(int Index) {
		childIndex = Index;
	}

	private void SetColor(Color c) {
		if (sprite != null) {
			sprite.color = c;
		}
	}

	/// <summary>
	/// Flags this cell as occupied; To be used on structure placement
	/// </summary>
	public void SetOccupied() {
		SetColor(color_occupied);
		occupied = true;
	}
}//end of class
