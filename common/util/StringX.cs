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

	/// <summary>
	/// Convert to float, if not then return Default.
	/// </summary>
	/// <param name="str">The string to parse</param>
	/// <param name="Default">The default value to return in case parsing fails</param>
	/// <returns>The parsed float</returns>
	public static float ToFloat(this string str, float Default = 0f) {
		if(float.TryParse(str, out float result)) {
			return result;
		}

		return Default;
	}
}
