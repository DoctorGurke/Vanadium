namespace Vanadium.Renderer.RenderData.MaterialUniforms;

public class FloatUniform : MaterialUniform<float>
{
	public FloatUniform( string name ) : base( name, 0.0f ) { }
	public FloatUniform( string name, float value ) : base( name, value ) { }

	public override void SetUniform( Shader shader )
	{
		shader.Set(Name, Value);
	}
}
