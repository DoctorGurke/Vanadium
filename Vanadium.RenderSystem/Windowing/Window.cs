global using Vanadium.Common;
global using Vanadium.Common.Mathematics;
global using Vanadium.RenderSystem.Gui;
global using Vanadium.RenderSystem.RenderData;
global using Vanadium.RenderSystem.RenderData.Buffers;
global using Vanadium.RenderSystem.RenderData.View;
global using Vanadium.RenderSystem.Scene;
global using Vanadium.RenderSystem.Util;
global using OpenTKMath = OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace Vanadium.RenderSystem.Windowing;

public class Window : GameWindow
{
	public Window( Renderer renderer, GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings ) : base( gameWindowSettings, nativeWindowSettings )
	{
		Renderer = renderer;
	}

	private Renderer Renderer;

	private Stopwatch Timer { get; set; } = new();

	protected override void OnLoad()
	{
		base.OnLoad();
		Timer.Start();

		Log.Highlight( $"@ Window OnLoad" );
		Renderer.Load();

		CursorState = CursorState.Grabbed;

		Timer.Restart();

		Log.Highlight( "@ Window PostLoad" );
		Renderer.PostLoad();
	}

	protected override void OnRenderFrame( FrameEventArgs e )
	{
		base.OnRenderFrame( e );

		Renderer.PreRender();

		Renderer.Render( SceneWorld.Main );

		Renderer.PostRender();

		SwapBuffers();
	}

	private TimeSince TimeSinceSecondTick;
	public int FramesPerSecond;
	public double FrameTime;

	public bool UiMode = false;

	public bool WasUiMode = false;

	SceneObject? spheretest;

	protected override void OnUpdateFrame( FrameEventArgs e )
	{
		base.OnUpdateFrame( e );

		// update input first
		Input.Update( KeyboardState, MouseState );

		// calc frame statistics
		FramesPerSecond = (int)(1.0 / e.Time);
		FrameTime = e.Time;
		Time.Update( (float)e.Time, Timer.ElapsedMilliseconds * 0.001f );

		Camera.BuildActiveCamera();

		if ( TimeSinceSecondTick >= 1 )
		{
			TimeSinceSecondTick = 0;
			OnSecondTick();
		}

		// update ui
		Renderer.ImguiController?.Update( this, (float)e.Time );

		Renderer.Update();

		// do not process any input if we're not focused
		if ( !IsFocused )
		{
			return;
		}

		if ( Input.IsPressed( Keys.F1 ) )
		{
			UiMode = !UiMode;
		}

		if ( spheretest is not null )
			spheretest.Rotation = spheretest.Rotation.RotateAroundAxis( Vector3.Up, Time.Delta * 35.0f );

		if ( UiMode )
		{
			// ui was just opened, reset mouse
			if ( !WasUiMode )
			{
				// TODO: find a way to actually (re)set the mouse, lol
			}

			WasUiMode = true;
			CursorState = CursorState.Normal;
		}
		else
		{
			CursorState = CursorState.Grabbed;

			var cam = Camera.ActiveCamera;
			// no cam initialized
			if ( cam is null ) return;

			// reset lastpos so the camera doesn't snap to cursor after closing ui
			if ( cam is FirstPersonCamera fcam && WasUiMode )
				fcam.ResetLastPosition( MouseState.Position );

			cam.BuildInput( KeyboardState, MouseState );

			if ( Input.IsPressed( MouseButton.Left ) )
			{
				spheretest = new SceneObject
				{
					Position = cam.Position + cam.Rotation.Forward * 3,
					//Rotation = cam.Rotation,
					Model = Model.Primitives.Sphere,
					Scale = 0.5f
				};
				spheretest.SetMaterialOverride( "materials/discoball.vanmat" );

				for ( int i = 0; i < 5; i++ )
				{
					var child = new SceneObject
					{
						Position = Vector3.Up + Vector3.Right * (float)Math.Sin( (360.0f / 5 * i).DegreeToRadian() ) + Vector3.Forward * (float)Math.Cos( (360.0f / 5 * i).DegreeToRadian() ),
						Model = Model.Primitives.Sphere,
						Scale = 0.5f
					};
					child.SetMaterialOverride( "materials/discoball.vanmat" );
					child.Parent = spheretest;
				}

				//DebugDraw.Box( cam.Position, new Vector3( 0.2f, 0.2f, 0.2f ), -new Vector3( 0.2f, 0.2f, 0.2f ), Color.Green, 100, false );
				//DebugDraw.Line( cam.Position, cam.Position + cam.Rotation.Forward * 5, Color.White, 100, false );
				//DebugDraw.Sphere( cam.Position + cam.Rotation.Forward * 5, 0.2f, Color.Red, 100, false );
				//DebugDraw.Sphere( cam.Position + cam.Rotation.Forward * 3, Rand.Float(0, 1), Color.Random, 100 );
			}

			if ( Input.IsPressed( MouseButton.Right ) )
			{
				if ( Input.IsDown( Keys.LeftAlt ) )
				{
					Renderer.SceneLight.AddDirLight( cam.Rotation, DebugOverlay.RandomLightColor ? Color.Random : DebugOverlay.LightColor, DebugOverlay.LightBrightnessMultiplier );
				}
				else if ( Input.IsDown( Keys.LeftShift ) )
				{
					Renderer.SceneLight.AddSpotlight( cam.Position, cam.Rotation, DebugOverlay.RandomLightColor ? Color.Random : DebugOverlay.LightColor, 30, 35, 0, 0, 1, DebugOverlay.LightBrightnessMultiplier );
				}
				else
				{
					Renderer.SceneLight.AddPointlight( cam.Position + cam.Rotation.Forward, DebugOverlay.RandomLightColor ? Color.Random : DebugOverlay.LightColor, 0, 0, 1, DebugOverlay.LightBrightnessMultiplier );
				}
			}

			WasUiMode = false;
		}
	}

	protected override void OnTextInput( TextInputEventArgs e )
	{
		base.OnTextInput( e );
		Renderer.ImguiController?.OnTextInput( e );
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
		Renderer.ImguiController?.WindowResized( ClientSize );
	}
}
