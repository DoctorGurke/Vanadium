namespace Vanadium.Renderer.RenderData;

public class Model : IDisposable
{
	public Mesh[] Meshes;
	public bool IsError => Name == Error;
	private readonly string Name;
	public static string Error => "models/primitives/error.fbx";
	public static Model ErrorModel => Load( Error );
	private static readonly Dictionary<string, Model> _cache = new();
	public BBox RenderBounds { get; private set; }

	public Model( string name, Mesh[] meshes )
	{
		Name = name;
		Meshes = meshes;
		RenderBounds = GenerateRenderBounds( meshes );
	}

	public static void Precache( string path )
	{
		_ = Load( path );
	}

	public void Draw( DrawCommand cmd )
	{
		foreach ( var mesh in Meshes )
		{
			mesh.Draw( cmd );
		}
	}

	public static Model Load( string path )
	{
		if ( _cache.TryGetValue( path, out var cachedmodel ) )
		{
			return cachedmodel;
		}

		var builder = new ModelBuilder().FromFile( path );
		var model = builder.Build();
		_cache.Add( path, model );
		return model;
	}

	private static BBox GenerateRenderBounds( Mesh[] meshes )
	{
		Vector3 mins = default;
		Vector3 maxs = default;

		foreach ( var mesh in meshes )
		{
			foreach ( var vertex in mesh.Vertices )
			{
				var vert = vertex.position;
				if ( vert.x < mins.x )
					mins.x = vert.x;
				else if ( vert.x > maxs.x )
					maxs.x = vert.x;

				if ( vert.y < mins.y )
					mins.y = vert.y;
				else if ( vert.y > maxs.y )
					maxs.y = vert.y;

				if ( vert.z < mins.z )
					mins.z = vert.z;
				else if ( vert.z > maxs.z )
					maxs.z = vert.z;
			}
		}

		return new BBox( mins, maxs );
	}

	public void Dispose()
	{
		foreach ( var mesh in Meshes )
		{
			mesh.Dispose();
		}
		GC.SuppressFinalize( this );
	}

	public static class Primitives
	{
		public static Model Axis => Load( "models/primitives/axis.fbx" );
		public static Model Error => Load( "models/primitives/error.fbx" );
		public static Model Cube => Load( "models/primitives/cube.fbx" );
		public static Model InvertedCube => Load( "models/primitives/cube_inv.fbx" );
		public static Model Sphere => Load( "models/primitives/sphere.fbx" );
		public static Model ForwardCone => Load( "models/primitives/cone_forward.fbx" );
	}
}
