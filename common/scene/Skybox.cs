using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Vanadium;

public class Skybox {
	public static Skybox ActiveSkybox { get; private set; }

	public static void Load(string path) {
		var skybox = new Skybox();
		skybox.Setup(path);
		ActiveSkybox = skybox;
	}

	private Model Model;

	public void Setup(string path) {
		Model = Model.Load("models/cube_inv.fbx");
		Model.SetMaterialOverride(path);
	}

	public void Draw() {
		GL.DepthFunc(DepthFunction.Lequal);

		var cam = Camera.ActiveCamera;
		var view = cam.ViewMatrix;//cam.Rotation.Matrix;
		Model.Draw(Matrix4.Identity, view, cam.ProjectionMatrix);
	}
}
