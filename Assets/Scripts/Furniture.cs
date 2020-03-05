using UnityEngine;
using AreaManagerNS;

public class Furniture : MonoBehaviour {
	private static bool editEnabled;
	private static bool canFinalize;
	private static bool usePlayerResources;
	private static Furniture editObj;
	private static Vector3 editPos;

	private Structure parent;
	private SpriteRenderer sr;
	private Color defaultColor;

	public static bool EditEnabled { get { return editEnabled; } }

	private void Awake() {
		sr = GetComponent<SpriteRenderer>();
		defaultColor = sr.color;
	}

	public static void BeginEdit(Furniture f) {
		editObj = f;
		editEnabled = true;
	}

	public static void Create(string furnitureName, Structure parentStructure) {
		if (editEnabled) { //if editing underway
			if (editObj != null) { //if there's an object being edited
				return; //do nothing
			}
		}
		Furniture temp = Resources.Load<Furniture>("Furniture/" + furnitureName);
		if (temp != null) {
			temp = GameObject.Instantiate(temp) as Furniture;
			temp.SetParent(parentStructure);
			usePlayerResources = true;
			BeginEdit(temp);
		}
	}

	public static void CancelEdit() {
		Destroy(editObj.gameObject);
		editObj = null;
		editEnabled = false;
		canFinalize = false;
	}

	public static void FinalizeEdit() {
		if (canFinalize) {
			editObj.sr.color = editObj.defaultColor;
			editEnabled = false;
			canFinalize = false;
			if (usePlayerResources) {
				//remove resources from player inventory
				usePlayerResources = false;
			}
		} else {
			//error noise
		}
	}

	public string GetOwner() {
		if (parent != null) {
			return parent.Owner;
		}
		return "";
	}

	private void OnTriggerExit(Collider other) {
		if (editEnabled) {
			if (editObj.Equals(this)) {
				if (other.CompareTag("structure")) {
					if (parent != null && other.GetComponent<Structure>().Equals(parent)) {
						sr.color = StructureCell.Color_Occupied;
						canFinalize = false;
					}
				}
			}
		}
	}

	private void OnTriggerStay(Collider other) {
		if (editEnabled) { //if editing a furniture obj
			if (editObj.Equals(this)) { //if this is the obj
				if (other.CompareTag("structure")) { //if triggered by structure
					if (parent == null || other.GetComponent<Structure>().Equals(parent)) { //if triggered by parent structure or no parent structure
						sr.color = StructureCell.Color_Unoccupied; //color = green
						canFinalize = true; //can place here
					} else { //triggered by random other structure
						sr.color = StructureCell.Color_Occupied; //color = red
						canFinalize = false; //cannot place here
					}
				}
			}
		}
	}

	private void Start() {
		transform.SetParent(AreaManager.GetEntityParent("Furniture"));
	}

	public void SetParent(Structure s) {
		if (s != null) {
			s.RegisterFurniture(this);
			parent = s;
		}
	}

	private void Update() {
		if (editEnabled) {
			if (editObj.Equals(this)) {
				editPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				editPos.z = 0;
				transform.position = editPos;

				if (parent == null) {
					if (!canFinalize) {
						sr.color = StructureCell.Color_Unoccupied;
						canFinalize = true;
					}
				}
			}
		}
	}
}
