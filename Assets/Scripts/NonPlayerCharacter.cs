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

		private List<string> routine; //"time;methodName(parameter);methodName(parameter); time;methodName(parameter);..."
		private Transform home; //store transform so position will update if structure moved
		private NPCType type;
		private int scaleRating;
		private float time_delay_remaining;

		public string Type { get { return type.Name; } }

		private void AssignType(string typeName) {
			type = NPCDatabase.GetType(typeName);
			if (type != null) {
				maxHp = type.base_hp;
				maxStamina = type.base_stamina;
				base_magic_attack = type.base_magic_attack;
				base_magic_resistance = type.base_magic_resistance;
				base_physical_attack = type.base_physical_attack;
				base_physical_resistance = type.base_physical_resistance;

				ResetRoutine();
			}
		}

		private float GetDelay() {
			float delay = base_action_delay;
			if (GameManager.instance.Difficulty <= 0) {
				delay += 1f;
			} else if (GameManager.instance.Difficulty <= 1) {
				delay += 0.6f;
			}
			return delay;
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

			scaleRating = Mathf.Max(AreaManager.instance.Position.x, AreaManager.instance.Position.y); //enemies become stronger as player goes southeast
			if (scaleRating > 0) {
				maxHp += 5 * scaleRating; //health formula becomes: 10 + (x * [1,11])
				maxStamina += 3 * scaleRating;
				base_magic_attack += 2 * scaleRating;
				base_magic_resistance += (int)(1.2f * scaleRating);
				base_physical_attack += 2 * scaleRating;
				base_physical_resistance += (int)(1.5f * scaleRating);
			}

			base.Initialize();
		}

		public void Load(string Name = "", string npcTypeName = "default", int hp_current = 0, float stamina_current = 0) {
			gameObject.name = Name;
			AssignType(npcTypeName);
			hp = hp_current;
			stamina = stamina_current;
		}

		private void ResetRoutine() {
			if (type != null) {
				if (routine == null) {
					routine = new List<string>();
				} else {
					routine.Clear();
				}
				routine.AddRange(type.Routine.Split(' '));
			}
		}

		private void Update() {
			if (status_normal) { //if not flinching, not attacking/blocking
				if (time_delay_remaining > 0) { //if being delayed
					time_delay_remaining -= Time.deltaTime; //update delay remaining
				} else { //no longer delayed
					if (!status_inCombat) { //if not in combat
											//do routine
					} else {
						//do combat
					}
				}
			} //end if status
		}

		private void UpdateRoutine(int time) {

		}
	}
}
