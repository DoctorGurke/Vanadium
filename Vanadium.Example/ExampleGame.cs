using ImGuiNET;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;
using Vanadium.RenderSystem.RenderData;
using Vanadium.RenderSystem.RenderData.View;
using Vanadium.RenderSystem.Scene;

namespace Vanadium.Example
{
	public class ExampleRenderer : Renderer
	{
		public ExampleRenderer() : base() { }

		public override void PostLoad()
		{
			// set skybox
			_ = new SceneSkyBox( Material.Load( "materials/skybox/skybox03.vanmat" ) );

			var floor = new SceneObject
			{
				Model = Model.Load( "models/brickwall.fbx" ),
				Position = Vector3.Down,
				Scale = 0.5f
			};
			floor.SetMaterialOverride( "materials/pbrtest/tiles.vanmat" );

			_ = new SceneObject
			{
				Model = Model.Primitives.Axis
			};

			// init camera
			_ = new FirstPersonCamera
			{
				Position = Vector3.Backward * 3 + Vector3.Up + Vector3.Right
			};
		}
	}
}
