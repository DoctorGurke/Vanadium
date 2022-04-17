using OpenTK.Mathematics;

namespace Vanadium.Renderer.RenderData.MaterialUniforms;

public class Matrix4Uniform : MaterialUniform<Matrix4>
{
	public Matrix4Uniform( string name ) : base( name, Matrix4.Identity ) { }
	public Matrix4Uniform( string name, Matrix4 value ) : base( name, value ) { }

	public override void SetUniform( Shader shader )
	{
		shader.Set(Name, Value);
	}
}
