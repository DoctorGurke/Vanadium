using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace Vanadium;

public class Material {
	public enum MaterialParamType {
		Unset,
		Boolean,
		Integer,
		UnsignedInteger,
		Float,
		Double,
		Vector2,
		Vector3,
		Vector4,
		Matrix4,
		Sampler2D
	}

	// the shader of the material (ie. pbr generic, unlit, vertex color generic, etc
	public Shader Shader { get; set; }

	/// <summary>
	/// What types the shader has.
	/// <remarks>[name, type]</remarks>
	/// </summary>
	public Dictionary<string, MaterialParamType> MaterialParameters { get; private set; } = new();

	/// <summary>
	/// What data the material provides.
	/// <remarks>[name, data]</remarks>
	/// </summary>
	public Dictionary<string, string> MaterialData { get; private set; } = new();

	// this is kinda dumb
	public void AddParameter(string type, string name) {
		MaterialParamType paramtype = MaterialParamType.Unset;
		switch(type) {
			case "bool":
				paramtype = MaterialParamType.Boolean;
				break;
			case "int":
				paramtype = MaterialParamType.Integer;
				break;
			case "uint":
				paramtype = MaterialParamType.UnsignedInteger;
				break;
			case "float":
				paramtype = MaterialParamType.Float;
				break;
			case "double":
				paramtype = MaterialParamType.Double;
				break;
			case "sampler2D":
				paramtype = MaterialParamType.Sampler2D;
				break;
			case "vec2":
				paramtype = MaterialParamType.Vector2;
				break;
			case "vec3":
				paramtype = MaterialParamType.Vector3;
				break;
			case "vec4":
				paramtype = MaterialParamType.Vector4;
				break;
			case "mat4":
				paramtype = MaterialParamType.Matrix4;
				break;
		}
		MaterialParameters.Add(name, paramtype);
	}

	public void AddData(string name, string data) {
		MaterialData.Add(name, data);
	}

	public static void Load(string path) {
		var data = Json.ReadFromJson(path);
		var parameters = JObject.Parse(data);

		if(!parameters.ContainsKey("shader")) {
			Debug.WriteLine($"ERROR LOADING MATERIAL {path}! NO VALID SHADER NAME FOUND!");
			return; // TODO error material
		}
		
		Debug.WriteLine("material json--------------------");
		foreach(var param in parameters.Properties()) {
			Debug.WriteLine($"{param.Name} = {param.Value}");
		}
	}
}
