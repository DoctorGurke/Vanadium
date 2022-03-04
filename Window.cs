using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Vanadium;

public class Window : GameWindow {
	public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) { }

	private Model? model;

	protected override void OnLoad() {
		base.OnLoad();

		GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

		//GL.Enable(EnableCap.DepthTest);
		//GL.Enable(EnableCap.CullFace);

		model = Model.Load("resources/models/fcube.obj");

		// init camera
		_ = new FirstPersonCamera();
		CursorGrabbed = true;
	}

	protected override void OnRenderFrame(FrameEventArgs e) {
		base.OnRenderFrame(e);

		//var fps = (int)(1f / e.Time);

		GL.Clear(ClearBufferMask.ColorBufferBit);

		model?.Draw();

		SwapBuffers();
	}

	protected override void OnUpdateFrame(FrameEventArgs e) {
		base.OnUpdateFrame(e);

		Time.Update((float) e.Time);
		Camera.BuildActiveCamera();

		// do not process any input if we're not focused
		if(!IsFocused) {
			return;
		}

		Camera.ActiveCamera.BuildInput(KeyboardState, MouseState);

		var input = KeyboardState;
		// close the window when ESC is pressed down
		if(input.IsKeyDown(Keys.Escape)) {
			Close();
		}
	}

	protected override void OnResize(ResizeEventArgs e) {
		base.OnResize(e);

		Screen.UpdateSize(Size.X, Size.Y);
		GL.Viewport(0, 0, Size.X, Size.Y);
	}
}
