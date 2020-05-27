﻿using UnityEngine;
using UnityEngine.UI;

//Written by Justin Ortiz
public class Inventory : Container {
	public static Inventory instance;

	private static Toggle tab;
	private static Transform playerInfo;

	private void Awake() {
		if (instance != null) {
			Destroy(gameObject);
		} else {
			instance = this;
		}
	}

	public override void Display(bool changeState = true) {
		StartCoroutine(RefreshDisplay(tab, changeState));
	}

	protected override void Initialize() {
		playerInfo = GameObject.Find("Inventory_Container_PlayerInfo").transform; //get the player info

		tab = GameObject.Find("Toggle_Inventory_Player").GetComponent<Toggle>(); //get the tab for the player's bag
		tab.onValueChanged.AddListener(OnToggleValueChanged); //ensure pressing the tab marks this container as displayed

		maxWeight = 100.0f;
		instanceID = -777; //no other container will have <1 id
		optout_populateItems = true; //prevents items being populated & auto assign of instanceID
		DisableInteraction(); //ensure the player doesn't interact with their own inventory
		base.Initialize(); //initialize base container attributes
	}

	private void OnDestroy() {
		SelfDestruct(destroySelf: false);
		tab.onValueChanged.RemoveAllListeners();
	}

	protected override void RefreshWeight() {
		base.RefreshWeight(); //ensure the weight is accurate
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
