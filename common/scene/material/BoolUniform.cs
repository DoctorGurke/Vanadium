namespace Vanadium;

public class BoolUniform : MaterialUniform<bool>
{
	public BoolUniform( string name ) : base( name, false ) { }
	public BoolUniform( string name, bool value ) : base( name, value ) { }

	public override void SetUniform( Shader shader )
	{
		shader.Set(Name, Value);
	}
}
