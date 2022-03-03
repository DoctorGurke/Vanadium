using OpenTK.Mathematics;

namespace Vanadium;

public struct Rotation : IEquatable<Rotation> {
	internal Quaternion _quat;

	public static readonly Rotation Identity = new Rotation {
		_quat = Quaternion.Identity
	};

	public float x {
		get {
			return _quat.X;
		}
		set {
			_quat.X = value;
		}
	}
	public float y {
		get {
			return _quat.Y;
		}
		set {
			_quat.Y = value;
		}
	}
	public float z {
		get {
			return _quat.Z;
		}
		set {
			_quat.Z = value;
		}
	}
	public float w {
		get {
			return _quat.W;
		}
		set {
			_quat.W = value;
		}
	}

	public Vector3 Forward => Vector3.Forward * this;
	public Vector3 Backward => Vector3.Backward * this;
	public Vector3 Right => Vector3.Right * this;
	public Vector3 Left => Vector3.Left * this;
	public Vector3 Up => Vector3.Up * this;
	public Vector3 Down => Vector3.Down * this;

	public static Rotation FromAxis(Vector3 axis, float degrees) {
		return Quaternion.FromAxisAngle(axis, degrees.DegreeToRadian());
	}
	public Rotation RotateAroundAxis(Vector3 axis, float degrees) {
		return this * FromAxis(axis, degrees);
	}

	public static Vector3 operator *(Rotation f, Vector3 c1) {
		return OpenTK.Mathematics.Vector3.Transform(c1._vec, f._quat);
	}
	public static Rotation operator *(Rotation a, Rotation b) {
		return Quaternion.Multiply(a._quat, b._quat);
	}
	public static Rotation operator *(Rotation a, float f) {
		return Quaternion.Slerp(Quaternion.Identity, a._quat, f);
	}
	public static Rotation operator /(Rotation a, float f) {
		return Quaternion.Slerp(Quaternion.Identity, a._quat, 1f / f);
	}
	public static Rotation operator +(Rotation a, Rotation b) {
		return Quaternion.Add(a._quat, b._quat);
	}
	public static Rotation operator -(Rotation a, Rotation b) {
		return Quaternion.Sub(a._quat, b._quat);
	}
	public static implicit operator Rotation(Quaternion value) {
		Rotation result = default(Rotation);
		result._quat = value;
		return result;
	}

	public static bool operator ==(Rotation left, Rotation right) {
		return left.Equals(right);
	}
	public static bool operator !=(Rotation left, Rotation right) {
		return !(left == right);
	}

	public override bool Equals(object? obj) {
		if(obj is Rotation rot) {
			return Equals(rot);
		}
		return false;
	}
	public bool Equals(Rotation o) {
		return _quat.X == o._quat.X && _quat.Y == o._quat.Y && _quat.Z == o._quat.Z && _quat.W == o._quat.W;
	}
	public override int GetHashCode() {
		return HashCode.Combine(_quat);
	}

	public override string ToString() {
		return $"{x:0.#####},{y:0.#####},{z:0.#####},{w:0.#####}";
	}
}
