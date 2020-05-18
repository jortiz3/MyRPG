using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Notes:
/// -find a way to tie in OnActionComplete()
/// Written by Justin Ortiz
/// </summary>
namespace NPC {
	public class NonPlayerCharacter : Character {
		private static float base_action_delay = 0.2f;

		private NPCType type;
		private int scaleRating;
		private string routine_current; //contains all methods to go through
		private string routine_action; //current method
		private int routine_action_index; //index within the current routine
		private bool routine_action_invoked;
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

		private void GetRoutine(int time) {
			if (type != null) {
				string[] routines = type.Routine.Split(' '); //all routines for this npc
				string[] routine_info_loop; //routine currently being checked within loop
				int time_parsed; //the time for current parse
				for (int i = routines.Length - 1; 0 <= i; i--) { //go in reverse order to always get the last closest routine to given time
					routine_info_loop = routines[i].Split(';'); //separate routine methods & time
					if (int.TryParse(routine_info_loop[0], out time_parsed)) { //if time retrieved from string
						if (time >= time_parsed) { //if the checked routine is acceptable for the time
							if (!routine_current.Equals(routines[i])) { //if the current routine is not the one being checked
								routine_current = routines[i]; //set the new current routine
								routine_action_index = 0;
							}
							break;
						}
					}
				}
			}
		}

		private void GoTo(string locationInfo) {
			Vector3 pos = Vector3.zero;
			if (locationInfo.Equals("home")) {

			} else if (locationInfo.Equals("bed")) {

			} else if (locationInfo.Equals("friend")) {

			} else {
				pos = Vector2IntS.Parse(locationInfo).ToVector3();
			}
			navAgent.SetDestination(pos);
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

			WorldManager.OnHour.AddListener(GetRoutine);
			base.Initialize();
		}

		private void InvokeNextAction() {
			string[] actions = routine_current.Split(';');
			if (routine_action_index + 1 < actions.Length) {
				routine_action = actions[++routine_action_index];
			}

			if (!routine_action.Equals("")) { //if there is no action currently assigned
				string[] methodInfo = routine_action.Replace(")", "").Split('('); //deal with parenthesis to get method info
				if (methodInfo.Length == 2) { //method info needs exactly method name and parameters
					typeof(NonPlayerCharacter).GetMethod(methodInfo[0]).Invoke(this, BindingFlags.NonPublic | BindingFlags.Instance, null, methodInfo[1].Split(','), null);
				}
			}
			routine_action_invoked = true;
		}

		public void Load(string Name = "", string npcTypeName = "default", int hp_current = 0, float stamina_current = 0) {
			gameObject.name = Name;
			AssignType(npcTypeName);
			hp = hp_current;
			stamina = stamina_current;
		}

		private void OnActionComplete() {
			time_delay_remaining = GetDelay();
			routine_action_invoked = false;
		}

		private void Update() {
			if (status_normal) { //if not flinching, not attacking/blocking
				if (time_delay_remaining > 0) { //if being delayed
					time_delay_remaining -= Time.deltaTime; //update delay remaining
				} else { //no longer delayed
					if (!status_inCombat) { //if not in combat
						if (!routine_action_invoked) {
							InvokeNextAction();
						}
					} else {
						//do combat
					}
				}
			} //end if status
		}
	}
}
