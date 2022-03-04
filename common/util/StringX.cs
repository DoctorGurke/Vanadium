namespace Vanadium;

public static class StringX {
	public static string Clean(this string s) {
		return s.Trim().Replace("\\", "/").Replace("\n", "").Replace("\r", "");
	}
}
