using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace Vanadium;

// A simple class meant to help create shaders
public class Shader : IDisposable
{
	public readonly int Handle;

	public Dictionary<string, int> UniformLocations { get; private set; } = new Dictionary<string, int>();

	public Shader( string vertPath, string fragPath, Material material )
	{
		// load vertex shader and compile
		var shaderSource = Load( vertPath, material );
		GLUtil.CreateShader( ShaderType.VertexShader, "Vert", Path.GetFileName( vertPath ), out int vertexShader );
		GL.ShaderSource( vertexShader, shaderSource );
		CompileShader( vertexShader );

		// load fragment shader and compile
		shaderSource = Load( fragPath, material );
		GLUtil.CreateShader( ShaderType.FragmentShader, "Frag", Path.GetFileName( fragPath ), out int fragmentShader );
		GL.ShaderSource( fragmentShader, shaderSource );
		CompileShader( fragmentShader );

		// create opengl shader program
		GLUtil.CreateProgram( $"{Path.GetFileName( vertPath )}-{Path.GetFileName( fragPath )}", out Handle );

		// attach vert + fragment shaders and link
		GL.AttachShader( Handle, vertexShader );
		GL.AttachShader( Handle, fragmentShader );
		LinkProgram( Handle );

		// When the shader program is linked, it no longer needs the individual shaders attached to it; the compiled code is copied into the shader program.
		// Detach them, and then delete them.
		GL.DetachShader( Handle, vertexShader );
		GL.DetachShader( Handle, fragmentShader );
		GL.DeleteShader( fragmentShader );
		GL.DeleteShader( vertexShader );

		// The shader is now ready to go, but first, we're going to cache all the shader uniform locations.
		// Querying this from the shader is very slow, so we do it once on initialization and reuse those values
		// later.

		// First, we have to get the number of active uniforms in the shader.
		GL.GetProgram( Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms );

		// bind per view uniform buffer
		var matricesblockindex = GL.GetUniformBlockIndex( Handle, "PerViewUniformBuffer" );
		if ( matricesblockindex >= 0 )
		{
			GL.UniformBlockBinding( Handle, matricesblockindex, 0 );
		}

		// bind per view uniform light buffer
		var lightblockindex = GL.GetUniformBlockIndex( Handle, "PerViewLightingUniformBuffer" );
		if ( lightblockindex >= 0 )
		{
			GL.UniformBlockBinding( Handle, lightblockindex, 1 );
		}

		// Loop over all the uniforms,

		for ( var i = 0; i < numberOfUniforms; i++ )
		{
			// get the name of this uniform,
			var key = GL.GetActiveUniform( Handle, i, out _, out _ );

			// get the location,
			var location = GL.GetUniformLocation( Handle, key );

			// and then add it to the dictionary.
			UniformLocations.Add( key, location );
		}
	}

	public static string Load( string path, Material material )
	{
		var data = File.ReadAllText( $"resources/{path}" );

		// handle any includes the shader file might have
		data = HandleIncludes( data, path, material );
		data = HandleMaterial( data, path, material );

		return data;
	}

	private static string HandleIncludes( string data, string path, Material mat )
	{
		Log.Info( $"handling includes for {path}" );
		// scan data for any #include macros
		var regex = @"#include[\s](.+)";
		var includeMatches = Regex.Matches( data, regex );

		foreach ( Match match in includeMatches )
		{
			var matchstring = $"{match}".Clean();
			var includePath = $"{match.Groups[1]}".Clean();
			Log.Info( $"include found ({includePath})" );

			// make sure a file isn't trying to include itself
			if ( includePath == path )
			{
				Log.Info( $"Recursive include for {includePath} found! Defusing..." );
				data = data.Replace( matchstring, "" );
				continue;
			}

			var includeData = Load( includePath, mat );
			data = data.Replace( matchstring, includeData );
		}

		return data;
	}

	private static string HandleMaterial( string data, string path, Material material )
	{
		Log.Info( $"handling material for {path}" );

		// scan data for any #material macros
		var regex = @"#material[\s]\W*(bool|int|uint|float|double|sampler2D|samplerCube|vec2|vec3|vec4|mat4)\W*[\s](.+)";
		var includeMatches = Regex.Matches( data, regex );

		foreach ( Match match in includeMatches )
		{
			var matchstring = $"{match}".Clean();
			var type = $"{match.Groups[1]}".Clean();
			var name = $"{match.Groups[2]}".Clean();

			var field = $"uniform {type} {name};";

			Log.Info( $"material found ({field})" );
			material.AddParameter( type, name );

			data = data.Replace( matchstring, field );
		}

		return data;
	}

	public int GetUniformLocation( string name )
	{
		return GL.GetUniformLocation( Handle, name );
	}

	private static void CompileShader( int shader )
	{
		// Try to compile the shader
		GL.CompileShader( shader );

		// Check for compilation errors
		GL.GetShader( shader, ShaderParameter.CompileStatus, out var code );
		if ( code != (int)All.True )
		{
			// We can use `GL.GetShaderInfoLog(shader)` to get information about the error.
			var infoLog = GL.GetShaderInfoLog( shader );
			throw new Exception( $"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}" );
		}
	}

	private static void LinkProgram( int program )
	{
		// We link the program
		GL.LinkProgram( program );

		// Check for linking errors
		GL.GetProgram( program, GetProgramParameterName.LinkStatus, out var code );
		if ( code != (int)All.True )
		{
			// We can use `GL.GetProgramInfoLog(program)` to get information about the error.
			throw new Exception( $"Error occurred whilst linking Program({program})" );
		}
	}

	// A wrapper function that enables the shader program.
	public void Use()
	{
		GL.UseProgram( Handle );
	}

	// The shader sources provided with this project use hardcoded layout(location)-s. If you want to do it dynamically,
	// you can omit the layout(location=X) lines in the vertex shader, and use this in VertexAttribPointer instead of the hardcoded values.
	public int GetAttribLocation( string attribName )
	{
		return GL.GetAttribLocation( Handle, attribName );
	}

	/// <summary>
	/// Set a uniform int on this shader.
	/// </summary>
	/// <param name="name">The name of the uniform</param>
	/// <param name="data">The data to set</param>
	public void Set( string name, bool data )
	{
		Set( name, data ? 1 : 0 );
	}

	/// <summary>
	/// Set a uniform int on this shader.
	/// </summary>
	/// <param name="name">The name of the uniform</param>
	/// <param name="data">The data to set</param>
	public void Set( string name, int data )
	{
		if ( UniformLocations.TryGetValue( name, out var location ) )
		{
			GL.UseProgram( Handle );
			GL.Uniform1( location, data );
			return;
		}
		//Debug.WriteLine($"Error setting int uniform {name} | {data}!");
	}

	/// <summary>
	/// Set a uniform float on this shader.
	/// </summary>
	/// <param name="name">The name of the uniform</param>
	/// <param name="data">The data to set</param>
	public void Set( string name, float data )
	{
		if ( UniformLocations.TryGetValue( name, out var location ) )
		{
			GL.UseProgram( Handle );
			GL.Uniform1( location, data );
			return;
		}
		//Debug.WriteLine($"Error setting float uniform {name} | {data}!");
	}

	/// <summary>
	/// Set a uniform int on this shader.
	/// </summary>
	/// <param name="name">The name of the uniform</param>
	/// <param name="data">The data to set</param>
	public void Set( string name, double data )
	{
		if ( UniformLocations.TryGetValue( name, out var location ) )
		{
			GL.UseProgram( Handle );
			GL.Uniform1( location, data );
			return;
		}
		//Debug.WriteLine($"Error setting int uniform {name} | {data}!");
	}

	/// <summary>
	/// Set a uniform Vector3 on this shader.
	/// </summary>
	/// <param name="name">The name of the uniform</param>
	/// <param name="data">The data to set</param>
	public void Set( string name, Vector2 data )
	{
		if ( UniformLocations.TryGetValue( name, out var location ) )
		{
			GL.UseProgram( Handle );
			GL.Uniform2( location, data );
			return;
		}
		//Debug.WriteLine($"Error setting vec3 uniform {name} | {data}!");
	}

	/// <summary>
	/// Set a uniform Vector3 on this shader.
	/// </summary>
	/// <param name="name">The name of the uniform</param>
	/// <param name="data">The data to set</param>
	public void Set( string name, Vector3 data )
	{
		if ( UniformLocations.TryGetValue( name, out var location ) )
		{
			GL.UseProgram( Handle );
			GL.Uniform3( location, data );
			return;
		}
		//Debug.WriteLine($"Error setting vec3 uniform {name} | {data}!");
	}

	/// <summary>
	/// Set a uniform Vector3 on this shader.
	/// </summary>
	/// <param name="name">The name of the uniform</param>
	/// <param name="data">The data to set</param>
	public void Set( string name, Vector4 data )
	{
		if ( UniformLocations.TryGetValue( name, out var location ) )
		{
			GL.UseProgram( Handle );
			GL.Uniform4( location, data );
			return;
		}
		//Debug.WriteLine($"Error setting vec3 uniform {name} | {data}!");
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
	public void Set( string name, Matrix4 data )
	{
		if ( UniformLocations.TryGetValue( name, out var location ) )
		{
			GL.UseProgram( Handle );
			GL.UniformMatrix4( location, true, ref data );
			return;
		}
		//Debug.WriteLine($"Error setting mat4 uniform {name} | {data}!");
	}

	public void Dispose()
	{
		GL.DeleteShader( Handle );
		GC.SuppressFinalize( this );
	}
}
