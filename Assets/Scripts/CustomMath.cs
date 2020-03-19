//Written by Justin Ortiz
public class CustomMath {
	public static int GetLargestFactor(int num) {
		for (int i = num / 2; i > 1; i--) { //start at halfway point, go down to 2
			if (num % i == 0) { //at first occurrence of no remainder
				return i; //return i
			}
		}
		return num; //if nothing found, return the number itself
	}
}
