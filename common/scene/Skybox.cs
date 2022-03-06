using OpenTK.Graphics.OpenGL4;

namespace Vanadium;

public class Skybox {
	public static Skybox ActiveSkybox { get; private set; }

	public static void Load(string path) {
		var skybox = new Skybox();
		skybox.Setup(path);
		ActiveSkybox = skybox;
	}

	private int vao, vbo;
	private Material _material;

	public void Setup(string path) {
		_material = Material.Load(path);
		_material.Use();

		vbo = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
		GL.BufferData(BufferTarget.ArrayBuffer, skyboxVertices.Length * sizeof(float), skyboxVertices, BufferUsageHint.StaticDraw);

		vao = GL.GenVertexArray();
		GL.BindVertexArray(vao);

		// vertex positions
		GL.EnableVertexAttribArray(0);
		GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
	}

	public void Draw() {
		GL.DepthFunc(DepthFunction.Greater);

		_material.Use();

		//var model = sceneobject.GlobalTransform;
		//_material.Set("model", model);
		var view = Camera.ActiveCamera.ViewMatrix;
		_material.Set("view", view);
		var proj = Camera.ActiveCamera.ProjectionMatrix;
		_material.Set("projection", proj);

		GL.BindVertexArray(vao);
		GL.DrawArrays(PrimitiveType.Triangles, 0, skyboxVertices.Length);
	}

	private float[] skyboxVertices = {
        // positions          
        -1.0f,  1.0f, -1.0f,
		-1.0f, -1.0f, -1.0f,
		 1.0f, -1.0f, -1.0f,
		 1.0f, -1.0f, -1.0f,
		 1.0f,  1.0f, -1.0f,
		-1.0f,  1.0f, -1.0f,

		-1.0f, -1.0f,  1.0f,
		-1.0f, -1.0f, -1.0f,
		-1.0f,  1.0f, -1.0f,
		-1.0f,  1.0f, -1.0f,
		-1.0f,  1.0f,  1.0f,
		-1.0f, -1.0f,  1.0f,

		 1.0f, -1.0f, -1.0f,
		 1.0f, -1.0f,  1.0f,
		 1.0f,  1.0f,  1.0f,
		 1.0f,  1.0f,  1.0f,
		 1.0f,  1.0f, -1.0f,
		 1.0f, -1.0f, -1.0f,

		-1.0f, -1.0f,  1.0f,
		-1.0f,  1.0f,  1.0f,
		 1.0f,  1.0f,  1.0f,
		 1.0f,  1.0f,  1.0f,
		 1.0f, -1.0f,  1.0f,
		-1.0f, -1.0f,  1.0f,

		-1.0f,  1.0f, -1.0f,
		 1.0f,  1.0f, -1.0f,
		 1.0f,  1.0f,  1.0f,
		 1.0f,  1.0f,  1.0f,
		-1.0f,  1.0f,  1.0f,
		-1.0f,  1.0f, -1.0f,

		-1.0f, -1.0f, -1.0f,
		-1.0f, -1.0f,  1.0f,
		 1.0f, -1.0f, -1.0f,
		 1.0f, -1.0f, -1.0f,
		-1.0f, -1.0f,  1.0f,
		 1.0f, -1.0f,  1.0f
	};
}
