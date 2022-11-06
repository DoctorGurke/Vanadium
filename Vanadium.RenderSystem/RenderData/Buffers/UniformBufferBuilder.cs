using System.Runtime.InteropServices;
using Vanadium.RenderSystem.RenderData.Buffers.BufferData;

namespace Vanadium.RenderSystem.RenderData.Buffers;

public class UniformBufferBuilder
{
	private readonly string Name;
	private readonly Dictionary<string, IBufferSetting> BufferData = new();
	private int Count;

	public UniformBufferBuilder( string name )
	{
		Name = name;
		Count = 0;
	}

	/// <summary>
	/// Add a new field to the UniformBufferBuilder.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="identifier">The identifier of the field.</param>
	/// <param name="postpad">How much padding to add after the field, in bytes.</param>
	/// <returns></returns>
	/// <exception cref="BufferFieldFormatException"></exception>
	public UniformBufferBuilder AddField<T>( string identifier, int postpad = 0 )
	{
		if ( string.IsNullOrWhiteSpace( identifier ) )
			throw new BufferFieldFormatException( "BufferData name cannot be empty.", nameof( identifier ) );

		if ( postpad < 0 )
			throw new BufferFieldFormatException( "BufferData postpad cannot be smaller than 0.", nameof( postpad ) );

		var size = Marshal.SizeOf( typeof( T ) );

		// increment with data size and pad
		BufferData.Add( identifier, new BufferData<T>( identifier, Count, size ) );
		Count += size + postpad;

		return this;
	}

	/// <summary>
	/// Add a new array field to the UniformBufferBuilder.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="identifier">The identifier of the array field.</param>
	/// <param name="length">The intended length of the array field.</param>
	/// <param name="postpad">How much padding to add after the array field, in bytes.</param>
	/// <returns></returns>
	/// <exception cref="BufferFieldFormatException"></exception>
	public UniformBufferBuilder AddArrayField<T>( string identifier, int length, int postpad = 0 )
	{
		if ( string.IsNullOrWhiteSpace( identifier ) )
			throw new BufferFieldFormatException( "BufferArrayData name cannot be empty.", nameof( identifier ) );

		if ( length <= 0 )
			throw new BufferFieldFormatException( "BufferArrayData length must be larger than 0.", nameof( length ) );

		if ( postpad < 0 )
			throw new BufferFieldFormatException( "BufferArrayData postpad cannot be smaller than 0.", nameof( postpad ) );

		var size = Marshal.SizeOf( typeof( T ) ) * length;

		// increment with data size and pad
		BufferData.Add( identifier, new BufferArrayData<T>( identifier, Count, size, length ) );
		Count += size + postpad;

		return this;
	}

	/// <summary>
	/// Adds a standard int sized padding to the current UniformBufferBuilder.
	/// </summary>
	/// <param name="count">How many int sized pads to add.</param>
	/// <returns></returns>
	public UniformBufferBuilder AddPadding( int count = 1 )
	{
		Count += count * sizeof( int );
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
		UniformBuffer.All.Add( Name, buffer );
		Log.Info( $"built buffer {Name} : {handle} and size {Count}" );
		// init buffer
		buffer.Initialize();
		return buffer;
	}

	public class BufferFieldFormatException : ArgumentException
	{
		public BufferFieldFormatException() : base() { }
		public BufferFieldFormatException( string? message ) : base( message ) { }
		public BufferFieldFormatException( string? message, string? paramname ) : base( message, paramname ) { }
	}
}
