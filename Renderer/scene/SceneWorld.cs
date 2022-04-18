namespace Vanadium.Renderer.Scene;

public class SceneWorld
{
	/// <summary>
	/// An Enumerable of all SceneObjects in the Scene.
	/// </summary>
	public static IEnumerable<SceneObject> SceneObjects => OpaqueObjects.Concat( TransparentObjects );
	private static List<SceneObject> OpaqueObjects { get; set; } = new();
	private static List<SceneObject> TransparentObjects { get; set; } = new();

	/// <summary>
	/// Add a SceneObject to the Scene as an Opaque Renderable.
	/// </summary>
	/// <param name="obj">The SceneObject to add.</param>
	public static void AddOpaque( SceneObject obj )
	{
		TransparentObjects.Remove( obj );
		OpaqueObjects.Add( obj );
	}

	/// <summary>
	/// Add a SceneObject to the Scene as a Transparent Renderable.
	/// </summary>
	/// <param name="obj">The SceneObject to add.</param>
	public static void AddTransparent( SceneObject obj )
	{
		OpaqueObjects.Remove( obj );
		TransparentObjects.Add( obj );
	}

	/// <summary>
	/// Remove a SceenObject from the Scene.
	/// </summary>
	/// <param name="obj">The SceneObject to remove.</param>
	public static void Remove(SceneObject obj)
	{
		OpaqueObjects.Remove( obj );
		TransparentObjects.Remove( obj );
	}

	/// <summary>
	/// Draw all Opaque SceneObjects in the Scene.
	/// </summary>
	public static void DrawOpaques()
	{
		foreach ( var opaque in OpaqueObjects )
		{
			opaque.Draw();
		}
	}

	/// <summary>
	/// Draw all Transparent SceneObjects in the Scene, back to front.
	/// </summary>
	public static void DrawTransparents()
	{
		var campos = Camera.ActiveCamera?.Position ?? Vector3.Zero;

		var SortedTransparents = TransparentObjects.OrderBy( x => -(x.Position - campos).Length ).ToList();
		foreach ( var transparent in SortedTransparents )
		{
			transparent.Draw();
		}
	}
}
