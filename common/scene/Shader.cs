using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Text.RegularExpressions;

namespace Vanadium;

// A simple class meant to help create shaders. Taken from https://github.com/opentk/LearnOpenTK
public class Shader {
	public readonly int Handle;

	public Dictionary<string, int> UniformLocations { get; private set; } = new Dictionary<string, int>();

	public static string Load(string path) {
		var data = File.ReadAllText(path);

		// scan data for any include 'macros'
		var includeMatches = Regex.Matches(data, "#include .+");

		foreach(var match in includeMatches) {
			var matchstring = $"{match}".Clean();
			var includePath = matchstring.Replace("#include", "").Clean();

			// make sure a file isn't trying to include itself
			if(includePath == path) {
				Debug.WriteLine($"Recursive include for {includePath} found! Defusing...");
				data.Replace(matchstring, "");
				continue;
			}

			var includeData = Load(includePath);
			data = data.Replace(matchstring, includeData);
		}

		return data;
	}

	public Shader(string vertPath, string fragPath) {
		// load vertex shader and compile
		var shaderSource = Load(vertPath);
		var vertexShader = GL.CreateShader(ShaderType.VertexShader);
		GL.ShaderSource(vertexShader, shaderSource);
		CompileShader(vertexShader);

		// load fragment shader and compile
		shaderSource = Load(fragPath);
		var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
		GL.ShaderSource(fragmentShader, shaderSource);
		CompileShader(fragmentShader);

		// create opengl shader program
		Handle = GL.CreateProgram();

		// attach vert + fragment shaders and link
		GL.AttachShader(Handle, vertexShader);
		GL.AttachShader(Handle, fragmentShader);
		LinkProgram(Handle);

		// When the shader program is linked, it no longer needs the individual shaders attached to it; the compiled code is copied into the shader program.
		// Detach them, and then delete them.
		GL.DetachShader(Handle, vertexShader);
		GL.DetachShader(Handle, fragmentShader);
		GL.DeleteShader(fragmentShader);
		GL.DeleteShader(vertexShader);

		// The shader is now ready to go, but first, we're going to cache all the shader uniform locations.
		// Querying this from the shader is very slow, so we do it once on initialization and reuse those values
		// later.

		// First, we have to get the number of active uniforms in the shader.
		GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

		// Loop over all the uniforms,
		for(var i = 0; i < numberOfUniforms; i++) {
			// get the name of this uniform,
			var key = GL.GetActiveUniform(Handle, i, out _, out _);

			// get the location,
			var location = GL.GetUniformLocation(Handle, key);

			// and then add it to the dictionary.
			UniformLocations.Add(key, location);
		}
	}

	private static void CompileShader(int shader) {
		// Try to compile the shader
		GL.CompileShader(shader);

		// Check for compilation errors
		GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
		if(code != (int)All.True) {
			// We can use `GL.GetShaderInfoLog(shader)` to get information about the error.
			var infoLog = GL.GetShaderInfoLog(shader);
			throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
		}
	}

	private static void LinkProgram(int program) {
		// We link the program
		GL.LinkProgram(program);

		// Check for linking errors
		GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
		if(code != (int)All.True) {
			// We can use `GL.GetProgramInfoLog(program)` to get information about the error.
			throw new Exception($"Error occurred whilst linking Program({program})");
		}
	}

	// A wrapper function that enables the shader program.
	public void Use() {
		GL.UseProgram(Handle);
	}

	// The shader sources provided with this project use hardcoded layout(location)-s. If you want to do it dynamically,
	// you can omit the layout(location=X) lines in the vertex shader, and use this in VertexAttribPointer instead of the hardcoded values.
	public int GetAttribLocation(string attribName) {
		return GL.GetAttribLocation(Handle, attribName);
	}

	/// <summary>
	/// Set a uniform int on this shader.
	/// </summary>
	/// <param name="name">The name of the uniform</param>
	/// <param name="data">The data to set</param>
	public void Set(string name, int data) {
		if(UniformLocations.TryGetValue(name, out var location)) {
			GL.UseProgram(Handle);
			GL.Uniform1(location, data);
			return;
		}
		Debug.WriteLine($"Error setting int uniform {name} | {data}!");
	}

	/// <summary>
	/// Set a uniform float on this shader.
	/// </summary>
	/// <param name="name">The name of the uniform</param>
	/// <param name="data">The data to set</param>
	public void Set(string name, float data) {
		if(UniformLocations.TryGetValue(name, out var location)) {
			GL.UseProgram(Handle);
			GL.Uniform1(location, data);
			return;
		}
		Debug.WriteLine($"Error setting float uniform {name} | {data}!");
	}

	/// <summary>
	/// Set a uniform Vector3 on this shader.
	/// </summary>
	/// <param name="name">The name of the uniform</param>
	/// <param name="data">The data to set</param>
	public void Set(string name, Vector3 data) {
		if(UniformLocations.TryGetValue(name, out var location)) {
			GL.UseProgram(Handle);
			GL.Uniform3(location, data);
			return;
		}
		Debug.WriteLine($"Error setting vec3 uniform {name} | {data}!");
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
		if(UniformLocations.TryGetValue(name, out var location)) {
			GL.UseProgram(Handle);
			GL.UniformMatrix4(location, true, ref data);
			return;
		}
		Debug.WriteLine($"Error setting mat4 uniform {name} | {data}!");
	}
}
