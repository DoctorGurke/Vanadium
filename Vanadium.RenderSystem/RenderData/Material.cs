﻿using Newtonsoft.Json.Linq;
using Vanadium.RenderSystem.RenderData.MaterialUniforms;

namespace Vanadium.RenderSystem.RenderData;

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
		SamplerCube,
		SamplerHDR
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
		GC.SuppressFinalize( this );
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
	public bool Translucent { get; private set; } = false;

	public bool Equals( Material? other )
	{
		return GetHashCode() == other?.GetHashCode();
	}

	public static Material Load( string path )
	{
		path = $"core/{path}";

		if ( PrecachedMaterials.TryGetValue( path, out var material ) )
		{
			return material;
		}

		var mat = new Material();

		if ( !Json.ReadFromJson( path, out var data ) )
		{
			Log.Warning( $"Error loading material: {path} : File not found" );
			mat.IsError = true;
			return mat;
		}
		var parameters = JObject.Parse( data );

		if ( !parameters.ContainsKey( "shader" ) )
		{
			Log.Warning( $"Error loading material: {path} : Invalid shader name" );
			mat.IsError = true;
			return mat;
		}

		// our material parameters get loaded into the material by the shader, according to #material preprocessors
		try
		{
			var shadername = parameters["shader"];
			var shader = new Shader( $"core/{shadername}.vfx", mat );
			mat.Shader = shader;
		}
		catch ( Exception )
		{
			Log.Warning( $"Error loading material: {path} : Error building shader" );
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
		foreach ( var param in parameters.Properties() )
		{
			var name = param.Name;
			var value = $"{param.Value}";

			// check for translucent property
			if ( bool.TryParse( value, out var translucent ) )
			{
				if ( name == "translucent" && translucent )
				{
					Translucent = true;
					continue;
				}
			}

			if ( MaterialParameters.TryGetValue( name, out var type ) )
			{
				switch ( type )
				{
					case MaterialParamType.Unset:
						throw new NotImplementedException();
					case MaterialParamType.Boolean:
						if ( bool.TryParse( value, out var bdata ) )
						{
							yield return new BoolUniform( name, bdata );
						}
						break;
					case MaterialParamType.Integer:
						if ( int.TryParse( value, out var idata ) )
						{
							yield return new IntUniform( name, idata );
						}
						break;
					case MaterialParamType.UnsignedInteger:
						if ( uint.TryParse( value, out var uidata ) )
						{
							yield return new UIntUniform( name, uidata );
						}
						break;
					case MaterialParamType.Float:
						if ( float.TryParse( value, out var fdata ) )
						{
							yield return new FloatUniform( name, fdata );
						}
						break;
					case MaterialParamType.Double:
						if ( double.TryParse( value, out var ddata ) )
						{
							yield return new DoubleUniform( name, ddata );
						}
						break;
					case MaterialParamType.Vector2:
						// TODO make wrapper for vec2 and add TryParse directly to type
						if ( Parse.TryParse( value, out OpenTKMath.Vector2 vec2data ) )
						{
							yield return new Vector2Uniform( name, vec2data );
						}
						break;
					case MaterialParamType.Vector3:
						if ( Vector3.TryParse( value, out var vec3data ) )
						{
							yield return new Vector3Uniform( name, vec3data );
						}
						break;
					case MaterialParamType.Vector4:
						// TODO make wrapper for vec4 and add TryParse directly to type
						if ( Parse.TryParse( value, out OpenTKMath.Vector4 vec4data ) )
						{
							yield return new Vector4Uniform( name, vec4data );
						}
						break;
					case MaterialParamType.Matrix4:
						// TODO make wrapper for mat4 and add TryParse directly to type (?)
						if ( Parse.TryParse( value, out OpenTKMath.Matrix4 mat4data ) )
						{
							yield return new Matrix4Uniform( name, mat4data );
						}
						break;
					case MaterialParamType.Sampler2D:
						var srgb = name == "diffuse" || name == "albedo";// hacky way to get color textures as srgb, the shader should define this for its texture sampler in the future
						yield return new TextureUniform( name, Texture.Load2D( value, true, srgb ) );
						break;
					case MaterialParamType.SamplerCube:
						yield return new TextureCubeUniform( name, Texture.LoadCube( value, true ) );
						break;
					case MaterialParamType.SamplerHDR:
						yield return new TextureCubeUniform( name, Texture.LoadHDR( value ) );
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
	public void Set( string name, OpenTKMath.Vector2 data )
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
	public void Set( string name, OpenTKMath.Vector4 data )
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
	public void Set( string name, OpenTKMath.Matrix4 data )
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
		var paramtype = type switch
		{
			"bool" => MaterialParamType.Boolean,
			"int" => MaterialParamType.Integer,
			"uint" => MaterialParamType.UnsignedInteger,
			"float" => MaterialParamType.Float,
			"double" => MaterialParamType.Double,
			"sampler2D" => MaterialParamType.Sampler2D,
			"samplerCube" => MaterialParamType.SamplerCube,
			"samplerHDR" => MaterialParamType.SamplerHDR,
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
