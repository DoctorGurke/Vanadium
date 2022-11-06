using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Linq;
using Vanadium.RenderSystem.Gui;
using Vanadium.RenderSystem.Windowing;
using Vector3 = Vanadium.Common.Mathematics.Vector3;

namespace Vanadium;

public class Renderer
{
	protected Window? Window { get; private set; }

	public UniformBufferManager UniformBufferManager = new();
	public SceneLightManager SceneLight = new();

	public ImGuiController? ImguiController;

	protected Renderer() : this( "Vanadium Renderer", new Vector2i( 1280, 800 ), true, 4 ) { }

	protected Renderer( string title, Vector2i size, bool srgb = true, int samples = 0 )
	{
		Assert.ResourcePresent( Shader.Error );
		Assert.ResourcePresent( Material.Error );
		Assert.ResourcePresent( Model.Error );
		Assert.ResourcePresent( Texture.Error );

		// init the settings for our main window
		var windowSettings = new NativeWindowSettings()
		{
			Size = size,
			Title = title,
			WindowState = WindowState.Normal,
			StartFocused = true,
			NumberOfSamples = samples,
			SrgbCapable = srgb
		};

		// init and run our window type
		using var window = new Window( this, GameWindowSettings.Default, windowSettings );
		Window = window;

		window.CenterWindow();
		window.Run();
	}

	public void Render( SceneWorld scene, bool clear = true )
	{
		// clear buffer
		if ( clear )
			GL.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

		// update per view buffer
		UniformBufferManager.UpdateSceneUniformBuffer();

		// drawing the scene

		// draw opaques first
		SceneWorld.Main.DrawOpaqueLayer();

		// draw skybox after opaques
		SceneWorld.Main.DrawSkyboxLayer();

		DebugDraw.Line( Vector3.Zero, Vector3.Up * 10, Color.Red );

		// process debug lines and sort them into their buffers
		DebugDraw.PrepareDraw();

		// draw lines with depth first
		DebugDraw.DrawDepthLines();

		// draw translucents last
		SceneWorld.Main.DrawTranslucentLayer();

		// draw lines without depth after everything
		DebugDraw.DrawNoDepthLines();

		// show debugoverlay in top left (fps, frametime and ui mode indicators)
		DebugOverlay.Draw( this );

		// draw ui
		ImguiController?.Draw();
	}

	public virtual void OnLoad() { }

	public virtual void PostLoad() { }

	public virtual void PreRender() { }

	public virtual void PostRender() { }

	public virtual void OnFrame() { }
}
