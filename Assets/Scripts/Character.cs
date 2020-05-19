using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

/// <summary>
/// The base class for all characters (npc & player)
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class Character : MonoBehaviour {
	private static CharacterEvent defeated;

	protected static float base_stamina_regen = 0.1f; //qty per second format

	protected int hp;
	protected int maxHp;
	protected float stamina;
	protected int maxStamina;
	protected float walkSpeed;
	protected float sprintSpeed;
	protected int base_physical_attack;
	protected int base_physical_resistance;
	protected int base_magic_attack;
	protected int base_magic_resistance;
	
	//status
	protected bool status_normal;
	protected bool status_sprinting;
	protected bool status_inCombat;
	private float invulnerableTimer;
	private float flinchTimer;
	private UnityEvent onHit;

	private Faction faction;

	[SerializeField]
	private Animator animator;

	//navigation
	protected NavMeshAgent navAgent;
	private Directions lookDirection;

	public static CharacterEvent Defeated { get { return defeated; } }

	public int HP { get { return hp; } }
	public int MaxHP { get { return maxHp; } }
	public float Stamina { get { return stamina; } }
	public float MaxStamina { get { return maxStamina; } }
	public Directions LookDirection { get { return lookDirection; } }
	public UnityEvent OnHit { get { return onHit; } }

	public virtual bool Attack() {
		return true;
	}

	private void Awake() {
		Initialize();
	}

	private void FixedUpdate() {
		Update_Animations();
	}

	public virtual int GetStat_MagicResistance() {
		return base_magic_resistance;
	}

	public virtual int GetStat_PhysicalResistance() {
		return base_physical_resistance;
	}

	protected virtual float GetStat_StaminaRegen() {
		return base_stamina_regen;
	}

	protected virtual void Initialize() {
		if (defeated == null) {
			defeated = new CharacterEvent();
		}
		onHit = new UnityEvent();

		if (hp <= 0) {
			hp = maxHp;
		}
		if (stamina <= 0) {
			stamina = maxStamina;
		}

		if (animator == null) { //if animator not established in inspector
			animator = GetComponent<Animator>(); //try to find on gameobject

			if (animator == null) { //if not on gameobject
				if (transform.childCount > 0) { //try to find on first child
					animator = transform.GetChild(0).GetComponent<Animator>();
				}
			}
		}

		navAgent = GetComponent<NavMeshAgent>();

		invulnerableTimer = 0;
		flinchTimer = 0;
	}

	public void MoveDirection(Directions direction, bool sprintEnabled) { //mainly used by player
		if (status_sprinting != sprintEnabled) {
			ToggleSprint(); //toggle sprint when necessary/possible
		}

		if (direction != Directions.none) {
			if (lookDirection != direction) {
				lookDirection = direction;
			}
		}

		navAgent.SetDestination(transform.position + InputManager.ConvertDirectionToVector3(direction));
	}

	protected virtual void ReceiveHit(int physicalDamage, int magicalDamage, bool trueDamage = false) {
		int totalDamage = 0;
		if (physicalDamage > 0 || magicalDamage > 0) { //if given damage to work with
			if (!trueDamage) { //if resistances will be taken into account
				physicalDamage -= GetStat_PhysicalResistance(); //reduce physical by resistance
				magicalDamage -= GetStat_MagicResistance(); //reduce magical by resistance
			}
			physicalDamage = Mathf.Clamp(physicalDamage, 0, int.MaxValue); //clamp values
			magicalDamage = Mathf.Clamp(magicalDamage, 0, int.MaxValue);
			totalDamage = physicalDamage + magicalDamage; //get total
		}
		totalDamage = totalDamage <= 0 ? 1 : totalDamage; //enforce dmg minimum of 1
		hp -= totalDamage;

		if (hp <= 0) {
			defeated.Invoke(this);
			gameObject.SetActive(false);
		} else {
			flinchTimer = 0.2f;
			onHit.Invoke();
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

	protected void ToggleSprint() {
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
		Update_Character();
	}

	protected virtual void Update_Animations() {

	}

	protected virtual void Update_Character() {
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
					stamina += Time.deltaTime * GetStat_StaminaRegen();
				}

				if (stamina > maxStamina) {
					stamina = maxStamina;
				}
			}
		}
	}
}
