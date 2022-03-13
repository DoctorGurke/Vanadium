using OpenTK.Mathematics;

namespace Vanadium;

public struct Transform
{
	public Vector3 Position;
	public Rotation Rotation;
	public float Scale;
	public Matrix4 TransformMatrix => GetTransformMatrix();
	public Matrix4 TranslationMatrix => Matrix4.CreateTranslation( Position );
	public Matrix4 RotationMatrix => Rotation.Matrix;
	public Matrix4 ScaleMatrix => Matrix4.CreateScale( Scale );

	public static implicit operator Matrix4( Transform transform )
	{
		return transform.TransformMatrix;
	}

	public Transform(Vector3 pos, Rotation rot, float scale)
	{
		Position = pos;
		Rotation = rot;
		Scale = scale;
	}

	private Matrix4 GetTransformMatrix()
	{
		var transform = Matrix4.Identity;
		transform *= ScaleMatrix;
		transform *= Rotation.Matrix;
		transform *= TranslationMatrix;

		return transform;
	}
}
