namespace Vanadium.Renderer.RenderData.Buffers.BufferData;

public interface IBufferSetting
{
	public bool IsDirty { get; }
	public abstract void Set( Buffer buffer );
}
