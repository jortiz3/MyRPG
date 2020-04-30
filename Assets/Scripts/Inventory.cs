using UnityEngine;
using UnityEngine.UI;

//Written by Justin Ortiz
public class Inventory : Container {
	public static Inventory instance;

	private static Transform playerInfo;

	private void Awake() {
		if (instance != null) {
			Destroy(gameObject);
		} else {
			instance = this;
			gameObject.tag = "inventory";
		}
	}

	public override void Display() {
		SetContainerActive(false); //since this container will not be opened via interacting, we must hide the other container on display
		base.Display(); //otherwise, display like normal container
	}

	public void Drop(Item item) {
		if (nonplayerContainer != null) {
			Transfer(item, nonplayerContainer);
		} else {
			item.transform.position = Player.instance.transform.position + InputManager.ConvertDirectionToVector3(Player.instance.LookDirection);
			item.ContainerID = 0;
			item.SetInteractionActive();
		}
	}

	protected override void Initialize() {
		currDisplayParent = GameObject.Find("Inventory_Container_Player_Content").transform; //get new parent
		playerInfo = GameObject.Find("Inventory_Player_Info").transform; //get the player info
		maxWeight = 100.0f;
		instanceID = -777; //no other container will have <1 id
		optout_populateItems = true; //prevents items being populated & auto assign of instanceID
		DisableInteraction(); //ensure the player doesn't interact with their own inventory
		base.Initialize(); //initialize base container attributes
	}

	protected override void RefreshWeightElement() {
		playerInfo.Find("Inventory_Player_TotalWeight").GetComponent<Text>().text = "Weight: " + TotalWeight + "/" + MaxWeight+ " kg"; //display the total weight
	}

	public override void SelfDestruct(bool destroyItems = true, bool destroySelf = false) {
		if (!GameManager.instance.State_Play) { //if the call to self destruct isn't during gameplay (i.e. loading areas)
			base.SelfDestruct(destroyItems, destroySelf);
		}
	}

	protected override void UseItem(Item item) { //item used in player's inventory
		item.Use(); //use the item
		if (item.Quantity <= 0) { //if quantity has dropped to 0
			Remove(item); //remove item from this container
			Destroy(item.gameObject); //destroy from scene
		}
	}
}
