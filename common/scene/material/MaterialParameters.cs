﻿using System.Collections.Immutable;

namespace Vanadium;

public class MaterialParameters : IRenderSetting
{
	private readonly ImmutableArray<IRenderSetting> _settings;

	public MaterialParameters( IEnumerable<IRenderSetting> parameters )
	{
		_settings = parameters.ToImmutableArray();
	}

	private int TexCount = 0;

	public void Set( Shader shader )
	{
		TexCount = 0;
		foreach ( var setting in _settings )
		{
			// figure out a better way to bind these
			if ( setting is TextureUniform tex )
			{
				tex.SetTexture( shader, TexCount );
				TexCount++;
			}
			else if ( setting is TextureCubeUniform texcube)
			{
				texcube.SetTexture( shader, TexCount );
				TexCount++;
			}
			else
			{
				setting.Set( shader );
			}
		}
	}
}
