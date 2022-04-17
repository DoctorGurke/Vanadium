namespace Vanadium.Common;

public static class Log
{
	public static void Debug( object message )
	{
		System.Diagnostics.Debug.WriteLine( message );
	}
	public static void Info( object message )
	{
		Console.WriteLine( message );
	}
}
