using OpenTK.Mathematics;

namespace Vanadium.Renderer.RenderData.MaterialUniforms;

public class Vector2Uniform : MaterialUniform<Vector2>
{
	public Vector2Uniform( string name ) : base( name, Vector2.Zero ) { }
	public Vector2Uniform( string name, Vector2 value ) : base( name, value ) { }

	public override void SetUniform( Shader shader )
	{
		shader.Set(Name, Value);
	}
}
