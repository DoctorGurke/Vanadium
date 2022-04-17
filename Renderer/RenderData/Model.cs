using Assimp;
using System.Reflection;

namespace Vanadium.Renderer.RenderData;

public class Model : IDisposable
{
	public Mesh[]? Meshes;

	public bool IsError = false;

	public static string Error => "models/primitives/error.fbx";
	public static Model ErrorModel => Load( Error );

	private static readonly Dictionary<string, Model> PrecachedModels = new();

	public BBox RenderBounds { get; private set; }

	public void SetMaterialOverride( string path )
	{
		if ( IsError ) return;
		SetMeshMaterials( Material.Load( path ) );
	}

	public void SetMeshMaterials( Material mat )
	{
		if ( Meshes is null ) return;
		foreach ( var mesh in Meshes )
		{
			mesh.Material = mat;
		}
	}

	public void SetupMeshes()
	{
		if ( Meshes is null ) return;
		foreach ( var mesh in Meshes )
		{
			mesh.SetupMesh();
		}
	}

	public void SetupMeshes( Material mat )
	{
		if ( Meshes is null ) return;
		foreach ( var mesh in Meshes )
		{
			mesh.SetupMesh( mat );
		}
	}

	public static void Precache( string path )
	{
		Load( path );
	}

	public void Dispose()
	{
		if ( Meshes is not null )
		{
			foreach ( var mesh in Meshes )
			{
				mesh.Dispose();
			}
		}
		GC.SuppressFinalize( this );
	}

	public static Model Load( string path )
	{
		var oldpath = path;
		path = $"resources/{path}";

		if ( PrecachedModels.TryGetValue( path, out var mdl ) )
		{
			return mdl;
		}

		Log.Info( $"loading model: {path}" );
		var fileName = Path.Combine( $"{Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location )}", path );

		AssimpContext importer = new();

		Assimp.Scene? scene;
		try
		{
			scene = importer.ImportFile( fileName, PostProcessPreset.TargetRealTimeMaximumQuality | PostProcessSteps.FlipUVs | PostProcessSteps.Triangulate | PostProcessSteps.CalculateTangentSpace );
		}
		catch ( FileNotFoundException ex )
		{
			Log.Info( $"ERROR IMPORTING MODEL {fileName} ({ex})" );
			return ErrorModel;
		}

		if ( scene is null || scene.SceneFlags == SceneFlags.Incomplete || scene.RootNode is null )
		{
			Log.Info( "ASSIMP IMPORT ERROR" );
			return ErrorModel;
		}

		Model model = new();
		model.Meshes = new Mesh[scene.MeshCount];
		model.ProcessNode( scene.RootNode, scene );
		model.RenderBounds = model.GetRenderBounds();
		PrecachedModels.Add( path, model );

		if ( oldpath == Error )
			model.IsError = true;

		Log.Info( $"finished loading model: {path}" );
		return model;
	}

	public void Draw()
	{
		Draw( null );
	}

	public void Draw( SceneObject? sceneobject )
	{
		if ( Meshes is null ) return;
		foreach ( var mesh in Meshes )
		{
			mesh.Draw( sceneobject );
		}
	}

	private void ProcessNode( Node node, Assimp.Scene scene )
	{
		if ( Meshes is null ) return;
		foreach ( int index in node.MeshIndices )
		{
			Assimp.Mesh mesh = scene.Meshes[index];
			Meshes[index] = ProcessMesh( mesh, scene );
		}
		for ( int i = 0; i < node.ChildCount; i++ )
		{
			ProcessNode( node.Children[i], scene );
		}
	}

	private static Mesh ProcessMesh( Assimp.Mesh mesh, Assimp.Scene scene )
	{
		Mesh.Vertex[] vertices = new Mesh.Vertex[mesh.VertexCount];
		int[] indices = new int[mesh.FaceCount * 3];

		for ( int v = 0; v < mesh.VertexCount; v++ )
		{
			Mesh.Vertex vertex;
			OpenTKMath.Vector2 uv0 = new();
			OpenTKMath.Vector2 uv1 = new();
			OpenTKMath.Vector2 uv2 = new();
			OpenTKMath.Vector2 uv3 = new();

			if ( mesh.TextureCoordinateChannelCount > 0 )
			{
				uv0 = new( mesh.TextureCoordinateChannels[0][v].X, mesh.TextureCoordinateChannels[0][v].Y );
			}
			if ( mesh.TextureCoordinateChannelCount > 1 )
			{
				uv1 = new( mesh.TextureCoordinateChannels[1][v].X, mesh.TextureCoordinateChannels[1][v].Y );
			}
			if ( mesh.TextureCoordinateChannelCount > 2 )
			{
				uv2 = new( mesh.TextureCoordinateChannels[2][v].X, mesh.TextureCoordinateChannels[2][v].Y );
			}
			if ( mesh.TextureCoordinateChannelCount > 3 )
			{
				uv3 = new( mesh.TextureCoordinateChannels[3][v].X, mesh.TextureCoordinateChannels[3][v].Y );
			}

			Vector3 normal = new();
			if ( mesh.HasNormals )
			{
				normal = mesh.Normals[v];
			}

			Vector3 tangent = new();
			Vector3 bitangent = new();
			if ( mesh.HasTangentBasis )
			{
				tangent = mesh.Tangents[v];
				bitangent = mesh.BiTangents[v];
			}

			Vector3 color = new();
			if ( mesh.VertexColorChannelCount > 0 )
			{
				color.x = mesh.VertexColorChannels[0][v].R;
				color.y = mesh.VertexColorChannels[0][v].G;
				color.z = mesh.VertexColorChannels[0][v].B;
			}

			vertex = new Mesh.Vertex( mesh.Vertices[v], normal, tangent, bitangent, uv0, uv1, uv2, uv3, color );
			vertices[v] = vertex;
		}

		for ( int fa = 0; fa < mesh.FaceCount; fa++ )
		{
			Face face = mesh.Faces[fa];
			for ( int ind = 0; ind < face.IndexCount; ind++ )
			{
				indices[fa * 3 + ind] = face.Indices[ind];
			}
		}

		foreach ( var vert in vertices.Reverse() )
		{
			vert.tangent.Normalize();
		}

		Log.Info( $"new mesh v:{vertices.Length} i:{indices.Length} mat:{scene.Materials[mesh.MaterialIndex].Name}" );
		Mesh fmesh = new( vertices, indices, scene.Materials[mesh.MaterialIndex].Name );
		return fmesh;
	}

	private BBox GetRenderBounds()
	{
		if ( Meshes is null ) return new BBox();
		Vector3 mins = Meshes[0].Vertices[0].position;
		Vector3 maxs = Meshes[0].Vertices[0].position;

		foreach ( var mesh in Meshes )
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
