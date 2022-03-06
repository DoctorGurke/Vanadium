namespace Vanadium;

public struct TimeSince : IEquatable<TimeSince> {
	private float time;

	public float Absolute => time;

	public float Relative => this;

	public static implicit operator float(TimeSince ts) {
		return Time.Now - ts.time;
	}

	public static implicit operator TimeSince(float ts) {
		TimeSince result = default(TimeSince);
		result.time = Time.Now - ts;
		return result;
	}

	public override string ToString() {
		return Relative.ToString();
	}

	public static bool operator ==(TimeSince left, TimeSince right) {
		return left.Equals(right);
	}

	public static bool operator !=(TimeSince left, TimeSince right) {
		return !(left == right);
	}

	public override bool Equals(object obj) {
		if(obj is TimeSince) {
			TimeSince o = (TimeSince)obj;
			return Equals(o);
		}

		return false;
	}

	public bool Equals(TimeSince o) {
		return time == o.time;
	}

	public override int GetHashCode() {
		return time.GetHashCode();
	}
}
