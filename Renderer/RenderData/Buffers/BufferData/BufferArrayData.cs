namespace Vanadium.Renderer.RenderData.Buffers.BufferData;

public class BufferArrayData<T> : IBufferSetting
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
	/// <summary>
	/// length of the array
	/// </summary>
	public readonly int DataLength;

	private T[]? _Value;
	public T[]? Value
	{
		get => _Value;
		set
		{
			_Value = value?.Take( DataLength ).ToArray();
			SetDirty();
		}
	}

	private bool _IsDirty;
	public bool IsDirty { get => _IsDirty; }

	public void SetDirty() => _IsDirty = true;

	public BufferArrayData( string name, int offset, int size, int datalength )
	{
		Name = name;
		Offset = offset;
		Size = size;
		DataLength = datalength;
		Value = default;
	}

	public void Set( Buffer buffer )
	{

	}
}
