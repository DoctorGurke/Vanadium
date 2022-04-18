using OpenTK.Graphics.OpenGL4;

namespace Vanadium.Renderer.Scene;

public class Skybox
{
	public static Skybox? ActiveSkybox { get; private set; }

	public static void Load( string path )
	{
		var skybox = new Skybox( path );
		ActiveSkybox = skybox;
	}

	private Skybox( string path )
	{
		Model = Model.Primitives.InvertedCube;
		Material = Material.Load( path );
	}

	private readonly Model Model;
	private readonly Material Material;

	public void Draw()
	{
		GL.DepthFunc( DepthFunction.Lequal );

		Model.Draw( DrawCommand.FromMaterialOverride( Material ) );

		GL.DepthFunc( DepthFunction.Less );
	}
}
