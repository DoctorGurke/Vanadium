﻿using OpenTK.Windowing.Common;

namespace Vanadium;

public class SceneObject {
	private Model? _model;
	public Model? Model { 
		get {
			return _model;
		} 
		set {
			_model = value;
			_model.SceneObject = this;
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
	public Transform GlobalTransform => Parent is null ? LocalTransform : Parent.GlobalTransform + LocalTransform;

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
	}

	public void Render() {
		Model?.Draw();
		OnRender();
	}

	protected virtual void OnRender() {
		Rotation = Rotation.RotateAroundAxis(Vector3.Up, 10 * Time.Delta);
	}
}