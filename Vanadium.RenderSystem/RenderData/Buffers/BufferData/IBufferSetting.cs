namespace Vanadium.RenderSystem.RenderData.Buffers.BufferData;

public interface IBufferSetting
{
	public bool IsDirty { get; }
	public abstract void Set( Buffer buffer );
}
