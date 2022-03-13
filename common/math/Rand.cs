namespace Vanadium;

public static class Rand
{
	private static Random _random;

	private static Random CurrentRandom
	{
		get
		{
			if ( _random == null )
			{
				_random = new Random();
			}

			return _random;
		}
	}

	/// <summary>
	/// Sets the seed for these static classes
	/// </summary>
	public static void SetSeed( int seed )
	{
		_random = new Random( seed );
	}

	/// <summary>
	/// Returns a double between min and max
	/// </summary>
	public static double Double( double min, double max )
	{
		return CurrentRandom.Double( min, max );
	}

	/// <summary>
	/// Returns a random float between min and max
	/// </summary>
	public static float Float( float min, float max )
	{
		return CurrentRandom.Float( min, max );
	}

	/// <summary>
	/// Returns a random float between 0 and max (or 1)
	/// </summary>
	public static float Float( float max = 1f )
	{
		return Float( 0f, max );
	}

	/// <summary>
	/// Returns a random double between 0 and max (or 1)
	/// </summary>
	public static double Double( double max = 1.0 )
	{
		return Double( 0.0, max );
	}

	/// <summary>
	/// Returns a random int between min and max (inclusive)
	/// </summary>
	public static int Int( int min, int max )
	{
		return CurrentRandom.Next( min, max + 1 );
	}

	/// <summary>
	/// Returns a random int between 0 and max (inclusive)
	/// </summary>
	public static int Int( int max )
	{
		return Int( 0, max );
	}

	/// <summary>
	/// Returns a random value in an array
	/// </summary>
	public static T? FromArray<T>( T[] array, T? defVal = default )
	{
		return CurrentRandom.FromArray( array, defVal );
	}

	/// <summary>
	/// Returns a random value in a list
	/// </summary>
	public static T? FromList<T>( List<T> array, T? defVal = default )
	{
		return CurrentRandom.FromList( array, defVal );
	}
}

public static class RandomExtension
{
	/// <summary>
	/// Returns a double between min and max
	/// </summary>
	public static double Double( this Random self, double min, double max )
	{
		return min + (max - min) * self.NextDouble();
	}

	/// <summary>
	/// Returns a random float between min and max
	/// </summary>
	public static float Float( this Random self, float min, float max )
	{
		return min + (max - min) * (float)self.NextDouble();
	}

	/// <summary>
	/// Returns a random float between 0 and max (or 1)
	/// </summary>
	public static float Float( this Random self, float max = 1f )
	{
		return self.Float( 0f, max );
	}

	/// <summary>
	/// Returns a random double between 0 and max (or 1)
	/// </summary>
	public static double Double( Random self, double max = 1.0 )
	{
		return self.Double( 0.0, max );
	}

	/// <summary>
	/// Returns a random int between min and max (inclusive)
	/// </summary>
	public static int Int( this Random self, int min, int max )
	{
		return self.Next( min, max + 1 );
	}

	/// <summary>
	/// Returns a random int between 0 and max (inclusive)
	/// </summary>
	public static int Int( this Random self, int max )
	{
		return self.Int( 0, max );
	}

	/// <summary>
	/// Returns a random value in an array
	/// </summary>
	public static T? FromArray<T>( this Random self, T[] array, T? defVal = default )
	{
		if ( array == null || array.Length == 0 )
		{
			return defVal;
		}

		return array[self.Next( 0, array.Length )];
	}

	/// <summary>
	/// Returns a random value in a list
	/// </summary>
	public static T? FromList<T>( this Random self, List<T> array, T? defVal = default )
	{
		if ( array == null || array.Count == 0 )
		{
			return defVal;
		}

		return array[self.Next( 0, array.Count )];
	}
}
