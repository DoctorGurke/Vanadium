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
	public static int MaxSpotLights => 128;
	public static int MaxDirLights => 16;

	// 48 bytes
	public struct PointLight
	{
		public Vector3 Position;
		public float Constant;
		public Vector3 Color;
		public float Linear;
		public float Quadratic;
		public float Brightness;
		public float Pad;
		public float Pad1;
	}

	// 64 bytes
	public struct SpotLight
	{
		public Vector3 Position;
		public float Constant;
		public Vector3 Direction;
		public float Linear;
		public Vector3 Color;
		public float Quadratic;
		public float Brightness;
		public float InnerAngle;
		public float OuterAngle;
		public float Pad;
	}

	// 32 bytes
	public struct DirLight
	{
		public Vector3 Direction;
		public float Brightness;
		public Vector3 Color;
		public float Pad;
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
		AddPointlight( position, Color.White );
	}

	public void AddPointlight( Vector3 position, Color color, float constant = 0.0f, float linear = 0.0f, float quadratic = 1.0f, float brightness = 1.0f )
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

		PointLights[light].Position = position;
		PointLights[light].Color = new Vector3( color.r, color.g, color.b );
		PointLights[light].Constant = constant;
		PointLights[light].Linear = linear;
		PointLights[light].Quadratic = quadratic;
		PointLights[light].Brightness = brightness;
		NumPointLights++;
		// update whole buffer for now, this should use sub data later on
		UniformBufferManager.Current?.UpdatePointlights( PointLights, NumPointLights );
	}

	public void AddSpotlight( Vector3 position, Rotation rotation )
	{
		AddSpotlight( position, rotation, Color.White, 30, 35 );
	}

	public void AddSpotlight( Vector3 position, Rotation rotation, Color color )
	{
		AddSpotlight( position, rotation, color, 30, 35 );
	}

	public void AddSpotlight( Vector3 position, Rotation rotation, Color color, float innerangle, float outerangle, float constant = 0.0f, float linear = 0.0f, float quadratic = 1.0f, float brightness = 1.0f )
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

		SpotLights[light].Position = position;
		SpotLights[light].Direction = rotation.Forward;
		SpotLights[light].Color = new Vector3( color.r, color.g, color.b );
		SpotLights[light].Constant = constant;
		SpotLights[light].Linear = linear;
		SpotLights[light].Quadratic = quadratic;
		SpotLights[light].Brightness = brightness;
		SpotLights[light].InnerAngle = innerangle.DegreeToRadian();
		SpotLights[light].OuterAngle = outerangle.DegreeToRadian();
		NumSpotLights++;
		// update whole buffer for now, this should use sub data later on
		UniformBufferManager.Current?.UpdateSpotlights( SpotLights, NumSpotLights );
	}

	public void AddDirLight( Rotation rotation )
	{
		AddDirLight( rotation, Color.White );
	}

	public void AddDirLight( Rotation rotation, Color color, float brightness = 1.0f )
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

		DirLights[light].Direction = rotation.Forward;
		DirLights[light].Color = new Vector3( color.r, color.g, color.b );
		DirLights[light].Brightness = brightness;
		NumDirLights++;
		UniformBufferManager.Current?.UpdateDirlights( DirLights, NumDirLights );
	}
}
