using Newtonsoft.Json.Linq;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;
using System.Text.Json;

namespace Vanadium;

public class Material : IDisposable, IEquatable<Material>
{
	public enum MaterialParamType
	{
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
	public Shader Shader
	{
		get
		{
			if ( _shader is null ) return ErrorMaterial.Shader;
			return _shader;
		}
		set
		{
			_shader = value;
		}
	}

	private Shader? _shader;

	public void Dispose()
	{
		Shader.Dispose();
	}

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

	private static readonly Dictionary<string, Material> PrecachedMaterials = new();

	public static string Error => "materials/core/error.vanmat";
	public static Material ErrorMaterial => Load( Error );
	public bool IsError { get; private set; } = false;
	private MaterialParameters? Parameters;
	public bool Transparent { get; private set; } = false;

	public bool Equals( Material? other )
	{
		return GetHashCode() == other?.GetHashCode();
	}

	public static Material Load( string path )
	{
		path = $"resources/{path}";

		if ( PrecachedMaterials.TryGetValue( path, out var material ) )
		{
			return material;
		}

		var mat = new Material();

		if ( !Json.ReadFromJson( path, out var data ) )
		{
			Log.Info( $"ERROR LOADING MATERIAL {path}! MATERIAL FILE NOT FOUND!" );
			mat.IsError = true;
			return mat;
		}
		var parameters = JObject.Parse( data );

		if ( !parameters.ContainsKey( "shader" ) )
		{
			Log.Info( $"ERROR LOADING MATERIAL {path}! NO VALID SHADER NAME FOUND!" );
			mat.IsError = true;
			return mat;
		}

		// our material parameters get loaded into the material by the shader, according to #material preprocessors
		try
		{
			var shadername = parameters["shader"];
			var shader = new Shader( $"resources/{shadername}.vfx", mat );
			mat.Shader = shader;
		}
		catch ( Exception ex )
		{
			Log.Info( $"ERROR LOADING MATERIAL {path}! ERROR BUILDING SHADER! {ex}" );
			mat.IsError = true;
			return mat;
		}

		// the shader will have registered the parameter types at this point already, so we can parse the actual runtime types here
		// which then get fed to the shader uniforms via Material.Use()
		var settings = mat.SerializeMaterialParameters( parameters );
		mat.Parameters = new MaterialParameters( settings );
		PrecachedMaterials.Add( path, mat );
		return mat;
	}

	// get enumerable of IRenderSetting according to the types of the data
	public IEnumerable<IRenderSetting> SerializeMaterialParameters( JObject parameters )
	{
		Log.Info( "serializing material" );
		foreach ( var param in parameters.Properties() )
		{
			Log.Info( $"{param.Name} = {param.Value}" );
			var name = param.Name;
			var value = $"{param.Value}";

			// check for global transparent property
			if ( bool.TryParse( value, out var transparent ) )
			{
				if ( name == "transparent" && transparent )
				{
					Transparent = true;
					continue;
				}
			}

			if ( MaterialParameters.TryGetValue( name, out var type ) )
			{
				Log.Info( $"serializing for {name} {value} {type}" );

				switch ( type )
				{
					case MaterialParamType.Unset:
						throw new NotImplementedException();
					case MaterialParamType.Boolean:
						Log.Info( $"parsing bool material data {name} {value}" );
						if ( bool.TryParse( value, out var bdata ) )
						{
							yield return new BoolUniform( name, bdata );
						}
						break;
					case MaterialParamType.Integer:
						Log.Info( $"parsing int material data {name} {value}" );
						if ( int.TryParse( value, out var idata ) )
						{
							yield return new IntUniform( name, idata );
						}
						break;
					case MaterialParamType.UnsignedInteger:
						Log.Info( $"parsing uint material data {name} {value}" );
						if ( uint.TryParse( value, out var uidata ) )
						{
							yield return new UIntUniform( name, uidata );
						}
						break;
					case MaterialParamType.Float:
						Log.Info( $"parsing float material data {name} {value}" );
						if ( float.TryParse( value, out var fdata ) )
						{
							yield return new FloatUniform( name, fdata );
						}
						break;
					case MaterialParamType.Double:
						Log.Info( $"parsing double material data {name} {value}" );
						if ( double.TryParse( value, out var ddata ) )
						{
							yield return new DoubleUniform( name, ddata );
						}
						break;
					case MaterialParamType.Vector2:
						Log.Info( $"parsing vec2 material data {name} {value}" );
						// TODO make wrapper for vec2 and add TryParse directly to type
						if ( Parse.TryParse( value, out Vector2 vec2data ) )
						{
							yield return new Vector2Uniform( name, vec2data );
						}
						break;
					case MaterialParamType.Vector3:
						Log.Info( $"parsing vec3 material data {name} {value}" );
						if ( Vector3.TryParse( value, out var vec3data ) )
						{
							yield return new Vector3Uniform( name, vec3data );
						}
						break;
					case MaterialParamType.Vector4:
						Log.Info( $"parsing vec4 material data {name} {value}" );
						// TODO make wrapper for vec4 and add TryParse directly to type
						if ( Parse.TryParse( value, out Vector4 vec4data ) )
						{
							yield return new Vector4Uniform( name, vec4data );
						}
						break;
					case MaterialParamType.Matrix4:
						Log.Info( $"parsing mat4 material data {name} {value}" );
						// TODO make wrapper for mat4 and add TryParse directly to type (?)
						if ( Parse.TryParse( value, out Matrix4 mat4data ) )
						{
							yield return new Matrix4Uniform( name, mat4data );
						}
						break;
					case MaterialParamType.Sampler2D:
						Log.Info( $"parsing sampler2D material data {name} {value}" );
						// just directly add the string in the material. If it's not valid, we fall back to error texture anyways
						var srgb = name == "diffuse" || name == "albedo";
						Log.Info( $"tex {name} {value} srgb: {srgb}" );
						yield return new TextureUniform( name, value, srgb ); // hacky way to get color textures as srgb
						break;
					case MaterialParamType.SamplerCube:
						Log.Info( $"parsing samplerCube material data {name} {value}" );
						// just directly add the string in the material. If it's not valid, we fall back to error texture anyways
						yield return new TextureCubeUniform( name, value );
						break;
					default:
						throw new NotImplementedException();
				}
			}
		}
	}

	public int GetAttribLocation( string attribname )
	{
		if ( IsError )
		{
			return ErrorMaterial.Shader.GetAttribLocation( attribname );
		}
		return Shader.GetAttribLocation( attribname );
	}

	/// <summary>
	/// Set a uniform int on this shader.
	/// </summary>
	/// <param name="name">The name of the uniform</param>
	/// <param name="data">The data to set</param>
	public void Set( string name, bool data )
	{
		if ( IsError )
			ErrorMaterial.Shader.Set( name, data );
		else
			Shader.Set( name, data );
	}

	/// <summary>
	/// Set a uniform int on this shader.
	/// </summary>
	/// <param name="name">The name of the uniform</param>
	/// <param name="data">The data to set</param>
	public void Set( string name, int data )
	{
		if ( IsError )
			ErrorMaterial.Shader.Set( name, data );
		else
			Shader.Set( name, data );
	}

	/// <summary>
	/// Set a uniform float on this shader.
	/// </summary>
	/// <param name="name">The name of the uniform</param>
	/// <param name="data">The data to set</param>
	public void Set( string name, float data )
	{
		if ( IsError )
			ErrorMaterial.Shader.Set( name, data );
		else
			Shader.Set( name, data );
	}

	/// <summary>
	/// Set a uniform int on this shader.
	/// </summary>
	/// <param name="name">The name of the uniform</param>
	/// <param name="data">The data to set</param>
	public void Set( string name, double data )
	{
		if ( IsError )
			ErrorMaterial.Shader.Set( name, data );
		else
			Shader.Set( name, data );
	}

	/// <summary>
	/// Set a uniform Vector3 on this shader.
	/// </summary>
	/// <param name="name">The name of the uniform</param>
	/// <param name="data">The data to set</param>
	public void Set( string name, Vector2 data )
	{
		if ( IsError )
			ErrorMaterial.Shader.Set( name, data );
		else
			Shader.Set( name, data );
	}

	/// <summary>
	/// Set a uniform Vector3 on this shader.
	/// </summary>
	/// <param name="name">The name of the uniform</param>
	/// <param name="data">The data to set</param>
	public void Set( string name, Vector3 data )
	{
		if ( IsError )
			ErrorMaterial.Shader.Set( name, data );
		else
			Shader.Set( name, data );
	}

	/// <summary>
	/// Set a uniform Vector3 on this shader.
	/// </summary>
	/// <param name="name">The name of the uniform</param>
	/// <param name="data">The data to set</param>
	public void Set( string name, Vector4 data )
	{
		if ( IsError )
			ErrorMaterial.Shader.Set( name, data );
		else
			Shader.Set( name, data );
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
		if ( IsError )
			ErrorMaterial.Shader.Set( name, data );
		else
			Shader.Set( name, data );
	}

	/// <summary>
	/// Use this Material's shader in the current GL context and set all material parameters.
	/// </summary>
	public void Use()
	{

		if ( IsError )
		{
			ErrorMaterial.Use();
			return;
		}

		Shader.Use();

		Parameters?.Set( Shader );
	}

	// serialize string type to type enum
	public void AddParameter( string type, string name )
	{
		Log.Info( $"adding parameter {type} {name}" );

		var paramtype = type switch
		{
			"bool" => MaterialParamType.Boolean,
			"int" => MaterialParamType.Integer,
			"uint" => MaterialParamType.UnsignedInteger,
			"float" => MaterialParamType.Float,
			"double" => MaterialParamType.Double,
			"sampler2D" => MaterialParamType.Sampler2D,
			"samplerCube" => MaterialParamType.SamplerCube,
			"vec2" => MaterialParamType.Vector2,
			"vec3" => MaterialParamType.Vector3,
			"vec4" => MaterialParamType.Vector4,
			"mat4" => MaterialParamType.Matrix4,
			_ => throw new NotImplementedException()
		};

		MaterialParameters.Add( name, paramtype );
	}

	public override bool Equals( object? obj )
	{
		return obj is Material material && Equals( material );
	}

	public static bool operator ==( Material left, Material right )
	{
		return left.Equals( right );
	}

	public static bool operator !=( Material left, Material right )
	{
		return !(left == right);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
