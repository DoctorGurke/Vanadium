using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace Vanadium;

public class Window : GameWindow
{
	public Window( GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings ) : base( gameWindowSettings, nativeWindowSettings ) { }

	// debugging
	private static readonly DebugProc _debugProcCallback = DebugCallback;

	private readonly Stopwatch Timer = new();

	public static int PerViewUniformBufferHandle;

	private ImGuiController _guicontroller;

	private struct PerViewUniformBuffer
	{
		public Matrix4 g_matWorldToProjection;  // 16	(col0)
												// 16	(col1)
												// 16	(col2)
												// 16	(col3)
		public Matrix4 g_matWorldToView;        // 16	(col0)
												// 16	(col1)
												// 16	(col2)
												// 16	(col3)
		public Vector3 g_vCameraPositionWs;     // 12	
		public float pad1;                      // 4	(padding)
		public Vector3 g_vCameraDirWs;          // 12
		public float pad2;                      // 4	(padding)
		public Vector3 g_vCameraUpDirWs;        // 12
		public float pad3;                      // 4	(padding)
		public Vector2 g_vViewportSize;         // 8
		public float g_flTime;                  // 4
		public float g_flNearPlane;             // 4
		public float g_flFarPlane;              // 4
	}

	protected override void OnLoad()
	{
		base.OnLoad();

		// enable debugging
		_ = GCHandle.Alloc( _debugProcCallback );
		GL.DebugMessageCallback( _debugProcCallback, IntPtr.Zero );
		GL.Enable( EnableCap.DebugOutput );
		GL.Enable( EnableCap.DebugOutputSynchronous );

		// setup defaults
		GL.ClearColor( 0.1f, 0.1f, 0.1f, 1.0f );

		GL.Enable( EnableCap.CullFace );
		GL.CullFace( CullFaceMode.Back );

		GL.Enable( EnableCap.DepthTest );
		GL.DepthFunc( DepthFunction.Less );

		GL.Enable( EnableCap.Blend );
		GL.BlendFunc( BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha );
		GL.BlendFuncSeparate( BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha, BlendingFactorSrc.One, BlendingFactorDest.Zero );

		GL.Enable( EnableCap.Multisample );

		GL.Enable( EnableCap.TextureCubeMapSeamless );

		// setup per view uniform buffer
		GLUtil.CreateBuffer( "PerViewUniformBuffer", out PerViewUniformBufferHandle );
		GL.BindBuffer( BufferTarget.UniformBuffer, PerViewUniformBufferHandle );
		var perviewbuffersize = Marshal.SizeOf( typeof( PerViewUniformBuffer ) ).RoundUpToMultipleOf( 16 );
		GL.BufferData( BufferTarget.UniformBuffer, perviewbuffersize, IntPtr.Zero, BufferUsageHint.StaticDraw );
		GL.BindBuffer( BufferTarget.UniformBuffer, 0 );
		GL.BindBufferRange( BufferRangeTarget.UniformBuffer, 0, PerViewUniformBufferHandle, IntPtr.Zero, perviewbuffersize );

		// init debug line buffers
		DebugDraw.Init();

		// init ui
		_guicontroller = new ImGuiController( ClientSize );

		// precache error model
		Model.Precache( Model.ErrorModel );

		// set skybox
		Skybox.Load( "materials/skybox/skybox02.vanmat" );

		var floor = new SceneObject
		{
			Model = Model.Load( "models/brickwall.fbx" ),
			Position = Vector3.Down
		};
		floor.RenderColor = Color.Blue;
		floor.TintAmount = 0.5f;

		_ = new SceneObject
		{
			Model = Model.Primitives.Axis
		};

		// init camera
		_ = new FirstPersonCamera
		{
			Position = Vector3.Backward * 3 + Vector3.Up + Vector3.Right
		};

		CursorGrabbed = true;

		Timer.Start();
	}

	protected override void OnRenderFrame( FrameEventArgs e )
	{
		base.OnRenderFrame( e );

		// reset depth state
		GL.Enable( EnableCap.DepthTest );
		GL.DepthFunc( DepthFunction.Less );

		// reset cull state
		GL.Enable( EnableCap.CullFace );
		GL.CullFace( CullFaceMode.Back );

		// reset blending
		GL.Enable( EnableCap.Blend );
		GL.BlendFunc( BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha );
		GL.BlendFuncSeparate( BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha, BlendingFactorSrc.One, BlendingFactorDest.Zero );

		// clear buffer
		GL.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

		// update per view uniform buffer
		GL.BindBuffer( BufferTarget.UniformBuffer, PerViewUniformBufferHandle );
		// prepare data
		var perviewuniformbuffer = new PerViewUniformBuffer
		{
			g_matWorldToProjection = Camera.ActiveCamera.ProjectionMatrix,
			g_matWorldToView = Camera.ActiveCamera.ViewMatrix,
			g_vCameraPositionWs = Camera.ActiveCamera.Position,
			g_vCameraDirWs = Camera.ActiveCamera.Rotation.Forward,
			g_vCameraUpDirWs = Camera.ActiveCamera.Rotation.Up,
			g_flTime = Time.Now,
			g_flNearPlane = Camera.ActiveCamera.ZNear,
			g_flFarPlane = Camera.ActiveCamera.ZFar,
			g_vViewportSize = Screen.Size
		};
		// put data in buffer
		GL.BufferData( BufferTarget.UniformBuffer, Marshal.SizeOf( perviewuniformbuffer ).RoundUpToMultipleOf( 16 ), ref perviewuniformbuffer, BufferUsageHint.StaticDraw );

		// drawing the scene

		// draw opaques first
		SceneWorld.DrawOpaques();

		// draw skybox after opaques
		Skybox.ActiveSkybox.Draw();

		DebugDraw.Line( Vector3.Zero, Vector3.Up * 10, Color.Red );

		// process debug lines and sort them into their buffers
		DebugDraw.PrepareDraw();

		// draw lines with depth first
		DebugDraw.DrawDepthLines();

		// draw transparents last
		SceneWorld.DrawTransparents();

		// draw lines without depth after everything
		DebugDraw.DrawNoDepthLines();

		//ImGui.ShowDemoWindow();

		// show debugoverlay in top left (fps, frametime and ui mode indicators)
		DebugOverlay.Draw( this );

		// draw ui
		_guicontroller.Draw();

		SwapBuffers();
	}

	private TimeSince TimeSinceSecondTick;
	public int FramesPerSecond;
	public double FrameTime;

	public bool UiMode = false;

	public bool WasUiMode = false;

	protected override void OnUpdateFrame( FrameEventArgs e )
	{
		base.OnUpdateFrame( e );

		FramesPerSecond = (int)(1.0 / e.Time);
		FrameTime = e.Time;

		if ( TimeSinceSecondTick >= 1 )
		{
			TimeSinceSecondTick = 0;
			OnSecondTick();
		}

		Time.Update( (float)e.Time, Timer.ElapsedMilliseconds * 0.001f );
		Camera.BuildActiveCamera();

		// update ui
		_guicontroller.Update( this, (float)e.Time );

		// do not process any input if we're not focused
		if ( !IsFocused )
		{
			return;
		}

		var mouse = MouseState;
		var input = KeyboardState;

		if ( input.IsKeyPressed( Keys.F1 ) )
		{
			UiMode = !UiMode;
		}

		if ( UiMode )
		{
			// ui was just opened, reset mouse
			if ( !WasUiMode )
			{
				// TODO: find a way to actually (re)set the mouse, lol
			}

			WasUiMode = true;
			CursorGrabbed = false;
		}
		else
		{
			CursorGrabbed = true;

			var cam = Camera.ActiveCamera;
			// reset lastpos so the camera doesn't snap to cursor after closing ui
			if ( cam is FirstPersonCamera fcam && WasUiMode )
				fcam.ResetLastPosition( MouseState.Position );

			cam.BuildInput( KeyboardState, MouseState );

			if ( mouse.IsButtonDown( MouseButton.Button1 ) && !mouse.WasButtonDown( MouseButton.Button1 ) )
			{
				_ = new TestObject
				{
					Position = cam.Position + cam.Rotation.Forward
				};
				//new SceneObject
				//{
				//	Model = Model.Load( "models/transparency_test.fbx" ),
				//	Position = cam.Position + cam.Rotation.Forward,
				//	Rotation = cam.Rotation
				//};
				DebugDraw.Line( cam.Position, cam.Position + cam.Rotation.Forward * 10, Color.Random, 10, false );
			}

			if ( mouse.IsButtonDown( MouseButton.Button2 ) && !mouse.WasButtonDown( MouseButton.Button2 ) )
			{
				for ( int i = 0; i < 50_000; i++ )
				{
					DebugDraw.Box( cam.Position, Vector3.One * 3, Vector3.One * -3, Color.Random, 100, true );
				}
			}

			WasUiMode = false;
		}
	}

	protected override void OnTextInput( TextInputEventArgs e )
	{
		base.OnTextInput( e );
		_guicontroller.OnTextInput( e );
	}

	protected override void OnMouseWheel( MouseWheelEventArgs e )
	{
		base.OnMouseWheel( e );
		_guicontroller.OnMouseWheel( e );
	}

	public void OnSecondTick()
	{
		DebugOverlay.FPS = FramesPerSecond;
		DebugOverlay.FT = (float)FrameTime;
	}

	protected override void OnResize( ResizeEventArgs e )
	{
		base.OnResize( e );

		Screen.UpdateSize( ClientSize );
		GL.Viewport( 0, 0, ClientSize.X, ClientSize.Y );
		_guicontroller.WindowResized( ClientSize );
	}

	[DebuggerStepThrough]
	private static void DebugCallback( DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam )
	{
		string messageString = Marshal.PtrToStringAnsi( message, length );
		if ( !(severity == DebugSeverity.DebugSeverityNotification) )
			Console.WriteLine( $"{severity} : {type} | {messageString}" );

		if ( type == DebugType.DebugTypeError )
		{
			throw new Exception( messageString );
		}
	}
}
