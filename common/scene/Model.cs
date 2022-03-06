﻿using Assimp;
using Assimp.Configs;
using OpenTK.Mathematics;
using System.Reflection;

namespace Vanadium;

public class Model {
	private Mesh[] _meshes;

	public bool IsError = false;

	public static string ErrorModel = "models/error.fbx";

	private static Dictionary<string, Model> PrecachedModels = new();

	public static void Precache(string path) {
		Load(path);
	}

	public static Model Load(string path) {
		path = $"resources/{path}";

		if(PrecachedModels.TryGetValue(path, out var mdl)) {
			return mdl;
		}

		Debug.WriteLine($"loading model: {path}");
		var fileName = Path.Combine($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}", path);

		AssimpContext importer = new AssimpContext();

		Scene? scene;
		try {
			scene = importer.ImportFile(fileName, PostProcessPreset.TargetRealTimeMaximumQuality | PostProcessSteps.FlipUVs | PostProcessSteps.Triangulate);
		} catch(FileNotFoundException ex) {
			Debug.WriteLine($"ERROR IMPORTING MODEL {fileName} ({ex})");
			return Load(ErrorModel);
		}

		if(scene is null || scene.SceneFlags == SceneFlags.Incomplete || scene.RootNode is null) {
			Debug.WriteLine("ASSIMP IMPORT ERROR");
			return Load(ErrorModel);
		}

		var model = new Model();
		model._meshes = new Mesh[scene.MeshCount];
		model.ProcessNode(scene.RootNode, scene);
		PrecachedModels.Add(path, model);

		if(path == ErrorModel)
			model.IsError = true;

		Debug.WriteLine($"finished loading model: {path}");
		return model;
	}

	public void Draw(SceneObject sceneobject) {
		foreach(var mesh in _meshes) {
			mesh.Draw(sceneobject);
		}
	}

	private void ProcessNode(Node node, Scene scene) {
		foreach(int index in node.MeshIndices) {
			Assimp.Mesh mesh = scene.Meshes[index];
			_meshes[index] = processMesh(mesh, scene);
		}
		for(int i = 0; i < node.ChildCount; i++) {
			ProcessNode(node.Children[i], scene);
		}
	}

	private Mesh processMesh(Assimp.Mesh mesh, Scene scene) {
		Mesh.Vertex[] vertices = new Mesh.Vertex[mesh.VertexCount];
		int[] indices = new int[mesh.FaceCount * 3];

		for(int v = 0; v < mesh.VertexCount; v++) {
			Mesh.Vertex vertex;
			Vector2 uv = new(0, 0);
			Vector3 color = new();

			var pos = mesh.Vertices[v];
			var normal = mesh.Normals[v];
			var tangent = mesh.Tangents[v];
			var bitangent = mesh.BiTangents[v];

			uv.X = mesh.TextureCoordinateChannels[0][v].X;
			uv.Y = mesh.TextureCoordinateChannels[0][v].Y;

			if(mesh.VertexColorChannelCount >= 1) {
				color.x = mesh.VertexColorChannels[0][v].R;
				color.y = mesh.VertexColorChannels[0][v].G;
				color.z = mesh.VertexColorChannels[0][v].B;
			}

			vertex = new Mesh.Vertex(pos, normal, tangent, bitangent, uv, color);
			vertices[v] = vertex;
		}

		for(int fa = 0; fa < mesh.FaceCount; fa++) {
			Face face = mesh.Faces[fa];
			for(int ind = 0; ind < face.IndexCount; ind++) {
				indices[fa * 3 + ind] = face.Indices[ind];
			}
		}

		foreach(var vert in vertices.Reverse()) {
			vert.tangent.Normalize();
		}

		Debug.WriteLine($"new mesh v:{vertices.Length} i:{indices.Length} mat:{scene.Materials[mesh.MaterialIndex].Name}");
		Mesh fmesh = new Mesh(vertices, indices, scene.Materials[mesh.MaterialIndex].Name);
		fmesh.Model = this;
		return fmesh;
	}
}
