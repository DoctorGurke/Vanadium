namespace Vanadium.Renderer.RenderData.MaterialUniforms;

public abstract class MaterialUniform<T> : IRenderSetting
{
	protected readonly string Name;
	public T? Value { get; set; }

	public MaterialUniform( string name, T? value)
	{
		Name = name;
		Value = value;
	}

	public abstract void SetUniform( Shader shader );

	public void Set( Shader shader ) => SetUniform( shader );
}
