namespace Vanadium.Renderer.RenderData.MaterialUniforms;

public class TextureHDRUniform : TextureUniform
{
	public TextureHDRUniform( string name, Texture value ) : base( name, value ) { }

	public override void SetUniform( Shader shader ) { }
}
