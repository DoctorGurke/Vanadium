using OpenTK.Mathematics;

namespace Vanadium;

public static class Parse
{
	public static bool TryParse( string str, out Vector2 parsed )
	{
		str = str.Trim( '[', ']', ' ', '\n', '\r', '\t', '"' );
		string[] array = str.Split( new char[5]
		{
			' ',
			',',
			';',
			'\n',
			'\r'
		}, StringSplitOptions.RemoveEmptyEntries );
		if ( array.Length != 2 )
		{
			parsed = Vector2.Zero;
			return false;
		}

		parsed = new Vector2( array[0].ToFloat(), array[1].ToFloat() );
		return true;
	}

	public static bool TryParse( string str, out Vector4 parsed )
	{
		str = str.Trim( '[', ']', ' ', '\n', '\r', '\t', '"' );
		string[] array = str.Split( new char[5]
		{
			' ',
			',',
			';',
			'\n',
			'\r'
		}, StringSplitOptions.RemoveEmptyEntries );
		if ( array.Length != 4 )
		{
			parsed = Vector4.Zero;
			return false;
		}

		parsed = new Vector4( array[0].ToFloat(), array[1].ToFloat(), array[2].ToFloat(), array[3].ToFloat() );
		return true;
	}

	public static bool TryParse( string str, out Matrix4 parsed )
	{
		str = str.Trim( '[', ']', ' ', '\n', '\r', '\t', '"' );
		string[] array = str.Split( new char[5]
		{
			' ',
			',',
			';',
			'\n',
			'\r'
		}, StringSplitOptions.RemoveEmptyEntries );
		if ( array.Length != 16 )
		{
			parsed = Matrix4.Identity;
			return false;
		}

		parsed = new Matrix4( array[0].ToFloat(), array[1].ToFloat(), array[2].ToFloat(), array[3].ToFloat(),
							array[4].ToFloat(), array[5].ToFloat(), array[6].ToFloat(), array[7].ToFloat(),
							array[8].ToFloat(), array[9].ToFloat(), array[10].ToFloat(), array[11].ToFloat(),
							array[12].ToFloat(), array[13].ToFloat(), array[14].ToFloat(), array[15].ToFloat() );
		return true;
	}
}
