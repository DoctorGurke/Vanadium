using OpenTK.Mathematics;

namespace Vanadium;

public class Vector4Uniform : MaterialUniform<Vector4>
{
	public Vector4Uniform( string name ) : base( name, Vector4.Zero ) { }
	public Vector4Uniform( string name, Vector4 value ) : base( name, value ) { }

	public override void SetUniform( Shader shader )
	{
		shader.Set(Name, Value);
	}
}
