using OpenTK.Graphics.OpenGL4;
using StbImageSharp;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace Vanadium.RenderSystem.RenderData;

public partial class Texture
{
	private static readonly Dictionary<string, ImageResult> ImageData = new();
	private static readonly Dictionary<string, ImageResultFloat> ImageDataFloat = new();

	public static string Error => "textures/core/error.png";
	public static Texture ErrorTexture => Load2D( Error );

	/// <summary>
	/// Load image data from a filepath.
	/// This caches data for repeated use.
	/// </summary>
	/// <param name="path">The image filepath, relative to the resource directory.</param>
	/// <returns>The ImageResult of the image file.</returns>
	private static ImageResult LoadImageData( string path, ColorComponents components = ColorComponents.RedGreenBlueAlpha )
	{
		path = $"core/{path}";

		if ( ImageData.TryGetValue( path, out var result ) )
		{
			return result;
		}

		ImageResult image;
		try
		{
			using var stream = File.OpenRead( path );
			Log.Info( $"Loading image data for: {path}" );
			image = ImageResult.FromStream( stream, components );
		}
		catch ( Exception ex )
		{
			if ( ex is FileNotFoundException || ex is DirectoryNotFoundException )
			{
				// bail with error texture
				using var stream = File.OpenRead( $"core/{Error}" );
				Log.Info( $"Error loading image data for: {path}, File not found!" );
				image = ImageResult.FromStream( stream, components );
			}
			else
			{
				throw;
			}
		}
		ImageData.Add( path, image );
		return image;
	}

	private static ImageResultFloat LoadFloatImageData( string path )
	{
		path = $"core/{path}";

		if ( ImageDataFloat.TryGetValue( path, out var result ) )
		{
			return result;
		}

		ImageResultFloat image;
		try
		{
			using var stream = File.OpenRead( path );
			Log.Info( $"Loading image data for: {path}" );
			image = ImageResultFloat.FromStream( stream, ColorComponents.RedGreenBlue );
		}
		catch ( Exception ex )
		{
			if ( ex is FileNotFoundException || ex is DirectoryNotFoundException )
			{
				// bail with error texture
				using var stream = File.OpenRead( $"core/{Error}" );
				Log.Info( $"Error loading image data for: {path}, File not found!" );
				image = ImageResultFloat.FromStream( stream, ColorComponents.RedGreenBlue );
			}
			else
			{
				throw;
			}
		}
		ImageDataFloat.Add( path, image );
		return image;
	}

	public static Texture Load2D( string name, int width, int height, IntPtr data, bool srgb = false )
	{
		GLUtil.CreateTexture( TextureTarget.Texture2D, name, out int handle );
		var texture = new Texture( handle, TextureTarget.Texture2D, width, height );

		GL.TextureStorage2D( handle, 1, srgb ? (SizedInternalFormat)All.Srgb8Alpha8 : SizedInternalFormat.Rgba8, width, height );

		GL.TextureSubImage2D( handle, 0, 0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, data );

		texture.SetWrap( TextureCoordinate.S, TextureWrapMode.Repeat );
		texture.SetWrap( TextureCoordinate.T, TextureWrapMode.Repeat );
		return texture;
	}

	public static Texture Load2D( string path, bool genmips = true, bool srgb = false )
	{
		// Generate handle
		GLUtil.CreateTexture( TextureTarget.Texture2D, Path.GetFileName( path ), out int handle );

		// Bind the handle
		GL.ActiveTexture( TextureUnit.Texture0 );
		GL.BindTexture( TextureTarget.Texture2D, handle );

		ImageResult image = LoadImageData( path, ColorComponents.RedGreenBlue );

		var width = image.Width;
		var height = image.Height;

		GL.TexImage2D( TextureTarget.Texture2D, 0, srgb ? PixelInternalFormat.Srgb8 : PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, image.Data );

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
		return tex;
	}

	private static readonly TextureTarget[] targets = {
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

	public static Texture LoadCube( string path, bool srgb = false )
	{
		var sides = new List<string>();

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
			ImageResult image = LoadImageData( sides[i] );

			width = image.Width;
			height = image.Height;

			GL.TexImage2D( targets[i], 0, srgb ? PixelInternalFormat.Srgb8Alpha8 : PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data );

			// texture filtering
			GL.TexParameter( TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear );
			GL.TexParameter( TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear );

			// texture wrap
			GL.TexParameter( TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge );
			GL.TexParameter( TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge );
			GL.TexParameter( TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge );
		}

		var tex = new Texture( handle, TextureTarget.TextureCubeMap, width, height );
		return tex;
	}

	public static Texture LoadHDR( string path )
	{
		// Generate handle
		GLUtil.CreateTexture( TextureTarget.Texture2D, Path.GetFileName( path ), out int handle );

		// Bind the handle
		GL.ActiveTexture( TextureUnit.Texture0 );
		GL.BindTexture( TextureTarget.Texture2D, handle );

		ImageResultFloat image = LoadFloatImageData( path );

		var width = image.Width;
		var height = image.Height;

		GL.TexImage2D( TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb16f, width, height, 0, PixelFormat.Rgb, PixelType.Float, image.Data );

		// texture filtering
		GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear );
		GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear );

		// texture wrap
		GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat );
		GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat );

		var tex = new Texture( handle, TextureTarget.Texture2D, width, height );
		return tex;
	}
}
