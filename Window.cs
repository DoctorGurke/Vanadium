using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Vanadium;

public class Window : GameWindow {
	public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) { }

	protected override void OnUpdateFrame(FrameEventArgs args) {
		base.OnUpdateFrame(args);

		// close the window when ESC is pressed down
		if(KeyboardState.IsKeyDown(Keys.Escape)) {
			Close();
		}
	}
}
