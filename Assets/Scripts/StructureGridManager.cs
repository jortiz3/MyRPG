using UnityEngine;

/// <summary>
/// Manages the grid with which structure positions are conformed to. Written by Justin Ortiz
/// </summary>
public class StructureGridManager : MonoBehaviour {
	public static StructureGridManager instance;

	[SerializeField]
	private int columns = 10; //x
	[SerializeField]
	private int rows = 10; //y

	private bool editEnabled;
	private bool canFinalize;

	private Structure currEditStructure;

	private CanvasGroup canvasGroup;

	public bool EditEnabled { get { return editEnabled; } }

	private void Awake() {
		if (instance != null) { //if another instance exists
			Destroy(gameObject); //get rid of this one
		} else {
			instance = this; //set the instance

			if (columns <= 0) { //verify columns assigned
				columns = 10;
			}
			if (rows <= 0) { //verify rows assigned
				rows = 10;
			}

			editEnabled = false;
			canFinalize = false;

			InitializeGrid();

			canvasGroup = GetComponent<CanvasGroup>(); //shows/hides the cells
			SetGridActive(false); //uses canvasGroup
		}
	}

	public void BeginStructureEdit(string structureName) {
		currEditStructure = Resources.Load<Structure>(structureName); //attempt to load structure from assets

		if (currEditStructure != null) { //if it was retrieved
			SetGridActive(true); //show the grid
			editEnabled = true; //start edit
		}
	}

	public void CancelStructureEdit() {
		Destroy(currEditStructure.gameObject); //remove the temp structure from scene
		SetGridActive(false); //hide the grid
		editEnabled = false; //end edit
	}

	/// <summary>
	/// Determines whether the given structure can be finalized at the given cell.
	/// </summary>
	/// <param name="cellIndex">The 2D index of a cell on the grid.</param>
	/// <returns></returns>
	private bool CheckForFinalize(Vector2Int cellIndex, Structure s) {
		for (int x = cellIndex.x; x < cellIndex.x + s.Dimensions.x; x++) { //start current x, go to dimension size
			for (int y = cellIndex.y; y < cellIndex.y + s.Dimensions.y; y++) { //start current y, go to dimension size
				if (GetCell(x, y).Occupied) { //if the cell is occupied
					return false;
				}
			}
		}
		return true;
	}

	/// <summary>
	/// Ends editing & registers the structure to the grid.
	/// </summary>
	/// <param name="cell">The top-left cell within which to place the structure</param>
	public void FinalizeStructureEdit(StructureCell cell) {
		FinalizeStructureEdit(cell, currEditStructure);
	}

	/// <summary>
	/// Ends editing & registers the structure to the grid.
	/// </summary>
	/// <param name="cell">The top-left cell within which to place the structure</param>
	/// <param name="s">The structure to finalize</param>
	public void FinalizeStructureEdit(StructureCell cell, Structure s) {
		if (!canFinalize) {
			//play error noise?
		} else {
			SetGridActive(false);
			s.ResetColor();

			Vector2Int cellPos = cell.GetGridIndex(columns, rows);
			for (int x = cellPos.x; x < cellPos.x + s.Dimensions.x; x++) { //start current x, go to dimension size
				for (int y = cellPos.y; y < cellPos.y + s.Dimensions.y; y++) { //start current y, go to dimension size
					GetCell(x, y).SetOccupied();
				}
			}

			editEnabled = false;
			canFinalize = false;
		}
	}

	/// <summary>
	/// Gets a cell by converting 2D position into 1D.
	/// </summary>
	/// <param name="x">Grid Position X</param>
	/// <param name="y">Grid Position Y</param>
	/// <returns>StructureCell requested cell</returns>
	private StructureCell GetCell(int x, int y) {
		int index = (y * columns) + x; //convert from 2D index to 1D
		if (index < transform.childCount) { //if index is valid
			return transform.GetChild(index).GetComponent<StructureCell>(); //return the cell
		}
		return null; //not a valid grid index
	}

	private StructureCell GetNearestCell(Vector3 worldPosition) {
		StructureCell nearestCell = transform.GetChild(0).GetComponent<StructureCell>(); //default to first cell
		float nearestDistance = Mathf.Abs(Vector3.Distance(transform.GetChild(0).position, worldPosition)); //default to distance from first cell
		float currentDistance;
		for (int i = 0; i < transform.childCount; i++) { //go through all cells
			currentDistance = Mathf.Abs(Vector3.Distance(transform.GetChild(i).position, worldPosition)); //get absolute value of current distance
			if (currentDistance < nearestDistance) { //if current is closer than nearest
				nearestCell = transform.GetChild(i).GetComponent<StructureCell>(); //get the cell
				nearestDistance = currentDistance; //update nearest
			}
		}

		return nearestCell; //return the nearest cell
	}

	private void InitializeGrid() {
		ResetGridStatus(); //reset status of current cells

		if (transform.childCount > 0) { //if template cell is available
			GameObject cellTemplate = transform.GetChild(0).gameObject; //get template
			int count = rows * columns; //get total number of cells

			for (int i = transform.childCount; i < count; i++) { //start with current child count, increase to desired count
				StructureCell sc = Instantiate(cellTemplate, transform).GetComponent<StructureCell>(); //instantiate template, ensure transform remains parent
				sc.SetChildIndex(i); //store the cell's index for later use
			}
		}
	}

	public void MoveStructureEdit(StructureCell cell) {
		currEditStructure.transform.position = cell.transform.position; //move the structure to the cell's world position
		Vector2Int cellPos = cell.GetGridIndex(columns, rows); //get the cell's 2D index

		canFinalize = CheckForFinalize(cellPos, currEditStructure); //check to see if structure can be finalized in this index

		if (canFinalize) { //if all the required cells were unoccupied
			currEditStructure.SetColor(StructureCell.Color_Unoccupied); //show the player they are able to place there
		}
	}

	public void RegisterExistingStructure(Structure s) {
		StructureCell temp = GetNearestCell(s.transform.position); //get the nearest cell to the given structure
		canFinalize = CheckForFinalize(temp.GetGridIndex(columns, rows), s); //see if structure can be placed where it already is

		if (canFinalize) { //if structure can be placed where it already is
			FinalizeStructureEdit(temp, s); //ensure all cells know they are occupied
		} else {
			//find alternate position
		}
	}

	/// <summary>
	/// Resets all cells in grid to unoccupied status
	/// </summary>
	public void ResetGridStatus() {
		foreach (StructureCell cell in transform) {
			cell.Reset();
		}
	}

	private void SetGridActive(bool active) {
		canvasGroup.interactable = active; //set whether events are triggered
		canvasGroup.blocksRaycasts = active; //set whether anything behind it is triggered
		canvasGroup.alpha = active ? 1f : 0f; //set the visibility
	}
}
