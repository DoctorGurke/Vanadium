namespace Vanadium;

public struct Vector3 : IEquatable<Vector3> {
	internal OpenTK.Mathematics.Vector3 _vec;

	public float x {
		readonly get {
			return _vec.X;
		}
		set {
			_vec.X = value;
		}
	}
	public float y {
		readonly get {
			return _vec.Y;
		}
		set {
			_vec.Y = value;
		}
	}
	public float z {
		readonly get {
			return _vec.Z;
		}
		set {
			_vec.Z = value;
		}
	}

	public static readonly Vector3 One = new Vector3(1f);

    public static readonly Vector3 Zero = new Vector3(0f);

    public static readonly Vector3 Forward = new Vector3(1f, 0f, 0f);

    public static readonly Vector3 Backward = new Vector3(-1f, 0f, 0f);

    public static readonly Vector3 Up = new Vector3(0f, 0f, 1f);

    public static readonly Vector3 Down = new Vector3(0f, 0f, -1f);

    public static readonly Vector3 Right = new Vector3(0f, -1f, 0f);

    public static readonly Vector3 Left = new Vector3(0f, 1f, 0f);

    public static readonly Vector3 OneX = new Vector3(1f, 0f, 0f);

    public static readonly Vector3 OneY = new Vector3(0f, 1f, 0f);

    public static readonly Vector3 OneZ = new Vector3(0f, 0f, 1f);

	public readonly float Length => MathF.Sqrt(LengthSquared);
	public readonly float LengthSquared => _vec.LengthSquared;

	public readonly Vector3 Normal => _vec.Normalized();

	public Vector3(float x, float y, float z) {
		_vec = new OpenTK.Mathematics.Vector3(x, y, z);
	}

	public Vector3(Vector3 other) : this(other.x, other.y, other.z) { }
	public Vector3(float all = 0.0f) : this(all, all, all) { }
	internal Vector3(OpenTK.Mathematics.Vector3 other) : this(other.X, other.Y, other.Z) { }

	public static implicit operator Vector3(OpenTK.Mathematics.Vector3 value) {
		Vector3 result = default(Vector3);
		result._vec = value;
		return result;
	}
	public static implicit operator OpenTK.Mathematics.Vector3(Vector3 value) {
		return new OpenTK.Mathematics.Vector3(value.x, value.y, value.z);
	}

	public static Vector3 operator +(Vector3 c1, Vector3 c2) {
		return new Vector3(c1.x + c2.x, c1.y + c2.y, c1.z + c2.z);
	}
	public static Vector3 operator -(Vector3 c1, Vector3 c2) {
		return new Vector3(c1.x - c2.x, c1.y - c2.y, c1.z - c2.z);
	}
	public static Vector3 operator *(Vector3 c1, float f) {
		return new Vector3(c1.x * f, c1.y * f, c1.z * f);
	}
	public static Vector3 operator *(Vector3 c1, Rotation f) {
		return OpenTK.Mathematics.Vector3.Transform(c1._vec, f._quat);
	}
	public static Vector3 operator *(Vector3 c1, Vector3 c2) {
		return new Vector3(c1.x * c2.x, c1.y * c2.y, c1.z * c2.z);
	}
	public static Vector3 operator *(float f, Vector3 c1) {
		return new Vector3(c1.x * f, c1.y * f, c1.z * f);
	}
	public static Vector3 operator /(Vector3 c1, float f) {
		return new Vector3(c1.x / f, c1.y / f, c1.z / f);
	}
	public static Vector3 operator -(Vector3 value) {
		return new Vector3(0f - value.x, 0f - value.y, 0f - value.z);
	}

	public static bool operator ==(Vector3 left, Vector3 right) {
		return left.Equals(right);
	}
	public static bool operator !=(Vector3 left, Vector3 right) {
		return !(left == right);
	}
	public override bool Equals(object? obj) {
		if(obj is Vector3 vec) {
			return vec.x == x && vec.y == y && vec.z == z;
		}
		return false;
	}
	public bool Equals(Vector3 other) {
		return Equals(other);
	}


	public static Vector3 Cross(Vector3 a, Vector3 b) {
		return new Vector3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
	}
	public readonly Vector3 Cross(Vector3 b) {
		return new Vector3(y * b.z - z * b.y, z * b.x - x * b.z, x * b.y - y * b.x);
	}
	public static float Dot(Vector3 a, Vector3 b) {
		return OpenTK.Mathematics.Vector3.Dot(a._vec, b._vec);
	}
	public readonly float Dot(Vector3 b) {
		return Dot(this, b);
	}
	public readonly float Distance(Vector3 target) {
		return DistanceBetween(this, target);
	}
	public static float DistanceBetween(Vector3 a, Vector3 b) {
		return (b - a).Length;
	}
	public static Vector3 Reflect(Vector3 direction, Vector3 normal) {
		return direction - 2f * Dot(direction, normal) * normal;
	}

	public override int GetHashCode() {
		return HashCode.Combine(_vec);
	}

	public override string ToString() {
		return $"{x:0.####},{y:0.####},{z:0.####}";
	}

}
