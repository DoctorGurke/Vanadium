using OpenTK.Graphics.OpenGL4;
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

	private Texture _texture;

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
		var vertexPositionLocation = _shader.GetAttribLocation("aPosition");
		GL.EnableVertexAttribArray(vertexPositionLocation);
		GL.VertexAttribPointer(vertexPositionLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);

		// uv0
		var uv0Location = _shader.GetAttribLocation("aTexCoord0");
		GL.EnableVertexAttribArray(uv0Location);
		GL.VertexAttribPointer(uv0Location, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));

		// vertex color
		var vertexColorLocation = _shader.GetAttribLocation("aColor");
		GL.EnableVertexAttribArray(vertexColorLocation);
		GL.VertexAttribPointer(vertexColorLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 5 * sizeof(float));

		// create, bind and populate ebo
		_elementBufferObject = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
		GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

		_timer = new Stopwatch();
		_timer.Start();

		_texture = Texture.LoadFromFile("resources/textures/rudy.jpg");
		_texture.Use(TextureUnit.Texture0);
	}

	protected override void OnRenderFrame(FrameEventArgs args) {
		base.OnRenderFrame(args);

		GL.Clear(ClearBufferMask.ColorBufferBit);

		_texture.Use(TextureUnit.Texture0);
		_shader.Use();

		double time = _timer.Elapsed.TotalSeconds;
		float tintAmount = ((float)Math.Sin(time) + 1 ) / 2;

		if (_shader.UniformLocations.TryGetValue("tintAmount", out int tintAmountLocation)) {
			GL.Uniform1(tintAmountLocation, tintAmount);
		}

		GL.BindVertexArray(_vertexArrayObject);

		GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
		
		SwapBuffers();
	}

	protected override void OnUpdateFrame(FrameEventArgs args) {
		base.OnUpdateFrame(args);

		// close the window when ESC is pressed down
		if(KeyboardState.IsKeyDown(Keys.Escape)) {
			Close();
		}
	}

	protected override void OnResize(ResizeEventArgs e) {
		base.OnResize(e);

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
