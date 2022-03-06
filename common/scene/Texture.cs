﻿using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using System.Drawing.Imaging;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace Vanadium;

//Taken from https://github.com/opentk/LearnOpenTK
public class Texture {
	public readonly int Handle;

	private static Dictionary<string, Texture> PrecachedTextures = new();

	public static Texture Load(string path) {

		if(PrecachedTextures.TryGetValue(path, out var texture)) {
			return texture;
		}

		// Generate handle
		int handle = GL.GenTexture();

		// Bind the handle
		GL.ActiveTexture(TextureUnit.Texture0);
		GL.BindTexture(TextureTarget.Texture2D, handle);

		// For this example, we're going to use .NET's built-in System.Drawing library to load textures.

		Bitmap image;
		try {
			Log.Info("Loading texture: \t\t" + path);
			image = new Bitmap($"resources/{path}");
		} catch {
			Log.Info("Error loading texture: \t" + path + " \tFile is missing or invalid");
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
	public void Use(TextureUnit unit) {
		GL.ActiveTexture(unit);
		GL.BindTexture(TextureTarget.Texture2D, Handle);
	}
}
