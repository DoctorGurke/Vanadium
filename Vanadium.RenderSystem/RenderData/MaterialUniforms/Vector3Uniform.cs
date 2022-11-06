namespace Vanadium.RenderSystem.RenderData.MaterialUniforms;

public class Vector3Uniform : MaterialUniform<Vector3>
{
	public Vector3Uniform( string name ) : base( name, Vector3.Zero ) { }
	public Vector3Uniform( string name, Vector3 value ) : base( name, value ) { }

	public override void SetUniform( Shader shader )
	{
		shader.Set(Name, Value);
	}
}
