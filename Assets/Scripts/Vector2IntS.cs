using System;
using UnityEngine;

[Serializable]
public class Vector2IntS {
	public static Vector2IntS zero { get { return new Vector2IntS(0, 0); } }
	public static Vector2IntS one { get { return new Vector2IntS(1, 1); } }

	public int x;
	public int y;

	public Vector2IntS() {
		x = 0;
		y = 0;
	}

	public Vector2IntS(int X, int Y) {
		x = X;
		y = Y;
	}

	public Vector2IntS(Vector2Int vector) {
		x = vector.x;
		y = vector.y;
	}

	public static Vector2IntS Parse(string text) {
		text = text.Replace(" ", "");
		text = text.Replace("(", "");
		text = text.Replace(")", "");
		string[] textArray = text.Split(',');
		Vector2IntS temp = new Vector2IntS();
		try {
			temp.x = int.Parse(textArray[0]);
		} catch {
			temp.x = 0;
		}

		try {
			temp.y = int.Parse(textArray[1]);
		} catch {
			temp.y = 0;
		}
		return temp;
	}

	public override string ToString() {
		return "(" + x + ", " + y + ")";
	}

	public Vector2Int ToVector2Int() {
		return new Vector2Int(x, y);
	}

	public Vector3 ToVector3() {
		return new Vector3(x, y);
	}
}
