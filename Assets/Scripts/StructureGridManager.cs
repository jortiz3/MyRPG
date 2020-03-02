using System.Collections;
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

	private bool gridInitialized;
	private bool editEnabled;
	private bool editFinalizing;
	private bool canFinalize;

	private Structure currEditStructure;

	private CanvasGroup canvasGroup;

	public bool EditEnabled { get { return editEnabled; } }
	public bool EditFinalizing { get { return editFinalizing; } }

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
			gridInitialized = false;

			StartCoroutine(InitializeGrid());

			canvasGroup = GetComponent<CanvasGroup>(); //shows/hides the cells
			SetGridActive(false); //uses canvasGroup
		}
	}

	public void BeginStructureEdit(string structureName) {
		currEditStructure = Resources.Load<Structure>("Structures/" + structureName); //attempt to load structure from assets

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
		if (gridInitialized) { //if the grid has been initialized, then we can check
			Vector2Int originalIndex;
			for (int x = cellIndex.x; x < cellIndex.x + s.Dimensions.x; x++) { //start current x, go to dimension size
				for (int y = cellIndex.y; y < cellIndex.y + s.Dimensions.y; y++) { //start current y, go to dimension size
					originalIndex = cellIndex; //store current index
					cellIndex.Clamp(Vector2Int.zero, new Vector2Int(columns, rows)); //try to clamp the current index to bounds

					if (!originalIndex.Equals(cellIndex)) { //if index was clamped, then original was outside of bounds
						return false; //cannot finalize if part of structure is out of bounds
					}
					if (GetCell(x, y).Occupied) { //if the cell is occupied
						return false;
					}
				}
			}
			return true;
		}
		return false;
	}

	public void FinalizeStructureEdit(StructureCell cell) {
		FinalizeStructureEdit(GetCellIndex(cell));
	}

	/// <summary>
	/// Ends editing & registers the current edit structure to the grid.
	/// </summary>
	/// <param name="cellIndex">The top-left cell within which to place the structure</param>
	public void FinalizeStructureEdit(Vector2Int cellIndex) {
		if (currEditStructure != null) {
			StartCoroutine(FinalizeStructureEdit(cellIndex, currEditStructure));
		}
	}

	/// <summary>
	/// Ends editing & registers the structure to the grid.
	/// </summary>
	/// <param name="cellIndex">The top-left cell within which to place the structure</param>
	/// <param name="s">The structure to finalize</param>
	public IEnumerator FinalizeStructureEdit(Vector2Int cellIndex, Structure s) {
		while (editFinalizing) {
			yield return new WaitForEndOfFrame();
		}

		if (!canFinalize) {
			//play error noise?
		} else {
			editFinalizing = true;
			SetGridActive(false);
			s.ResetColor();

			for (int x = cellIndex.x; x < cellIndex.x + s.Dimensions.x; x++) { //start current x, go to dimension size
				for (int y = cellIndex.y; y < cellIndex.y + s.Dimensions.y; y++) { //start current y, go to dimension size
					GetCell(x, y).SetOccupied();
				}
			}

			yield return new WaitForSeconds(0.2f);

			editEnabled = false;
			editFinalizing = false;
			canFinalize = false;
		}
	}

	private StructureCell GetCell(Vector2Int index) {
		return GetCell(index.x, index.y);
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

	private Vector2Int GetCellIndex(StructureCell cell) {
		return cell.GetGridIndex(columns, rows);
	}

	private Vector2Int GetNearestCellIndex(Vector3 worldPosition) {
		Vector2Int nearestCell = Vector2Int.zero; //default to first cell
		float nearestDistance = Mathf.Abs(Vector3.Distance(transform.GetChild(0).position, worldPosition)); //default to distance from first cell
		float currentDistance;
		for (int i = 0; i < transform.childCount; i++) { //go through all cells
			currentDistance = Mathf.Abs(Vector3.Distance(transform.GetChild(i).position, worldPosition)); //get absolute value of current distance
			if (currentDistance < nearestDistance) { //if current is closer than nearest
				nearestCell = GetCellIndex(transform.GetChild(i).GetComponent<StructureCell>()); //get the cell
				nearestDistance = currentDistance; //update nearest
			}
		}

		return nearestCell; //return the nearest cell
	}

	private IEnumerator InitializeGrid() {
		ResetGridStatus(); //reset status of current cells
		yield return new WaitForSeconds(0.01f);

		if (transform.childCount > 0) { //if template cell is available
			GameObject cellTemplate = transform.GetChild(0).gameObject; //get template
			int count = rows * columns; //get total number of cells

			for (int i = transform.childCount; i < count; i++) { //start with current child count, increase to desired count
				StructureCell sc = Instantiate(cellTemplate, transform).GetComponent<StructureCell>(); //instantiate template, ensure transform remains parent
				sc.SetChildIndex(i); //store the cell's index for later use

				if (i % 10 == 0) { //every 5 cells
					yield return new WaitForSeconds(0.01f); //insert pause
				}
			}
		}
		gridInitialized = true;
	}

	public void MoveStructureEdit(StructureCell cell) {
		currEditStructure.transform.position = cell.transform.position; //move the structure to the cell's world position
		Vector2Int cellPos = GetCellIndex(cell); //get the cell's 2D index

		canFinalize = CheckForFinalize(cellPos, currEditStructure); //check to see if structure can be finalized in this index

		if (canFinalize) { //if all the required cells were unoccupied
			currEditStructure.SetColor(StructureCell.Color_Unoccupied); //show the player they are able to place there
		}
	}

	public IEnumerator RegisterExistingStructure(Structure s) {
		while (!gridInitialized) {
			yield return new WaitForEndOfFrame();
		}

		Vector2Int tempCellIndex = GetNearestCellIndex(s.transform.position); //get the nearest cell to the given structure
		canFinalize = CheckForFinalize(tempCellIndex, s); //see if structure can be placed where it already is

		if (canFinalize) { //if structure can be placed where it already is
			StartCoroutine(FinalizeStructureEdit(tempCellIndex, s)); //ensure all cells know they are occupied
		} else { //another structure is taking some of the required space; move is required
			bool alternateIndexFound = false;
			Vector2Int currIndex;
			Vector2Int adjustment;
			int attempts = 0;
			while (!alternateIndexFound) {
				if (attempts % 4 == 0) { //every 4 attempts
					adjustment = Vector2Int.left * attempts; //go left
				} else if (attempts % 4 == 1) {
					adjustment = Vector2Int.right * attempts; //go right
				} else if (attempts % 4 == 2) {
					adjustment = Vector2Int.down * attempts; //go up (reversed)
				} else {
					adjustment = Vector2Int.up * attempts; //go down
				}
				currIndex = tempCellIndex + adjustment; //add adjustment
				currIndex.Clamp(Vector2Int.zero, new Vector2Int(columns, rows)); //ensure the index isn't out of bounds

				yield return new WaitForEndOfFrame();
				canFinalize = CheckForFinalize(currIndex, s); //see if structure can be placed at adjusted position
				yield return new WaitForSeconds(0.01f);

				if (canFinalize) {
					s.transform.position = GetCell(currIndex).transform.position; //set the position of the structure to the position of the cell
					StartCoroutine(FinalizeStructureEdit(currIndex, s)); //ensure required cells are flagged as occupied
					alternateIndexFound = true;
				}

				if (attempts >= 100) { //if we have tried too many times
					Destroy(s.gameObject); //give up on the structure
					break; //leave the loop
				}
				attempts++;
			} //new position loop
		}
	}

	/// <summary>
	/// Resets all cells in grid to unoccupied status
	/// </summary>
	public void ResetGridStatus() {
		StructureCell currCell;
		for (int i = 0; i < transform.childCount; i++) {
			currCell = transform.GetChild(i).GetComponent<StructureCell>();
			if (currCell != null) {
				currCell.Reset();
			}
		}
	}

	private void SetGridActive(bool active) {
		canvasGroup.interactable = active; //set whether events are triggered
		canvasGroup.blocksRaycasts = active; //set whether anything behind it is triggered
		canvasGroup.alpha = active ? 1f : 0f; //set the visibility
	}
}
