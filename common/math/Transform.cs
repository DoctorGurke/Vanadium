using OpenTK.Mathematics;

namespace Vanadium;

public struct Transform {
	public Vector3 Position;
	public Rotation Rotation;
	public float Scale;
	public Matrix4 ModelMatrix => GetModelMatrix();

	private Matrix4 GetModelMatrix() {
		var model = Matrix4.Identity;
		model *= Matrix4.CreateScale(Scale);
		model *= Matrix4.CreateTranslation(Position);
		model *= Rotation.Matrix;

		return model;
	}

	public static Transform operator +(Transform c1, Transform c2) {
		return new Transform { Position = c1.Position + c2.Position, Rotation = c1.Rotation * c2.Rotation, Scale = c1.Scale * c2.Scale };
	}

	public static Transform operator -(Transform c1, Transform c2) {
		return new Transform { Position = c1.Position - c2.Position, Rotation = c1.Rotation * c2.Rotation.Inverse, Scale = c1.Scale * c2.Scale };
	}
}
