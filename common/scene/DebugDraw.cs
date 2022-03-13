using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;

namespace Vanadium;

public static class DebugDraw
{
	private class DebugLine
	{
		public Vector3 start;
		public Vector3 end;
		public Color color;
		public float duration;
		public bool depthtest;
		public TimeSince TimeSinceSpawned;

		public DebugLine( Vector3 start, Vector3 end, Color color, float duration, bool depthtest )
		{
			this.start = start;
			this.end = end;
			this.color = color;
			this.duration = duration;
			this.depthtest = depthtest;

			TimeSinceSpawned = 0;
		}

		public override string ToString()
		{
			return $"start: {start} end: {end} color: {color} duration: {duration} depth: {depthtest}";
		}
	}

	private static IList<DebugLine> DebugLines = new List<DebugLine>();

	private static Material Material = Material.Load( "materials/core/debugline.vanmat" );

	private static int vao, vbo, ebo;

	public static void Init()
	{
		Material.Use();

		GLUtil.CreateBuffer( "Debugline VBO", out vbo );
		GL.BindBuffer( BufferTarget.ArrayBuffer, vbo );

		GLUtil.CreateVertexArray( "Debugline VAO", out vao );
		GL.BindVertexArray( vao );

		// vertex positions
		var vertexPositionLocation = Material.GetAttribLocation( "vPosition" );
		if ( vertexPositionLocation >= 0 )
		{
			GL.EnableVertexAttribArray( vertexPositionLocation );
			GL.VertexAttribPointer( vertexPositionLocation, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf( typeof( Vector3 ) ), 0 );
		}

		GLUtil.CreateBuffer( "Debugline EBO", out ebo );
		GL.BindBuffer( BufferTarget.ElementArrayBuffer, ebo );
	}

	private static IList<Vector3> LineVertices = new List<Vector3>();

	private static void DrawLines()
	{
		GL.Enable( EnableCap.DepthTest );
		Material.Use();
		Material.Set( "color", Color.Red );

		GL.BindVertexArray( vao );

		// update vertex positions
		GL.BindBuffer( BufferTarget.ArrayBuffer, vbo );
		GL.BufferData( BufferTarget.ArrayBuffer, LineVertices.Count * Marshal.SizeOf( typeof( Vector3 ) ), LineVertices.ToArray(), BufferUsageHint.StaticDraw );

		var indices = new uint[LineVertices.Count * 2];
		for ( uint i = 0; i < LineVertices.Count * 2; i++ )
		{
			indices[i] = i;
		}

		GL.BindBuffer( BufferTarget.ElementArrayBuffer, ebo );
		GL.BufferData( BufferTarget.ElementArrayBuffer, indices.Length * sizeof( uint ), indices, BufferUsageHint.StaticDraw );

		GL.DrawElements( PrimitiveType.Lines, indices.Length, DrawElementsType.UnsignedInt, 0 );
	}

	public static void Draw()
	{
		// clear prev line vertices
		LineVertices.Clear();

		foreach ( var line in DebugLines.Reverse() )
		{
			LineVertices.Add( line.start );
			LineVertices.Add( line.end );

			// check lifetime of debugline
			if ( line.TimeSinceSpawned >= line.duration )
			{
				DebugLines.Remove( line );
			}
		}

		DrawLines();
	}

	public static void Line( Vector3 start, Vector3 end, Color color, float duration = 0, bool depthtest = true )
	{
		DebugLines.Add( new DebugLine( start, end, color, duration, depthtest ) );
	}

	public static void Box( Vector3 position, Vector3 mins, Vector3 maxs, Color color, float duration = 0, bool depthtest = true )
	{
		var bbox = new BBox( mins, maxs );
		Box( position, bbox, color, duration, depthtest );
	}

	public static void Box( Vector3 position, BBox box, Color color, float duration = 0, bool depthtest = true )
	{
		var corners = box.Corners.ToArray();

		// bottom
		Line( position + corners[0], position + corners[1], color, duration, depthtest );
		Line( position + corners[1], position + corners[5], color, duration, depthtest );
		Line( position + corners[5], position + corners[4], color, duration, depthtest );
		Line( position + corners[4], position + corners[0], color, duration, depthtest );

		// top
		Line( position + corners[3], position + corners[2], color, duration, depthtest );
		Line( position + corners[2], position + corners[6], color, duration, depthtest );
		Line( position + corners[6], position + corners[7], color, duration, depthtest );
		Line( position + corners[7], position + corners[3], color, duration, depthtest );

		// sides
		Line( position + corners[0], position + corners[3], color, duration, depthtest );
		Line( position + corners[1], position + corners[2], color, duration, depthtest );
		Line( position + corners[5], position + corners[6], color, duration, depthtest );
		Line( position + corners[4], position + corners[7], color, duration, depthtest );
	}
}
