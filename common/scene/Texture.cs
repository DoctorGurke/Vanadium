using OpenTK.Graphics.OpenGL4;

namespace Vanadium;

public enum TextureCoordinate
{
	S = TextureParameterName.TextureWrapS,
	T = TextureParameterName.TextureWrapT,
	R = TextureParameterName.TextureWrapR
}

public partial class Texture : IDisposable
{
	public readonly int Handle;
	public readonly TextureTarget Target;
	public readonly int Width;
	public readonly int Height;

	public Texture( int glHandle, TextureTarget target, int width, int height )
	{
		Handle = glHandle;
		Target = target;
		Width = width;
		Height = height;
	}

	// activate texture
	public void Use( TextureUnit unit )
	{
		GL.ActiveTexture( unit );
		GL.BindTexture( Target, Handle );
	}

	public void SetMinFilter( TextureMinFilter filter )
	{
		GL.TextureParameter( Handle, TextureParameterName.TextureMinFilter, (int)filter );
	}

	public void SetMagFilter( TextureMagFilter filter )
	{
		GL.TextureParameter( Handle, TextureParameterName.TextureMagFilter, (int)filter );
	}

	public void SetWrap( TextureCoordinate coord, TextureWrapMode mode )
	{
		GL.TextureParameter( Handle, (TextureParameterName)coord, (int)mode );
	}

	public void Dispose()
	{
		GL.DeleteTexture( Handle );
		GC.SuppressFinalize( this );
	}
}
