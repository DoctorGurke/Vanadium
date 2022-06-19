namespace Vanadium.Renderer.RenderData.Buffers;

public class UniformBufferManager
{
	public static UniformBufferManager? Current { get; private set; }

	public UniformBufferManager()
	{
		Current = this;
	}

	public void UpdateSceneUniformBuffer()
	{
		if ( UniformBuffer.All.TryGetValue( "SceneUniformBuffer", out var buffer ) )
		{
			buffer.Set( "g_matWorldToProjection", Camera.ActiveCamera?.ProjectionMatrix ?? default );
			buffer.Set( "g_matWorldToView", Camera.ActiveCamera?.ViewMatrix ?? default );

			buffer.Set( "g_vCameraPositionWs", Camera.ActiveCamera?.Position ?? default );
			buffer.Set( "g_vCameraDirWs", Camera.ActiveCamera?.Rotation.Forward ?? default );
			buffer.Set( "g_vCameraUpDirWs", Camera.ActiveCamera?.Rotation.Up ?? default );

			buffer.Set( "g_vViewportSize", Screen.Size );

			buffer.Set( "g_flTime", Time.Now );
			buffer.Set( "g_flNearPlane", Camera.ActiveCamera?.ZNear ?? default );
			buffer.Set( "g_flFarPlane", Camera.ActiveCamera?.ZFar ?? default );
			buffer.Set( "g_flGamma", DebugOverlay.Gamma );
			buffer.Update();
		}
	}

	public void UpdateAmbientLightColor( Color col )
	{
		if ( UniformBuffer.All.TryGetValue( "SceneLightingUniformBuffer", out var buffer ) )
		{
			buffer.Set( "g_vAmbientLightingColor", col );
			buffer.Update();
		}
	}

	public void UpdatePointlights( SceneLightManager.PointLight[] lights, int num )
	{
		if ( UniformBuffer.All.TryGetValue( "SceneLightingUniformBuffer", out var buffer ) )
		{
			buffer.Set( "g_nNumPointlights", num );
			buffer.Set( "g_PointLights", lights );
			buffer.Update();
		}
	}

	public void UpdateSpotlights( SceneLightManager.SpotLight[] lights, int num )
	{
		if ( UniformBuffer.All.TryGetValue( "SceneLightingUniformBuffer", out var buffer ) )
		{
			buffer.Set( "g_nNumSpotlights", num );
			buffer.Set( "g_SpotLights", lights );
			buffer.Update();
		}
	}

	public void UpdateDirlights( SceneLightManager.DirLight[] lights, int num )
	{
		if ( UniformBuffer.All.TryGetValue( "SceneLightingUniformBuffer", out var buffer ) )
		{
			buffer.Set( "g_nNumDirlights", num );
			buffer.Set( "g_DirLights", lights );
			buffer.Update();
		}
	}
}
