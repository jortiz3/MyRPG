using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using internal_Items;

/// <summary>
/// Stores a list of items and displays them to the screen. Written by Justin Ortiz
/// </summary>
public class Container : Interactable {
	protected static Container nonplayerContainer; //non-player-inventory container
	protected static Container displayedContainer;

	private static Toggle containerTab;
	private static Transform containerParent;
	private static Transform containerElementPrefab;
	private static int nextInstanceID;

	protected List<Item> items;
	protected Transform currDisplayParent;
	private float totalWeight;
	protected float maxWeight;
	protected int instanceID; //never 0 or less than
	protected bool optout_populateItems;

	public List<Item> Items { get { return items; } }
	public float TotalWeight { get { return totalWeight; } }
	public float MaxWeight { get { return maxWeight; } }
	public int InstanceID { get { return instanceID; } }

	public bool Add(Item item) {
		if (item != null) {
			if (GameManager.instance.ElapsedGameTime - lastUpdated > 2 || item.LastUpdated >= lastUpdated) { //if container not updated recently, and item attempting to add is new
				if (totalWeight + item.GetWeight() <= maxWeight) {
					int itemIndex = IndexOf(item);
					if (itemIndex >= 0) { //container has some of this item already
						items[itemIndex].Quantity += item.Quantity; //increase the quantity of the item already in container
						Destroy(item.gameObject); //remove item from scene
						items[itemIndex].LastUpdated = GameManager.instance.ElapsedGameTime;
					} else { //first time this item is added
						items.Add(item);
						item.ContainerID = instanceID; //ensure the item knows which container it is in
						CreateContainerElement(item); //ensure there's a ui element for the item
						item.SetInteractionActive(false); //hide the item from world space
						item.LastUpdated = GameManager.instance.ElapsedGameTime;
					}
					return true;
				}
			} else { //container is attempting to repopulate
				Destroy(item.gameObject); //destroy old item
			}
		}
		return false;
	}

	public bool Contains(string fullItemName) {
		for (int i = 0; i < items.Count; i++) {
			if (items[i].ToString().Equals(fullItemName)) {
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
		Transform temp = currDisplayParent.Find(i.ToString());
		if (temp == null) {
			temp = Instantiate(containerElementPrefab, currDisplayParent).transform;
			temp.name = i.ToString();
		}
		return temp;
	}

	public virtual void Display() {
		RefreshDisplay();
		MenuScript.instance.ChangeState("Inventory");
	}

	public static Container GetContainer(int instanceID) {
		if (instanceID >= 0) {
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

	protected Item GetItem(string fullItemName) {
		for (int i = 0; i < items.Count; i++) {
			if (items[i].ToString().Equals(fullItemName)) {
				return items[i];
			}
		}
		return null;
	}

	protected int GetNextInstanceID() {
		return ++nextInstanceID; //increment id then return it
	}

	private int IndexOf(Item item) {
		for (int i = 0; i < items.Count; i++) {
			if (items[i].Equals(item)) {
				return i;
			}
		}
		return -1;
	}

	protected override void Initialize() {
		if (containerParent == null) {
			containerParent = GameObject.Find("Inventory_Container_Other_Content").transform; //update object name

			containerElementPrefab = GameObject.Find("Inventory_Container_Element_Prefab").transform; //update object name
			containerElementPrefab.gameObject.SetActive(false);

			containerTab = GameObject.Find("Toggle_Inventory_Other").GetComponent<Toggle>();
		}

		if (currDisplayParent == null) {
			currDisplayParent = containerParent;
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
				Populate();
			}
		}

		gameObject.name += "_container_" + instanceID.ToString();

		base.Initialize();
	}

	protected override void InteractInternal() { //player walks up to interact
		SetContainerActive(true); //display the other container(this)
		Inventory.instance.RefreshDisplay(); //refresh player's inventory
		Display(); //display this
		base.InteractInternal(); //finish interaction
	}

	public void Load(int InstanceID, float LastUpdated) {
		lastUpdated = LastUpdated;

		if (InstanceID > 0) {
			instanceID = InstanceID;
			if (instanceID >= nextInstanceID) {
				nextInstanceID = instanceID; //ensure current instanceIDs do not get overwritten
			}

			if (GameManager.instance.ElapsedGameTime - lastUpdated > 600) { //10 mins since last update?
				Populate();
				lastUpdated = GameManager.instance.ElapsedGameTime;
			}
		}
	}

	protected void Populate() {
		SelfDestruct(destroySelf: false); //ensure there are no items in the container

		int numItems = UnityEngine.Random.Range(5, 15); //determine how many times items will be instantiated into the chest
		ItemInfo[] dropTable = ItemDatabase.GetDropTable(transform.name.Split('_')); //determine which items can be spawned in this container -- determined by texture name (i.e. "chest_default")
		if (dropTable != null && dropTable.Length > 0) {
			int dropTableIndex; //the currently selected item to add
			for (int i = 0; i < numItems; i++) {
				dropTableIndex = UnityEngine.Random.Range(0, dropTable.Length); //get one of the items from the drop table
				AssetManager.instance.InstantiateItem(position: transform.position, itemID: dropTable[dropTableIndex].id,
					containerID: this.instanceID, itemPrefix: dropTable[dropTableIndex].prefix, itemSuffix: dropTable[dropTableIndex].suffix,
					textureName: dropTable[dropTableIndex].texture_default, lastUpdated: lastUpdated); //instantiate the item -- the item will add itself to this container
			}
		}
	}

	protected void RefreshDisplay() {
		StartCoroutine(RefreshDisplay(currDisplayParent));
	}

	/// <summary>
	/// Iterates through all elements available for containers in the scene and only shows the elements for this container.
	/// </summary>
	private IEnumerator RefreshDisplay(Transform parent) {
		foreach (Transform child in parent) {
			child.gameObject.SetActive(false);
		}
		yield return new WaitForEndOfFrame();

		totalWeight = 0;
		for (int i = 0; i < items.Count; i++) { //loop through all items
			RefreshUIElement(items[i]); //refresh the ui element for the item
			totalWeight += items[i].GetWeight(); //increase total weight
			yield return new WaitForEndOfFrame(); //wait a frame
		}
		RefreshWeightElement(); //display the total weight

		//resize the container slide area
		float elementHeight = containerElementPrefab.GetComponent<RectTransform>().sizeDelta.y; //get element height
		RectTransform currParentRect = currDisplayParent.GetComponent<RectTransform>(); //we reference 2 times, so store in variable
		currParentRect.sizeDelta = new Vector2(currParentRect.sizeDelta.x, items.Count * elementHeight); //set new rect bounds

		displayedContainer = this;
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
		Destroy(currDisplayParent.Find(item.ToString()).gameObject);
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

	protected void SetContainerActive(bool active) {
		containerParent.gameObject.SetActive(active); //show/hide the container scroll rect
		containerTab.gameObject.SetActive(active); //show/hide tab
		containerTab.isOn = active; //ensures the container is shown if enabled
		nonplayerContainer = active ? this : null; //if the container is active, then we need to assign this container to static reference
	}

	protected void Transfer(Item item, Container other) {
		if (other.Add(item)) { //try to move item to other container
			Remove(item); //if the item was added, remove from the container
		}
	}

	protected virtual void UseItem(Item item) { //if an item is used in any other container
		Transfer(item, Inventory.instance); //transfer this item to player's inventory if possible
	}

	/// <summary>
	/// Tells displayed container to use this item. Called by button via inspector.
	/// </summary>
	/// <param name="element">The button being pressed.</param>
	public void UseUIElement(Transform element) {
		if (displayedContainer != null) {
			Item currentItem = displayedContainer.GetItem(element.name);
			displayedContainer.UseItem(currentItem);
		}
	}
}
