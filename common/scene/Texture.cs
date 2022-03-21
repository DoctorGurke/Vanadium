using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using System.Drawing.Imaging;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace Vanadium;

public partial class Texture : IDisposable
{
	public readonly int Handle;
	public readonly int Width;
	public readonly int Height;

	public Texture( int glHandle, int width, int height )
	{
		Handle = glHandle;
		Width = width;
		Height = height;
	}

	// activate texture
	public void Use( TextureUnit unit )
	{
		GL.ActiveTexture( unit );
		GL.BindTexture( TextureTarget.Texture2D, Handle );
	}

	public void Dispose()
	{
		GL.DeleteTexture( Handle );
		GC.SuppressFinalize( this );
	}
}
