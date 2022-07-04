namespace Vanadium.Renderer.RenderData.MaterialUniforms;

public class IntUniform : MaterialUniform<int>
{
	public IntUniform( string name ) : base( name, 0 ) { }
	public IntUniform( string name, int value ) : base( name, value ) { }

	public override void SetUniform( Shader shader )
	{
		shader.Set(Name, Value);
	}
}
