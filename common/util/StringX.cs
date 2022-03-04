namespace Vanadium;

public static class StringX {

	/// <summary>
	/// Utility function to clean strings for common usage.
	/// </summary>
	/// <remarks>Will trim the string, replace '\' with '/' and remove newline and carry return characters.</remarks>
	/// <param name="s">String extension.</param>
	/// <returns>The cleaned st ring.</returns>
	public static string Clean(this string s) {
		return s.Trim().Replace("\\", "/").Replace("\n", "").Replace("\r", "");
	}
}
