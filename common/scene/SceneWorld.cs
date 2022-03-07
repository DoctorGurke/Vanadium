namespace Vanadium;

public class SceneWorld {

	public static IEnumerable<SceneObject> SceneObjects => OpaqueObjects.Concat(TransparentObjects);
	private static List<SceneObject> OpaqueObjects { get; set; } = new();
	private static List<SceneObject> TransparentObjects { get; set; } = new();

	public static void AddOpaque(SceneObject obj) {
		TransparentObjects.Remove(obj);
		OpaqueObjects.Add(obj);
	}

	public static void AddTransparent(SceneObject obj) {
		OpaqueObjects.Remove(obj);
		TransparentObjects.Add(obj);
	}


	public static void Draw() {
		foreach(var opaque in OpaqueObjects) {
			opaque.Draw();
		}

		var campos = Camera.ActiveCamera is null ? Vector3.Zero : Camera.ActiveCamera.Position;

		var SortedTransparents = TransparentObjects.OrderBy(x => -(x.Position - campos).Length).ToList();

		foreach(var transparent in SortedTransparents) {
			transparent.Draw();
		}
	}
}
