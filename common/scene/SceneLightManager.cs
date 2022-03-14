namespace Vanadium;

public class SceneLightManager
{
	private UniformBufferManager.PerViewLightingUniformBuffer LightStruct;

	public void SetAmbientLightColor( Color col )
	{
		LightStruct.g_vAmbientLightingColor = col.WithAlpha( 1.0f );
		// update whole buffer for now, this should use sub data later on
		UpdateBuffer();
	}

	private void UpdateBuffer()
	{
		UniformBufferManager.Current.UpdatePerViewLightingUniformBuffer( LightStruct );
	}
}
