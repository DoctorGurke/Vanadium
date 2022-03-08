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
		Model = Model.Primitives.InvertedCube;
		Model.SetMaterialOverride(path);
	}

	public void Draw() {
		GL.DepthFunc(DepthFunction.Lequal);

		Model.Draw();
	}
}
