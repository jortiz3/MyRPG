﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using internal_Items;

/// <summary>
/// Stores a list of items and displays them to the screen. Written by Justin Ortiz
/// </summary>
public class Container : Interactable {
	protected static Container nonplayerContainer; //container the player is currently interacting with
	protected static Container displayedContainer;
	protected static Toggle currTab;
	private static int numContainersToPopulate;

	private static Toggle npcTab; //non-player container tab
	private static Text npcTabText; //text displayed on non-player container tab -- to display the name of container
	private static Transform containerElementPrefab;
	private static int nextInstanceID;

	protected List<Item> items;
	private float totalWeight;
	protected float maxWeight;
	protected int instanceID; //never 0 or less than
	protected bool optout_populateItems;
	protected Transform displayParent; //the parent for this container's ui elements

	public static bool PopulationInProgress { get { return numContainersToPopulate > 0; } }
	public float TotalWeight { get { return totalWeight; } }
	public float MaxWeight { get { return maxWeight; } }
	public int InstanceID { get { return instanceID; } }

	public bool Add(Item item) {
		if (item != null) { //if given valid reference
			if (!Contains(item)) { //if the same item (in memory) is being stored
				if (GameManager.instance.ElapsedGameTime - lastUpdated > 2 || item.LastUpdated >= lastUpdated || optout_populateItems) { //if container not updated recently OR updated recently  OR does not populate items automatically
					if (totalWeight + item.GetWeight() <= maxWeight) { //if weight remains in limits with adding new item's weight
						int itemIndex = IndexOf(item); //attempt to get index of same item (database-wise) inside this container
						if (itemIndex >= 0) { //container has some of this item already
							items[itemIndex].Quantity += item.Quantity; //increase the quantity of the item already in container
							Destroy(item.gameObject); //remove item from scene
							items[itemIndex].LastUpdated = GameManager.instance.ElapsedGameTime;
						} else { //first time this item is added
							items.Add(item); //store item in list
							item.ContainerID = instanceID; //ensure the item knows which container it is in
							item.LastUpdated = GameManager.instance.ElapsedGameTime;
						}
						return true;
					}
				} else { //container just updated, item is old
					Destroy(item.gameObject); //destroy old item
				}
			}
		}
		return false;
	}

	public bool Contains(Item item) {
		for (int i = 0; i < items.Count; i++) {
			if (items[i].Equals(item)) { //if they are the same object in memory
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Creates a UI element or finds an existing element for an item.
	/// </summary>
	/// <param name="i">The item to be added to or found in the UI.</param>
	/// <returns>Reference to the created or found transform.</returns>
	private Transform CreateContainerElement(Item i) {
		string elementName = GetElementName(i);
		Transform temp = displayParent.Find(elementName);
		if (temp == null) {
			temp = Instantiate(containerElementPrefab, displayParent).transform;
			temp.name = elementName;
		}
		return temp;
	}

	private bool DeleteContainerElement(Item i) {
		Transform temp = displayParent.Find(i.ToString());
		if (temp != null) {
			Destroy(temp.gameObject);
			return true;
		}
		return false;
	}

	public virtual void Display(bool changeState = true) {
		StartCoroutine(RefreshDisplay(npcTab, changeState));
	}

	protected virtual void Drop(Item item, int quantity) {
		if (item != null) {
			Vector3 dropPosition = Player.instance.transform.position + InputManager.ConvertDirectionToVector3(Player.instance.LookDirection);
			if (item.Quantity - quantity > 0) { //if dropping less than max quantity
				item.Quantity -= quantity; //update item quantity
				AssetManager.instance.InstantiateItem(dropPosition, item.ID, 0, item.Prefix, item.BaseName, item.Suffix, quantity, item.GetTextureName(), GameManager.instance.ElapsedGameTime); //drop the provided quantity
				RefreshUIElement(item); //update the quantity in UI
			} else {
				item.transform.position = dropPosition; //update item position
				item.ContainerID = 0; //remove link to this container
				item.SetActive(); //show the item to the player
				DeleteContainerElement(item); //remove the ui element
			}
		}
	}

	public static void Drop(string itemFullName, int quantity) {
		if (displayedContainer != null) {
			displayedContainer.Drop(displayedContainer.GetItem(itemFullName), quantity);
		}
	}

	public static Container GetContainer(int instanceID) {
		if (instanceID == Inventory.instance.instanceID) {
			return Inventory.instance;
		} else if (instanceID >= 0) {
			Transform furnitureParent = AreaManager.GetEntityParent("furniture");
			string[] currNameInfo;
			foreach (Transform child in furnitureParent) {
				if (child.name.Contains("container")) {
					currNameInfo = child.name.Split('_');
					if (currNameInfo[currNameInfo.Length - 1].Equals(instanceID.ToString())) {
						return child.GetComponent<Container>();
					}
				}
			}
		}
		return null;
	}

	public static Item GetDisplayedItem(string elementName) {
		if (displayedContainer != null) {
			elementName = elementName.Split('_')[0];
			return displayedContainer.GetItem(elementName);
		}
		return null;
	}

	private string GetElementName(Item item) {
		return item.ToString() + "_" + instanceID;
	}

	public Item GetItem(string itemFullName) {
		for (int i = 0; i < items.Count; i++) {
			if (items[i].ToString().Equals(itemFullName)) {
				return items[i];
			}
		}
		return null;
	}

	protected int GetNextInstanceID() {
		return ++nextInstanceID; //increment id then return it
	}

	public static bool GetTransferAvailable() {
		return nonplayerContainer != null ? true : false; //if interacting with container, return true
	}

	private int IndexOf(Item item) {
		for (int i = 0; i < items.Count; i++) {
			if (items[i].Equals(item, false)) { //if they are the same item within the database
				return i;
			}
		}
		return -1;
	}

	protected override void Initialize() {
		if (npcTab == null) {
			containerElementPrefab = GameObject.Find("Inventory_Container_Element_Prefab").transform; //update object name
			containerElementPrefab.gameObject.SetActive(false);

			npcTab = GameObject.Find("Toggle_Inventory_Other").GetComponent<Toggle>();
			npcTabText = npcTab.transform.Find("Label_Toggle_Inventory_Other").GetComponent<Text>();
		}

		if (displayParent == null) {
			displayParent = GameObject.Find("Inventory_Container_Other_Content").transform;
		}

		if (items == null) {
			items = new List<Item>();
			totalWeight = 0;
		}

		if (maxWeight <= 0) {
			maxWeight = 1000.0f;
		}

		if (!optout_populateItems) { //only player inventory opts out of autopopulating
			if (instanceID <= 0) {
				instanceID = GetNextInstanceID();
				StartCoroutine(Populate());
			}
		}

		gameObject.name += "_container_" + instanceID.ToString();

		base.Initialize();
	}

	protected override void InteractInternal() { //player walks up to interact
		Display(); //display this
		base.InteractInternal(); //finish interaction
	}

	public static bool Item_Equip(string itemFullName) {
		if (displayedContainer != null) { //ensure a container is displayed
			Item item = displayedContainer.GetItem(itemFullName); //get the item from displayed container
			if (item.Equipable) {
				if (displayedContainer != Inventory.instance) { //if looking at npc container
					if (!displayedContainer.Transfer(item, item.Quantity, Inventory.instance)) { //item must be in player's inventory to equip, so try transfer
						return false; //if wasn't able to transfer, return failed equip
					}
				}
				Player.instance.Equip(item); //equip the item to the player
				return true; //return successful equip
			}
		}
		return false; //return failed equip
	}

	public static bool Item_Use(string itemFullName) {
		Item item = displayedContainer.GetItem(itemFullName); //retrieve the item from displayed container
		if (item != null) { //if retrieved
			item.Use(); //use the item -- potentially affecting quantity or other stats
			displayedContainer.RefreshUIElement(item); //refresh the display for the item
			return true;
		}
		return false;
	}

	public void Load(int InstanceID = 0, string Owner = "", float LastUpdated = -float.MaxValue, Texture2D Texture = null) {
		owner = Owner;
		lastUpdated = LastUpdated;

		if (InstanceID > 0) {
			instanceID = InstanceID;
			if (instanceID >= nextInstanceID) {
				nextInstanceID = instanceID; //ensure current instanceIDs do not get overwritten
			}

			if (!owner.Equals("Player")) { //if not player's chest
				if (Mathf.Abs(GameManager.instance.ElapsedGameTime - lastUpdated) > 600) { //10 mins since last update?
					StartCoroutine(Populate()); //repopulate chest
				}
			}
		}

		SetSprite(Texture);

		Furniture furniture = GetComponent<Furniture>(); //check for furniture component
		if (furniture != null) { //if component retrieved
			furniture.Load(Owner: owner, LastUpdated: LastUpdated); //load necessary aspects
		}
	}

	protected IEnumerator Populate() {
		numContainersToPopulate++;

		if (!StructureGridManager.instance.GridInitialized || StructureGridManager.instance.RegisteringStructures) {
			yield return new WaitForEndOfFrame();
		}

		SelfDestruct(destroySelf: false); //ensure there are no items in the container

		lastUpdated = GameManager.instance.ElapsedGameTime; //mark this moment as the point of updating

		int numItems = UnityEngine.Random.Range(5, 15); //determine how many times items will be instantiated into the chest
		ItemInfo[] dropTable = ItemDatabase.GetDropTable(transform.name.Split('_')); //determine which items can be spawned in this container -- determined by texture name (i.e. "chest_default")
		if (dropTable != null && dropTable.Length > 0) {
			int dropTableIndex; //the currently selected item to add
			for (int i = 0; i < numItems; i++) {
				dropTableIndex = UnityEngine.Random.Range(0, dropTable.Length); //get one of the items from the drop table
				AssetManager.instance.InstantiateItem(position: transform.position, itemID: dropTable[dropTableIndex].id,
					containerID: this.instanceID, itemPrefix: dropTable[dropTableIndex].prefix, quantity: 1, itemSuffix: dropTable[dropTableIndex].suffix,
					textureName: dropTable[dropTableIndex].texture_default, lastUpdated: lastUpdated); //instantiate the item -- the item will add itself to this container
			}
		}

		numContainersToPopulate--;
	}

	/// <summary>
	/// Iterates through all elements available for containers in the scene and only shows the elements for this container.
	/// </summary>
	protected IEnumerator RefreshDisplay(Toggle tab, bool changeState) {
		if (changeState) { //only toggle state when necessary
			MenuScript.instance.ChangeState("Inventory"); //try to change to inventory state
			HUD.instance.RefreshSettings();
		}

		if (!MenuScript.instance.CurrentState.Equals("")) { //if successfully changed
			if (changeState) { //only change view of container when necessary
				if (tab != currTab) { //if a different display is being refreshed
					currTab = tab; //set the new current display parent

					bool npcActive = false; //default to false
					if (currTab == npcTab) { //if switching to non-player container
						npcActive = true; //mark true
					}

					npcTab.gameObject.SetActive(npcActive); //show/hide non-player container tab
					npcTabText.text = npcActive ? CustomFormatting.Capitalize(gameObject.name.Split('_')[0]) : "null"; //show the name of the container
					nonplayerContainer = npcActive ? this : null; //if the container is active, then we need to assign this container to static reference
					currTab.isOn = false;
				}
			}

			foreach (Transform child in displayParent) { //go through all children
				child.gameObject.SetActive(false); //hide them
			}
			yield return new WaitForEndOfFrame(); //pause

			totalWeight = 0;
			for (int i = 0; i < items.Count; i++) { //loop through all items
				RefreshUIElement(items[i]); //refresh the ui element for the item
				totalWeight += items[i].GetWeight(); //increase total weight
				yield return new WaitForEndOfFrame(); //wait a frame
			}
			RefreshWeightElement(); //display the total weight

			//resize the container slide area
			float elementHeight = containerElementPrefab.GetComponent<RectTransform>().sizeDelta.y; //get element height
			RectTransform currParentRect = displayParent.GetComponent<RectTransform>(); //we reference 2 times, so store in variable
			currParentRect.sizeDelta = new Vector2(currParentRect.sizeDelta.x, items.Count * elementHeight); //set new rect bounds

			displayedContainer = this; //since displaying is complete, store reference to this container
			currTab.isOn = true; //ensure the ui parents are hidden and displayed properly
		} else { //closing inventory
			ContextManager.instance.Hide(); //ensure any displayed context menus are hidden
		}
	}

	private void RefreshUIElement(Item item) {
		Transform element = CreateContainerElement(item);
		Text text;
		foreach (Transform child in element) {
			text = child.GetComponent<Text>();

			if (text != null) {
				if (text.name.Contains("Name")) {
					text.text = item.ToString(); //display item name with prefix/suffix
					if (item.Quantity > 1) { //if there is more than 1 of this item
						text.text += "(" + item.Quantity + ")"; //add the quantity in parenthesis >> (2)
					}
					text.color = item.GetQualityColor(); //change the color of the text so the player knows how epic the item is
				} else if (child.name.Contains("Magic")) {
					text.text = item.GetMagicStat().ToString(); //display magic stat
				} else if (child.name.Contains("Physical")) {
					text.text = item.GetPhysicalStat().ToString(); //display physical stat
				} else if (child.name.Contains("Weight")) {
					text.text = item.GetWeight().ToString(); //display weight
				} else if (child.name.Contains("Currency")) {
					text.text = item.GetValue().ToString(); //display currency value
				} else if (child.name.Contains("ItemType")) {
					text.text = item.GetItemType(); //display the item type
				}
			}
		}

		if (!element.gameObject.activeSelf) {
			element.gameObject.SetActive(true);
		}
	}

	protected virtual void RefreshWeightElement() {
		//normal containers do not display their weight, but the player's inventory will.
	}

	/// <summary>
	/// This container will no longer track this item.
	/// </summary>
	/// <param name="item"></param>
	public void Remove(Item item) {
		if (items.Remove(item)) { //if the item is removed
			totalWeight -= item.GetWeight();
			RefreshWeightElement();
		}
		RemoveUIElement(item);
	}

	private void RemoveUIElement(Item item) {
		Transform element = displayParent.Find(GetElementName(item));
		if (element != null) {
			Destroy(element.gameObject);
		}
	}

	public static void ResetInstanceIDs() {
		nextInstanceID = 0;
	}

	/// <summary>
	/// Removes all contained items from the scene, then destroys itself.
	/// </summary>
	public virtual void SelfDestruct(bool destroyItems = true, bool destroySelf = true) {
		for (int i = items.Count - 1; i >= 0; i--) {
			RemoveUIElement(items[i]); //either way, remove ui element
			if (destroyItems) { //if we are destroying the items
				Destroy(items[i].gameObject); //destroy item game object to remove all item info
			} else if (destroySelf) { //if !destroyItems, but destroying self
				items[i].ContainerID = -1; //remove link to containerID
			}
		}

		if (destroySelf) {
			Destroy(gameObject);
		}
	}

	protected bool Transfer(Item item, int quantity, Container other) {
		bool transferred = false;
		if (!other.Equals(this)) {
			quantity = Mathf.Clamp(quantity, 0, item.Quantity); //ensure the quantity is never bigger than item quantity
			if (item.Quantity - quantity <= 0) {
				if (other.Add(item)) { //try to move item to other container
					Remove(item); //if the item was added, remove from the container
					transferred = true;
				}
			} else {
				if (other.Add(AssetManager.instance.InstantiateItem(position: other.transform.position, itemID: item.ID, containerID: other.instanceID,
					quantity: quantity, textureName: item.GetTextureName(), lastUpdated: GameManager.instance.ElapsedGameTime))) {
					item.Quantity -= quantity;
					transferred = true;
				}
			}

			if (transferred) {
				other.Display(false); //display other first
				Display(false); //display this to overwrite displayedContainer
			}
		}

		return transferred;
	}

	public static bool Transfer(string elementName, int quantity) {
		if (nonplayerContainer != null) { //player is currently interacting with a container
			Container other;
			if (displayedContainer == Inventory.instance) {
				other = nonplayerContainer;
			} else {
				other = Inventory.instance;
			}

			return displayedContainer.Transfer(GetDisplayedItem(elementName), quantity, other);
		}
		return false;
	}

	/// <summary>
	/// Attempts to transfer an item, then returns a reference to the transferred item.
	/// </summary>
	/// <returns>'null' if unable to transfer.</returns>
	public static Item Transfer(Item item, int quantity) {
		if (nonplayerContainer != null) { //player is currently interacting with a container
			Container other;
			if (displayedContainer == Inventory.instance) {
				other = nonplayerContainer;
			} else {
				other = Inventory.instance;
			}

			if (displayedContainer.Transfer(item, quantity, other)) { //if successfully transferred
				return other.GetItem(item.ToString());
			}
		}
		return null;
	}

	protected virtual void UseItem(Item item) { //if an item is used in any other container
		Transfer(item, 1, Inventory.instance); //transfer this item to player's inventory if possible
	}
}
