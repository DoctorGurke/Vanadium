using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace Vanadium;

public class Mesh {

	private List<Vertex > _vertices;
	private List<int> _indices;

	public struct Vertex {
		public Vector3 position;
		public Vector3 normal;
		public Vector2 uv;

		public Vertex(Vector3 position, Vector3 normal, Vector2 uv) {
			this.position = position;
			this.normal = normal;
			this.uv = uv;
		}
	}

	public Mesh(List<Vertex> vertices, List<int> indices) {
		Debug.WriteLine($"new mesh v:{vertices.Count} i:{indices.Count}");
		_vertices = vertices;
		_indices = indices;
		SetupMesh();
	}

	private int vao, vbo, ebo;

	private Shader _shader;

	private void SetupMesh() {
		// create, bind and populate vbo
		vbo = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
		GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Count * Marshal.SizeOf(typeof(Vertex)), _vertices.ToArray(), BufferUsageHint.StaticDraw);

		vao = GL.GenVertexArray();
		GL.BindVertexArray(vao);

		// use shader first to get attributes
		_shader = new Shader("shaders/generic.vert", "shaders/generic.frag");
		_shader.Use();

		// vertex positions
		var vertexPositionLocation = _shader.GetAttribLocation("vPosition");
		GL.EnableVertexAttribArray(vertexPositionLocation);
		GL.VertexAttribPointer(vertexPositionLocation, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf(typeof(Vertex)), 0);

		// vertex normal
		var vertexNormalLocation = _shader.GetAttribLocation("vNormal");
		GL.EnableVertexAttribArray(vertexNormalLocation);
		GL.VertexAttribPointer(vertexNormalLocation, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf(typeof(Vertex)), Marshal.OffsetOf(typeof(Vertex), "normal"));

		// uv0
		var uv0Location = _shader.GetAttribLocation("vTexCoord0");
		GL.EnableVertexAttribArray(uv0Location);
		GL.VertexAttribPointer(uv0Location, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf(typeof(Vertex)), Marshal.OffsetOf(typeof(Vertex), "uv"));

		// create, bind and populate ebo
		ebo = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
		GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Count * sizeof(uint), _indices.ToArray(), BufferUsageHint.StaticDraw);

		_timer = new Stopwatch();
		_timer.Start();

		_texture0 = Texture.LoadFromFile("resources/textures/rudy.jpg");
		_texture0.Use(TextureUnit.Texture0);

		_texture1 = Texture.LoadFromFile("resources/textures/mask_debug.jpg");
		_texture1.Use(TextureUnit.Texture1);

		_shader.Set("texture0", 0);
		_shader.Set("texture1", 1);
	}

	private Stopwatch _timer;

	private Texture _texture0;
	private Texture _texture1;

	public void Draw() {
		_texture0.Use(TextureUnit.Texture0);
		_texture1.Use(TextureUnit.Texture1);
		_shader.Use();

		double time = _timer.Elapsed.TotalSeconds;
		float tintAmount = ((float)Math.Sin(time) + 1) / 2;

		_shader.Set("tintAmount", tintAmount);

		var model = Matrix4.Identity;
		//model *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians((float)_timer.Elapsed.TotalSeconds * 10));
		//model *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians((float)_timer.Elapsed.TotalSeconds * 14));
		//model *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians((float)_timer.Elapsed.TotalSeconds * 3));
		//model *= Matrix4.CreateScale(0.1f);

		_shader.Set("model", model);
		var view = Camera.ActiveCamera.ViewMatrix;
		_shader.Set("view", view);
		var proj = Camera.ActiveCamera.ProjectionMatrix;
		_shader.Set("projection", proj);

		GL.BindVertexArray(vao);
		GL.DrawElements(PrimitiveType.Triangles, _indices.Count, DrawElementsType.UnsignedInt, 0);
	}
}
