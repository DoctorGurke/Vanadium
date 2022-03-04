using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Vanadium;

public class Window : GameWindow {
	public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) { }

	private readonly float[] _vertices =
	{
		// vert pos				// uv0			// vert color
		 0.5f,  0.5f, 0.0f,		1.0f, 1.0f,		1.0f, 0.0f, 0.0f, // top right
         0.5f, -0.5f, 0.0f,		1.0f, 0.0f,		0.0f, 1.0f, 0.0f, // bottom right
        -0.5f, -0.5f, 0.0f,		0.0f, 0.0f,		0.0f, 0.0f, 1.0f, // bottom left
        -0.5f,  0.5f, 0.0f,		0.0f, 1.0f,		1.0f, 1.0f, 0.0f  // top left
    };

	private readonly uint[] _indices =
	{
        0, 1, 3, // The first triangle will be the bottom-right half of the triangle
        1, 2, 3  // Then the second will be the top-right half of the triangle
    };

	private int _vertexBufferObject;

	private int _vertexArrayObject;

	private int _elementBufferObject;

	private Shader _shader;

	private Stopwatch _timer;

	private Texture _texture0;
	private Texture _texture1;

	protected override void OnLoad() {
		base.OnLoad();

		GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

		// create, bind and populate vbo
		_vertexBufferObject = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
		GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

		_vertexArrayObject = GL.GenVertexArray();
		GL.BindVertexArray(_vertexArrayObject);

		// use shader first to get attributes
		_shader = new Shader("shaders/generic.vert", "shaders/generic.frag");
		_shader.Use();

		// vertex positions
		var vertexPositionLocation = _shader.GetAttribLocation("vPosition");
		GL.EnableVertexAttribArray(vertexPositionLocation);
		GL.VertexAttribPointer(vertexPositionLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);

		// uv0
		var uv0Location = _shader.GetAttribLocation("vTexCoord0");
		GL.EnableVertexAttribArray(uv0Location);
		GL.VertexAttribPointer(uv0Location, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));

		// vertex color
		var vertexColorLocation = _shader.GetAttribLocation("vColor");
		GL.EnableVertexAttribArray(vertexColorLocation);
		GL.VertexAttribPointer(vertexColorLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 5 * sizeof(float));

		// create, bind and populate ebo
		_elementBufferObject = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
		GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

		_timer = new Stopwatch();
		_timer.Start();

		_texture0 = Texture.LoadFromFile("resources/textures/rudy.jpg");
		_texture0.Use(TextureUnit.Texture0);

		_texture1 = Texture.LoadFromFile("resources/textures/mask_debug.jpg");
		_texture1.Use(TextureUnit.Texture1);

		_shader.Set("texture0", 0);
		_shader.Set("texture1", 1);

		// init camera
		_ = new FirstPersonCamera();
		CursorGrabbed = true;
	}

	protected override void OnRenderFrame(FrameEventArgs e) {
		base.OnRenderFrame(e);

		//var fps = (int)(1f / e.Time);

		GL.Clear(ClearBufferMask.ColorBufferBit);

		_texture0.Use(TextureUnit.Texture0);
		_texture1.Use(TextureUnit.Texture1);
		_shader.Use();

		double time = _timer.Elapsed.TotalSeconds;
		float tintAmount = ((float)Math.Sin(time) + 1 ) / 2;

		_shader.Set("tintAmount", tintAmount);

		var model = Matrix4.Identity;
		//model *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians((float)_timer.Elapsed.TotalSeconds * 10));
		//model *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians((float)_timer.Elapsed.TotalSeconds * 14));
		//model *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians((float)_timer.Elapsed.TotalSeconds * 3));
		//model *= Matrix4.CreateScale(tintAmount);

		_shader.Set("model", model);
		var view = Camera.ActiveCamera.ViewMatrix;
		_shader.Set("view", view);
		var proj = Camera.ActiveCamera.ProjectionMatrix;
		_shader.Set("projection", proj);

		GL.BindVertexArray(_vertexArrayObject);
		GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
		
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

	// cleanup, this might not be necessary, but it is neat
	protected override void OnUnload() {
		// Unbind all the resources by binding the targets to 0/null.
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		GL.BindVertexArray(0);
		GL.UseProgram(0);

		// Delete all the resources.
		GL.DeleteBuffer(_vertexBufferObject);
		GL.DeleteVertexArray(_vertexArrayObject);

		GL.DeleteProgram(_shader.Handle);

		base.OnUnload();
	}
}
