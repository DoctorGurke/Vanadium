using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Vanadium.RenderSystem.Gui;
using Vanadium.RenderSystem.Windowing;
using Keys = OpenTK.Windowing.GraphicsLibraryFramework;
using Vector3 = Vanadium.Common.Mathematics.Vector3;

namespace Vanadium;

public class Renderer
{
	protected Window? Window { get; private set; }

	public UniformBufferManager UniformBufferManager = new();
	public SceneLightManager SceneLight = new();

	public ImGuiController? ImguiController;

	// debugging
	private static readonly DebugProc _debugProcCallback = DebugCallback;

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
		// render to framebuffer
		BindFramebuffer();

		GL.ClearColor( 1.0f, 0.0f, 0.0f, 1.0f );

		// clear buffer
		if ( clear )
			GL.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit );

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

		// switch back to window drawing
		UnbindFramebuffer();
		Window?.MakeCurrent();

		GL.ClearColor( 0.0f, 0.0f, 1.0f, 1.0f );

		// clear buffer
		if ( clear )
			GL.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit );

		// disable depth for screen quad
		GL.Disable( EnableCap.DepthTest );

		shader.Use();
		GL.BindVertexArray( vao );
		GL.BindTexture( TextureTarget.Texture2D, color );
		GL.DrawArrays( PrimitiveType.Triangles, 0, 6 );
	}

	private float[] quadVertices = {
        // positions   // texCoords
        -1.0f,  1.0f,  0.0f, 1.0f,
		-1.0f, -1.0f,  0.0f, 0.0f,
		 1.0f, -1.0f,  1.0f, 0.0f,

		-1.0f,  1.0f,  0.0f, 1.0f,
		 1.0f, -1.0f,  1.0f, 0.0f,
		 1.0f,  1.0f,  1.0f, 1.0f
	};

	private int fbo;
	private int color;
	private int depthStencil;

	private int vao;
	private int vbo;

	private Shader shader;

	private void PrepareFramebuffer()
	{
		GLUtil.CreateFrameBuffer( "Main Scene", out fbo );

		BindFramebuffer();

		// color attachment texture
		GL.GenTextures( 1, out color );
		GL.BindTexture( TextureTarget.Texture2D, color );

		GL.TexImage2D( TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, 1280, 800, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero );

		GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear );
		GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear );

		// unbind color texture
		GL.BindTexture( TextureTarget.Texture2D, 0 );

		// add color texture to framebuffer
		GL.FramebufferTexture2D( FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, color, 0 );

		// depth attachment texture
		GL.GenTextures( 1, out depthStencil );
		GL.BindTexture( TextureTarget.Texture2D, depthStencil );

		GL.TexImage2D( TextureTarget.Texture2D, 0, PixelInternalFormat.Depth24Stencil8, 1280, 800, 0, PixelFormat.DepthStencil, PixelType.UnsignedInt248, IntPtr.Zero );

		// unbind depth texture
		GL.BindTexture( TextureTarget.Texture2D, 0 );

		// add depth/stencil texture to framebuffer
		GL.FramebufferTexture2D( FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, TextureTarget.Texture2D, depthStencil, 0 );

		if ( GL.CheckFramebufferStatus( FramebufferTarget.Framebuffer ) != FramebufferErrorCode.FramebufferComplete )
			throw new InvalidOperationException( "incomplete framebuffer" );

		UnbindFramebuffer();

		// prepare screen quad
		GL.GenVertexArrays( 1, out vao );
		GL.GenBuffers( 1, out vbo );
		GL.BindVertexArray( vao );
		GL.BindBuffer( BufferTarget.ArrayBuffer, vbo );
		GL.BufferData( BufferTarget.ArrayBuffer, sizeof( float ) * 4 * 6, quadVertices, BufferUsageHint.StaticDraw );
		GL.EnableVertexAttribArray( 0 );
		GL.VertexAttribPointer( 0, 2, VertexAttribPointerType.Float, false, 4 * sizeof( float ), 0 );
		GL.EnableVertexAttribArray( 1 );
		GL.VertexAttribPointer( 1, 2, VertexAttribPointerType.Float, false, 4 * sizeof( float ), 2 * sizeof( float ) );

		// load framebuffer shader
		shader = new Shader( $"core/shaders/core/framebuffer.vfx", new Material() );
	}

	private void BindFramebuffer()
	{
		GL.BindFramebuffer( FramebufferTarget.Framebuffer, fbo );
	}

	private void UnbindFramebuffer()
	{
		GL.BindFramebuffer( FramebufferTarget.Framebuffer, 0 );
	}

	public void Load()
	{
		GL.ClearColor( 0.6f, 0.1f, 0.8f, 1.0f );

		// enable debugging
		_ = GCHandle.Alloc( _debugProcCallback );
		GL.DebugMessageCallback( _debugProcCallback, IntPtr.Zero );
		GL.Enable( EnableCap.DebugOutput );
		GL.Enable( EnableCap.DebugOutputSynchronous );

		// setup core uniform buffers
		UniformBuffer.InitCore();

		SceneLight.SetAmbientLightColor( Color.FromBytes( 36, 60, 102 ) );

		// init debug line buffers
		DebugDraw.Init();

		// init imgui
		if ( Window is not null )
			ImguiController = new ImGuiController( Window.ClientSize );

		OnLoad();
	}

	public void PostLoad()
	{
		PrepareFramebuffer();

		OnPostLoad();
	}

	public void PreRender()
	{
		PrepareRender();

		OnPreRender();
	}

	public void PrepareRender()
	{
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

		GL.Enable( EnableCap.FramebufferSrgb );

		GL.Enable( EnableCap.Multisample );

		GL.Enable( EnableCap.TextureCubeMapSeamless );
	}

	public void PostRender()
	{
		OnPostRender();

	}

	public void Update()
	{
		OnUpdate();
	}

	public virtual void OnLoad() { }

	public virtual void OnPostLoad() { }

	public virtual void OnPreRender() { }

	public virtual void OnPostRender() { }

	public virtual void OnUpdate()
	{
		if ( Input.IsPressed( Keys.Keys.F5 ) )
		{
			var timestamp = DateTime.Now.ToString( "yyyyMMddHHmmssffff" );
			Log.Highlight( $"new screenshot {timestamp}" );
			Screenshot( $"Vanadium_{timestamp}" );
		}
	}

	private void Screenshot( string name )
	{
		if ( Window is null )
			return;

		var width = Window.ClientSize.X;
		var height = Window.ClientSize.Y;

		var imagedata = new byte[3 * width * height];
		GL.ReadPixels( 0, 0, width, height, PixelFormat.Bgr, PixelType.UnsignedByte, imagedata );

		IntPtr intPtr = Marshal.AllocHGlobal( imagedata.Length );
		Marshal.Copy( imagedata, 0, intPtr, imagedata.Length );

		System.Drawing.Imaging.PixelFormat pixelFormat = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
		int strideFraction = (3 * width) % 4;
		if ( strideFraction > 0 ) strideFraction = 4 - strideFraction;

		var bmp = new System.Drawing.Bitmap( width, height, 3 * width + strideFraction, pixelFormat, intPtr );

		bmp.RotateFlip( System.Drawing.RotateFlipType.RotateNoneFlipY );
		bmp.Save( $"C:\\Users\\docgu\\OneDrive\\Desktop\\screenshots\\{name}.png", System.Drawing.Imaging.ImageFormat.Png );
		bmp.Dispose();
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
