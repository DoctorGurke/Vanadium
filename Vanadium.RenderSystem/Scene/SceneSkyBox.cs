using OpenTK.Graphics.OpenGL4;

namespace Vanadium.RenderSystem.Scene;

public class SceneSkyBox : SceneObject
{
	public SceneSkyBox( Material material ) : this( null, material ) { }
	public SceneSkyBox( SceneWorld? world, Material material ) : base( world )
	{
		Model = Model.Primitives.InvertedCube;
		Material = material;
	}

	private readonly Material Material;

	public override void Draw()
	{
		GL.DepthFunc( DepthFunction.Lequal );

		Model.Draw( DrawCommand.FromMaterialOverride( Material ) );

		GL.DepthFunc( DepthFunction.Less );
	}
}
