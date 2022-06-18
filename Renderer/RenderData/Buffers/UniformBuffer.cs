namespace Vanadium.Renderer.RenderData.Buffers;

public class UniformBuffer
{
	public readonly int Handle;
	public readonly string Name;

	public static HashSet<UniformBuffer> All = new HashSet<UniformBuffer>();

	public UniformBuffer( int handle, string name )	{
		Handle = handle;
		Name = name;
	}

	/// <summary>
	/// Start a buffer declaration with a given buffer handle name.
	/// </summary>
	/// <param name="name">The name of the buffer handle.</param>
	/// <returns>A UniformBufferBuilder instance, used to construct the UniformBuffer object.</returns>
	public static UniformBufferBuilder DeclareBuffer( string name )
	{
		UniformBufferBuilder builder = new UniformBufferBuilder( name );
		return builder;
	}

	/// <summary>
	/// Initializes the buffer's data.
	/// </summary>
	/// <exception cref="NotImplementedException"></exception>
	public void Initialize()
	{
		throw new NotImplementedException();
	}
}
