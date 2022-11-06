namespace Vanadium.RenderSystem.RenderData.MaterialUniforms;

public class TextureCubeUniform : TextureUniform
{
	public TextureCubeUniform( string name, Texture value ) : base( name, value ) { }

	public override void SetUniform( Shader shader ) { }
}
