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
		private List<string> routine; //"methodName(parameter);"
		private Transform home; //store transform so position will update if structure moved

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
			maxHp = 10;
			maxStamina = 10;
			base_physical_attack = 3;
			base_magic_attack = 0;

			int scaleFactor = Mathf.Max(AreaManager.instance.Position.x, AreaManager.instance.Position.y);
			if (scaleFactor > 0) {
				maxHp += 2 * scaleFactor; //health formula becomes: 10 + (2 * [1,11])
				maxStamina += 4 * scaleFactor;
			}

			GenerateRoutine();

			base.Initialize();
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
