namespace Vanadium.Renderer.RenderData.MaterialUniforms;

public class UIntUniform : MaterialUniform<uint>
{
	public UIntUniform( string name ) : base( name, 0 ) { }
	public UIntUniform( string name, uint value ) : base( name, value ) { }

	public override void SetUniform( Shader shader )
	{
		shader.Set(Name, Value);
	}
}
