namespace Vanadium.Common;

public static class Log
{
	public static void Info( object message )
	{
		Console.ForegroundColor = ConsoleColor.White;
		Print( message );
	}

	public static void Warning( object message )
	{
		Console.ForegroundColor = ConsoleColor.DarkYellow;
		Print( message );
	}

	public static void Highlight( object message )
	{
		Console.ForegroundColor = ConsoleColor.Green;
		Print( message );
	}

	private static void Print( object message )
	{
		Console.WriteLine( $"[{DateTime.Now.ToString( "hh:mm:ss" )}] {message}" );
	}
}
