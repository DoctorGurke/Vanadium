using OpenTK.Mathematics;

namespace Vanadium;

public struct Transform {
	public Vector3 Position;
	public Rotation Rotation;
	public float Scale;
	public Matrix4 TransformMatrix => GetTransformMatrix();

	private Matrix4 GetTransformMatrix() {
		var model = Matrix4.Identity;
		model *= Matrix4.CreateScale(Scale);
		model *= Rotation.Matrix;
		model *= Matrix4.CreateTranslation(Position);

		return model;
	}
}
