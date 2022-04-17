using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Runtime.InteropServices;

namespace Vanadium;

public class Window : GameWindow
{
	public Window( GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings ) : base( gameWindowSettings, nativeWindowSettings ) { }

	// debugging
	private static readonly DebugProc _debugProcCallback = DebugCallback;

	private readonly Stopwatch Timer = new();

	private ImGuiController? _guicontroller;
	public UniformBufferManager UniformBufferManager = new();
	public SceneLightManager SceneLight = new();

	protected override void OnLoad()
	{
		base.OnLoad();
		Timer.Start();

		// enable debugging
		_ = GCHandle.Alloc( _debugProcCallback );
		GL.DebugMessageCallback( _debugProcCallback, IntPtr.Zero );
		GL.Enable( EnableCap.DebugOutput );
		GL.Enable( EnableCap.DebugOutputSynchronous );

		// setup defaultse
		GL.ClearColor( 0.1f, 0.1f, 0.1f, 1.0f );

		GL.Enable( EnableCap.CullFace );
		GL.CullFace( CullFaceMode.Back );

		GL.Enable( EnableCap.DepthTest );
		GL.DepthFunc( DepthFunction.Less );

		GL.Enable( EnableCap.Blend );
		GL.BlendFunc( BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha );
		GL.BlendFuncSeparate( BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha, BlendingFactorSrc.One, BlendingFactorDest.Zero );

		GL.Enable( EnableCap.FramebufferSrgb );

		GL.Enable( EnableCap.Multisample );

		GL.Enable( EnableCap.TextureCubeMapSeamless );

		// setup uniform buffers
		UniformBufferManager.Init();

		SceneLight.SetAmbientLightColor( new Color( 36.0f / 255.0f, 60.0f / 255.0f, 102.0f / 255.0f ) );

		// init debug line buffers
		DebugDraw.Init();

		// init ui
		_guicontroller = new ImGuiController( ClientSize );

		// precache error model
		Model.Precache( Model.Error );

		// set skybox
		Skybox.Load( "materials/skybox/skybox03.vanmat" );

		var floor = new SceneObject
		{
			Model = Model.Load( "models/brickwall.fbx" ),
			Position = Vector3.Down,
			Scale = 0.5f
		};
		floor.SetMaterialOverride( "materials/pbrtest/tiles.vanmat" );

		_ = new SceneObject
		{
			Model = ModelPrimitives.Axis
		};

		// init camera
		_ = new FirstPersonCamera
		{
			Position = Vector3.Backward * 3 + Vector3.Up + Vector3.Right
		};

		CursorGrabbed = true;

		Log.Debug( $"Load took: {Timer.ElapsedMilliseconds}ms" );

		Timer.Restart();
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

		// update per view buffer
		UniformBufferManager.UpdatePerViewUniformBuffer();

		// drawing the scene

		// draw opaques first
		SceneWorld.DrawOpaques();

		// draw skybox after opaques
		Skybox.ActiveSkybox?.Draw();

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
		_guicontroller?.Draw();

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

		Input.Update( KeyboardState, MouseState );

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
		_guicontroller?.Update( this, (float)e.Time );

		// do not process any input if we're not focused
		if ( !IsFocused )
		{
			return;
		}

		if ( Input.IsPressed( Keys.F1 ) )
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
			// no cam initialized
			if ( cam is null ) return;

			// reset lastpos so the camera doesn't snap to cursor after closing ui
			if ( cam is FirstPersonCamera fcam && WasUiMode )
				fcam.ResetLastPosition( MouseState.Position );

			cam.BuildInput( KeyboardState, MouseState );

			if ( Input.IsPressed( MouseButton.Left ) )
			{
				var sphere = new SceneObject
				{
					Position = cam.Position + cam.Rotation.Forward * 3,
					//Rotation = cam.Rotation,
					Model = ModelPrimitives.Sphere
					//Scale = 0.3f
				};
				sphere.SetMaterialOverride( "materials/discoball.vanmat" );

				//DebugDraw.Box( cam.Position, new Vector3( 0.2f, 0.2f, 0.2f ), -new Vector3( 0.2f, 0.2f, 0.2f ), Color.Green, 100, false );
				//DebugDraw.Line( cam.Position, cam.Position + cam.Rotation.Forward * 5, Color.White, 100, false );
				//DebugDraw.Sphere( cam.Position + cam.Rotation.Forward * 5, 0.2f, Color.Red, 100, false );
				//DebugDraw.Sphere( cam.Position + cam.Rotation.Forward * 3, Rand.Float(0, 1), Color.Random, 100 );
			}

			if ( Input.IsPressed( MouseButton.Right ) )
			{
				if ( Input.IsDown( Keys.LeftAlt ) )
				{
					SceneLight.AddDirLight( cam.Rotation );
				}
				else if ( Input.IsDown( Keys.LeftShift ) )
				{
					SceneLight.AddSpotlight( cam.Position, cam.Rotation, Color.White, 30, 35, 0, 0, 1, DebugOverlay.LightBrightnessMultiplier );
				}
				else
				{
					SceneLight.AddPointlight( cam.Position + cam.Rotation.Forward, Color.Random, 0, 0, 1, DebugOverlay.LightBrightnessMultiplier );
				}
			}

			WasUiMode = false;
		}
	}

	protected override void OnTextInput( TextInputEventArgs e )
	{
		base.OnTextInput( e );
		_guicontroller?.OnTextInput( e );
	}

	protected override void OnMouseWheel( MouseWheelEventArgs e )
	{
		base.OnMouseWheel( e );
		ImGuiController.OnMouseWheel( e );
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
		_guicontroller?.WindowResized( ClientSize );
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
