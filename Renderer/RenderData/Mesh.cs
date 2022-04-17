using OpenTK.Graphics.OpenGL4;
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
		SetupMesh();
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
		// create, bind and populate vbo
		GLUtil.CreateBuffer( "Mesh VBO", out vbo );
		GL.BindBuffer( BufferTarget.ArrayBuffer, vbo );
		GL.BufferData( BufferTarget.ArrayBuffer, Vertices.Length * Marshal.SizeOf( typeof( Vertex ) ), Vertices, BufferUsageHint.StaticDraw );

		GLUtil.CreateVertexArray( "Mesh VAO", out vao );
		GL.BindVertexArray( vao );

		SetupRenderAttributes();

		// create, bind and populate ebo
		GLUtil.CreateBuffer( "Mesh EBO", out ebo );
		GL.BindBuffer( BufferTarget.ElementArrayBuffer, ebo );
		GL.BufferData( BufferTarget.ElementArrayBuffer, Indices.Length * sizeof( uint ), Indices, BufferUsageHint.StaticDraw );

		GL.BindVertexArray( 0 );
	}

	private static void SetupRenderAttributes()
	{
		GL.EnableVertexAttribArray( 0 );
		GL.VertexAttribPointer( 0, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf( typeof( Vertex ) ), 0 );

		GL.EnableVertexAttribArray( 1 );
		GL.VertexAttribPointer( 1, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf( typeof( Vertex ) ), Marshal.OffsetOf( typeof( Vertex ), "normal" ) );

		GL.EnableVertexAttribArray( 2 );
		GL.VertexAttribPointer( 2, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf( typeof( Vertex ) ), Marshal.OffsetOf( typeof( Vertex ), "tangent" ) );

		GL.EnableVertexAttribArray( 3 );
		GL.VertexAttribPointer( 3, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf( typeof( Vertex ) ), Marshal.OffsetOf( typeof( Vertex ), "bitangent" ) );

		GL.EnableVertexAttribArray( 4 );
		GL.VertexAttribPointer( 4, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf( typeof( Vertex ) ), Marshal.OffsetOf( typeof( Vertex ), "uv0" ) );

		GL.EnableVertexAttribArray( 5 );
		GL.VertexAttribPointer( 5, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf( typeof( Vertex ) ), Marshal.OffsetOf( typeof( Vertex ), "uv1" ) );

		GL.EnableVertexAttribArray( 6 );
		GL.VertexAttribPointer( 6, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf( typeof( Vertex ) ), Marshal.OffsetOf( typeof( Vertex ), "uv2" ) );

		GL.EnableVertexAttribArray( 7 );
		GL.VertexAttribPointer( 7, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf( typeof( Vertex ) ), Marshal.OffsetOf( typeof( Vertex ), "uv3" ) );

		GL.EnableVertexAttribArray( 8 );
		GL.VertexAttribPointer( 8, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf( typeof( Vertex ) ), Marshal.OffsetOf( typeof( Vertex ), "color" ) );
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
