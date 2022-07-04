namespace Vanadium.Renderer.RenderData
{
	public struct DrawCommand
	{
		public Material? MaterialOverride;
		public OpenTKMath.Matrix4 Transform;
		public Color RenderColor;
		public float TintAmount;

		public DrawCommand( OpenTKMath.Matrix4 transform, Color renderColor, float tintAmount, Material? materialOverride )
		{
			Transform = transform;
			RenderColor = renderColor;
			TintAmount = tintAmount;
			MaterialOverride = materialOverride;
		}

		public static DrawCommand FromMaterialOverride( Material mat )
		{
			return new DrawCommand( OpenTKMath.Matrix4.Identity, Color.White, 1.0f, mat );
		}

		public static DrawCommand FromSceneObject( SceneObject obj )
		{
			return new( obj.GlobalTransform, obj.RenderColor, obj.TintAmount, obj.MaterialOverride );
		}
	}
}
