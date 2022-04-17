using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Vanadium.Common;

public static class Input
{
	private static KeyboardState? _keyboard;
	private static KeyboardState Keyboard
	{
		get
		{
			if ( _keyboard is null ) throw new Exception( "Tried to query unitialized input!" );
			return _keyboard;
		}
		set
		{
			_keyboard = value;
		}
	}

	private static MouseState? _mouse;

	private static MouseState Mouse
	{
		get
		{
			if ( _mouse is null ) throw new Exception("Tried to query unitialized input!");
			return _mouse;
		}

		set
		{
			_mouse = value;
		}
	}


	public static void Update(KeyboardState keyboard, MouseState mouse)
	{
		Keyboard = keyboard;
		Mouse = mouse;
	}

	public static bool IsPressed(Keys key) => Keyboard.IsKeyPressed(key);
	public static bool IsReleased(Keys key) => Keyboard.IsKeyReleased(key);
	public static bool IsDown(Keys keys) => Keyboard.IsKeyDown(keys);

	public static bool IsPressed( MouseButton key ) => Mouse.IsButtonDown( key ) && !Mouse.WasButtonDown( key );
	public static bool IsReleased( MouseButton key ) => !Mouse.IsButtonDown( key ) && Mouse.WasButtonDown( key );
	public static bool IsDown( MouseButton keys ) => Mouse.IsButtonDown( keys );
}
