namespace Vanadium.Renderer.RenderData.Buffers;

public class UniformBufferBuilder
{
	private readonly string Name;

	public UniformBufferBuilder( string name )
	{
		Name = name;
	}

	public UniformBufferBuilder AddField<T>( string identifier )
	{
		return this;
	}

	/// <summary>
	/// Builds and initializes the builder's UniformBuffer object.
	/// </summary>
	/// <returns>An instance of the UniformBuffer object.</returns>
	public UniformBuffer Build()
	{
		// create empty buffer handle
		GLUtil.CreateBuffer( Name, out var handle );
		var buffer = new UniformBuffer( handle, Name );

		// add to static hashset so we can keep track of it
		UniformBuffer.All.Add( buffer );
		// init buffer
		buffer.Initialize();
		return buffer;
	}
}
