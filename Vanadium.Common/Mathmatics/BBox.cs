namespace Vanadium.Common.Mathematics;

public struct BBox : IEquatable<BBox>
{
	public Vector3 Mins;

	public Vector3 Maxs;

	/// <summary>
	/// Returns all Corners of the BBox
	/// </summary>
	public IEnumerable<Vector3> Corners
	{
		get
		{
			yield return new Vector3( Mins.x, Mins.y, Mins.z );
			yield return new Vector3( Maxs.x, Mins.y, Mins.z );
			yield return new Vector3( Maxs.x, Maxs.y, Mins.z );
			yield return new Vector3( Mins.x, Maxs.y, Mins.z );
			yield return new Vector3( Mins.x, Mins.y, Maxs.z );
			yield return new Vector3( Maxs.x, Mins.y, Maxs.z );
			yield return new Vector3( Maxs.x, Maxs.y, Maxs.z );
			yield return new Vector3( Mins.x, Maxs.y, Maxs.z );
		}
	}

	/// <summary>
	/// The Center point of the BBox
	/// </summary>
	public Vector3 Center => Mins + Size * 0.5f;

	public Vector3 Size => Maxs - Mins;

	/// <summary>
	/// Get a random point inside of this BBox
	/// </summary>
	public Vector3 RandomPointInside
	{
		get
		{
			Vector3 size = Size;
			size.x *= Rand.Float( 0f, 1f );
			size.y *= Rand.Float( 0f, 1f );
			size.z *= Rand.Float( 0f, 1f );
			return Mins + size;
		}
	}

	public float Volume => MathF.Abs( Mins.x - Maxs.x ) * MathF.Abs( Mins.y - Maxs.y ) * MathF.Abs( Mins.z - Maxs.z );

	public BBox( Vector3 mins, Vector3 maxs )
	{
		Mins = mins;
		Maxs = maxs;
	}
	public BBox( Vector3 center )
	{
		Mins = center;
		Maxs = center;
	}

	/// <summary>
	/// Returns true if this BBox completely contains bbox
	/// </summary>
	public readonly bool Contains( BBox b )
	{
		return b.Mins.x >= Mins.x && b.Maxs.x < Maxs.x && b.Mins.y >= Mins.y && b.Maxs.y < Maxs.y && b.Mins.z >= Mins.z && b.Maxs.z < Maxs.z;
	}

	/// <summary>
	/// Returns true if this BBox somewat overlaps bbox
	/// </summary>
	public readonly bool Overlaps( BBox b )
	{
		return Mins.x < b.Maxs.x && b.Mins.x < Maxs.x && Mins.y < b.Maxs.y && b.Mins.y < Maxs.y && Mins.z < b.Maxs.z && b.Mins.z < Maxs.z;
	}

	/// <summary>
	/// Returns this BBox but stretched to include this point
	/// </summary>
	public BBox AddPoint( Vector3 point )
	{
		BBox result = this;
		result.Mins.x = MathF.Min( result.Mins.x, point.x );
		result.Mins.y = MathF.Min( result.Mins.y, point.y );
		result.Mins.z = MathF.Min( result.Mins.z, point.z );
		result.Maxs.x = MathF.Max( result.Maxs.x, point.x );
		result.Maxs.y = MathF.Max( result.Maxs.y, point.y );
		result.Maxs.z = MathF.Max( result.Maxs.z, point.z );
		return result;
	}

	/// <summary>
	/// Returns the closest point on this bbox to another point
	/// </summary>
	public Vector3 ClosestPoint( Vector3 point )
	{
		return new Vector3( Math.Clamp( point.x, Mins.x, Maxs.x ), Math.Clamp( point.y, Mins.y, Maxs.y ), Math.Clamp( point.z, Mins.z, Maxs.z ) );
	}

	/// <summary>
	/// Creates a bbox of radius length and depth, and height height
	/// </summary>
	public static BBox FromHeightAndRadius( float height, float radius )
	{
		return new BBox( (Vector3.One * (0f - radius)).WithZ( 0f ), (Vector3.One * radius).WithZ( height ) );
	}
	public static BBox FromPositionAndSize( Vector3 center, float size )
	{
		BBox result = default;
		result.Mins = center - size * 0.5f;
		result.Maxs = center + size * 0.5f;
		return result;
	}
	public static BBox operator *( BBox c1, float c2 )
	{
		c1.Mins *= c2;
		c1.Maxs *= c2;
		return c1;
	}
	public static BBox operator +( BBox c1, Vector3 c2 )
	{
		c1.Mins += c2;
		c1.Maxs += c2;
		return c1;
	}

	/// <summary>
	/// Formats the BBox into a string "mins, maxs"
	/// </summary>
	public override string ToString()
	{
		return $"mins {Mins:0.###}, maxs {Maxs:0.###}";
	}
	public static bool operator ==( BBox left, BBox right )
	{
		return left.Equals( right );
	}
	public static bool operator !=( BBox left, BBox right )
	{
		return !(left == right);
	}
	public override bool Equals( object? obj )
	{
		if ( obj is BBox bbox)
		{
			return bbox.Equals( this );
		}
		return false;
	}
	public bool Equals( BBox o )
	{
		return Mins == o.Mins && Maxs == o.Maxs;
	}
	public override int GetHashCode()
	{
		return HashCode.Combine( Mins, Maxs );
	}
}
