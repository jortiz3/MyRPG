using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public enum Directions { none, up, down, left, right, up_left, up_right, down_left, down_right }

public class InputManager : MonoBehaviour {
	public static InputManager instance;

	private static Dictionary<string, KeyCode> keyBindings;
	private static KeyCode[] keyCodes;
	private static string keyBindingsFilePath;
	private static Transform uiParent;
	private static Transform uiPrefab;

	private string actionToRebind;

	private Directions moveDirection;
	private bool sprintEnabled;

	private void Awake() {
		if (instance != null) {
			Destroy(gameObject);
		} else {
			instance = this;
			actionToRebind = "";
			keyCodes = Enum.GetValues(typeof(KeyCode)) as KeyCode[];
			keyBindingsFilePath = Application.persistentDataPath + "/data/kb.dat";

			bool loadKeybindings = File.Exists(keyBindingsFilePath); //if there is file, load it

			if (false) {//loadKeybindings) {
				LoadKeyBindings();
			} else {
				InitializeDefaultKeyBindings();
				SaveKeyBindings();
			} //end if default keys

			InitializeControlsUI();
		} //end if singleton
	} //end Awake()

	private void CheckForCancel() {
		if (GameManager.instance.State_Play || GameManager.instance.State_Paused) {
			GameManager.instance.PauseToggle();
		} else if (StructureGridManager.instance.EditEnabled) {
			StructureGridManager.instance.CancelStructureEdit();
		} else if (Furniture.EditEnabled) {
			Furniture.CancelEdit();
		}
	}

	private void CheckForFinalize() {
		if (StructureGridManager.instance.EditEnabled) {
			StructureGridManager.instance.FinalizeStructureEdit();
		} else if (Furniture.EditEnabled) {
			Furniture.FinalizeEdit();
		}
	}

	public static Vector3 ConvertDirectionToVector3(Directions direction) {
		switch (direction) {
			case Directions.down:
				return Vector3.down;
			case Directions.down_left:
				return Vector3.down + Vector3.left;
			case Directions.down_right:
				return Vector3.down + Vector3.right;
			case Directions.left:
				return Vector3.left;
			case Directions.right:
				return Vector3.right;
			case Directions.up:
				return Vector3.up;
			case Directions.up_left:
				return Vector3.up + Vector3.left;
			case Directions.up_right:
				return Vector3.up + Vector3.right;
			default: //default is none
				return Vector3.zero;
		}
	}

	public void EditHotkey(Transform button_ui) {
		actionToRebind = button_ui.parent.name;
		button_ui.GetChild(0).GetComponent<Text>().text = "...";
	}

	public string GetKeyCodeName(string axisName) {
		string keyCodeName = keyBindings[axisName].ToString();
		keyCodeName = keyCodeName.Replace("Escape", "Esc");
		keyCodeName = keyCodeName.Replace("Alpha", "");
		keyCodeName = keyCodeName.Replace("Mouse0", "LMB");
		keyCodeName = keyCodeName.Replace("Mouse1", "RMB");
		keyCodeName = keyCodeName.Replace("Mouse2", "MMB");
		return keyCodeName;
	}

	private void InitializeControlsUI() {
		uiParent = GameObject.Find("Settings_Controls_ScrollContent").transform;
		uiPrefab = uiParent.Find("Template_Hotkey");

		float scrollViewHeight = uiParent.GetChild(0).GetComponent<RectTransform>().sizeDelta.y; //start with header height
		float prefabHeight = uiPrefab.GetComponent<RectTransform>().sizeDelta.y; //get the prefab height 1 time

		Transform temp;
		foreach (KeyValuePair<string, KeyCode> kvp in keyBindings) { //loop through keybindings
			temp = Instantiate(uiPrefab, uiParent); //instantiate copy of template
			temp.name = kvp.Key;
			temp.GetChild(0).GetComponent<Text>().text = kvp.Key; //set action name
			temp.GetChild(1).GetChild(0).GetComponent<Text>().text = GetKeyCodeName(kvp.Key); //set key name -- get button(child), get text(child of button)
			scrollViewHeight += prefabHeight; //add this height to total
		}

		uiParent.GetComponent<RectTransform>().sizeDelta = new Vector2(uiParent.GetComponent<RectTransform>().sizeDelta.x, scrollViewHeight); //resize the rect so everything fits
		uiParent.localPosition = Vector3.zero; //ensure player starts viewing at the top
		uiParent.parent.gameObject.SetActive(false); //hide from screen

		uiPrefab.gameObject.SetActive(false); //hide prefab now we are done
	}

	private void InitializeDefaultKeyBindings() {
		keyBindings = new Dictionary<string, KeyCode>();
		keyBindings.Add("Submit", KeyCode.Return);
		keyBindings.Add("Cancel", KeyCode.Escape);
		keyBindings.Add("Inventory", KeyCode.Tab);
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
		keyBindings.Add("Quicksave", KeyCode.F5);
		keyBindings.Add("Quickload", KeyCode.F9);
	}

	public void LoadKeyBindings() {
		FileStream file = File.OpenRead(keyBindingsFilePath);
		BinaryFormatter bf = new BinaryFormatter();
		DictionaryS temp = (DictionaryS)bf.Deserialize(file);
		file.Close();

		if (temp != null) {
			keyBindings = temp.GetDictionary();
		}
	}

	private void RefreshControlsUI() {
		Transform temp;
		foreach (KeyValuePair<string, KeyCode> kvp in keyBindings) { //loop through keybindings
			temp = uiParent.Find(kvp.Key);
			if (temp == null) { //ui element not found
				temp = Instantiate(uiPrefab, uiParent); //instantiate copy of template
				uiParent.GetComponent<RectTransform>().sizeDelta += new Vector2(0, uiPrefab.GetComponent<RectTransform>().sizeDelta.y);
			}

			temp.name = kvp.Key;
			temp.GetChild(0).GetComponent<Text>().text = kvp.Key; //set action name
			temp.GetChild(1).GetChild(0).GetComponent<Text>().text = GetKeyCodeName(kvp.Key); //set key name -- get button(child), get text(child of button)

			if (!temp.gameObject.activeSelf) {
				temp.gameObject.SetActive(true);
			}
		}
	}

	public void SaveKeyBindings() {
		FileStream file = File.Create(keyBindingsFilePath);
		BinaryFormatter bf = new BinaryFormatter();
		bf.Serialize(file, new DictionaryS(keyBindings));
		file.Close();
	}

	public void SetDefaultKeyBindings() {
		InitializeDefaultKeyBindings();
		RefreshControlsUI();
	}

	private void Update() {
		if (actionToRebind.Equals("")) { //no key rebinds happening
			if (GameManager.instance.State_Play) {
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

				if (Input.GetKeyDown(keyBindings["Quicksave"])) {
					GameManager.instance.SaveGame();
				}

				if (Input.GetKeyDown(keyBindings["Quickload"])) {
					GameManager.instance.QuickLoadGame();
				}

				for (int slot_index = 0; slot_index < 10; slot_index++) { //check all quick (item/spell) use slots
					if (Input.GetKeyDown(keyBindings["Slot_" + (slot_index + 1)])) {
						HUD.instance.UseHotbarSlot(slot_index);
					}
				}

				if (Input.GetKeyDown(keyBindings["Inventory"])) {
					Inventory.instance.Display();
				}
			} //end if game state play

			if (Input.GetKeyDown(keyBindings["Submit"])) {
				CheckForFinalize();
			}

			if (Input.GetKeyDown(keyBindings["Cancel"])) {
				CheckForCancel();
			}

			if (Input.GetKeyDown(keyBindings["Interact"])) {
				if (GameManager.instance.State_Play) {
					if (!Interactable.Interact()) {
						Inventory.instance.Display();
					}
				} else {
					CheckForFinalize();
				}
			}

			if (Input.GetKeyDown(keyBindings["Attack_Basic"])) {
				if (GameManager.instance.State_Play) {
					//call player attack
				} else {
					CheckForFinalize();
				}
			}

			if (Input.GetKeyDown(keyBindings["Attack_Special"])) {
				if (GameManager.instance.State_Play) { //game is active
					if (MenuScript.instance.CurrentState.Equals("")) { //no menus open
						//call player attack special
					}
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

			if (Input.GetKeyDown(KeyCode.N)) {
				GameManager.instance.StartNewGame(0);
			}
			if (Input.GetKeyDown(KeyCode.Comma)) {
				StructureGridManager.instance.BeginStructureCreate("City_CPR_0");
			}
			if (Input.GetKeyDown(KeyCode.Period)) {
				StructureGridManager.instance.BeginStructureEdit(AreaManager.GetEntityParent("Structure").GetChild(0).GetComponent<Structure>());
			}
			if (Input.GetKeyDown(KeyCode.F)) {
				Furniture.Create("Chest_0", null);
				//Furniture.Create("Chest_0", AreaManagerNS.AreaManager.GetEntityParent("Structure").GetChild(0).GetComponent<Structure>());
			}
#endif
		} else { //player is trying to rebind a key
			if (Input.anyKeyDown) { //if any key was pressed
				for (int kc = 0; kc < keyCodes.Length; kc++) { //loop through all keys
					if (Input.GetKeyDown(keyCodes[kc])) { //determine which key was pressed
						string actionToSwapKeysWith = keyBindings.FirstOrDefault(x => x.Value == keyCodes[kc]).Key; //check whether key is being used already
						if (actionToSwapKeysWith != null && !actionToSwapKeysWith.Equals("")) { //if action name was retrieved using the key
							keyBindings[actionToSwapKeysWith] = keyBindings[actionToRebind]; //assign new keycode
							UpdateUIForAction(actionToSwapKeysWith); //update the ui
						}

						keyBindings[actionToRebind] = keyCodes[kc]; //set the hotkey
						UpdateUIForAction(actionToRebind);
						actionToRebind = "";
						break;
					}
				}
			}
		} //end if editenabled
	}//end Update()

	private void UpdateUIForAction(string actionName) {
		Transform temp = uiParent.Find(actionName);

		if (temp != null) {
			temp.GetChild(1).GetChild(0).GetComponent<Text>().text = GetKeyCodeName(actionName);
		}
	}
}
