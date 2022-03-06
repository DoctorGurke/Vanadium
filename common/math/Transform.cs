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
		model *= Rotation.Matrix;
		model *= Matrix4.CreateTranslation(Position);

		return model;
	}
}
