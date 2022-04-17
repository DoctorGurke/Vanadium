﻿using Assimp;
using System.Reflection;

namespace Vanadium.Renderer.RenderData
{
	public class ModelBuilder
	{
		private string? name;
		private Mesh[]? meshes;
		private Material? materialoverride;

		public ModelBuilder FromFile( string path )
		{
			name = path;
			InitMeshesFromFile( path );
			return this;
		}

		public ModelBuilder WithMaterialOverride( string? path )
		{
			if ( path is null )
			{
				materialoverride = null;
				return this;
			}
			return WithMaterialOverride( Material.Load( path ) );
		}

		public ModelBuilder WithMaterialOverride( Material? mat )
		{
			materialoverride = mat;
			return this;
		}

		public Model Build()
		{
			// apply materialoverride, if any
			if ( meshes is not null && materialoverride is not null )
			{
				foreach ( var mesh in meshes )
				{
					mesh.Material = materialoverride;
				}
			}
			return new Model( name ?? Model.Error, meshes ?? Array.Empty<Mesh>() );
		}

		private void InitMeshesFromFile( string path )
		{
			Log.Info( $"loading model: {path}" );
			var fileName = Path.Combine( $"{Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location )}/resources/{path}" );

			AssimpContext importer = new();

			Assimp.Scene? scene;
			try
			{
				scene = importer.ImportFile( fileName, PostProcessPreset.TargetRealTimeMaximumQuality | PostProcessSteps.FlipUVs | PostProcessSteps.Triangulate | PostProcessSteps.CalculateTangentSpace );
			}
			catch ( FileNotFoundException ex )
			{
				Log.Info( $"ERROR IMPORTING MODEL {fileName} ({ex})" );
				meshes = null;
				return;
			}

			if ( scene is null || scene.SceneFlags == SceneFlags.Incomplete || scene.RootNode is null )
			{
				Log.Info( "ASSIMP IMPORT ERROR" );
				return;
			}

			meshes = new Mesh[scene.MeshCount];
			ProcessNode( scene.RootNode, scene );
		}

		private void ProcessNode( Node node, Assimp.Scene scene )
		{
			if ( meshes is null ) return;
			foreach ( int index in node.MeshIndices )
			{
				Assimp.Mesh mesh = scene.Meshes[index];
				meshes[index] = ProcessMesh( mesh, scene );
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
	}
}