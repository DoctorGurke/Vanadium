using OpenTK.Mathematics;

namespace Vanadium;

public class SceneLightManager
{
	private Color AmbientLightColor;
	private int NumPointLights;
	private PointLight[] Lights;

	public static int MaxPointLights => 512;

	public struct PointLight
	{
		public Vector4 Position;
		public Vector4 Color;
		public Vector4 Attenuation;
	}

	public SceneLightManager()
	{
		Lights = new PointLight[MaxPointLights];
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

		Log.Info( $"new light {light} {position} {color} {constant} {linear} {quadratic}" );
		var lightmodel = new SceneObject
		{
			Model = Model.Primitives.Sphere,
			Scale = 0.1f,
			Position = position
		};
		lightmodel.RenderColor = color;

		Lights[light].Position = new Vector4( position );
		Lights[light].Color = color;
		Lights[light].Attenuation = new Vector4( constant, linear, quadratic, 0.0f );
		NumPointLights++;
		// update whole buffer for now, this should use sub data later on
		UniformBufferManager.Current.UpdatePointlights( Lights, NumPointLights );
	}
}
