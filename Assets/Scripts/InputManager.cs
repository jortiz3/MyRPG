using System.Xml.Serialization;
using System.Collections.Generic;
using UnityEngine;

public enum Directions { none, up, down, left, right, up_left, up_right, down_left, down_right }

[XmlRoot(ElementName = "InputManager")]
public class InputManager : MonoBehaviour {
	public static InputManager instance;

	private Dictionary<string, KeyCode> keyBindings;

	[XmlArray("KeyBindings"), XmlArrayItem("Actions")]
	public Dictionary<string, KeyCode> KeyBindings { get { return keyBindings; } set { keyBindings = value; } }

	private Directions moveDirection;
	private bool sprintEnabled;

	private void Awake() {
		if (instance != null) {
			Destroy(gameObject);
		} else {
			instance = this;

			bool setDefaultKeys = true;
			//attempt to load keys from xml file

			if (setDefaultKeys) {
				keyBindings = new Dictionary<string, KeyCode>();
				keyBindings.Add("Submit", KeyCode.Return);
				keyBindings.Add("Cancel", KeyCode.Escape);
				keyBindings.Add("Interact", KeyCode.E);
				keyBindings.Add("Attack_Basic", KeyCode.Mouse0);
				keyBindings.Add("Attack_Special", KeyCode.Mouse1);
				keyBindings.Add("Movement_Up", KeyCode.W);
				keyBindings.Add("Movement_Down", KeyCode.S);
				keyBindings.Add("Movement_Left", KeyCode.A);
				keyBindings.Add("Movement_Right", KeyCode.D);
				keyBindings.Add("Movement_Sprint", KeyCode.LeftShift);
				keyBindings.Add("Movement_Dodge", KeyCode.Space);
				keyBindings.Add("Slot_1", KeyCode.Alpha1);
				keyBindings.Add("Slot_2", KeyCode.Alpha2);
				keyBindings.Add("Slot_3", KeyCode.Alpha3);
				keyBindings.Add("Slot_4", KeyCode.Alpha4);
				keyBindings.Add("Slot_5", KeyCode.Alpha5);
				keyBindings.Add("Slot_6", KeyCode.Alpha6);
				keyBindings.Add("Slot_7", KeyCode.Alpha7);
				keyBindings.Add("Slot_8", KeyCode.Alpha8);
				keyBindings.Add("Slot_9", KeyCode.Alpha9);
				keyBindings.Add("Slot_10", KeyCode.Alpha0);
			}
		}
	}

	public string GetKeyCodeName(string axisName) {
		return keyBindings[axisName].ToString();
	}

	private void Update() {
		moveDirection = Directions.none;
		sprintEnabled = false;

		//movement up & down -- prioritize up
		if (Input.GetKey(keyBindings["Movement_Up"])) {
			moveDirection = Directions.up;
		} else if (Input.GetKey(keyBindings["Movement_Down"])) {
			moveDirection = Directions.down;
		}

		//movement left & right -- prioritize left
		if (Input.GetKey(keyBindings["Movement_Left"])) {
			if (moveDirection == Directions.up) { //if up key and left are pressed
				moveDirection = Directions.up_left;
			} else if (moveDirection == Directions.down) {
				moveDirection = Directions.down_left;
			} else {
				moveDirection = Directions.left;
			}
		} else if (Input.GetKey(keyBindings["Movement_Right"])) {
			if (moveDirection == Directions.up) {
				moveDirection = Directions.up_right;
			} else if (moveDirection == Directions.down) {
				moveDirection = Directions.down_right;
			} else {
				moveDirection = Directions.right;
			}
		}

		//movement run/sprint
		if (Input.GetKey(keyBindings["Movement_Sprint"])) {
			sprintEnabled = true;
		}

		//send movement info to player
		if (Player.instance != null) {
			Player.instance.MoveDirection(moveDirection, sprintEnabled);
		}

		if (Input.GetKeyDown(keyBindings["Interact"])) {
			Interactable.Interact();
		}

		if (Input.GetKeyDown(keyBindings["Attack_Basic"])) {
			if (StructureGridManager.instance.EditEnabled) { //only trigger while edit is enabled
				StructureGridManager.instance.FinalizeStructureEdit();
			}
		}

		if (Input.GetKeyDown(keyBindings["Submit"])) {
			if (StructureGridManager.instance.EditEnabled) { //only trigger while edit is enabled
				StructureGridManager.instance.FinalizeStructureEdit();
			}
		}

		if (Input.GetKeyDown(keyBindings["Cancel"])) {
			if (StructureGridManager.instance.EditEnabled) {
				StructureGridManager.instance.CancelStructureEdit();
			}
		}

#if UNITY_EDITOR
		if (Input.GetKeyDown(KeyCode.LeftArrow)) {
			WorldManager.instance.LoadAdjacentArea(Directions.left);
		}
		if (Input.GetKeyDown(KeyCode.RightArrow)) {
			WorldManager.instance.LoadAdjacentArea(Directions.right);
		}
		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			WorldManager.instance.LoadAdjacentArea(Directions.up);
		}
		if (Input.GetKeyDown(KeyCode.DownArrow)) {
			WorldManager.instance.LoadAdjacentArea(Directions.down);
		}

		if (Input.GetKeyDown(KeyCode.G)) {
			WorldManager.instance.GenerateWorldAreas("Player1", "Debug World");
		}
		if (Input.GetKeyDown(KeyCode.L)) {
			WorldManager.instance.LoadAreaData("Player1", "Debug World");
		}
		if (Input.GetKeyDown(KeyCode.P)) {
			StructureGridManager.instance.BeginStructureEdit("City_0");
		}
#endif
	}
}
