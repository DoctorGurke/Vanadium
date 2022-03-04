namespace Vanadium;

/// <summary>
/// Utility class for math float extensions.
/// </summary>
public static class MathX {
	internal const float toDegrees = 57.2958f;

	internal const float toRadians = 0.0174533f;

	/// <summary>
	/// Utility function to convert Degrees to Radians.
	/// </summary>
	/// <param name="f">Float Extension.</param>
	/// <returns>The Radian value.</returns>
	public static float DegreeToRadian(this float f) {
		return f * toRadians;
	}

	/// <summary>
	/// Utility function to convert Radians to Degrees.
	/// </summary>
	/// <param name="f">Float Extension.</param>
	/// <returns>The Degree value.</returns>
	public static float RadianToDegree(this float f) {
		return f * toDegrees;
	}

	/// <summary>
	/// Sort two floats by their size, min = a, max = b.
	/// </summary>
	/// <param name="a">ref float a</param>
	/// <param name="b">ref float b</param>
	private static void Order(ref float a, ref float b) {
		if(!(a <= b)) {
			float num = a;
			a = b;
			b = num;
		}
	}

	/// <summary>
	/// Clamp a float to a min and max
	/// </summary>
	/// <param name="v">Float Extension.</param>
	/// <param name="min">The minimum allowed value.</param>
	/// <param name="max">The maximum allowed value.</param>
	/// <returns>The clamped value.</returns>
	public static float Clamp(this float f, float min, float max) {
		Order(ref min, ref max);
		return (f < min) ? min : ((f < max) ? f : max);
	}
}
