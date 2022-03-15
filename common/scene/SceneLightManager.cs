using OpenTK.Mathematics;

namespace Vanadium;

public class SceneLightManager
{
	private Color AmbientLightColor;
	private int NumPointLights;
	private int NumSpotLights;
	private PointLight[] PointLights;
	private SpotLight[] SpotLights;

	public static int MaxPointLights => 256;
	public static int MaxSpotLights => 256;

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
		public Vector4 Attenuation; // constant, linear, quadratic
		public Vector4 Params; // inner angle, outer angle
	}

	public SceneLightManager()
	{
		PointLights = new PointLight[MaxPointLights];
		SpotLights = new SpotLight[MaxSpotLights];
	}

	public void SetAmbientLightColor( Color col )
	{
		AmbientLightColor = col.WithAlpha( 1.0f );
		Log.Info( $"new ambient light color {col}" );
		UniformBufferManager.Current.UpdateAmbientLightColor( AmbientLightColor );
	}

	public void AddPointlight( Vector3 position )
	{
		AddPointlight( position, Color.White, 0.0f, 0.0f, 1.0f );
	}

	public void AddPointlight( Vector3 position, Color color )
	{
		AddPointlight( position, color, 0.0f, 0.0f, 1.0f );
	}

	public void AddPointlight( Vector3 position, Color color, float constant, float linear, float quadratic )
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
		PointLights[light].Attenuation = new Vector4( constant, linear, quadratic, 0.0f );
		NumPointLights++;
		// update whole buffer for now, this should use sub data later on
		UniformBufferManager.Current.UpdatePointlights( PointLights, NumPointLights );
	}

	public void AddSpotlight( Vector3 position, Rotation rotation, Color color, float innerangle, float outerangle, float constant, float linear, float quadratic )
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
		SpotLights[light].Attenuation = new Vector4( constant, linear, quadratic, 0.0f );
		SpotLights[light].Params = new Vector4( innerangle.DegreeToRadian(), outerangle.DegreeToRadian(), 0.0f, 0.0f );
		NumSpotLights++;
		// update whole buffer for now, this should use sub data later on
		UniformBufferManager.Current.UpdateSpotlights( SpotLights, NumSpotLights );
	}
}
