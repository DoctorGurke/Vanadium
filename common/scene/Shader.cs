using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Text;

namespace Vanadium;

// A simple class meant to help create shaders
public class Shader : IDisposable
{
	public readonly int Handle;

	public Dictionary<string, int> UniformLocations { get; private set; } = new Dictionary<string, int>();

	public Shader( string path, Material material )
	{
		var container = LoadContainer( path );

		if ( !VerifyIntegrity( container ) ) throw new Exception("Shader missing either Vertex or Fragment components!");

		// load vertex shader and compile
		var vertsource = container.First( x => x.Key == ShaderType.VertexShader ).Value;
		vertsource = HandleIncludes( vertsource, path, material );
		vertsource = HandleMaterial( vertsource, path, material );
		GLUtil.CreateShader( ShaderType.VertexShader, "Vert", Path.GetFileName( path ), out int vertexShader );
		GL.ShaderSource( vertexShader, vertsource );
		CompileShader( vertexShader );

		// see if there's a geo shader
		int geometryShader = -1;
		if(container.Any(x => x.Key == ShaderType.GeometryShader))
		{
			// load geo shader and compile
			var geosource = container.First( x => x.Key == ShaderType.GeometryShader ).Value;
			geosource = HandleIncludes( geosource, path, material );
			geosource = HandleMaterial( geosource, path, material );
			GLUtil.CreateShader( ShaderType.GeometryShader, "Geo", Path.GetFileName( path ), out geometryShader );
			GL.ShaderSource( vertexShader, geosource );
			CompileShader( vertexShader );
		}

		// load fragment shader and compile
		var fragsource = container.First( x => x.Key == ShaderType.FragmentShader ).Value;
		fragsource = HandleIncludes( fragsource, path, material );
		fragsource = HandleMaterial( fragsource, path, material );
		GLUtil.CreateShader( ShaderType.FragmentShader, "Frag", Path.GetFileName( path ), out int fragmentShader );
		GL.ShaderSource( fragmentShader, fragsource );
		CompileShader( fragmentShader );

		// create opengl shader program
		GLUtil.CreateProgram( $"Shader: {Path.GetFileName( path )}", out Handle );

		// attach vert + (geo) + fragment shaders and link
		GL.AttachShader( Handle, vertexShader );
		// link if exists
		if ( geometryShader >= 0 )
			GL.AttachShader( Handle, geometryShader );
		GL.AttachShader( Handle, fragmentShader );
		LinkProgram( Handle );

		// detatch shaders
		GL.DetachShader( Handle, vertexShader );
		// detach if exists
		if ( geometryShader >= 0 )
			GL.DetachShader( Handle, geometryShader );
		GL.DetachShader( Handle, fragmentShader );

		// delete shaders
		GL.DeleteShader( fragmentShader );
		// delete if exists
		if ( geometryShader >= 0 )
			GL.DetachShader( Handle, geometryShader );
		GL.DeleteShader( vertexShader );

		InitUniforms();
	}

	/// <summary>
	/// Init uniform dictionary and uniform block bindings
	/// </summary>
	private void InitUniforms()
	{
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

	/// <summary>
	/// Check if a serialized shader container collection has the minimum required components.
	/// </summary>
	/// <param name="sections"></param>
	/// <returns>True if it contains a vertex and fragment component.</returns>
	private static bool VerifyIntegrity( IEnumerable<KeyValuePair<ShaderType, string>> sections )
	{
		// check if shader has both vertex and fragment components
		if ( sections.Any( x => x.Key == ShaderType.VertexShader ) && sections.Any( x => x.Key == ShaderType.FragmentShader ) )
			return true;
		return false;
	}

	private static readonly Dictionary<string, ShaderType> shadertypes = new Dictionary<string, ShaderType>()
	{
		{"#VS", ShaderType.VertexShader},
		{"#VERT", ShaderType.VertexShader},
		{"#VERTEX", ShaderType.VertexShader},
		{"#FS", ShaderType.FragmentShader},
		{"#FRAG", ShaderType.FragmentShader},
		{"#FRAGMENT", ShaderType.FragmentShader},
		{"#GS", ShaderType.GeometryShader},
		{"#GEO", ShaderType.GeometryShader},
		{"#GEOMETRY", ShaderType.GeometryShader},
	};

	/// <summary>
	/// Split a shader container into the individual parts.
	/// </summary>
	/// <param name="path">The path to the shader file.</param>
	/// <returns>A Key value pair of the shader section and the shader type.</returns>
	private static IEnumerable<KeyValuePair<ShaderType, string>> LoadContainer( string path )
	{
		using var stream = new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite );
		using ( var sr = new StreamReader( stream ) )
		{
			var builder = new StringBuilder();
			ShaderType curtype = 0;

			string? line;
			while ( (line = sr.ReadLine()) != null )
			{
				// special case if we've reached the end of the file
				if ( sr.EndOfStream && curtype != 0 )
				{
					builder.Append( line );
					yield return new KeyValuePair<ShaderType, string>( curtype, builder.ToString() );
					continue;
				}

				// new shader section started, check lookup dict for key (#VS, #FS, #GS)
				if ( shadertypes.TryGetValue( line.Trim(), out var type ) )
				{
					// return code block as shader type
					if ( curtype != 0 )
					{
						yield return new KeyValuePair<ShaderType, string>( curtype, builder.ToString() );
					}
					curtype = type;

					// reset builder
					builder.Clear();
				}
				else
				{
					builder.AppendLine( line );
				}
			}
		};
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
			Log.Info(GL.GetProgramInfoLog( program ));
			// We can use `GL.GetProgramInfoLog(program)` to get information about the error.
			throw new Exception( $"Error occurred whilst linking Program({program}) {code}" );
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
			//GL.UseProgram( Handle );
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
			//GL.UseProgram( Handle );
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
			//GL.UseProgram( Handle );
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
			//GL.UseProgram( Handle );
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
			//GL.UseProgram( Handle );
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
			//GL.UseProgram( Handle );
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
			//GL.UseProgram( Handle );
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
