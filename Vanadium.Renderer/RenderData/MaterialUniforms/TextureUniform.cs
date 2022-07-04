using OpenTK.Graphics.OpenGL4;

namespace Vanadium.Renderer.RenderData.MaterialUniforms;

public class TextureUniform : MaterialUniform<Texture>
{
	public TextureUniform( string name, Texture value ) : base( name, value ) { }
	public override void SetUniform( Shader shader ) { }

	public void SetTexture(Shader shader, int tex )
	{
		if ( Value is null ) return;
		shader.Set( Name, tex );
		Value.Use( TextureUnit.Texture0 + tex );
	}
}
