namespace Vanadium;

/// <summary>
/// Utility class for math float extensions.
/// </summary>
public static class MathX {
	internal const float toDegrees = 57.2958f;

	internal const float toRadians = 0.0174533f;

	public static float DegreeToRadian(this float f) {
		return f * toRadians;
	}

	public static float RadianToDegree(this float f) {
		return f * toDegrees;
	}

	private static void Order(ref float a, ref float b) {
		if(!(a <= b)) {
			float num = a;
			a = b;
			b = num;
		}
	}

	public static float Clamp(this float v, float min, float max) {
		Order(ref min, ref max);
		return (v < min) ? min : ((v < max) ? v : max);
	}
}
