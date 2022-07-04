using OpenTK.Graphics.OpenGL4;

namespace Vanadium.Renderer.Scene;

public class SceneWorld
{
	/// <summary>
	/// An Enumerable of all SceneObjects in the Scene.
	/// </summary>
	public readonly HashSet<SceneObject> SceneObjects = new();

	public static readonly List<SceneWorld> All = new();
	public static SceneWorld Main { get; private set; } = new();

	public SceneWorld()
	{
		All.Add( this );
	}

	public void AddSceneObject( SceneObject obj )
	{
		SceneObjects.Add( obj );
	}

	public void RemoveSceneObject( SceneObject obj )
	{
		SceneObjects.Remove( obj );
	}

	/// <summary>
	/// Draw all Opaque SceneObjects in the Scene.
	/// </summary>
	public void DrawOpaqueLayer()
	{
		foreach ( var opaque in SceneObjects.Where( x => x.Flags.IsOpaque ) )
		{
			opaque.Draw();
		}
	}

	/// <summary>
	/// Draw all Translucent SceneObjects in the Scene, back to front.
	/// </summary>
	public void DrawTranslucentLayer()
	{
		var campos = Camera.ActiveCamera?.Position ?? Vector3.Zero;

		var SortedTranslucents = SceneObjects.Where( x => x.Flags.IsTranslucent ).OrderBy( x => -(x.Position - campos).Length ).ToList();

		GL.DepthMask( false );
		foreach ( var translucent in SortedTranslucents )
		{
			translucent.Draw();
		}
		GL.DepthMask( true );
	}

	/// <summary>
	/// Draw all Skybox SceneObjects in the Scene.
	/// </summary>
	public void DrawSkyboxLayer()
	{
		foreach ( var skybox in SceneObjects.Where( x => x.Flags.IsSkybox ) )
		{
			skybox.Draw();
		}
	}
}
