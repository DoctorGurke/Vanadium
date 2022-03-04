namespace Vanadium;

public class SceneWorld {

	public static List<SceneObject> SceneObjects = new();

	public static void Draw() {
		foreach(var obj in SceneObjects) {
			obj.Draw();
		}
	}
}
