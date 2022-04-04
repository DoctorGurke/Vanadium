using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using System.Drawing.Imaging;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace Vanadium;

public partial class Texture
{
	private static readonly Dictionary<string, Texture> PrecachedTextures = new();
	public static string ErrorTexture => "textures/core/error.png";

	public static Texture Load2D( string name, int width, int height, IntPtr data )
	{
		GLUtil.CreateTexture( TextureTarget.Texture2D, name, out int handle );
		var texture = new Texture( handle, TextureTarget.Texture2D, width, height );

		GL.TextureStorage2D( handle, 1, SizedInternalFormat.Rgba8, width, height );

		GL.TextureSubImage2D( handle, 0, 0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, data );

		texture.SetWrap( TextureCoordinate.S, TextureWrapMode.Repeat );
		texture.SetWrap( TextureCoordinate.T, TextureWrapMode.Repeat );
		return texture;
	}

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
			image = new Bitmap( $"resources/{ErrorTexture}" );
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

		var tex = new Texture( handle, TextureTarget.Texture2D, width, height );
		PrecachedTextures.Add( path, tex );
		return tex;
	}

	private static TextureTarget[] targets = {
		// right								// left
		TextureTarget.TextureCubeMapPositiveX, TextureTarget.TextureCubeMapNegativeX,
		// up									// down
		TextureTarget.TextureCubeMapPositiveY, TextureTarget.TextureCubeMapNegativeY,
		// back									// front
		TextureTarget.TextureCubeMapPositiveZ, TextureTarget.TextureCubeMapNegativeZ
	};

	private static readonly string[] cubesides = {
		"right",
		"left",
		"up",
		"down",
		"back",
		"front"
	};

	public static Texture LoadCube( string path )
	{

		if ( PrecachedTextures.TryGetValue( path, out var texture ) )
		{
			return texture;
		}

		var sides = new List<string>();
		var oldpath = path;
		var ext = Path.GetExtension( path );
		path = path.Replace( ext, "" );
		foreach ( var side in cubesides )
		{
			var full = $"{path}_{side}{ext}";
			Log.Info( $"adding cubemap side {full}" );
			sides.Add( full );
		}

		// Generate handle
		GLUtil.CreateTextureCube( TextureTarget.TextureCubeMap, $"{Path.GetFileName( path )}", out int handle );

		// Bind the handle
		GL.ActiveTexture( TextureUnit.Texture0 );
		GL.BindTexture( TextureTarget.TextureCubeMap, handle );

		int width = 0;
		int height = 0;

		for ( int i = 0; i < sides.Count; i++ )
		{
			var cubepath = $"resources/{sides[i]}";
			Bitmap image;
			try
			{
				Log.Info( "Loading cube texture: " + cubepath );
				image = new Bitmap( cubepath );
			}
			catch
			{
				Log.Info( "Error loading cube texture: " + cubepath + " File is missing or invalid" );
				image = new Bitmap( "resources/textures/core/error.png" );
			}

			width = image.Width;
			height = image.Height;

			using ( image )
			{
				if ( targets[i] == TextureTarget.TextureCubeMapPositiveY || targets[i] == TextureTarget.TextureCubeMapNegativeY )
				{
					image.RotateFlip( RotateFlipType.Rotate180FlipNone );
				}
				var data = image.LockBits( new Rectangle( 0, 0, image.Width, image.Height ), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb );

				GL.TexImage2D( targets[i], 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0 );
			}

			// texture filtering
			GL.TexParameter( TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear );
			GL.TexParameter( TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear );

			// texture wrap
			GL.TexParameter( TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge );
			GL.TexParameter( TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge );
			GL.TexParameter( TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge );
		}

		var tex = new Texture( handle, TextureTarget.TextureCubeMap, width, height );
		PrecachedTextures.Add( oldpath, tex );
		return tex;
	}
}
