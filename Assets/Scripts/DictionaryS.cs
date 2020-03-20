using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DictionaryS {
	[SerializeField]
	private List<string> keys;
	[SerializeField]
	private List<string> values;

	public DictionaryS(Dictionary<string, KeyCode> source) {
		keys = new List<string>();
		values = new List<string>();

		keys.AddRange(source.Keys);

		foreach (KeyValuePair<string, KeyCode> kvp in source) {
			values.Add(kvp.Value.ToString());
		}
	}

	public Dictionary<string, KeyCode> GetDictionary() {
		if (keys != null && values != null) {
			if (keys.Count == values.Count) {
				Dictionary<string, KeyCode> temp = new Dictionary<string, KeyCode>();

				for (int i = 0; i < keys.Count; i++) { //for each key & value
					temp.Add(keys[i], (KeyCode)Enum.Parse(typeof(KeyCode), values[i])); //add new kvp to temp
				}

				return temp;
			} else {
				Debug.Log("Save Data Error: DictionaryS.toDictionary() => keys(" + keys.Count + ") != values(" + values.Count + ")");
			}
		}
		return null;
	}
}
