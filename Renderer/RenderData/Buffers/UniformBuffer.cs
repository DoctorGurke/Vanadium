using Vanadium.Renderer.RenderData.Buffers.BufferData;

namespace Vanadium.Renderer.RenderData.Buffers;

public partial class UniformBuffer : Buffer
{
	public static HashSet<UniformBuffer> All = new HashSet<UniformBuffer>();

	public IReadOnlyDictionary<string, IBufferSetting> BufferData => InternalBufferData;
	private Dictionary<string, IBufferSetting> InternalBufferData = new();

	public UniformBuffer( int handle, string name, Dictionary<string, IBufferSetting> bufferdata ) : base( handle, name )
	{
		InternalBufferData = bufferdata;
	}

	/// <summary>
	/// Try to set the data of a BufferData entry.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="name">The name of the BufferData entry.</param>
	/// <param name="data">The data to set.</param>
	/// <returns>True if the entry was found and set, false otherwise.</returns>
	public bool TrySet<T>( string name, T data )
	{
		if ( !InternalBufferData.ContainsKey( name ) )
			return false;

		var entry = InternalBufferData[name];

		if ( entry is not BufferData<T> bufferdata )
			return false;

		bufferdata.Value = data;

		return true;
	}

	/// <summary>
	/// Try to set the data of a BufferArrayData entry.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="name">The name of the BufferArrayData entry.</param>
	/// <param name="data">The data to set.</param>
	/// <returns>True if the entry was found and set, false otherwise.</returns>
	public bool TrySet<T>( string name, T[] data )
	{
		if ( !InternalBufferData.ContainsKey( name ) )
			return false;

		var entry = InternalBufferData[name];

		if ( entry is not BufferArrayData<T> bufferdata )
			return false;

		bufferdata.Value = data;

		return true;
	}

	/// <summary>
	/// Set the data of a BufferData entry.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="name">The name of the BufferData entry.</param>
	/// <param name="data">The data to set.</param>
	/// <remarks>Will throw if setting the data was not possible due to invalid name key or data type.</remarks>
	/// <exception cref="KeyNotFoundException"></exception>
	/// <exception cref="ArgumentException"></exception>
	public void Set<T>( string name, T data )
	{
		if ( InternalBufferData[name] is not BufferData<T> entry )
			throw new ArgumentException( $"Invalid type for BufferData entry at {name}." );
		entry.Value = data;
	}

	/// <summary>
	/// Set the data of a BufferArrayData entry.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="name">The name of the BufferArrayData entry.</param>
	/// <param name="data">The data to set.</param>
	/// <remarks>Will throw if setting the data was not possible due to invalid name key or data type.</remarks>
	/// <exception cref="KeyNotFoundException"></exception>
	/// <exception cref="ArgumentException"></exception>
	public void Set<T>( string name, T[] data )
	{
		if ( InternalBufferData[name] is not BufferArrayData<T> entry )
			throw new ArgumentException( $"Invalid type for BufferData entry at {name}." );
		entry.Value = data;
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
