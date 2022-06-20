﻿using Vanadium.Renderer.RenderData.Buffers;

namespace Vanadium.Renderer.Scene;

public class SceneLightManager
{
	private Color AmbientLightColor;
	private int NumPointLights;
	private int NumSpotLights;
	private int NumDirLights;
	private readonly List<PointLight> PointLights = new();
	private readonly List<SpotLight> SpotLights = new();
	private readonly List<DirLight> DirLights = new();

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
		var index = NumPointLights; // current number is index for new light (ie, 0 lights means insert at index 0)
		if ( index >= MaxPointLights )
		{
			Log.Info( $"UNABLE TO ADD MORE POINT LIGHTS {MaxPointLights}" );
			return;
		}

		Log.Info( $"new pointlight {index} {position} {color} {constant} {linear} {quadratic}" );
		var lightmodel = new SceneObject
		{
			Model = Model.Primitives.Sphere,
			Scale = 0.1f,
			Position = position
		};
		lightmodel.RenderColor = color;

		// init and add light
		var light = new PointLight();
		light.Position = position;
		light.Color = new Vector3( color.r, color.g, color.b );
		light.Constant = constant;
		light.Linear = linear;
		light.Quadratic = quadratic;
		light.Brightness = brightness;
		PointLights.Add( light );

		NumPointLights++;
		// update whole buffer for now, this should use sub data later on
		UniformBufferManager.Current?.UpdatePointlights( PointLights.ToArray(), NumPointLights );
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
		var index = NumSpotLights; // current number is index for new light (ie, 0 lights means insert at index 0)
		if ( index >= MaxSpotLights )
		{
			Log.Info( $"UNABLE TO ADD MORE POINT LIGHTS {MaxSpotLights}" );
			return;
		}

		Log.Info( $"new spotlight {index} {position} {rotation.Forward} {color} {innerangle} {outerangle} {constant} {linear} {quadratic}" );
		var lightmodel = new SceneObject
		{
			Model = Model.Primitives.ForwardCone,
			Scale = 0.1f,
			Position = position,
			Rotation = rotation
		};
		lightmodel.RenderColor = color;

		// init and add light
		var light = new SpotLight();
		light.Position = position;
		light.Direction = rotation.Forward;
		light.Color = new Vector3( color.r, color.g, color.b );
		light.Constant = constant;
		light.Linear = linear;
		light.Quadratic = quadratic;
		light.Brightness = brightness;
		light.InnerAngle = innerangle.DegreeToRadian();
		light.OuterAngle = outerangle.DegreeToRadian();
		SpotLights.Add( light );

		NumSpotLights++;
		// update whole buffer for now, this should use sub data later on
		UniformBufferManager.Current?.UpdateSpotlights( SpotLights.ToArray(), NumSpotLights );
	}

	public void AddDirLight( Rotation rotation )
	{
		AddDirLight( rotation, Color.White );
	}

	public void AddDirLight( Rotation rotation, Color color, float brightness = 1.0f )
	{
		var index = NumDirLights; // current number is index for new light (ie, 0 lights means insert at index 0)
		if ( index >= MaxDirLights )
		{
			Log.Info( $"UNABLE TO ADD MORE DIRECTIONAL LIGHTS {MaxDirLights}" );
			return;
		}

		Log.Info( $"new dirlight {index} {rotation.Forward} {color}" );
		var lightmodel = new SceneObject
		{
			Model = Model.Primitives.ForwardCone,
			Scale = 0.1f,
			Position = Vector3.Zero,
			Rotation = rotation
		};
		lightmodel.RenderColor = color;

		// init and add light
		var light = new DirLight();
		light.Direction = rotation.Forward;
		light.Color = new Vector3( color.r, color.g, color.b );
		light.Brightness = brightness;
		DirLights.Add( light );

		NumDirLights++;
		UniformBufferManager.Current?.UpdateDirlights( DirLights.ToArray(), NumDirLights );
	}
}
