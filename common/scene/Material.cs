using Newtonsoft.Json.Linq;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;
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
		Sampler2D,
		SamplerCube
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

	private static Dictionary<string, Material> PrecachedMaterials = new();

	public static Material ErrorMaterial => Load("materials/error.vanmat");
	public bool IsError { get; private set; } = false;
	public bool Transparent { get; private set; } = false;

	public static Material Load(string path) {
		path = $"resources/{path}";

		if(PrecachedMaterials.TryGetValue(path, out var material)) {
			return material;
		}

		var mat = new Material();

		if(!Json.ReadFromJson(path, out var data)) {
			Log.Info($"ERROR LOADING MATERIAL {path}! MATERIAL FILE NOT FOUND!");
			mat.IsError = true;
			return mat;
		}
		var parameters = JObject.Parse(data);

		if(!parameters.ContainsKey("shader")) {
			Log.Info($"ERROR LOADING MATERIAL {path}! NO VALID SHADER NAME FOUND!");
			mat.IsError = true;
			return mat;
		}

		// our material parameters get loaded into the material by the shader, according to #material macros
		try {
			var shadername = parameters["shader"];
			var shader = new Shader($"{shadername}.vert", $"{shadername}.frag", mat);
			mat.Shader = shader;
		} catch (Exception ex) {
			Log.Info($"ERROR LOADING MATERIAL {path}! ERROR BUILDING SHADER! {ex}");
			mat.IsError = true;
			return mat;
		}

		Log.Info("material json");
		foreach(var param in parameters.Properties()) {
			mat.AddData(param.Name, $"{param.Value}");
			Log.Info($"{param.Name} = {param.Value}");
		}

		// the shader will have registered the parameter types and data at this point already, so we can parse the actual runtime types here
		// which then get fed to the shader via Material.Use()
		mat.SerializeMaterialParameters();
		PrecachedMaterials.Add(path, mat);
		return mat;
	}

	public int GetAttribLocation(string attribname) {
		if(IsError) {
			return ErrorMaterial.Shader.GetAttribLocation(attribname);
		}
		return Shader.GetAttribLocation(attribname);
	}

	/// <summary>
	/// Set a uniform int on this shader.
	/// </summary>
	/// <param name="name">The name of the uniform</param>
	/// <param name="data">The data to set</param>
	public void Set(string name, bool data) {
		if(IsError)
			ErrorMaterial.Shader.Set(name, data);
		else
			Shader.Set(name, data);
	}

	/// <summary>
	/// Set a uniform int on this shader.
	/// </summary>
	/// <param name="name">The name of the uniform</param>
	/// <param name="data">The data to set</param>
	public void Set(string name, int data) {
		if(IsError)
			ErrorMaterial.Shader.Set(name, data);
		else
			Shader.Set(name, data);
	}

	/// <summary>
	/// Set a uniform float on this shader.
	/// </summary>
	/// <param name="name">The name of the uniform</param>
	/// <param name="data">The data to set</param>
	public void Set(string name, float data) {
		if(IsError)
			ErrorMaterial.Shader.Set(name, data);
		else
			Shader.Set(name, data);
	}

	/// <summary>
	/// Set a uniform int on this shader.
	/// </summary>
	/// <param name="name">The name of the uniform</param>
	/// <param name="data">The data to set</param>
	public void Set(string name, double data) {
		if(IsError)
			ErrorMaterial.Shader.Set(name, data);
		else
			Shader.Set(name, data);
	}

	/// <summary>
	/// Set a uniform Vector3 on this shader.
	/// </summary>
	/// <param name="name">The name of the uniform</param>
	/// <param name="data">The data to set</param>
	public void Set(string name, Vector2 data) {
		if(IsError)
			ErrorMaterial.Shader.Set(name, data);
		else
			Shader.Set(name, data);
	}

	/// <summary>
	/// Set a uniform Vector3 on this shader.
	/// </summary>
	/// <param name="name">The name of the uniform</param>
	/// <param name="data">The data to set</param>
	public void Set(string name, Vector3 data) {
		if(IsError)
			ErrorMaterial.Shader.Set(name, data);
		else
			Shader.Set(name, data);
	}

	/// <summary>
	/// Set a uniform Vector3 on this shader.
	/// </summary>
	/// <param name="name">The name of the uniform</param>
	/// <param name="data">The data to set</param>
	public void Set(string name, Vector4 data) {
		if(IsError)
			ErrorMaterial.Shader.Set(name, data);
		else
			Shader.Set(name, data);
	}

	/// <summary>
	/// Set a uniform Matrix4 on this shader
	/// </summary>
	/// <param name="name">The name of the uniform</param>
	/// <param name="data">The data to set</param>
	/// <remarks>
	///   <para>
	///   The matrix is transposed before being sent to the shader.
	///   </para>
	/// </remarks>
	public void Set(string name, Matrix4 data) {
		if(IsError)
			ErrorMaterial.Shader.Set(name, data);
		else
			Shader.Set(name, data);
	}

	/// <summary>
	/// Use this Material's shader in the current GL context and set all material parameters.
	/// </summary>
	public void Use() {

		if(IsError) {
			ErrorMaterial.Use();
			return;
		}

		Shader.Use();

		Shader.Set("cameraPos", Camera.ActiveCamera is null ? Vector3.Zero : Camera.ActiveCamera.Position);
		Shader.Set("curTime", Time.Now);

		foreach(var data in BooleanData) {
			Shader.Set(data.Key, data.Value);
		}

		foreach(var data in IntegerData) {
			Shader.Set(data.Key, data.Value);
		}

		foreach(var data in UnsignedIntegerData) {
			Shader.Set(data.Key, data.Value);
		}

		foreach(var data in FloatData) {
			Shader.Set(data.Key, data.Value);
		}

		foreach(var data in DoubleData) {
			Shader.Set(data.Key, data.Value);
		}

		foreach(var data in Vector2Data) {
			Shader.Set(data.Key, data.Value);
		}

		foreach(var data in Vector3Data) {
			Shader.Set(data.Key, data.Value);
		}

		foreach(var data in Vector4Data) {
			Shader.Set(data.Key, data.Value);
		}

		foreach(var data in Matrix4Data) {
			Shader.Set(data.Key, data.Value);
		}

		for(int tex = 0; tex < TextureData.Count; tex++) {
			var data = TextureData.ElementAtOrDefault(tex);
			var texture = Texture.Load(data.Value);
			texture.Use(TextureUnit.Texture0 + tex);
			Shader.Set(data.Key, tex);
		}

		for(int cube = 0; cube < CubeTextureData.Count; cube++) {
			var texture = CubeTextureData.ElementAtOrDefault(cube);
			texture.Value.Use(TextureUnit.Texture0 + cube);
			Shader.Set(texture.Key, cube);
		}
	}

	// this is kinda dumb
	public void AddParameter(string type, string name) {
		Log.Info($"adding parameter {type} {name}");
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
			case "samplerCube":
				paramtype = MaterialParamType.SamplerCube;
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
		Log.Info($"adding material data {name} {data}");
		MaterialData.Add(name, data);

		if(bool.TryParse(data, out var bdata)) {
			if(name == "transparent" && bdata) {
				Transparent = true;
			}
		}
	}

	// this is a bit dumb
	public void SerializeMaterialParameters() {
		Log.Info($"Serializing material parameters");
		foreach(var param in MaterialData) {
			Log.Info($"serializing for {param.Key} {param.Value}");
			if(MaterialParameters.TryGetValue(param.Key, out var type)) {
				switch(type) {
					case MaterialParamType.Unset:
						Log.Info("unset data!");
						break;
					case MaterialParamType.Boolean:
						Log.Info($"parsing material data {param.Key} {param.Value}");
						if(bool.TryParse(param.Value, out var bdata)) {
							BooleanData.Add(param.Key, bdata);
						}
						break;
					case MaterialParamType.Integer:
						Log.Info($"parsing material data {param.Key} {param.Value}");
						if(int.TryParse(param.Value, out var idata)) {
							IntegerData.Add(param.Key, idata);
						}
						break;
					case MaterialParamType.UnsignedInteger:
						Log.Info($"parsing material data {param.Key} {param.Value}");
						if(uint.TryParse(param.Value, out var uidata)) {
							UnsignedIntegerData.Add(param.Key, uidata);
						}
						break;
					case MaterialParamType.Float:
						Log.Info($"parsing material data {param.Key} {param.Value}");
						if(float.TryParse(param.Value, out var fdata)) {
							FloatData.Add(param.Key, fdata);
						}
						break;
					case MaterialParamType.Double:
						Log.Info($"parsing material data {param.Key} {param.Value}");
						if(double.TryParse(param.Value, out var ddata)) {
							DoubleData.Add(param.Key, ddata);
						}
						break;
					case MaterialParamType.Vector2:
						Log.Info($"parsing material data {param.Key} {param.Value}");
						// TODO make wrapper for vec2 and add TryParse directly to type
						if(Parse.TryParse(param.Value, out Vector2 vec2data)) {
							Vector2Data.Add(param.Key, vec2data);
						}
						break;
					case MaterialParamType.Vector3:
						Log.Info($"parsing material data {param.Key} {param.Value}");
						if(Vector3.TryParse(param.Value, out var vec3data)) {
							Vector3Data.Add(param.Key, vec3data);
						}
						break;
					case MaterialParamType.Vector4:
						Log.Info($"parsing material data {param.Key} {param.Value}");
						// TODO make wrapper for vec4 and add TryParse directly to type
						if(Parse.TryParse(param.Value, out Vector4 vec4data)) {
							Vector4Data.Add(param.Key, vec4data);
						}
						break;
					case MaterialParamType.Matrix4:
						Log.Info($"parsing material data {param.Key} {param.Value}");
						// TODO make wrapper for mat4 and add TryParse directly to type (?)
						if(Parse.TryParse(param.Value, out Matrix4 mat4data)) {
							Matrix4Data.Add(param.Key, mat4data);
						}
						break;
					case MaterialParamType.Sampler2D:
						Log.Info($"parsing material data {param.Key} {param.Value}");
						// just directly add the string in the material. If it's not valid, we fall back to error texture anyways
						TextureData.Add(param.Key, param.Value);
						break;
					case MaterialParamType.SamplerCube:
						Log.Info($"parsing material data {param.Key} {param.Value} texcube");
						var sides = new List<string>();
						var path = param.Value;
						var ext = Path.GetExtension(path);
						path = path.Replace(ext, "");
						foreach(var side in skyboxSides) {
							var full = $"{path}_{side}{ext}";
							Log.Info($"adding cubemap side {full}");
							sides.Add(full);
						}
						var cube = TextureCube.Load(sides);
						// keep the reference to the texture
						CubeTextureData.Add(param.Key, cube);
						break;
				}
			}
		}
	}

	private static string[] skyboxSides = {
		"right",
		"left",
		"top",
		"bottom",
		"front",
		"back"
	};

	// this is dumb
	private Dictionary<string, bool> BooleanData = new();
	private Dictionary<string, int> IntegerData = new();
	private Dictionary<string, uint> UnsignedIntegerData = new();
	private Dictionary<string, float> FloatData = new();
	private Dictionary<string, double> DoubleData = new();
	private Dictionary<string, Vector2> Vector2Data = new();
	private Dictionary<string, Vector3> Vector3Data = new();
	private Dictionary<string, Vector4> Vector4Data = new();
	private Dictionary<string, Matrix4> Matrix4Data = new();
	private Dictionary<string, string> TextureData = new(); // just keep the path, we load it from the Texture class
	private Dictionary<string, TextureCube> CubeTextureData = new(); // keep reference since we don't cache cubemap textures
}
