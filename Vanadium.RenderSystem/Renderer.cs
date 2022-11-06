using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Linq;
using Vanadium.RenderSystem.Windowing;

namespace Vanadium;

public class Renderer
{
	protected Window? Window { get; private set; }

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

	public virtual void OnLoad() { }

	public virtual void PostLoad() { }
}
