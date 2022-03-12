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

		public DebugLine(Vector3 start, Vector3 end, Color color, float duration, bool depthtest)
		{
			this.start = start;
			this.end = end;
			this.color = color;
			this.duration = duration;
			this.depthtest = depthtest;

			TimeSinceSpawned = 0;

			SetupMesh();
		}

		private int vao, vbo, ebo;

		public Material Material;

		private void SetupMesh()
		{
			Material = Material.Load( "materials/core/debugline.vanmat" );
			Material.Use();

			GLUtil.CreateBuffer( "Debugline VBO", out vbo );
			GL.BindBuffer(BufferTarget.ArrayBuffer, vbo );
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

			GLUtil.CreateBuffer( "Debugline EBO", out ebo );
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo );
			GL.BufferData( BufferTarget.ElementArrayBuffer, 2 * sizeof( uint ), new uint[] { 0, 1 }, BufferUsageHint.StaticDraw );

			GL.BindVertexArray( 0 );
		}

		public void Draw()
		{
			Material.Use();
			Material.Set( "color", color );

			if(!depthtest)
				GL.Disable( EnableCap.DepthTest );

			GL.BindVertexArray( vao );
			GL.DrawElements( PrimitiveType.Lines, 2, DrawElementsType.UnsignedInt, 0 );

			// re-enable depth test
			GL.Enable( EnableCap.DepthTest );
		}

		public override string ToString()
		{
			return $"start: {start} end: {end} color: {color} duration: {duration} depth: {depthtest}";
		}
	}

	private static IList<DebugLine> DebugLines = new List<DebugLine>();

	public static void Line(Vector3 start, Vector3 end, Color color, float duration = 0, bool depthtest = true)
	{
		DebugLines.Add( new DebugLine( start, end, color, duration, depthtest ));
	}

	public static void Draw()
	{
		foreach( var line in DebugLines.Reverse() )
		{
			line.Draw();

			// check lifetime of debugline
			if(line.TimeSinceSpawned >= line.duration)
			{
				DebugLines.Remove( line );
			}
		}
	}
}
