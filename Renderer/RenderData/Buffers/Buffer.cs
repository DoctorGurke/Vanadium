namespace Vanadium.Renderer.RenderData.Buffers;

public abstract class Buffer
{
	public readonly int Handle;
	public readonly string Name;

	public Buffer( int handle, string name )
	{
		Handle = handle;
		Name = name;
	}
}
