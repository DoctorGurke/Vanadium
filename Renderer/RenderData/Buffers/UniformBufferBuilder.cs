using System.Runtime.InteropServices;
using Vanadium.Renderer.RenderData.Buffers.BufferData;

namespace Vanadium.Renderer.RenderData.Buffers;

public class UniformBufferBuilder
{
	private readonly string Name;
	private Dictionary<string, IBufferSetting> BufferData = new();
	private int Count;

	public UniformBufferBuilder( string name )
	{
		Name = name;
		Count = default;
	}

	public UniformBufferBuilder AddField<T>( string identifier, int postpad = 0 )
	{
		if ( typeof( T ).IsArray )
			throw new ArgumentException( "Unable to add Array type as BufferData, use AddArrayField<T> instead." );

		var size = Marshal.SizeOf( typeof( T ) );

		// increment with data size and pad
		Count += size + postpad;
		BufferData.Add( identifier, new BufferData<T>( identifier, Count, size ) );

		return this;
	}

	public UniformBufferBuilder AddArrayField<T>( string identifier, int length, int postpad = 0 )
	{
		if ( !typeof( T ).IsArray )
			throw new ArgumentException( "Unable to add non-Array type as BufferData, use AddField<T> instead." );

		var size = Marshal.SizeOf( typeof( T ) ) * length;

		// increment with data size and pad
		Count += size + postpad;
		BufferData.Add( identifier, new BufferData<T>( identifier, Count, size ) );

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
		var buffer = new UniformBuffer( handle, Name, Count, BufferData );

		// add to static hashset so we can keep track of it
		UniformBuffer.All.Add( buffer );
		// init buffer
		buffer.Initialize();
		return buffer;
	}
}
