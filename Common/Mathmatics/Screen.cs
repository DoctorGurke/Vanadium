using OpenTK.Mathematics;

namespace Vanadium.Common.Mathematics;

public static class Screen
{
	public static int Width => Size.X;
	public static int Height => Size.Y;

	public static Vector2i Size { get; private set; }
	public static float AspectRatio => (float)Width / Height;

	public static void UpdateSize( Vector2i size )
	{
		Size = size;
	}

	public static void UpdateSize( int width = 1280, int height = 800 )
	{
		Size = new Vector2i( width, height );
	}
}
