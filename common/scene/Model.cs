using Assimp;
using Assimp.Configs;
using OpenTK.Mathematics;
using System.Reflection;

namespace Vanadium;

public class Model {
	private List<Mesh> _meshes = new();

	public static Model? Load(string path) {
		Debug.WriteLine($"loading model: {path}");
		var fileName = Path.Combine($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}", path);

		AssimpContext importer = new AssimpContext();
		importer.SetConfig(new NormalSmoothingAngleConfig(66.0f));

		Scene scene = null;
		try {
			scene = importer.ImportFile(fileName, PostProcessPreset.TargetRealTimeMaximumQuality | PostProcessSteps.FlipUVs);
		} catch(FileNotFoundException ex) {
			Debug.WriteLine($"ERROR IMPORTING MODEL {fileName} ({ex})");
			return null;
		}
		if(scene is null || scene.SceneFlags == SceneFlags.Incomplete || scene.RootNode is null) {
			Debug.WriteLine("ASSIMP IMPORT ERROR");
			return null; // TODO: error model instead of nullable
		}

		var model = new Model();
		model.ProcessNode(scene.RootNode, scene);
		Debug.WriteLine($"finished loading model: {path}");
		return model;
	}

	public void Draw() {
		foreach(var mesh in _meshes) {
			mesh.Draw();
		}
	}

	private void ProcessNode(Node node, Scene scene) {
		foreach(int index in node.MeshIndices) {
			Assimp.Mesh mesh = scene.Meshes[index];
			_meshes.Add(processMesh(mesh, scene));
		}
		for(int i = 0; i < node.ChildCount; i++) {
			ProcessNode(node.Children[i], scene);
		}
	}

	private Mesh processMesh(Assimp.Mesh mesh, Scene scene) {
		List<Mesh.Vertex> vertices = new List<Mesh.Vertex>();
		List<int> indices = new List<int>();

		for(int v = 0; v < mesh.VertexCount; v++) {
			Mesh.Vertex vertex;
			Vector3 pos = new();
			Vector3 normal = new();
			Vector2 uv = new Vector2(0, 0);

			pos.x = mesh.Vertices[v].X;
			pos.y = mesh.Vertices[v].Y;
			pos.z = mesh.Vertices[v].Z;

			normal.x = mesh.Normals[v].X;
			normal.y = mesh.Normals[v].Y;
			normal.z = mesh.Normals[v].Z;

			if(mesh.TextureCoordinateChannelCount >= 1) {
				uv.X = mesh.TextureCoordinateChannels[0][v].X;
				uv.Y = mesh.TextureCoordinateChannels[0][v].Y;
			}

			vertex = new Mesh.Vertex(pos, normal, uv);
			vertices.Add(vertex);
		}

		for(int f = 0; f < mesh.FaceCount; f++) {
			Face face = mesh.Faces[f];
			for(int i = 0; i < face.IndexCount; i++) {
				indices.Add(face.Indices[i]);
			}
		}

		Mesh fmesh = new Mesh(vertices, indices);
		return fmesh;
	}
}
