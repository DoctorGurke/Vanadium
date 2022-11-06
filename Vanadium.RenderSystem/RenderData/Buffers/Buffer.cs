namespace Vanadium.RenderSystem.RenderData.Buffers;

public abstract class Buffer
{
	public readonly int Handle;
	public readonly string Name;
	public readonly int Size;

	public Buffer( int handle, string name, int size )
	{
		Handle = handle;
		Name = name;
		Size = size;
	}
}
