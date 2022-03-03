namespace Vanadium;

public static class Screen {
	public static float Width { get; private set; }
	public static float Height { get; private set; }
	public static float AspectRatio => Width / Height;

	public static void UpdateSize(int width = 1280, int height = 800) {
		Width = width;
		Height = height;
	}
}
