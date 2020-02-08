using System.Xml.Serialization;
using System.Collections.Generic;
using UnityEngine;

public enum Directions { up, down, left, right, up_left, up_right, down_left, down_right }

[XmlRoot(ElementName = "InputManager")]
public class InputManager : MonoBehaviour {
	private Dictionary<string, KeyCode> keyBindings;

	[XmlArray("KeyBindings"), XmlArrayItem("Actions")]
	public Dictionary<string, KeyCode> KeyBindings { get { return keyBindings; } set { keyBindings = value; } }

	private void Awake() {
		bool setDefaultKeys = true;
		//attempt to load keys from xml file

		if (setDefaultKeys) {
			keyBindings = new Dictionary<string, KeyCode>();
			keyBindings.Add("Movement_Up", KeyCode.W);
			keyBindings.Add("Movement_Down", KeyCode.S);
			keyBindings.Add("Movement_Left", KeyCode.A);
			keyBindings.Add("Movement_Right", KeyCode.D);
		}
	}

	void Update() {
		if (Input.GetKey(keyBindings["Movement_Up"])) {
			//move
		}
		if (Input.GetKey(keyBindings["Movement_Down"])) {
			//move
		}
		if (Input.GetKey(keyBindings["Movement_Left"])) {
			//move
		}
		if (Input.GetKey(keyBindings["Movement_Right"])) {
			//move
		}


		if (Input.GetKeyDown(KeyCode.LeftArrow)) {
			GameManager.worldManager.LoadAdjacentArea(Directions.left);
		}
		if (Input.GetKeyDown(KeyCode.RightArrow)) {
			GameManager.worldManager.LoadAdjacentArea(Directions.right);
		}
		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			GameManager.worldManager.LoadAdjacentArea(Directions.up);
		}
		if (Input.GetKeyDown(KeyCode.DownArrow)) {
			GameManager.worldManager.LoadAdjacentArea(Directions.down);
		}

		if (Input.GetKeyDown(KeyCode.Return)) {
			GameManager.worldManager.GenerateWorldAreas("Player1", "Debug World");
		}
		if (Input.GetKeyDown(KeyCode.Space)) {
			GameManager.worldManager.LoadAreaData("Player1", "Debug World");
		}
	}
}
