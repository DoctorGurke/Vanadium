using OpenTK.Graphics.OpenGL4;

namespace Vanadium.Renderer.Scene;

public class SceneWorld
{
	/// <summary>
	/// An Enumerable of all SceneObjects in the Scene.
	/// </summary>
	public readonly List<SceneObject> SceneObjects = new();

	public static SceneWorld? Main { get; private set; }
	public static readonly List<SceneWorld> All = new();

	public string Name { get; private set; }

	public SceneWorld() : this( $"SceneWorld{All.Count + 1}" ) { }

	public SceneWorld( string name )
	{
		if ( Main is null ) Main = this;
		Name = name;
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
	public void DrawOpaques()
	{
		foreach ( var opaque in SceneObjects.Where( x => x.Flags.IsOpaque ) )
		{
			opaque.Draw();
		}
	}

	/// <summary>
	/// Draw all Translucent SceneObjects in the Scene, back to front.
	/// </summary>
	public void DrawTranslucents()
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
}
