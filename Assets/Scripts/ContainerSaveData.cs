using System;
using System.Collections.Generic;
using UnityEngine;
using internal_Items;

/// <summary>
/// Saves the items stored in a container. To be used for serializing player inventory.
/// </summary>
[Serializable]
public class ContainerSaveData {
	[SerializeField]
	private List<ItemSaveData> items;

	public ContainerSaveData() {
		items = new List<ItemSaveData>();
	}

	public ContainerSaveData(Container c) {
		items = new List<ItemSaveData>();
		AddRange(c.Items);
	}

	private void AddRange(List<Item> Items) {
		foreach (Item i in Items) {
			items.Add(i.ToItemSaveData());
		}
	}

	public List<Item> GetItems() {
		List<Item> temp = new List<Item>();
		foreach(ItemSaveData info in items) {
			temp.Add(Item.Create(info));
		}
		return temp;
	}
}
