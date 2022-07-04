using System.Diagnostics;

namespace Vanadium.Common;

public static class Assert
{
	public static void ResourcePresent( string path )
	{
		Debug.Assert( File.Exists( $"core/{path}" ), "Resource Present Failed!", $"Resource: {path} missing or not found!" );
	}

	public static void NotNull( object obj )
	{
		Debug.Assert( obj is not null );
	}

	public static void True( bool cond )
	{
		Debug.Assert( cond );
	}
}
