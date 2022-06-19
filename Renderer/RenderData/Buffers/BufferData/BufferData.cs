namespace Vanadium.Renderer.RenderData.Buffers.BufferData;

public class BufferData<T> : IBufferSetting
{
	/// <summary>
	/// name of the key for this data
	/// </summary>
	public readonly string Name;
	/// <summary>
	/// data offset into its buffer in bytes
	/// </summary>
	public readonly int Offset;
	/// <summary>
	/// total size of the allocated data in bytes
	/// </summary>
	public readonly int Size;

	private T? _Value;
	public T? Value
	{
		get => _Value;
		set
		{
			_Value = value;
			SetDirty();
		}
	}

	private bool _IsDirty;
	public bool IsDirty { get => _IsDirty; }

	public void SetDirty() => _IsDirty = true;

	public BufferData( string name, int offset, int size )
	{
		Name = name;
		Offset = offset;
		Size = size;
		Value = default;
	}

	public void Set( Buffer buffer )
	{
	}
}
