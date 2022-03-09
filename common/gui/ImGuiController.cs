using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Runtime.CompilerServices;

namespace Vanadium;

public class ImGuiController : IDisposable {

	private bool _frameBegun;

	private int _vertexArray;
	private int _vertexBuffer;
	private int _vertexBufferSize;
	private int _indexBuffer;
	private int _indexBufferSize;

	private FontTexture _fontTexture;
	private Material _material;

	private int _windowWidth;
	private int _windowHeight;

	private System.Numerics.Vector2 _scaleFactor = System.Numerics.Vector2.One;

	public ImGuiController(Vector2i size) {
		_windowWidth = size.X;
		_windowHeight = size.Y;

		IntPtr context = ImGui.CreateContext();
		ImGui.SetCurrentContext(context);
		var io = ImGui.GetIO();
		io.Fonts.AddFontDefault();

		io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

		CreateDeviceResources();
		SetKeyMappings();

		SetPerFrameImGuiData(1f / 60f);

		ImGui.NewFrame();
		_frameBegun = true;
	}

	public void WindowResized(Vector2i size) {
		_windowWidth = size.X;
		_windowHeight = size.Y;
	}

	public void DestroyDeviceObjects() {
		Dispose();
	}

	public void CreateDeviceResources() {
		GLUtil.CreateVertexArray("ImGui", out _vertexArray);

		_vertexBufferSize = 10000;
		_indexBufferSize = 2000;

		GLUtil.CreateVertexBuffer("ImGui", out _vertexBuffer);
		GLUtil.CreateElementBuffer("ImGui", out _indexBuffer);
		GL.NamedBufferData(_vertexBuffer, _vertexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
		GL.NamedBufferData(_indexBuffer, _indexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

		RecreateFontDeviceTexture();

		_material = Material.Load("materials/ui.vanmat");

		GL.VertexArrayVertexBuffer(_vertexArray, 0, _vertexBuffer, IntPtr.Zero, Unsafe.SizeOf<ImDrawVert>());
		GL.VertexArrayElementBuffer(_vertexArray, _indexBuffer);

		GL.EnableVertexArrayAttrib(_vertexArray, 0);
		GL.VertexArrayAttribBinding(_vertexArray, 0, 0);
		GL.VertexArrayAttribFormat(_vertexArray, 0, 2, VertexAttribType.Float, false, 0);

		GL.EnableVertexArrayAttrib(_vertexArray, 1);
		GL.VertexArrayAttribBinding(_vertexArray, 1, 0);
		GL.VertexArrayAttribFormat(_vertexArray, 1, 2, VertexAttribType.Float, false, 8);

		GL.EnableVertexArrayAttrib(_vertexArray, 2);
		GL.VertexArrayAttribBinding(_vertexArray, 2, 0);
		GL.VertexArrayAttribFormat(_vertexArray, 2, 4, VertexAttribType.UnsignedByte, true, 16);

		GLUtil.CheckGLError("End of ImGui setup");
	}

	public void RecreateFontDeviceTexture() {
		ImGuiIOPtr io = ImGui.GetIO();
		io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int bytesPerPixel);

		_fontTexture = new FontTexture("ImGui Text Atlas", width, height, pixels);

		io.Fonts.SetTexID((IntPtr)_fontTexture.GLTexture);

		io.Fonts.ClearTexData();
	}

	public void Draw() {
		if(_frameBegun) {
			_frameBegun = false;
			ImGui.Render();
			RenderImDrawData(ImGui.GetDrawData());
		}
	}

	public void Update(GameWindow window, float delta) {
		if(_frameBegun) {
			ImGui.Render();
		}

		SetPerFrameImGuiData(delta);
		UpdateImGuiInput(window);

		_frameBegun = true;
		ImGui.NewFrame();
	}

	private void SetPerFrameImGuiData(float deltaSeconds) {
		ImGuiIOPtr io = ImGui.GetIO();
		io.DisplaySize = new System.Numerics.Vector2(
			_windowWidth / _scaleFactor.X,
			_windowHeight / _scaleFactor.Y);
		io.DisplayFramebufferScale = _scaleFactor;
		io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
	}

	readonly List<char> PressedChars = new List<char>();

	private void UpdateImGuiInput(GameWindow wnd) {
		ImGuiIOPtr io = ImGui.GetIO();

		MouseState MouseState = wnd.MouseState;
		KeyboardState KeyboardState = wnd.KeyboardState;

		io.MouseDown[0] = MouseState[MouseButton.Left];
		io.MouseDown[1] = MouseState[MouseButton.Right];
		io.MouseDown[2] = MouseState[MouseButton.Middle];

		var screenPoint = new Vector2i((int)MouseState.X, (int)MouseState.Y);
		var point = screenPoint;//wnd.PointToClient(screenPoint);

		// only update cursor when in ui mode
		if(wnd is Window window) {
			if(window.UiMode) {
				io.MousePos = new System.Numerics.Vector2(point.X, point.Y);
				io.MouseDrawCursor = true;
				io.WantCaptureMouse = true;
			} else {
				io.MouseDrawCursor = false;
				io.MousePos = new System.Numerics.Vector2(-1, -1);
				io.WantCaptureMouse = false;
			}
		}

		foreach(Keys key in Enum.GetValues(typeof(Keys))) {
			if(key == Keys.Unknown) {
				continue;
			}
			io.KeysDown[(int)key] = KeyboardState.IsKeyDown(key);
		}

		foreach(var c in PressedChars) {
			io.AddInputCharacter(c);
		}
		PressedChars.Clear();

		io.KeyCtrl = KeyboardState.IsKeyDown(Keys.LeftControl) || KeyboardState.IsKeyDown(Keys.RightControl);
		io.KeyAlt = KeyboardState.IsKeyDown(Keys.LeftAlt) || KeyboardState.IsKeyDown(Keys.RightAlt);
		io.KeyShift = KeyboardState.IsKeyDown(Keys.LeftShift) || KeyboardState.IsKeyDown(Keys.RightShift);
		io.KeySuper = KeyboardState.IsKeyDown(Keys.LeftSuper) || KeyboardState.IsKeyDown(Keys.RightSuper);
	}

	internal void PressChar(char keyChar) {
		PressedChars.Add(keyChar);
	}

	internal void MouseScroll(Vector2 offset) {
		ImGuiIOPtr io = ImGui.GetIO();

		io.MouseWheel = offset.Y;
		io.MouseWheelH = offset.X;
	}

	private static void SetKeyMappings() {
		ImGuiIOPtr io = ImGui.GetIO();
		io.KeyMap[(int)ImGuiKey.Tab] = (int)Keys.Tab;
		io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Keys.Left;
		io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Keys.Right;
		io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Keys.Up;
		io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Keys.Down;
		io.KeyMap[(int)ImGuiKey.PageUp] = (int)Keys.PageUp;
		io.KeyMap[(int)ImGuiKey.PageDown] = (int)Keys.PageDown;
		io.KeyMap[(int)ImGuiKey.Home] = (int)Keys.Home;
		io.KeyMap[(int)ImGuiKey.End] = (int)Keys.End;
		io.KeyMap[(int)ImGuiKey.Delete] = (int)Keys.Delete;
		io.KeyMap[(int)ImGuiKey.LeftAlt] = (int)Keys.LeftAlt;
		io.KeyMap[(int)ImGuiKey.LeftCtrl] = (int)Keys.LeftControl;
		io.KeyMap[(int)ImGuiKey.Backspace] = (int)Keys.Backspace;
		io.KeyMap[(int)ImGuiKey.Enter] = (int)Keys.Enter;
		io.KeyMap[(int)ImGuiKey.Escape] = (int)Keys.Escape;
		io.KeyMap[(int)ImGuiKey.W] = (int)Keys.W;
		io.KeyMap[(int)ImGuiKey.A] = (int)Keys.A;
		io.KeyMap[(int)ImGuiKey.S] = (int)Keys.S;
		io.KeyMap[(int)ImGuiKey.D] = (int)Keys.D;
		io.KeyMap[(int)ImGuiKey.Q] = (int)Keys.Q;
		io.KeyMap[(int)ImGuiKey.E] = (int)Keys.E;
		io.KeyMap[(int)ImGuiKey.C] = (int)Keys.C;
		io.KeyMap[(int)ImGuiKey.V] = (int)Keys.V;
		io.KeyMap[(int)ImGuiKey.X] = (int)Keys.X;
		io.KeyMap[(int)ImGuiKey.Y] = (int)Keys.Y;
		io.KeyMap[(int)ImGuiKey.Z] = (int)Keys.Z;
		io.KeyMap[(int)ImGuiKey.R] = (int)Keys.R;
		io.KeyMap[(int)ImGuiKey.T] = (int)Keys.T;
	}

	private void RenderImDrawData(ImDrawDataPtr draw_data) {
		if(draw_data.CmdListsCount == 0) {
			return;
		}

		

		for(int i = 0; i < draw_data.CmdListsCount; i++) {
			ImDrawListPtr cmd_list = draw_data.CmdListsRange[i];

			int vertexSize = cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
			if(vertexSize > _vertexBufferSize) {
				int newSize = (int)Math.Max(_vertexBufferSize * 1.5f, vertexSize);
				GL.NamedBufferData(_vertexBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
				_vertexBufferSize = newSize;

				Console.WriteLine($"Resized dear imgui vertex buffer to new size {_vertexBufferSize}");
			}

			int indexSize = cmd_list.IdxBuffer.Size * sizeof(ushort);
			if(indexSize > _indexBufferSize) {
				int newSize = (int)Math.Max(_indexBufferSize * 1.5f, indexSize);
				GL.NamedBufferData(_indexBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
				_indexBufferSize = newSize;

				Console.WriteLine($"Resized dear imgui index buffer to new size {_indexBufferSize}");
			}
		}

		// Setup orthographic projection matrix into our constant buffer
		ImGuiIOPtr io = ImGui.GetIO();
		Matrix4 mvp = Matrix4.CreateOrthographicOffCenter( 0.0f, io.DisplaySize.X, io.DisplaySize.Y, 0.0f, -1.0f, 1.0f);

		_material.Use();
		GL.UniformMatrix4(_material.Shader.GetUniformLocation("projection_matrix"), false, ref mvp);
		GL.Uniform1(_material.Shader.GetUniformLocation("in_fontTexture"), 0);
		GLUtil.CheckGLError("Projection");

		GL.BindVertexArray(_vertexArray);
		GLUtil.CheckGLError("VAO");

		draw_data.ScaleClipRects(io.DisplayFramebufferScale);

		GL.Enable(EnableCap.Blend);
		GL.Enable(EnableCap.ScissorTest);
		GL.BlendEquation(BlendEquationMode.FuncAdd);
		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		GL.Disable(EnableCap.CullFace);
		GL.Disable(EnableCap.DepthTest);

		// Render command lists
		for(int n = 0; n < draw_data.CmdListsCount; n++) {
			ImDrawListPtr cmd_list = draw_data.CmdListsRange[n];

			GL.NamedBufferSubData(_vertexBuffer, IntPtr.Zero, cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(), cmd_list.VtxBuffer.Data);
			GLUtil.CheckGLError($"Data Vert {n}");

			GL.NamedBufferSubData(_indexBuffer, IntPtr.Zero, cmd_list.IdxBuffer.Size * sizeof(ushort), cmd_list.IdxBuffer.Data);
			GLUtil.CheckGLError($"Data Idx {n}");

			for(int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++) {
				ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];
				if(pcmd.UserCallback != IntPtr.Zero) {
					throw new NotImplementedException();
				} else {
					GL.ActiveTexture(TextureUnit.Texture0);
					GL.BindTexture(TextureTarget.Texture2D, (int)pcmd.TextureId);
					GLUtil.CheckGLError("Texture");

					// We do _windowHeight - (int)clip.W instead of (int)clip.Y because gl has flipped Y when it comes to these coordinates
					var clip = pcmd.ClipRect;
					GL.Scissor((int)clip.X, _windowHeight - (int)clip.W, (int)(clip.Z - clip.X), (int)(clip.W - clip.Y));
					GLUtil.CheckGLError("Scissor");

					if((io.BackendFlags & ImGuiBackendFlags.RendererHasVtxOffset) != 0) {
						GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (IntPtr)(pcmd.IdxOffset * sizeof(ushort)), (int) pcmd.VtxOffset);
					} else {
						GL.DrawElements(BeginMode.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (int)pcmd.IdxOffset * sizeof(ushort));
					}
					GLUtil.CheckGLError("Draw");
				}
			}
		}

		GL.Disable(EnableCap.Blend);
		GL.Disable(EnableCap.ScissorTest);
	}

	public void OnTextInput(TextInputEventArgs e) {
		PressChar((char)e.Unicode);
	}

	public void OnMouseWheel(MouseWheelEventArgs e) {
		MouseScroll(e.Offset);
	}

	public void Dispose() {
		_fontTexture.Dispose();
	}
}
