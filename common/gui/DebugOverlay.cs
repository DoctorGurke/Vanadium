using ImGuiNET;

namespace Vanadium;

public class DebugOverlay
{
	public static int FPS;
	public static float FT;
	public static float Gamma = 2.2f;

	public static void Draw( Window window )
	{

		ImGuiWindowFlags flags = 0;
		flags |= ImGuiWindowFlags.NoMove;
		flags |= ImGuiWindowFlags.NoResize;
		flags |= ImGuiWindowFlags.NoBackground;

		ImGui.SetNextWindowSizeConstraints( new System.Numerics.Vector2( 200, 200 ), new System.Numerics.Vector2( float.MaxValue, float.MaxValue ) );

		ImGui.StyleColorsDark();
		ImGui.Begin( "Debug Overlay", flags );
		ImGui.SetWindowPos( new System.Numerics.Vector2( 10, 10 ) );
		ImGui.SetWindowFontScale( 1f );
		ImGui.Text( $"FPS: {FPS}" );
		ImGui.Text( $"FT: {FT:0.####}s" );
		ImGui.Text( $"UI: {window.UiMode}" );
		ImGui.SliderFloat( "Gamma", ref Gamma, 1, 3.5f );
		ImGui.End();
	}
}
