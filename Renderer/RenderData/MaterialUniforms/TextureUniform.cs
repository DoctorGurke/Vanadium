using OpenTK.Graphics.OpenGL4;

namespace Vanadium;

public class TextureUniform : MaterialUniform<string>
{
	public TextureUniform( string name ) : base( name, "" ) { }
	public TextureUniform( string name, string value ) : base( name, value ) { }

	public bool Srgb { get; private set; }
	public TextureUniform( string name, string value, bool srgb = false ) : base(name, value)
	{
		Srgb = srgb;
	}

	public override void SetUniform( Shader shader ) { }

	public void SetTexture(Shader shader, int tex )
	{
		if ( Value is null ) return;
		var texture = Texture.Load2D( Value, true, Srgb );
		shader.Set( Name, tex );
		texture.Use( TextureUnit.Texture0 + tex );
	}
}
