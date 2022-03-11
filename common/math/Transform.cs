using OpenTK.Mathematics;

namespace Vanadium;

public struct Transform {
	public Vector3 Position;
	public Rotation Rotation;
	public float Scale;
	public Matrix4 TransformMatrix => GetTransformMatrix();

	private Matrix4 GetTransformMatrix() {
		var transform = Matrix4.Identity;
		transform *= Matrix4.CreateScale(Scale);
		transform *= Rotation.Matrix;
		transform *= Matrix4.CreateTranslation(Position);

		return transform;
	}
}
