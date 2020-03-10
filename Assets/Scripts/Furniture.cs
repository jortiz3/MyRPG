using UnityEngine;

public class Furniture : MonoBehaviour {
	private static bool editEnabled;
	private static bool canFinalize;
	private static bool usePlayerResources;
	private static Furniture editObj;
	private static Vector3 editPos;

	[SerializeField]
	private Structure parent;
	private SpriteRenderer sr;
	private Color defaultColor;
	private Vector3 desiredScale;
	private float triggered;

	public static bool EditEnabled { get { return editEnabled; } }

	private void Awake() {
		sr = GetComponent<SpriteRenderer>();
		defaultColor = sr.color;
		desiredScale = transform.localScale;
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
			if (parentStructure != null) {
				parentStructure.RegisterFurniture(temp);
			}
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

	private void OnTriggerStay(Collider other) {
		if (other.CompareTag("structure")) { //if triggered by structure
			Structure currStructure = other.GetComponent<Structure>(); //get structure script
			if (currStructure != null) { //if script found
				if (editEnabled) { //if editing a furniture obj
					if (editObj.Equals(this)) { //if this is the obj
						if (parent == null || currStructure.Equals(parent)) { //if triggered by parent structure or no parent structure
							sr.color = StructureCell.Color_Unoccupied; //color = green
							canFinalize = true; //can place here
							triggered = 0f; //flag canFinalize as triggered recently
						}
					}
				} else if (parent == null) {
					currStructure.RegisterFurniture(this);
				}
			}
		}
	}

	public void ResetTransformParent() {
		SetTransformParent(AreaManager.GetEntityParent("Furniture"));
	}

	private void Start() {
		if (parent != null) {
			SetTransformParent(parent.transform);
			parent.RegisterFurniture(this);
		} else {
			ResetTransformParent();
		}
	}

	public void SetStructureParent(Structure s) {
		if (s != null) {
			parent = s;
		}
	}

	public void SetTransformParent(Transform t) {
		transform.SetParent(t);

		Vector3 newScale = Vector3.one;
		if (t.localScale.sqrMagnitude > desiredScale.sqrMagnitude) {
			newScale.x =  desiredScale.x / t.localScale.x;
			newScale.y = desiredScale.y / t.localScale.y;
		} else if (t.localScale.sqrMagnitude > desiredScale.sqrMagnitude) {
			newScale.x = t.localScale.x / desiredScale.x;
			newScale.y = t.localScale.y / desiredScale.y;
		}

		transform.localScale = newScale;
	}

	private void Update() {
		if (editEnabled) {
			if (editObj.Equals(this)) {
				editPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				editPos.z = 0;
				transform.position = editPos;

				if (triggered < 0.15f) { //if triggered recently
					triggered += Time.deltaTime;
				} else { //not triggered recently
					sr.color = StructureCell.Color_Occupied;
					canFinalize = false; //flag for not placing
				}
			}
		}
		
		
	}
}
