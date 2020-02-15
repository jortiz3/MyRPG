using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;

[XmlRoot(ElementName = "Faction")]
public class Faction {
	private static int maxFavor = 500;
	private static int minFavor = -500;

	[SerializeField]
	private string name;
	private int playerFavor;
	[SerializeField]
	private List<string> hostileFactions;

	[XmlElement("Name")]
	public string Name { get { return name; } set { name = value; } }
	[XmlElement("PlayerFavor")]
	public int PlayerFavor { get { return playerFavor; } set { playerFavor = value; } }
	[XmlArray("HostileFactions"), XmlArrayItem("Faction")]
	public List<string> HostileFactions { get { return hostileFactions; } set { hostileFactions = value; } }

	public Faction() {
		name = "empty";
		playerFavor = 0;
		hostileFactions = new List<string>();
	}

	public Faction(string Name, int PlayerFavor, List<string> HostileFactions) {
		name = Name;
		playerFavor = PlayerFavor;
		hostileFactions = HostileFactions;
	}

	public void DecreaseFavor(int amount) {
		if (amount < 0) { //if amount is negative
			amount *= -1; //make it positive
		}

		playerFavor -= amount; //decrease favor

		if (playerFavor <= minFavor / 2) { //if favor drops below halfway to min
			if (!hostileFactions.Contains("Player")) { //if player isn't already hostile faction
				hostileFactions.Add("Player"); //add to hostile factions
			}
		}

		if (playerFavor < minFavor) { //clamp the favor at min
			playerFavor = minFavor;
		}
	}

	public void IncreaseFavor(int amount) {
		if (amount < 0) { //if amount is negative
			amount *= -1; //make it positive
		}

		playerFavor += amount; //increase favor

		if (playerFavor > minFavor / 2) { //if favor drops below halfway to min
			if (hostileFactions.Contains("Player")) { //if player is hostile faction
				hostileFactions.Remove("Player"); //remove
			}
		}

		if (playerFavor > maxFavor) { //clamp the favor at max
			playerFavor = maxFavor;
		}
	}
}
