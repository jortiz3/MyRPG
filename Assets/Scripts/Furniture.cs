using UnityEngine;

public class Furniture : Interactable {
	private static bool editEnabled;
	private static bool canFinalize;
	private static bool usePlayerResources;
	private static Furniture editObj;
	private static Vector3 editPos;

	[SerializeField]
	private Structure parent;
	private Color defaultColor;
	private Vector3 desiredScale;
	private float triggered;

	public static bool EditEnabled { get { return editEnabled; } }

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
			editObj.sprite.color = editObj.defaultColor;
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
		return owner;
	}

	protected override void Initialize() {
		sprite = gameObject.GetComponent<SpriteRenderer>(); //get the sprite component
		defaultColor = sprite.color;
		desiredScale = transform.localScale;

		ResetTransformParent();
		DisableInteraction(); //disable interaction as of now -- some furniture may open crafting menus later on
		base.Initialize();
	}

	public void Load(Texture2D texture = null, Structure parentStructure = null, string Owner = "", float LastUpdated = -float.MaxValue) {
		owner = Owner;
		lastUpdated = LastUpdated;

		SetSprite(texture);

		if (parentStructure != null) {
			parentStructure.RegisterFurniture(this);

			if (Owner.Equals("")) {
				owner = parentStructure.Owner;
			}
		}
	}

	private void OnTriggerStay(Collider other) {
		if (other.CompareTag("structure")) { //if triggered by structure
			Structure currStructure = other.GetComponent<Structure>(); //get structure script
			if (currStructure != null) { //if script found
				if (editEnabled) { //if editing a furniture obj
					if (editObj.Equals(this)) { //if this is the obj
						if (parent == null || currStructure.Equals(parent)) { //if triggered by parent structure or no parent structure
							sprite.color = GameManager.Color_Available; //color = green
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

	public void SetStructureParent(Structure s, bool setTransformParent = false) {
		if (s != null) {
			parent = s;
			if (setTransformParent) {
				SetTransformParent(s.transform);
			}
		}
	}

	/// <summary>
	/// Sets the transform parent and ensures the furniture object remains the same size on screen.
	/// </summary>
	public void SetTransformParent(Transform t) {
		if (transform.parent != t) { //if new transform to set to parent
			transform.SetParent(t); //set parent in inspector

			Vector3 newScale = Vector3.one; //start with scale of 1
			if (t.localScale.sqrMagnitude < desiredScale.sqrMagnitude) { //if the parent scale is smaller than what we want
				newScale.x = desiredScale.x / t.localScale.x;
				newScale.y = desiredScale.y / t.localScale.y;
			} else if (t.localScale.sqrMagnitude > desiredScale.sqrMagnitude) { //if parent scale is bigger than what we want
				newScale.x = t.localScale.x / desiredScale.x;
				newScale.y = t.localScale.y / desiredScale.y;
			}

			transform.localScale = newScale; //set the scale
		}
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
					sprite.color = GameManager.Color_Unavailable;
					canFinalize = false; //flag for not placing
				}
			}
		}
	}
}
