using OpenTK.Mathematics;

namespace Vanadium;

public class SceneLightManager
{
	private Color AmbientLightColor;
	private int NumPointLights;
	private int NumSpotLights;
	private int NumDirLights;
	private readonly PointLight[] PointLights;
	private readonly SpotLight[] SpotLights;
	private readonly DirLight[] DirLights;

	// Values need to match common.glsl
	public static int MaxPointLights => 128;
	public static int MaxSpotLights => 64;
	public static int MaxDirLights => 16;

	public struct PointLight
	{
		public Vector4 Position;
		public Vector4 Color;
		public Vector4 Attenuation; // constant, linear, quadratic
	}

	public struct SpotLight
	{
		public Vector4 Position;
		public Vector4 Direction;
		public Vector4 Color;
		public Vector4 Attenuation; // constant, linear, quadratic, brightness mul
		public Vector4 Params; // inner angle, outer angle
	}

	public struct DirLight
	{
		public Vector4 Direction;
		public Vector4 Color;
	}

	public SceneLightManager()
	{
		PointLights = new PointLight[MaxPointLights];
		SpotLights = new SpotLight[MaxSpotLights];
		DirLights = new DirLight[MaxDirLights];
	}

	public void SetAmbientLightColor( Color col )
	{
		AmbientLightColor = col.WithAlpha( 1.0f );
		UniformBufferManager.Current?.UpdateAmbientLightColor( AmbientLightColor );
	}

	public void AddPointlight( Vector3 position )
	{
		AddPointlight( position, Color.White, 0.0f, 0.0f, 1.0f );
	}

	public void AddPointlight( Vector3 position, Color color )
	{
		AddPointlight( position, color, 0.0f, 0.0f, 1.0f );
	}

	public void AddPointlight( Vector3 position, Color color, float constant, float linear, float quadratic, float brightness = 1.0f )
	{
		var light = NumPointLights; // current number is index for new light (ie, 0 lights means insert at index 0)
		if ( light >= MaxPointLights )
		{
			Log.Info( $"UNABLE TO ADD MORE POINT LIGHTS {MaxPointLights}" );
			return;
		}

		Log.Info( $"new pointlight {light} {position} {color} {constant} {linear} {quadratic}" );
		var lightmodel = new SceneObject
		{
			Model = Model.Primitives.Sphere,
			Scale = 0.1f,
			Position = position
		};
		lightmodel.RenderColor = color;

		PointLights[light].Position = new Vector4( position );
		PointLights[light].Color = color;
		PointLights[light].Attenuation = new Vector4( constant, linear, quadratic, brightness );
		NumPointLights++;
		// update whole buffer for now, this should use sub data later on
		UniformBufferManager.Current?.UpdatePointlights( PointLights, NumPointLights );
	}

	public void AddSpotlight( Vector3 position, Rotation rotation )
	{
		AddSpotlight( position, rotation, Color.White, 30, 35, 0, 0, 1 );
	}

	public void AddSpotlight( Vector3 position, Rotation rotation, Color color )
	{
		AddSpotlight( position, rotation, color, 30, 35, 0, 0, 1 );
	}

	public void AddSpotlight( Vector3 position, Rotation rotation, Color color, float innerangle, float outerangle, float constant, float linear, float quadratic, float brightness = 1.0f )
	{
		var light = NumSpotLights; // current number is index for new light (ie, 0 lights means insert at index 0)
		if ( light >= MaxSpotLights )
		{
			Log.Info( $"UNABLE TO ADD MORE POINT LIGHTS {MaxSpotLights}" );
			return;
		}

		Log.Info( $"new spotlight {light} {position} {rotation.Forward} {color} {innerangle} {outerangle} {constant} {linear} {quadratic}" );
		var lightmodel = new SceneObject
		{
			Model = Model.Primitives.ForwardCone,
			Scale = 0.1f,
			Position = position,
			Rotation = rotation
		};
		lightmodel.RenderColor = color;

		SpotLights[light].Position = new Vector4( position );
		SpotLights[light].Direction = new Vector4( rotation.Forward );
		SpotLights[light].Color = color;
		SpotLights[light].Attenuation = new Vector4( constant, linear, quadratic, brightness );
		SpotLights[light].Params = new Vector4( innerangle.DegreeToRadian(), outerangle.DegreeToRadian(), 0.0f, 0.0f );
		NumSpotLights++;
		// update whole buffer for now, this should use sub data later on
		UniformBufferManager.Current?.UpdateSpotlights( SpotLights, NumSpotLights );
	}

	public void AddDirLight( Rotation rotation )
	{
		AddDirLight( rotation, Color.White );
	}

	public void AddDirLight( Rotation rotation, Color color )
	{
		var light = NumDirLights; // current number is index for new light (ie, 0 lights means insert at index 0)
		if ( light >= MaxDirLights )
		{
			Log.Info( $"UNABLE TO ADD MORE DIRECTIONAL LIGHTS {MaxDirLights}" );
			return;
		}

		Log.Info( $"new dirlight {light} {rotation.Forward} {color}" );
		var lightmodel = new SceneObject
		{
			Model = Model.Primitives.ForwardCone,
			Scale = 0.1f,
			Position = Vector3.Zero,
			Rotation = rotation
		};
		lightmodel.RenderColor = color;

		DirLights[light].Direction = new Vector4( rotation.Forward );
		DirLights[light].Color = color;
		NumDirLights++;
		UniformBufferManager.Current?.UpdateDirlights( DirLights, NumDirLights );
	}
}
