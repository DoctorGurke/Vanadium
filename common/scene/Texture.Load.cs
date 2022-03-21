using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using System.Drawing.Imaging;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace Vanadium;

public partial class Texture
{
	private static readonly Dictionary<string, Texture> PrecachedTextures = new();
	public static string ErrorTexture => "textures/core/error.png";

	public static Texture Load2D( string path, bool genmips = true )
	{
		path = $"resources/{path}";

		if ( PrecachedTextures.TryGetValue( path, out var texture ) )
		{
			return texture;
		}

		// Generate handle
		GLUtil.CreateTexture( TextureTarget.Texture2D, Path.GetFileName( path ), out int handle );

		// Bind the handle
		GL.ActiveTexture( TextureUnit.Texture0 );
		GL.BindTexture( TextureTarget.Texture2D, handle );

		Bitmap image;
		try
		{
			Log.Info( "Loading texture: " + path );
			image = new Bitmap( path );
		}
		catch
		{
			Log.Info( "Error loading texture: " + path + " File is missing or invalid" );
			image = new Bitmap( ErrorTexture );
		}

		var width = image.Width;
		var height = image.Height;

		// Load the image
		using ( image )
		{

			// load pixel data
			var data = image.LockBits( new Rectangle( 0, 0, width, height ), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb );

			// generate gl texture
			GL.TexImage2D( TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0 );
		}

		// texture filtering
		GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, genmips ? (int)TextureMinFilter.LinearMipmapLinear : (int)TextureMinFilter.Linear );
		GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear );

		// texture wrap
		GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat );
		GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat );

		// generate mips
		if ( genmips )
			GL.GenerateMipmap( GenerateMipmapTarget.Texture2D );

		var tex = new Texture( handle, width, height );
		PrecachedTextures.Add( path, tex );
		return tex;
	}
}
