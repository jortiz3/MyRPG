﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Character : MonoBehaviour {
	private int instanceID; //is this necessary for item load?

	//attributes
	private int hp;
	[SerializeField]
	private int maxHp;
	private float stamina;
	[SerializeField]
	private int maxStamina;
	[SerializeField]
	private float walkSpeed;
	[SerializeField]
	private float sprintSpeed;
	protected int base_resistance_physical;
	protected int base_resistance_magic;

	//status
	private bool status_normal;
	private bool status_sprinting;
	private bool status_inCombat;
	private float invulnerableTimer;
	private float flinchTimer;

	private Faction faction;

	[SerializeField]
	private Animator animator;

	//navigation
	private NavMeshAgent navAgent;
	protected Vector3 moveDestination;
	protected List<Vector3> routineDestinations;
	protected int routineDestinationIndex;
	private Directions lookDirection;

	private delegate bool Action();
	private List<Action> routine;

	public int HP { get { return hp; } set { hp = value; } }
	public int MaxHP { get { return maxHp; } set { maxHp = value; } }
	public Directions LookDirection { get { return lookDirection; } }

	public virtual bool Attack() {
		return true;
	}

	private void Awake() {
		Initialize();
	}

	public void Equip(Item item) {

	}

	private void FixedUpdate() {
		//to do: update animator
	}

	public virtual int GetMagicResistance() {
		return base_resistance_magic;
	}

	protected bool GetNextRoutineDestination() {
		if (routineDestinations != null) { //if the list is instantiated
			if (routineDestinations.Count > 0) { //if the list has 1 or more locations
				if (routineDestinationIndex < routineDestinations.Count) { //if the current index is within bounds
					moveDestination = routineDestinations[routineDestinationIndex];
					routineDestinationIndex++;
				} else if (routineDestinationIndex >= routineDestinations.Count) {
					routineDestinationIndex = 0;
					return GetNextRoutineDestination();
				}
			}
		}
		return true; //always return true
	}

	public virtual int GetPhysicalResistance() {
		return base_resistance_physical;
	}

	protected virtual void Initialize() {
		hp = maxHp;
		stamina = maxStamina;

		if (animator == null) { //if animator not established in inspector
			animator = GetComponent<Animator>(); //try to find on gameobject

			if (animator == null) { //if not on gameobject
				if (transform.childCount > 0) { //try to find on first child
					animator = transform.GetChild(0).GetComponent<Animator>();
				}
			}
		}

		navAgent = GetComponent<NavMeshAgent>();
		routine = new List<Action>();
		routineDestinationIndex = 0;

		invulnerableTimer = 0;
		flinchTimer = 0;
	}

	protected bool MoveNav() {
		navAgent.SetDestination(moveDestination);

		float distToDest = Vector3.Distance(moveDestination, transform.position);
		if (distToDest < navAgent.stoppingDistance) {
			return true;
		}

		if (status_inCombat) { //if in combat
			if (distToDest < navAgent.stoppingDistance * 10) { //close enough to walk
				if (status_sprinting) { //if sprinting
					ToggleSprint();
				}
			} else { //too far away
				if (!status_sprinting) {
					ToggleSprint();
				}
			}
		} else { //not in combat
			if (status_sprinting) { //if sprinting
				ToggleSprint(); //toggle to walking
			}
		}
		return false;
	}

	public void MoveDirection(Directions direction, bool sprintEnabled) {
		if (status_sprinting != sprintEnabled) {
			ToggleSprint(); //toggle sprint when necessary/possible
		}

		moveDestination = transform.position + InputManager.ConvertDirectionToVector3(direction); //set the destination

		if (direction != Directions.none) {
			if (lookDirection != direction) {
				lookDirection = direction;
			}
		}

		if (routine != null) {
			bool insertMoveAction = true;
			if (routine.Count > 0) {
				if (routine[0].Method.Name.Equals("MoveNav")) {
					insertMoveAction = false;
				}
			}

			if (insertMoveAction) {
				routine.Insert(0, MoveNav);
			}
		}
	}

	private void Start() {
		transform.SetParent(AreaManager.GetEntityParent("Character"));
	}

	protected IEnumerator Teleport(Vector3 position, bool refocusCamera = false) {
		navAgent.enabled = false;
		transform.position = position;
		yield return new WaitForEndOfFrame();
		navAgent.enabled = true;
		if (refocusCamera) {
			CameraManager.instance.RefocusOnTarget();
		}
	}

	public virtual void TeleportToPos(Vector3 position) {
		StartCoroutine(Teleport(position));
	}

	private void ToggleSprint() {
		if (status_sprinting) {
			navAgent.speed = walkSpeed;
			status_sprinting = false;
		} else {
			if (stamina > 1) {
				navAgent.speed = sprintSpeed;
				status_sprinting = true;
			}
		}
	}

	private void Update() {
		if (GameManager.instance.State_Play) { //only update when playing
			status_normal = true;

			if (flinchTimer > 0) {
				flinchTimer -= Time.deltaTime;
				status_normal = false;
			}

			if (status_sprinting) { //if sprinting
				if (stamina > 0) { //if there is stamina
					stamina -= Time.deltaTime * 0.5f; //reduce stamina
				} else {
					navAgent.speed = walkSpeed; //start walking
					status_sprinting = false; //update status
				}
			} else {
				if (stamina < maxStamina) {
					stamina += Time.deltaTime * 0.1f;
				}

				if (stamina > maxStamina) {
					stamina = maxStamina;
				}
			}

			//check animator for states

			if (status_normal) { //if not flinching, not attacking/blocking
				if (routine != null) { //if list of actions is instantiated
					if (routine.Count > 0) { //if there is an action left to do
						if (routine[0].Invoke()) { //call the action; if action is considered complete
							routine.RemoveAt(0); //remove the action from the list
						} //end if invoke
					} //end if count
				} //end if null
			} //end if status
		}
	}
}
