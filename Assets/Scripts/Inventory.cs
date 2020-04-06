using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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

	protected override void Initialize() {
		DisableInteraction(); //ensure the player doesn't interact with their own inventory
		base.Initialize(); //base sets currparent
		currParent = GameObject.Find("Inventory_Container_Player_Content").transform; //get new parent
		playerInfo = GameObject.Find("Inventory_Player_Info").transform; //get the player info
	}

	protected override void RefreshWeightElement() {
		playerInfo.Find("Inventory_Player_TotalWeight").GetComponent<Text>().text = "Weight: " + TotalWeight + "/100 kg"; //display the total weight
	}

	public override void SelfDestruct() {
		if (!GameManager.instance.State_Play) { //if the call to self destruct isn't during gameplay (i.e. loading areas)
			base.SelfDestruct();
		}
	}
}
