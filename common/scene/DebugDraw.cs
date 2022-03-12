using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;

namespace Vanadium;

public static class DebugDraw
{
	private class DebugLine : IDisposable
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

			SetupMesh();
		}

		public void Dispose()
		{
			GL.DeleteBuffer( vbo );
			GL.DeleteVertexArray( vao );
		}

		private int vao, vbo;

		public Material Material;

		private void SetupMesh()
		{
			Material = Material.Load( "materials/core/debugline.vanmat" );
			Material.Use();

			GLUtil.CreateBuffer( "Debugline VBO", out vbo );
			GL.BindBuffer( BufferTarget.ArrayBuffer, vbo );
			GL.BufferData( BufferTarget.ArrayBuffer, 2 * Marshal.SizeOf( typeof( Vector3 ) ), new Vector3[] { start, end }, BufferUsageHint.StaticDraw );

			GLUtil.CreateVertexArray( "Debugline VAO", out vao );
			GL.BindVertexArray( vao );

			// vertex positions
			var vertexPositionLocation = Material.GetAttribLocation( "vPosition" );
			if ( vertexPositionLocation >= 0 )
			{
				GL.EnableVertexAttribArray( vertexPositionLocation );
				GL.VertexAttribPointer( vertexPositionLocation, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf( typeof( Vector3 ) ), 0 );
			}

			GL.BindVertexArray( 0 );
		}

		public void Draw()
		{
			Material.Use();
			Material.Set( "color", color );

			if ( !depthtest )
				GL.Disable( EnableCap.DepthTest );

			GL.BindVertexArray( vao );
			GL.DrawArrays( PrimitiveType.Lines, 0, 2 );

			// re-enable depth test
			GL.Enable( EnableCap.DepthTest );
		}

		public override string ToString()
		{
			return $"start: {start} end: {end} color: {color} duration: {duration} depth: {depthtest}";
		}
	}

	private static IList<DebugLine> DebugLines = new List<DebugLine>();

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

	public static void Draw()
	{
		foreach ( var line in DebugLines.Reverse() )
		{
			line.Draw();

			// check lifetime of debugline
			if ( line.TimeSinceSpawned >= line.duration )
			{
				DebugLines.Remove( line );
				line.Dispose();
			}
		}
	}
}
