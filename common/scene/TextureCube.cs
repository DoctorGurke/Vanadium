﻿using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using System.Drawing.Imaging;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace Vanadium;

public class TextureCube {

	public readonly int Handle;

	private static Dictionary<string, TextureCube> PrecachedTextures = new();

	private static TextureTarget[] targets = {
		// right								// left
		TextureTarget.TextureCubeMapPositiveX, TextureTarget.TextureCubeMapNegativeX,
		// up									// down
		TextureTarget.TextureCubeMapPositiveY, TextureTarget.TextureCubeMapNegativeY,
		// back									// front
		TextureTarget.TextureCubeMapPositiveZ, TextureTarget.TextureCubeMapNegativeZ
	};

	public static TextureCube Load(List<string> SkyboxFaces, string path) {

		if(PrecachedTextures.TryGetValue(path, out var texture)) {
			return texture;
		}

		// Generate handle
		GLUtil.CreateTextureCube(TextureTarget.TextureCubeMap, $"{Path.GetFileName(path)}", out int handle);

		// Bind the handle
		GL.ActiveTexture(TextureUnit.Texture0);
		GL.BindTexture(TextureTarget.TextureCubeMap, handle);

		for(int i = 0; i < SkyboxFaces.Count; i++) {
			var cubepath = $"resources/{SkyboxFaces[i]}";
			Bitmap image;
			try {
				Log.Info("Loading cube texture: " + cubepath);
				image = new Bitmap(cubepath);
			} catch {
				Log.Info("Error loading cube texture: " + cubepath + " File is missing or invalid");
				image = new Bitmap("resources/textures/error.png");
			}

			using(image) {
				if(targets[i] == TextureTarget.TextureCubeMapPositiveY || targets[i] == TextureTarget.TextureCubeMapNegativeY) {
					image.RotateFlip(RotateFlipType.Rotate180FlipNone);
				}
				var data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				
				GL.TexImage2D(targets[i], 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
			}

			// texture filtering
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

			// texture wrap
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
		}

		var tex = new TextureCube(handle);
		PrecachedTextures.Add(path, tex);
		return tex;
	}

	public TextureCube(int glHandle) {
		Handle = glHandle;
	}

	// activate texture
	public void Use(TextureUnit unit) {
		GL.ActiveTexture(unit);
		GL.BindTexture(TextureTarget.TextureCubeMap, Handle);
	}
}
