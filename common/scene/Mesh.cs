﻿using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace Vanadium;

public class Mesh {

	public Model Model;
	private Vertex[] _vertices;
	private int[] _indices;

	public struct Vertex {
		public Vector3 position;
		public Vector3 normal;
		public Vector3 tangent;
		public Vector3 bitangent;
		public Vector2 uv0;
		public Vector2 uv1;
		public Vector2 uv2;
		public Vector2 uv3;
		public Vector3 color;

		public Vertex(Vector3 position, Vector3 normal, Vector3 tangent, Vector3 bitangent, Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector3 color) {
			this.position = position;
			this.normal = normal;
			this.tangent = tangent;
			this.bitangent = bitangent;
			this.uv0 = uv0;
			this.uv1 = uv1;
			this.uv2 = uv2;
			this.uv3 = uv3;
			this.color = color;
		}
	}

	public Mesh(Vertex[] vertices, int[] indices, string material) {
		_vertices = vertices;
		_indices = indices;
		_material = Material.Load(material);
		SetupMesh();
	}

	private int vao, vbo, ebo;

	private Material _material;

	private void SetupMesh() {
		// use shader first to get attributes
		_material.Use();

		// create, bind and populate vbo
		vbo = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
		GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * Marshal.SizeOf(typeof(Vertex)), _vertices, BufferUsageHint.StaticDraw);

		vao = GL.GenVertexArray();
		GL.BindVertexArray(vao);

		// vertex positions
		var vertexPositionLocation = _material.Shader.GetAttribLocation("vPosition");
		if(vertexPositionLocation >= 0) {
			GL.EnableVertexAttribArray(vertexPositionLocation);
			GL.VertexAttribPointer(vertexPositionLocation, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf(typeof(Vertex)), 0);
		}

		// vertex normal
		var vertexNormalLocation = _material.Shader.GetAttribLocation("vNormal");
		if(vertexNormalLocation >= 0) {
			GL.EnableVertexAttribArray(vertexNormalLocation);
			GL.VertexAttribPointer(vertexNormalLocation, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf(typeof(Vertex)), Marshal.OffsetOf(typeof(Vertex), "normal"));
		}

		// vertex tangent
		var vertexTangentLocation = _material.Shader.GetAttribLocation("vTangent");
		if(vertexTangentLocation >= 0) {
			GL.EnableVertexAttribArray(vertexTangentLocation);
			GL.VertexAttribPointer(vertexTangentLocation, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf(typeof(Vertex)), Marshal.OffsetOf(typeof(Vertex), "tangent"));
		}

		// vertex bitangent
		var vertexBitangentLocation = _material.Shader.GetAttribLocation("vBitangent");
		if(vertexBitangentLocation >= 0) {
			GL.EnableVertexAttribArray(vertexBitangentLocation);
			GL.VertexAttribPointer(vertexBitangentLocation, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf(typeof(Vertex)), Marshal.OffsetOf(typeof(Vertex), "bitangent"));
		}

		// uv0
		var uv0Location = _material.Shader.GetAttribLocation("vTexCoord0");
		if(uv0Location >= 0) {
			GL.EnableVertexAttribArray(uv0Location);
			GL.VertexAttribPointer(uv0Location, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf(typeof(Vertex)), Marshal.OffsetOf(typeof(Vertex), "uv0"));
		}

		// uv1
		var uv1Location = _material.Shader.GetAttribLocation("vTexCoord1");
		if(uv1Location >= 0) {
			GL.EnableVertexAttribArray(uv1Location);
			GL.VertexAttribPointer(uv1Location, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf(typeof(Vertex)), Marshal.OffsetOf(typeof(Vertex), "uv1"));
		}

		// uv2
		var uv2Location = _material.Shader.GetAttribLocation("vTexCoord2");
		if(uv2Location >= 0) {
			GL.EnableVertexAttribArray(uv2Location);
			GL.VertexAttribPointer(uv2Location, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf(typeof(Vertex)), Marshal.OffsetOf(typeof(Vertex), "uv2"));
		}

		// uv3
		var uv3Location = _material.Shader.GetAttribLocation("vTexCoord3");
		if(uv3Location >= 0) {
			GL.EnableVertexAttribArray(uv3Location);
			GL.VertexAttribPointer(uv3Location, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf(typeof(Vertex)), Marshal.OffsetOf(typeof(Vertex), "uv3"));
		}

		// vertex color
		var vertexColorLocation = _material.Shader.GetAttribLocation("vColor");
		if(vertexColorLocation >= 0) {
			GL.EnableVertexAttribArray(vertexColorLocation);
			GL.VertexAttribPointer(vertexColorLocation, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf(typeof(Vertex)), Marshal.OffsetOf(typeof(Vertex), "color"));
		}

		// create, bind and populate ebo
		ebo = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
		GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

		GL.BindVertexArray(0);

		_timer = new Stopwatch();
		_timer.Start();
	}

	private Stopwatch _timer;

	public void Draw(SceneObject sceneobject) {
		_material.Use();

		double time = _timer.Elapsed.TotalSeconds;
		//float tintAmount = ((float)Math.Sin(time) + 1) / 2;

		//_shader.Set("tintAmount", tintAmount);

		var model = sceneobject.GlobalTransform;
		_material.Shader.Set("model", model);
		var view = Camera.ActiveCamera.ViewMatrix;
		_material.Shader.Set("view", view);
		var proj = Camera.ActiveCamera.ProjectionMatrix;
		_material.Shader.Set("projection", proj);

		GL.BindVertexArray(vao);
		GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
	}
}
