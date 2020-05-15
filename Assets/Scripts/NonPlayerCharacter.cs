using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Notes:
/// -use reflection to iterate through current routine string?
/// Written by Justin Ortiz
/// </summary>
namespace NPC {
	public class NonPlayerCharacter : Character {
		private static float base_action_delay = 0.2f;

		private List<string> routine; //"methodName(parameter);"
		private Transform home; //store transform so position will update if structure moved
		private NPCType type;

		private void AssignType(string typeName) {
			type = NPCDatabase.GetType(typeName);
			if (type != null) {
				maxHp = type.base_hp;
				maxStamina = type.base_stamina;
				base_magic_attack = type.base_magic_attack;
				base_magic_resistance = type.base_magic_resistance;
				base_physical_attack = type.base_physical_attack;
				base_physical_resistance = type.base_physical_resistance;
			}
		}

		private void GenerateRoutine() {

		}

		private void GetNextAction() {
			//split curr routine ';'
			//typeof(NonPlayerCharacter).GetMethod("GoTo").Invoke(this, BindingFlags.NonPublic | BindingFlags.Instance, null, new string[] { "" }, null);
		}

		private void GoTo(string locationInfo) {
			//see if locationInfo is a character
			//see if named location; i.e. home,
			//vector2ints.parse().tovector3?
		}

		protected override void Initialize() {
			if (type == null) { //if no type assigned
				AssignType("default");
			}

			int scaleFactor = Mathf.Max(AreaManager.instance.Position.x, AreaManager.instance.Position.y); //enemies become stronger as player goes southeast
			if (scaleFactor > 0) {
				maxHp += 5 * scaleFactor; //health formula becomes: 10 + (x * [1,11])
				maxStamina += 3 * scaleFactor;
				base_magic_attack += 2 * scaleFactor;
				base_magic_resistance += (int)(1.2f * scaleFactor);
				base_physical_attack += 2 * scaleFactor;
				base_physical_resistance += (int)(1.5f * scaleFactor);
			}

			base.Initialize();
		}

		public void Load(string Name, string npcTypeName) {
			gameObject.name = Name;
			AssignType(npcTypeName);
		}

		private void Update() {
			if (status_normal) { //if not flinching, not attacking/blocking
				if (!status_inCombat) { //if not in combat
					//do routine
				} else {
					//do combat
				}
			} //end if status
		}
	}
}
