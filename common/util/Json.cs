using System.Text.Json;

namespace Vanadium;

// generic json utils specifically for our folder structure
public static class Json {
	/// <summary>
	/// Reads the contents of a json file with FileShare.ReadWrite.
	/// </summary>
	/// <param name="filepath">The filepath to the json, relative to the application's root directoy.</param>
	/// <returns>The contents of the json file.</returns>
	public static string ReadFromJson(string filepath) {
		using var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		using var sr = new StreamReader(stream);
		return sr.ReadToEnd();
	}

	/// <summary>
	/// Parses a Json string to an object type.
	/// </summary>
	/// <typeparam name="T">The type of the object.</typeparam>
	/// <param name="data">The Json string data.</param>
	/// <returns>An instance of the object type if deserialized successfully.</returns>
	public static T? FromString<T>(string data) {
		return JsonSerializer.Deserialize<T>(data);
	}
}
