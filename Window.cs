using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Vanadium;

public class Window : GameWindow {
	public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) { }

	private readonly float[] _vertices =
	{
		-0.5f, -0.5f, 0.0f, // Bottom-left vertex
            0.5f, -0.5f, 0.0f, // Bottom-right vertex
            0.0f,  0.5f, 0.0f  // Top vertex
    };

	private int _vertexBufferObject;

	private int _vertexArrayObject;

	private Shader _shader;

	protected override void OnLoad() {
		base.OnLoad();

		GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

		// create vbo
		_vertexBufferObject = GL.GenBuffer();
		// bind vbo
		GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
		// populate vbo
		GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

		_vertexArrayObject = GL.GenVertexArray();
		GL.BindVertexArray(_vertexArrayObject);

		GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
		// Enable variable 0 in the shader.
		GL.EnableVertexAttribArray(0);

		_shader = new Shader("shaders/generic.vert", "shaders/generic.frag");
		_shader.Use();
	}

	protected override void OnRenderFrame(FrameEventArgs args) {
		base.OnRenderFrame(args);

		GL.Clear(ClearBufferMask.ColorBufferBit);

		_shader.Use();

		GL.BindVertexArray(_vertexArrayObject);

		GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
		
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
