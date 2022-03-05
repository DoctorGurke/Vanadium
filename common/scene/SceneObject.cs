using OpenTK.Mathematics;

namespace Vanadium;

public class SceneObject {
	private Model _model;
	public Model Model { 
		get {
			return _model;
		} 
		set {
			value.SceneObject = this;
			_model = value;
		}
	}

	public Vector3 Position { 
		get {
			return LocalTransform.Position;
		}
		set {
			LocalTransform.Position = value;
		}
	}
	public Rotation Rotation { 
		get {
			return LocalTransform.Rotation;
		}
		set {
			LocalTransform.Rotation = value;
		}
	}
	public float Scale {
		get {
			return LocalTransform.Scale;
		}
		set {
			LocalTransform.Scale = value;
		}
	}
	public Transform LocalTransform;
	//public Transform GlobalTransform => Parent is null ? LocalTransform : Parent.GlobalTransform + LocalTransform;
	public Matrix4 GlobalTransform => Parent is null ? LocalTransform.ModelMatrix : Parent.GlobalTransform * LocalTransform.ModelMatrix;

	private SceneObject _parent;
	public SceneObject Parent {
		get {
			return _parent;
		}
		set {
			if(value == this) return;
			_parent = value;
			value.Children.Add(this);
		}
	}
	public List<SceneObject> Children = new();

	public SceneObject() {
		Position = Vector3.Zero;
		Rotation = Rotation.Identity;
		Scale = 1.0f;

		SceneWorld.SceneObjects.Add(this);
		OnSpawn();
	}

	public virtual void OnSpawn() { }

	public void Draw() {
		Model?.Draw();
		OnRender();
	}

	protected virtual void OnRender() {
		Rotation = Rotation.RotateAroundAxis(Vector3.Up, Time.Delta * 35);
	}
}
