using OpenTK.Mathematics;

namespace Vanadium;

public class SceneLightManager
{
	private Color AmbientLightColor;
	private int NumPointLights;
	private UniformBufferManager.Light[] Lights;

	public static int MaxPointLights => 128;

	public SceneLightManager()
	{
		Lights = new UniformBufferManager.Light[MaxPointLights];
	}

	public void SetAmbientLightColor( Color col )
	{
		AmbientLightColor = col.WithAlpha( 1.0f );
		Log.Info($"new ambient light color {col}");
		UniformBufferManager.Current.UpdateAmbientLightColor( AmbientLightColor );
	}

	public void AddPointlight(Vector3 position)
	{
		AddPointlight( position, Color.White, 0.0f, 0.0f, 1.0f );
	}

	public void AddPointlight(Vector3 position, Color color)
	{
		AddPointlight( position, color, 0.0f, 0.0f, 1.0f );
	}

	public void AddPointlight( Vector3 position, Color color, float constant, float linear, float quadratic )
	{
		var light = NumPointLights; // current number is index for new light (ie, 0 lights means insert at index 0)
		Log.Info( $"new light {light} {position} {color} {constant} {linear} {quadratic}" );
		Lights[light].Position = new Vector4( position );
		Lights[light].Color = color.WithAlpha( 1.0f );
		Lights[light].Params = new Vector4( constant, linear, quadratic, 0.0f );
		NumPointLights++;
		// update whole buffer for now, this should use sub data later on
		UniformBufferManager.Current.UpdatePointlights(Lights, NumPointLights);
	}
}
