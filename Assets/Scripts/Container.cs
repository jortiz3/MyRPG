using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Stores a list of items and displays them to the screen.
/// </summary>
public class Container : Interactable {
	private static Toggle containerTab;
	private static Transform containerParent;
	private static Transform containerElementPrefab;

	[SerializeField] //remove later
	protected List<Item> items;
	protected Transform currParent;
	private float totalWeight;

	public List<Item> Items { get { return items; } }
	public float TotalWeight { get { return totalWeight; } }

	public void Add(Item item) {
		if (items.Contains(item)) { //container has some of this item already
			items[items.IndexOf(item)].Quantity += item.Quantity;
		} else {
			items.Add(item);
			CreateContainerElement(item); //ensure there's a ui element for the item
		}
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
		Transform temp = currParent.Find(i.ToString());
		if (temp == null) {
			temp = Instantiate(containerElementPrefab, currParent).transform;
			temp.name = i.ToString();
		}
		return temp;
	}

	public virtual void Display() {
		RefreshDisplay();
		MenuScript.instance.ChangeState("Inventory");
	}

	protected override void Initialize() {
		if (containerParent == null) {
			containerParent = GameObject.Find("Inventory_Container_Other_Content").transform; //update object name

			containerElementPrefab = GameObject.Find("Inventory_Container_Element_Prefab").transform; //update object name
			containerElementPrefab.gameObject.SetActive(false);

			containerTab = GameObject.Find("Toggle_Inventory_Other").GetComponent<Toggle>();
		}
		currParent = containerParent;
		gameObject.tag = "container";

		if (items == null) {
			items = new List<Item>();
			totalWeight = 0;
		}
		base.Initialize();
	}

	protected override void InteractInternal() { //player walks up to interact
		SetContainerActive(true); //display the other container(this)
		Inventory.instance.RefreshDisplay(); //refresh player's inventory
		Display(); //display this
		base.InteractInternal(); //finish interaction
	}

	public void Load(ContainerSaveData data) {
		items = data.GetItems();
	}

	protected void RefreshDisplay() {
		StartCoroutine(RefreshDisplay(currParent));
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
		RectTransform currParentRect = currParent.GetComponent<RectTransform>(); //we reference 2 times, so store in variable
		currParentRect.sizeDelta = new Vector2(currParentRect.sizeDelta.x, items.Count * elementHeight); //set new rect bounds
	}

	private void RefreshUIElement(Item item) {
		Transform element = CreateContainerElement(item);
		Text text;
		foreach(Transform child in element) {
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
					text.text = item.BaseValue.ToString(); //display currency value
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
		Destroy(currParent.Find(item.ToString()).gameObject);
	}

	/// <summary>
	/// Removes all contained items from the scene, then destroys itself.
	/// </summary>
	public virtual void SelfDestruct() {
		for (int i = items.Count - 1; i >= 0; i--) {
			RemoveUIElement(items[i]);
			Destroy(items[i].gameObject);
		}
		Destroy(gameObject);
	}

	protected void SetContainerActive(bool active) {
		containerParent.gameObject.SetActive(active); //show/hide the container scroll rect
		containerTab.gameObject.SetActive(active); //show/hide tab
		containerTab.isOn = active; //ensures the container is shown if enabled
	}
}
