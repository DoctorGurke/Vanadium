using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Runtime.InteropServices;

namespace Vanadium;

public class Window : GameWindow {
	public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) { }

	private Model? model;

	// debugging
	private static DebugProc _debugProcCallback = DebugCallback;
	private static GCHandle _debugProcCallbackHandle;

	protected override void OnLoad() {
		base.OnLoad();

		// enable debugging
		_debugProcCallbackHandle = GCHandle.Alloc(_debugProcCallback);
		GL.DebugMessageCallback(_debugProcCallback, IntPtr.Zero);
		GL.Enable(EnableCap.DebugOutput);
		GL.Enable(EnableCap.DebugOutputSynchronous);

		GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

		GL.Enable(EnableCap.CullFace);
		GL.CullFace(CullFaceMode.Back);

		GL.Enable(EnableCap.DepthTest);
		GL.DepthFunc(DepthFunction.Less);

		model = Model.Load("resources/models/icosphere.fbx");

		// init camera
		_ = new FirstPersonCamera {
			Position = Vector3.Backward * 3
		};

		CursorGrabbed = true;
	}

	protected override void OnRenderFrame(FrameEventArgs e) {
		base.OnRenderFrame(e);

		//var fps = (int)(1f / e.Time);

		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

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

	[DebuggerStepThrough]
	private static void DebugCallback(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam) {
		string messageString = Marshal.PtrToStringAnsi(message, length);
		if(!(severity == DebugSeverity.DebugSeverityNotification))
			Console.WriteLine($"{severity} : {type} \t | \t {messageString}");

		if(type == DebugType.DebugTypeError) {
			throw new Exception(messageString);
		}
	}
}
