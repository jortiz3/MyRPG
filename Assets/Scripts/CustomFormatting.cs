/// <summary>
/// A collection of string & character formatting methods. Written by Justin Ortiz
/// </summary>
public class CustomFormatting {
	/// <summary>
	/// Formats the first character of the provided string to be uppercase and the remaining characters to lowercase.
	/// </summary>
	/// <param name="word">The string to be formatted.</param>
	/// <returns>Capitalized string.</returns>
	public static string Capitalize(string word) {
		return char.ToUpper(word[0]) + word.Substring(1).ToLower();
	}
}
