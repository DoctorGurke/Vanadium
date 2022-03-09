using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Runtime.InteropServices;

namespace Vanadium;

public class Window : GameWindow {
	public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) { }

	// debugging
	private static DebugProc _debugProcCallback = DebugCallback;

	private Stopwatch Timer = new();

	public static int MatricesUniformBuffer;
	protected override void OnLoad() {
		base.OnLoad();

		// enable debugging
		_ = GCHandle.Alloc(_debugProcCallback);
		GL.DebugMessageCallback(_debugProcCallback, IntPtr.Zero);
		GL.Enable(EnableCap.DebugOutput);
		GL.Enable(EnableCap.DebugOutputSynchronous);

		// setup defaults
		GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);

		GL.Enable(EnableCap.CullFace);
		GL.CullFace(CullFaceMode.Back);

		GL.Enable(EnableCap.DepthTest);
		GL.DepthFunc(DepthFunction.Less);

		GL.Enable(EnableCap.Blend);
		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		GL.BlendFuncSeparate(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha, BlendingFactorSrc.One, BlendingFactorDest.Zero);

		GL.Enable(EnableCap.Multisample);

		GL.Enable(EnableCap.TextureCubeMapSeamless);

		// setup matrices uniform buffer
		MatricesUniformBuffer = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.UniformBuffer, MatricesUniformBuffer);
		GL.BufferData(BufferTarget.UniformBuffer, 2 * Marshal.SizeOf(typeof(Matrix4)), IntPtr.Zero, BufferUsageHint.StaticDraw);
		GL.BindBuffer(BufferTarget.UniformBuffer, 0);
		GL.BindBufferRange(BufferRangeTarget.UniformBuffer, 0, MatricesUniformBuffer, IntPtr.Zero, 2 * Marshal.SizeOf(typeof(Matrix4)));

		// precache error model
		Model.Precache(Model.ErrorModel);

		// set skybox
		Skybox.Load("materials/skybox/skybox02.vanmat");

		new SceneObject {
			Model = Model.Load("models/brickwall.fbx"),
			Position = Vector3.Down
		};

		new SceneObject {
			Model = Model.Primitives.Axis
		};

		//var sceneObject1 = new SceneObject {
		//	Position = Vector3.Right * 3,
		//	Rotation = Rotation.Identity.RotateAroundAxis(Vector3.Right, 45)
		//};
		//sceneObject1.Model = Model.Load("models/suzanne.fbx");

		//var sceneObject2 = new SceneObject {
		//	Position = Vector3.Up * 5 + Vector3.Right * 3,
		//	Scale = 0.5f
		//};
		//sceneObject2.Parent = sceneObject1;
		//sceneObject2.Model = Model.Load("models/fancy.fbx");

		// init camera
		_ = new FirstPersonCamera {
			Position = Vector3.Backward * 3 + Vector3.Up + Vector3.Right
		};

		CursorGrabbed = true;
		Timer.Start();
	}

	protected override void OnRenderFrame(FrameEventArgs e) {
		base.OnRenderFrame(e);

		// reset depth state
		GL.Enable(EnableCap.DepthTest);
		GL.DepthFunc(DepthFunction.Less);

		// reset cull state
		GL.Enable(EnableCap.CullFace);
		GL.CullFace(CullFaceMode.Back);

		// clear buffer
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

		// update Matrices uniform buffer
		GL.BindBuffer(BufferTarget.UniformBuffer, MatricesUniformBuffer);
		// projection matrix
		var projection = Camera.ActiveCamera.ProjectionMatrix;
		GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, Marshal.SizeOf(typeof(Matrix4)), ref projection.Row0.X );
		// view matrix
		var view = Camera.ActiveCamera.ViewMatrix;
		GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr) Marshal.SizeOf(typeof(Matrix4)), Marshal.SizeOf(typeof(Matrix4)), ref view.Row0.X);

		// draw opaques first
		SceneWorld.DrawOpaques();

		// draw skybox after opaques
		Skybox.ActiveSkybox.Draw();

		// draw transparents last
		SceneWorld.DrawTransparents();

		SwapBuffers();
	}

	private TimeSince TimeSinceSecondTick;
	private int FramesPerSecond;
	private double FrameTime;

	protected override void OnUpdateFrame(FrameEventArgs e) {
		base.OnUpdateFrame(e);

		FramesPerSecond = (int)(1.0 / e.Time);
		FrameTime = e.Time;

		if(TimeSinceSecondTick >= 1) {
			TimeSinceSecondTick = 0;
			OnSecondTick();
		}

		Time.Update((float) e.Time, Timer.ElapsedMilliseconds * 0.001f);
		Camera.BuildActiveCamera();

		// do not process any input if we're not focused
		if(!IsFocused) {
			return;
		}

		Camera.ActiveCamera.BuildInput(KeyboardState, MouseState);

		var mouse = MouseState;
		var input = KeyboardState;
		// close the window when ESC is pressed down
		if(input.IsKeyDown(Keys.Escape)) {
			Close();
		}

		var cam = Camera.ActiveCamera;
		if(mouse.IsButtonDown(MouseButton.Button1) && !mouse.WasButtonDown(MouseButton.Button1)) {
			//var ent = new SceneObject {
			//	Position = cam.Position + cam.Rotation.Forward,
			//	Rotation = cam.Rotation
			//};
			//ent.Model = Model.Load("models/transparency_test.fbx");
			new TestObject {
				Position = cam.Position + cam.Rotation.Forward,
				Rotation = cam.Rotation
			};
		}
	}

	public void OnSecondTick() {
		Title = $"Vanadium FPS:{FramesPerSecond} FT: {FrameTime:0.####}s";
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
			Console.WriteLine($"{severity} : {type} | {messageString}");

		if(type == DebugType.DebugTypeError) {
			throw new Exception(messageString);
		}
	}
}
