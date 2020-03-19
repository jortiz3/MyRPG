using System.Collections.Generic;
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
	private bool usePlayerResources;
	private bool registeringStructure;

	private StructureCell currCell;
	private StructureCell originalCell; //the cell in which the edit began (pre-existing structure edit)
	private Structure currEditStructure;
	private List<Structure> registerQueue;

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
			registeringStructure = false;

			registerQueue = new List<Structure>();

			StartCoroutine(InitializeGrid());

			canvasGroup = GetComponent<CanvasGroup>(); //shows/hides the cells
			SetGridActive(false); //uses canvasGroup
		}
	}

	public void BeginStructureCreate(string structureName) {
		if (editEnabled) { //this method should not be called after editing starts.. but just in case
			if (currEditStructure != null) { //if there's already a structure being edited somehow
				return; //don't do anything
			}
		}

		currEditStructure = Resources.Load<Structure>("Structures/" + structureName); //attempt to load structure from assets

		if (currEditStructure != null) { //if it was retrieved
			SetGridActive(true); //show the grid
			

			currEditStructure = Instantiate(currEditStructure);

			currCell = GetCell(GetNearestCellIndex(Player.instance.transform.position));
			MoveStructureEdit(currCell);

			CameraManager.instance.ShowStructureCam(currEditStructure.transform);

			usePlayerResources = true;
			editEnabled = true; //start edit
		} else {
			Debug.Log("Asset Load Error: StructureGridManager.BeginStructureCreate(" + structureName + ")");
		}
	}

	public void BeginStructureEdit(Structure s) {
		if (editEnabled) { //if somehow edit is enabled
			if (currEditStructure != null) { //if there's a structure already being edited
				return; //do nothing
			}
		}

		currEditStructure = s; //set the structure to edit
		currCell = GetCell(GetNearestCellIndex(s.transform.position)); //establish current cell based on where structure already is
		originalCell = currCell;

		s.SetFurnitureAsTransformChildren(); //ensure furniture moves with structure

		UnregisterExistingStructure(s, currCell);
		MoveStructureEdit(currCell); //update the colors and finalization possibility

		SetGridActive(true); //show the grid

		CameraManager.instance.ShowStructureCam(currEditStructure.transform); //follow structure with cam

		usePlayerResources = false;
		editEnabled = true;
	}

	public void CancelStructureEdit() {
		CameraManager.instance.Reset(); //camera follow player again
		HUD.instance.HideInteractionText(); //ensure no text is displayed
		if (originalCell != null) {
			MoveStructureEdit(originalCell);
			FinalizeStructureEdit();
		} else {
			Destroy(currEditStructure.gameObject); //remove the created structure from scene
		}
		SetGridActive(false); //hide the grid
		currCell = null; //remove pointer to cell
		originalCell = null;
		currEditStructure = null;
		usePlayerResources = false;
		editEnabled = false; //end edit
	}

	/// <summary>
	/// Determines whether the given structure can be finalized at the given cell.
	/// </summary>
	/// <param name="cellIndex">The 2D index of a cell on the grid.</param>
	/// <returns></returns>
	private bool CheckForFinalize(Vector2Int cellIndex, Structure s) {
		if (gridInitialized) { //if the grid has been initialized, then we can check
			for (int x = cellIndex.x; x <= cellIndex.x + s.Dimensions.x; x++) { //start current x, go to dimension size
				if (x >= columns) { //if dimensions are going out of bounds
					return false;
				}

				for (int y = cellIndex.y; y <= cellIndex.y + s.Dimensions.y; y++) { //start current y, go to dimension size
					if (y >= rows) { //if dimensions are going out of bounds
						return false;
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

	/// <summary>
	/// Ends editing & registers the current edit structure to the grid.
	/// </summary>
	public void FinalizeStructureEdit() {
		if (canFinalize) { //if finalization is possible
			if (currEditStructure != null && currCell != null) { //if the variables we need have value
				StartCoroutine(FinalizeStructureEdit(GetCellIndex(currCell), currEditStructure)); //start coroutine

				currCell = null;
				originalCell = null;

				currEditStructure.ResetFurnitureTransformParent();
				currEditStructure = null;

				if (usePlayerResources) { //if player needs to spend resources
					//remove resources from player inventory
					usePlayerResources = false;
				}
			}
		}
	}

	/// <summary>
	/// Ends editing & registers the structure to the grid.
	/// </summary>
	/// <param name="cellIndex">The top-left cell within which to place the structure</param>
	/// <param name="s">The structure to finalize</param>
	public IEnumerator FinalizeStructureEdit(Vector2Int cellIndex, Structure s) {
		if (editFinalizing) { //this 'should' never happen, but if it does
			RegisterExistingStructure(s); //put it back in the queue to be registered
			registeringStructure = false; //end registering
			yield break; //abort
		}

		editFinalizing = true;
		SetGridActive(false);
		s.ResetColor();

		for (int x = cellIndex.x; x <= cellIndex.x + s.Dimensions.x; x++) { //start current x, go to dimension size
			for (int y = cellIndex.y; y <= cellIndex.y + s.Dimensions.y; y++) { //start current y, go to dimension size
				GetCell(x, y).SetOccupied();
			}
		}

		yield return new WaitForEndOfFrame();

		CameraManager.instance.Reset();
		HUD.instance.HideInteractionText();

		editEnabled = false;
		editFinalizing = false;
		canFinalize = false;
		registeringStructure = false;
	}

	private void FixedUpdate() {
		if (editEnabled) { //only update interactions during editing
			if (canFinalize) { //if the the building can be placed
				HUD.instance.ShowInteractionText("Left-Click to finalize.");
			} else {
				HUD.instance.HideInteractionText();
			}
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
		return cell.GetGridIndex(columns);
	}

	private Vector2Int GetNearestCellIndex(Vector3 worldPosition) {
		Vector2Int nearestCell = Vector2Int.zero; //default to first cell
		float nearestDistance = Mathf.Abs(Vector3.Distance(transform.GetChild(0).position, worldPosition)); //default to distance from first cell
		float currentDistance;
		for (int i = 1; i < transform.childCount; i++) { //go through all cells
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
		} else {
			currEditStructure.SetColor(StructureCell.Color_Occupied); //show the player they can't place there
		}

		currCell = cell;
	}

	public void RegisterExistingStructure(Structure s) {
		if (registerQueue != null) {
			registerQueue.Add(s);
		}
	}

	private IEnumerator RegisterStructure(Structure s) {
		registeringStructure = true;

		while (!gridInitialized) {
			yield return new WaitForEndOfFrame();
		}

		Vector2Int tempCellIndex = GetNearestCellIndex(s.transform.position); //get the nearest cell to the given structure
		bool tempFinalizeCheck = CheckForFinalize(tempCellIndex, s); //see if structure can be placed where it already is

		if (tempFinalizeCheck) { //if structure can be placed where it already is
			s.transform.position = GetCell(tempCellIndex).transform.position; //ensure structure snaps to grid
			StartCoroutine(FinalizeStructureEdit(tempCellIndex, s)); //finalize this structure to occupy the cells
		} else { //another structure is taking some of the required space; move is required
			bool alternateIndexFound = false;
			Vector2Int currIndex;
			Vector2Int adjustment;
			int attempts = 1;
			while (!alternateIndexFound) {
				if (attempts % 2 == 0) { //every other attempt; try left, then try right
					adjustment = ((Vector2Int.left + (Vector2Int.left * s.Dimensions)) * attempts) + Vector2Int.left; //set adjustment to left + dimensions.x, then amplify by attempts, then add 1 cell padding
				} else {
					adjustment = (Vector2Int.right * attempts) + Vector2Int.right; //set adjustment right amplified by attempt number, then add 1 cell padding
				}
				currIndex = tempCellIndex + adjustment; //add adjustment to cell index
				currIndex.Clamp(Vector2Int.zero, new Vector2Int(columns, rows)); //ensure the index isn't out of bounds

				yield return new WaitForEndOfFrame();
				tempFinalizeCheck = CheckForFinalize(currIndex, s); //see if structure can be placed at adjusted position
				yield return new WaitForSeconds(0.01f);

				if (tempFinalizeCheck) {
					s.transform.position = GetCell(currIndex).transform.position; //set the position of the structure to the position of the cell
					StartCoroutine(FinalizeStructureEdit(currIndex, s)); //ensure required cells are flagged as occupied
					alternateIndexFound = true;
				}

				if (attempts >= 100) { //if we have tried too many times
					Destroy(s.gameObject); //give up on the structure
					registeringStructure = false;
					break; //leave the loop
				}
				attempts++;
			} //new position loop
		} //end if
	}

	/// <summary>
	/// Resets all cells in grid to unoccupied status
	/// </summary>
	public void ResetGridStatus() {
		for (int i = 0; i < transform.childCount; i++) {
			currCell = transform.GetChild(i).GetComponent<StructureCell>();
			if (currCell != null) {
				currCell.Reset();
			}
		}
		currCell = null;
	}

	private void SetGridActive(bool active) {
		canvasGroup.interactable = active; //set whether events are triggered
		canvasGroup.blocksRaycasts = active; //set whether anything behind it is triggered
		canvasGroup.alpha = active ? 1f : 0f; //set the visibility
	}

	/// <summary>
	/// Removes the occupied flag for cells currently occupied by a structure.
	/// </summary>
	private void UnregisterExistingStructure(Structure s, StructureCell cellOrigin) {
		if (gridInitialized) { //if the grid has been initialized, then we can check
			Vector2Int cellIndex = cellOrigin.GetGridIndex(columns);
			for (int x = cellIndex.x; x <= cellIndex.x + s.Dimensions.x; x++) { //start current x, go to dimension size
				if (x < columns) { //if dimensions are in bounds
					for (int y = cellIndex.y; y <= cellIndex.y + s.Dimensions.y; y++) { //start current y, go to dimension size
						if (y < rows) { //if dimensions are in bounds
							GetCell(x, y).Reset(); //free up the cells
						}
					} //y loop
				} //x if
			} //x loop
		}
	}

	private void Update() {
		if (registerQueue != null && registerQueue.Count > 0) { //if there are structures to register
			if (!registeringStructure) { //if registering isn't underway
				StartCoroutine(RegisterStructure(registerQueue[0])); //begin fixing position against other structures
				registerQueue.RemoveAt(0); //remove from queue
			}
		}
	}
}
