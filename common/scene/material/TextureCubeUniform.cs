using OpenTK.Graphics.OpenGL4;

namespace Vanadium;

public class TextureCubeUniform : MaterialUniform<TextureCube>
{
	public TextureCubeUniform( string name ) : base( name, null ) { } // figure out a way to handle a default
	public TextureCubeUniform( string name, TextureCube value ) : base( name, value ) { }

	public override void SetUniform( Shader shader ) { }

	public void SetTexture( Shader shader, int tex )
	{
		if ( Value is null ) return;
		var texture = Value;
		shader.Set( Name, tex );
		texture.Use( TextureUnit.Texture0 + tex );
	}
}
