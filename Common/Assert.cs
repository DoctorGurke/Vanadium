namespace Vanadium.Common;

public static class Assert
{
	public static void ResourcePresent( string path )
	{
		Debug.Assert( File.Exists( $"resources/{path}" ) );
	}

	public static void NotNull(object obj)
	{
		Debug.Assert( obj is not null );
	}

	public static void True(bool cond)
	{
		Debug.Assert( cond );
	}
}
