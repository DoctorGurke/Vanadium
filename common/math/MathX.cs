namespace Vanadium;

/// <summary>
/// Utility class for math float extensions.
/// </summary>
public static class MathX
{
	internal const float toDegrees = 57.2958f;

	internal const float toRadians = 0.0174533f;

	/// <summary>
	/// Utility function to convert Degrees to Radians.
	/// </summary>
	/// <param name="f">Float Extension.</param>
	/// <returns>The Radian value.</returns>
	public static float DegreeToRadian( this float f )
	{
		return f * toRadians;
	}

	/// <summary>
	/// Utility function to convert Radians to Degrees.
	/// </summary>
	/// <param name="f">Float Extension.</param>
	/// <returns>The Degree value.</returns>
	public static float RadianToDegree( this float f )
	{
		return f * toDegrees;
	}

	/// <summary>
	/// Sort two floats by their size, min = a, max = b.
	/// </summary>
	/// <param name="a">ref float a</param>
	/// <param name="b">ref float b</param>
	private static void Order( ref float a, ref float b )
	{
		if ( !(a <= b) )
		{
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
	public static float Clamp( this float f, float min, float max )
	{
		Order( ref min, ref max );
		return (f < min) ? min : ((f < max) ? f : max);
	}

	/// <summary>
	/// Lerp to a value on a range.
	/// </summary>
	/// <param name="from">From value</param>
	/// <param name="to">To value</param>
	/// <param name="delta">The amount between the values, 0-1</param>
	/// <param name="clamp">Whether to clamp the result</param>
	/// <returns>The lerped float</returns>
	public static float LerpTo( this float from, float to, float delta, bool clamp = true )
	{
		if ( clamp )
		{
			delta = delta.Clamp( 0f, 1f );
		}

		return from + delta * (to - from);
	}

	public static int RoundUpToMultipleOf( this int from, int multiple )
	{
		if ( multiple == 0 )
			return from;

		int remainder = Math.Abs( from ) % multiple;
		if ( remainder == 0 )
			return from;

		if ( from < 0 )
			return -(Math.Abs( from ) - remainder);
		else
			return from + multiple - remainder;
	}
}
