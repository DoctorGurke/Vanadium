using OpenTK.Mathematics;

namespace Vanadium.Common.Mathematics;

public struct Color
{
	public float r;
	public float g;
	public float b;
	public float a;

	public static Color White => new( 1f, 1f, 1f );
	public static Color Gray => new( 0.5f, 0.5f, 0.5f );
	public static Color Black => new( 0f, 0f, 0f );
	public static Color Red => new( 1f, 0f, 0f );
	public static Color Green => new( 0f, 1f, 0f );
	public static Color Blue => new( 0f, 0f, 1f );
	public static Color Yellow => new( 1f, 1f, 0f );
	public static Color Orange => new( 1f, 0.6f, 0f );
	public static Color Cyan => new( 0f, 1f, 1f );
	public static Color Magenta => new( 1f, 0f, 1f );
	public static Color Transparent => new( 0f, 0f, 0f, 0f );
	public static Color Random => new( Rand.Float( 0, 1 ), Rand.Float( 0, 1 ), Rand.Float( 0, 1 ) );

	public static Color FromBytes( byte r, byte g, byte b, byte a = 255 )
	{
		return new Color( r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f );
	}

	public Color WithRed( float r )
	{
		return new Color( r, g, b, a );
	}

	public Color WithGreen( float g )
	{
		return new Color( r, g, b, a );
	}

	public Color WithBlue( float b )
	{
		return new Color( r, g, b, a );
	}

	public Color WithAlpha( float a )
	{
		return new Color( r, g, b, a );
	}

	public Color( float r, float g, float b )
	{
		this.r = r;
		this.g = g;
		this.b = b;
		a = 1.0f;
	}

	public Color( float r, float g, float b, float a )
	{
		this.r = r;
		this.g = g;
		this.b = b;
		this.a = a;
	}

	public static implicit operator System.Numerics.Vector4( Color value )
	{
		return new System.Numerics.Vector4( value.r, value.g, value.b, value.a );
	}
	public static implicit operator Color( System.Numerics.Vector4 value )
	{
		return new Color( value.X, value.Y, value.Z, value.W );
	}

	public static implicit operator Vector4( Color value )
	{
		return new Vector4( value.r, value.g, value.b, value.a );
	}
	public static implicit operator Color( Vector4 value )
	{
		return new Color( value.X, value.Y, value.Z, value.W );
	}

	public override string ToString()
	{
		return $"r:{r} g:{g} b:{b} a:{a}";
	}
}
