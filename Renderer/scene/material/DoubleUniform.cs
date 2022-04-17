namespace Vanadium;

public class DoubleUniform : MaterialUniform<double>
{
	public DoubleUniform( string name ) : base( name, 0.0 ) { }
	public DoubleUniform( string name, double value ) : base( name, value ) { }

	public override void SetUniform( Shader shader )
	{
		shader.Set(Name, Value);
	}
}
