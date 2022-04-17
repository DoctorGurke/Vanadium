using ImGuiNET;

namespace Vanadium;

public class DebugOverlay
{
	public static int FPS;
	public static float FT;
	public static float Gamma = 1.0f;
	public static float LightBrightnessMultiplier = 1.0f;
	private static System.Numerics.Vector4 AmbientColor = new Color( 36.0f / 255.0f, 60.0f / 255.0f, 102.0f / 255.0f );
	private static System.Numerics.Vector4 PrevAmbientColor = Color.White;


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

		if(ImGui.CollapsingHeader("Stats"))
		{
			ImGui.Text( $"FPS: {FPS}" );
			ImGui.Text( $"FT: {FT:0.####}s" );
			ImGui.Text( $"UI: {window.UiMode}" );
		}
		if(ImGui.CollapsingHeader("Settings"))
		{
			ImGui.SliderFloat( "Gamma", ref Gamma, 0.7f, 1.3f );
			ImGui.SliderFloat( "Light Brightness factor", ref LightBrightnessMultiplier, 0.0f, 100.0f );
			ImGui.Text( "Ambient Color" );
			ImGui.ColorPicker4( "", ref AmbientColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoAlpha );
			if(AmbientColor != PrevAmbientColor )
			{
				window.SceneLight.SetAmbientLightColor(AmbientColor);
				PrevAmbientColor = AmbientColor;
			}
		}
		ImGui.End();
	}
}
