﻿using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;

namespace Vanadium.Renderer.RenderData;

public class Mesh : IDisposable
{
	public Vertex[] Vertices;
	public int[] Indices;

	public struct Vertex
	{
		public Vector3 position;
		public Vector3 normal;
		public Vector3 tangent;
		public Vector3 bitangent;
		public OpenTKMath.Vector2 uv0;
		public OpenTKMath.Vector2 uv1;
		public OpenTKMath.Vector2 uv2;
		public OpenTKMath.Vector2 uv3;
		public Vector3 color;

		public Vertex( Vector3 position, Vector3 normal, Vector3 tangent, Vector3 bitangent, OpenTKMath.Vector2 uv0, OpenTKMath.Vector2 uv1, OpenTKMath.Vector2 uv2, OpenTKMath.Vector2 uv3, Vector3 color )
		{
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

	public Mesh( Vertex[] vertices, int[] indices, string material )
	{
		Vertices = vertices;
		Indices = indices;
		Material = Material.Load( $"{material}.vanmat" );
	}

	private int vao, vbo, ebo;

	private Material? _material;

	public Material Material
	{
		get
		{
			return _material ?? Material.ErrorMaterial;
		}
		set
		{
			if ( _material is not null && _material.Equals( value ) ) return;

			_material = value;
			SetupMesh( value );
		}
	}

	public void Dispose()
	{
		Material.Dispose();
		GL.DeleteVertexArray( vao );
		GL.DeleteBuffer( vbo );
		GL.DeleteBuffer( ebo );
		GC.SuppressFinalize( this );
	}

	public void SetupMesh()
	{
		SetupMesh( Material );
	}

	public void SetupRenderAttributes( Material mat )
	{
		// enable again in case we called this from outside of setupmesh
		GL.BindVertexArray( vao );

		// vertex positions
		var vertexPositionLocation = mat.GetAttribLocation( "vPosition" );
		if ( vertexPositionLocation >= 0 )
		{
			GL.EnableVertexAttribArray( vertexPositionLocation );
			GL.VertexAttribPointer( vertexPositionLocation, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf( typeof( Vertex ) ), 0 );
		}

		// vertex normal
		var vertexNormalLocation = mat.GetAttribLocation( "vNormal" );
		if ( vertexNormalLocation >= 0 )
		{
			GL.EnableVertexAttribArray( vertexNormalLocation );
			GL.VertexAttribPointer( vertexNormalLocation, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf( typeof( Vertex ) ), Marshal.OffsetOf( typeof( Vertex ), "normal" ) );
		}

		// vertex tangent
		var vertexTangentLocation = mat.GetAttribLocation( "vTangent" );
		if ( vertexTangentLocation >= 0 )
		{
			GL.EnableVertexAttribArray( vertexTangentLocation );
			GL.VertexAttribPointer( vertexTangentLocation, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf( typeof( Vertex ) ), Marshal.OffsetOf( typeof( Vertex ), "tangent" ) );
		}

		// vertex bitangent
		var vertexBitangentLocation = mat.GetAttribLocation( "vBitangent" );
		if ( vertexBitangentLocation >= 0 )
		{
			GL.EnableVertexAttribArray( vertexBitangentLocation );
			GL.VertexAttribPointer( vertexBitangentLocation, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf( typeof( Vertex ) ), Marshal.OffsetOf( typeof( Vertex ), "bitangent" ) );
		}

		// uv0
		var uv0Location = mat.GetAttribLocation( "vTexCoord0" );
		if ( uv0Location >= 0 )
		{
			GL.EnableVertexAttribArray( uv0Location );
			GL.VertexAttribPointer( uv0Location, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf( typeof( Vertex ) ), Marshal.OffsetOf( typeof( Vertex ), "uv0" ) );
		}

		// uv1
		var uv1Location = mat.GetAttribLocation( "vTexCoord1" );
		if ( uv1Location >= 0 )
		{
			GL.EnableVertexAttribArray( uv1Location );
			GL.VertexAttribPointer( uv1Location, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf( typeof( Vertex ) ), Marshal.OffsetOf( typeof( Vertex ), "uv1" ) );
		}

		// uv2
		var uv2Location = mat.GetAttribLocation( "vTexCoord2" );
		if ( uv2Location >= 0 )
		{
			GL.EnableVertexAttribArray( uv2Location );
			GL.VertexAttribPointer( uv2Location, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf( typeof( Vertex ) ), Marshal.OffsetOf( typeof( Vertex ), "uv2" ) );
		}

		// uv3
		var uv3Location = mat.GetAttribLocation( "vTexCoord3" );
		if ( uv3Location >= 0 )
		{
			GL.EnableVertexAttribArray( uv3Location );
			GL.VertexAttribPointer( uv3Location, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf( typeof( Vertex ) ), Marshal.OffsetOf( typeof( Vertex ), "uv3" ) );
		}

		// vertex color
		var vertexColorLocation = mat.GetAttribLocation( "vColor" );
		if ( vertexColorLocation >= 0 )
		{
			GL.EnableVertexAttribArray( vertexColorLocation );
			GL.VertexAttribPointer( vertexColorLocation, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf( typeof( Vertex ) ), Marshal.OffsetOf( typeof( Vertex ), "color" ) );
		}
	}

	public void SetupMesh( Material mat )
	{
		// use shader first to get attributes
		mat.Use();

		// create, bind and populate vbo
		GLUtil.CreateBuffer( "Mesh VBO", out vbo );
		GL.BindBuffer( BufferTarget.ArrayBuffer, vbo );
		GL.BufferData( BufferTarget.ArrayBuffer, Vertices.Length * Marshal.SizeOf( typeof( Vertex ) ), Vertices, BufferUsageHint.StaticDraw );

		GLUtil.CreateVertexArray( "Mesh VAO", out vao );
		GL.BindVertexArray( vao );

		SetupRenderAttributes( mat );

		// create, bind and populate ebo
		GLUtil.CreateBuffer( "Mesh EBO", out ebo );
		GL.BindBuffer( BufferTarget.ElementArrayBuffer, ebo );
		GL.BufferData( BufferTarget.ElementArrayBuffer, Indices.Length * sizeof( uint ), Indices, BufferUsageHint.StaticDraw );

		GL.BindVertexArray( 0 );
	}

	public void Draw( SceneObject? sceneobject )
	{
		Material.Use();

		OpenTKMath.Matrix4 transform = OpenTKMath.Matrix4.Identity;
		if ( sceneobject is not null )
		{
			transform = sceneobject.GlobalTransform;
			Material.Set( "renderColor", sceneobject.RenderColor );
			Material.Set( "tintAmount", sceneobject.TintAmount );
		}

		Draw( transform );
	}

	private void Draw( OpenTKMath.Matrix4 transform )
	{
		Material.Set( "transform", transform );

		GL.BindVertexArray( vao );
		GL.DrawElements( PrimitiveType.Triangles, Indices.Length, DrawElementsType.UnsignedInt, 0 );
	}
}