namespace Vanadium;

public class TestObject : SceneObject {
	public override void OnSpawn() {
		Model = Model.Load("models/suzanne.fbx");

		var objects = 5;
		var rotation = Rotation.Identity;
		var anglestep = 360.0f / objects;
		for(int i = 0; i < objects; i++) {
			var ent = new SceneObject {
				Model = Model.Load("models/fancy.fbx"),
				Position = Vector3.Up + rotation.RotateAroundAxis(Vector3.Up, anglestep * i).Forward * 3
			};
			ent.Parent = this;
		}
	}

	protected override void OnRender() {
		Rotation = Rotation.RotateAroundAxis(Vector3.Up, Time.Delta * 35);
	}
}
