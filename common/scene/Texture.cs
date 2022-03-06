using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using System.Drawing.Imaging;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace Vanadium;

//Taken from https://github.com/opentk/LearnOpenTK
public class Texture {
	public readonly int Handle;

	private static Dictionary<string, Texture> PrecachedTextures = new();

	private static string[] skyboxSides = {
		"right",
		"left",
		"top",
		"bottom",
		"front",
		"back"
	};

	private static TextureTarget[] targets = {
		TextureTarget.TextureCubeMapNegativeX, TextureTarget.TextureCubeMapNegativeY,
		TextureTarget.TextureCubeMapNegativeZ, TextureTarget.TextureCubeMapPositiveX,
		TextureTarget.TextureCubeMapPositiveY, TextureTarget.TextureCubeMapPositiveZ
	};

	public static Texture LoadCube(string path) {
		if(PrecachedTextures.TryGetValue(path, out var texture)) {
			return texture;
		}

		var oldpath = path;
		var extension = Path.GetExtension(path);
		path = path.Replace(extension, "");

		// Generate handle
		int handle = GL.GenTexture();

		// Bind the handle
		GL.ActiveTexture(TextureUnit.Texture0);
		GL.BindTexture(TextureTarget.TextureCubeMap, handle);

		for(int i = 0; i < skyboxSides.Length; i++) {
			var cubepath = $"resources/{path}_{skyboxSides[i]}{extension}";
			Bitmap image;
			try {
				Log.Info("Loading cube texture: " + cubepath);
				image = new Bitmap(cubepath);
			} catch {
				Log.Info("Error loading cube texture: " + cubepath + " File is missing or invalid");
				image = new Bitmap("resources/textures/error.png");
			}

			using(image) {
				var data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

				GL.TexImage2D(targets[i], 0, PixelInternalFormat.Rgb, image.Width, image.Height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, data.Scan0);
			}

			// texture filtering
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

			// texture wrap
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
		}

		var tex = new Texture(handle);
		PrecachedTextures.Add(oldpath, tex);
		return tex;
	}

	public static Texture Load(string path) {

		if(PrecachedTextures.TryGetValue(path, out var texture)) {
			return texture;
		}

		// Generate handle
		int handle = GL.GenTexture();

		// Bind the handle
		GL.ActiveTexture(TextureUnit.Texture0);
		GL.BindTexture(TextureTarget.Texture2D, handle);

		Bitmap image;
		try {
			Log.Info("Loading texture: " + path);
			image = new Bitmap($"resources/{path}");
		} catch {
			Log.Info("Error loading texture: " + path + " File is missing or invalid");
			image = new Bitmap("resources/textures/error.png");
		}

		// Load the image
		using(image) {

			// load pixel data
			var data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			// generate gl texture
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
		}

		// texture filtering
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

		// texture wrap
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

		// generate mips
		GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

		var tex = new Texture(handle);
		PrecachedTextures.Add(path, tex);
		return tex;
	}

	public Texture(int glHandle) {
		Handle = glHandle;
	}

	// activate texture
	public void Use(TextureUnit unit, TextureTarget target = TextureTarget.Texture2D) {
		GL.ActiveTexture(unit);
		GL.BindTexture(target, Handle);
	}
}
