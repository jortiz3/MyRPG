using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
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
		private float time_delay_remaining; //amount of time to wait (give players time to react)
		private bool action_complete; //flag to know when to start next action
		private bool routine_complete; //flag to know when current routine is done
		private Structure home;
		private int instanceID_home;

		public string Type { get { return type != null ? type.Name : "null"; } }
		public int InstanceID_Home { get { return instanceID_home; } }

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

		private void ClearDelay() {
			time_delay_remaining = 0;
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
				string routine_selected = "";
				int time_parsed; //the time for current parse
				for (int i = routines.Length - 1; 0 <= i; i--) { //go in reverse order to always get the last closest routine to given time
					routine_info_loop = routines[i].Split(';'); //separate routine methods & time
					if (int.TryParse(routine_info_loop[0], out time_parsed)) { //if time retrieved from string
						if (time >= time_parsed) { //if the checked routine is acceptable for the time || nothing was accepted
							routine_selected = routines[i];
							break;
						}
					}
				}

				if (routine_selected.Equals("")) { //if no suitable routine was found
					if (routines.Length > 0) { //double-check routines available
						routine_selected = routines[routines.Length - 1]; //select the last routine
					}
				}

				if (!routine_selected.Equals("")) { //if suitable routine found
					if (routine_current == null || !routine_current.Equals(routine_selected)) { //if selected is not the same as current
						routine_current = routine_selected; //set the new current routine
						routine_action_index = 0; //reset current action in current routine
						action_complete = true;
						routine_complete = false;
					}
				}
			}
		}

		private void GoTo(string locationInfo) {
			Vector3 pos = Vector3.zero;
			if (locationInfo.Equals("home")) {
				if (home != null) {
					pos = home.transform.position;
				}
			} else if (locationInfo.Equals("bed")) {
				if (home != null) {
					//find unoccupied bed
				}
			} else if (locationInfo.Equals("friend")) {
				//have list of friends
				//pick random friend
				//go to friend's house
			} else if (locationInfo.Equals("inn")) {
				//find an available inn
			}
			navAgent.isStopped = false;
			navAgent.ResetPath();
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

			if (0 < instanceID_home) {
				home = Structure.GetStructure(instanceID_home);

				if (home != null) {
					home.SetOwner(gameObject.name);
				}
			}
			GetRoutine(WorldManager.instance.GetCurrentHour());
			WorldManager.OnHour.AddListener(GetRoutine);
			base.Initialize();
			OnHit.AddListener(ClearDelay);
		}

		private void InvokeNextRoutineAction() {
			if (!routine_current.Equals("")) {
				string[] actions = routine_current.Split(';');
				if (routine_action_index + 1 < actions.Length) {
					routine_action = actions[++routine_action_index];

					if (!routine_action.Equals("")) { //if there is no action currently assigned
						string[] methodInfo = routine_action.Replace(")", "").Split('('); //deal with parenthesis to get method info
						if (methodInfo.Length == 2) { //method info needs exactly method name and parameters
							try {
								GetType().GetMethod(methodInfo[0], BindingFlags.NonPublic | BindingFlags.Instance, null,
									new Type[] { typeof(string) }, null).Invoke(this, methodInfo[1].Split(',')); //find the method in the class, then  invoke it
							} catch {
								Debug.Log("Invoke Error: Failed to call 'NonPlayerCharacter." + methodInfo[0] + "(" + methodInfo[1] + ");'");
							}
						}
					}
					action_complete = false;
				} else {
					routine_complete = true;
				}
			}
		}

		public void Load(string Name = "", string npcTypeName = "default", int HomeID = 0, int hp_current = 0, float stamina_current = 0) {
			gameObject.name = Name.Equals("") ? NPCDatabase.GenerateName() : Name;
			AssignType(npcTypeName);
			hp = hp_current;
			stamina = stamina_current;
			instanceID_home = HomeID; //somehow value is changed to 0 after this point
		}

		private void OnActionComplete() {
			time_delay_remaining = GetDelay();
			action_complete = true;
			navAgent.isStopped = true;
			Debug.Log("action complete");
		}

		protected override void Update_Character() {
			if (status_normal) { //if not flinching, not attacking/blocking
				if (time_delay_remaining > 0) { //if being delayed
					time_delay_remaining -= Time.deltaTime; //update delay remaining
				} else { //no longer delayed
					if (action_complete) { //if either attack was complete or destination reached
						if (!status_inCombat) { //if not in combat
							if (!routine_complete) { //if not done with current routine
								InvokeNextRoutineAction(); //start next routine action
							}
						} else { //in combat
								 //do combat things
						}
					}
				}

				if (!action_complete) {
					//if attack animation complete >> animation state attack rest?
					if (navAgent.remainingDistance <= navAgent.stoppingDistance) {
						OnActionComplete();
					}
				}
			} //end if status
			base.Update_Character();
		}
	}
}
