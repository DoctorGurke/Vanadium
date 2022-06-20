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

	public bool Transparent { get; set; }

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
		foreach ( var opaque in SceneObjects.Where( x => !x.Transparent ) )
		{
			opaque.Draw();
		}
	}

	/// <summary>
	/// Draw all Transparent SceneObjects in the Scene, back to front.
	/// </summary>
	public void DrawTransparents()
	{
		var campos = Camera.ActiveCamera?.Position ?? Vector3.Zero;

		var SortedTransparents = SceneObjects.Where( x => x.Transparent ).OrderBy( x => -(x.Position - campos).Length ).ToList();

		GL.DepthMask( false );
		foreach ( var transparent in SortedTransparents )
		{
			transparent.Draw();
		}
		GL.DepthMask( true );
	}
}
