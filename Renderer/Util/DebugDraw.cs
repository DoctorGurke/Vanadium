using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;

namespace Vanadium.Renderer.Util;

public static class DebugDraw
{

	/// <summary>
	/// Spawn a debug line
	/// </summary>
	/// <param name="start">Start position</param>
	/// <param name="end">End position</param>
	/// <param name="color">Color of the primitive</param>
	/// <param name="duration">Lifetime of the line</param>
	/// <param name="depthtest">Whether or not to enable depthtesting on this primitive</param>
	public static void Line( Vector3 start, Vector3 end, Color color, float duration = 0, bool depthtest = true )
	{
		DebugLines.Add( new DebugLine( start, end, color, duration, depthtest ) );
	}

	/// <summary>
	/// Spawn a debug box
	/// </summary>
	/// <param name="position">Position of the box (Center)</param>
	/// <param name="mins">Local Mins to the position</param>
	/// <param name="maxs">Local Maxs to the position</param>
	/// <param name="color">Color of the box</param>
	/// <param name="duration">Lifetime of the box</param>
	/// <param name="depthtest">Whether or not to enable depthesting on this box</param>
	public static void Box( Vector3 position, Vector3 mins, Vector3 maxs, Color color, float duration = 0, bool depthtest = true )
	{
		var bbox = new BBox( mins, maxs );
		Box( position, bbox, color, duration, depthtest );
	}

	/// <summary>
	/// Spawn a debug box
	/// </summary>
	/// <param name="position">Position of the box (Center)</param>
	/// <param name="box">The Bounding box, local to the position</param>
	/// <param name="color">Color of the box</param>
	/// <param name="duration">Lifetime of the box</param>
	/// <param name="depthtest">Whether or not to enable depthesting on this box</param>
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

	/// <summary>
	/// Spawn a debug Sphere
	/// </summary>
	/// <param name="position">Center position</param>
	/// <param name="radius">Radius of the Sphere</param>
	/// <param name="color">Color of the Sphere</param>
	/// <param name="duration">Lifetime of the Sphere</param>
	/// <param name="depthtest">Whether or not to enable depthesting on this Sphere</param>
	public static void Sphere( Vector3 position, float radius, Color color, float duration = 0, bool depthtest = true )
	{
		var top = position + Vector3.Up * radius;
		var bottom = position + Vector3.Down * radius;

		var equatorsegments = 12f;
		var incr = MathF.PI * 2.0f / equatorsegments;

		Vector3 lasttop = Vector3.Zero;
		Vector3 lastcenter = Vector3.Zero;
		Vector3 lastbottom = Vector3.Zero;

		Vector3 firsttop = Vector3.Zero;
		Vector3 firstcenter = Vector3.Zero;
		Vector3 firstbottom = Vector3.Zero;

		for ( int i = 0; i < equatorsegments; i++ )
		{
			// center offset
			var offset = (Vector3.Forward * MathF.Sin( incr * i ) + Vector3.Right * MathF.Cos( incr * i )).Normal * radius;

			// top offset
			var upoffset = (offset + Vector3.Up * radius * 0.85f).Normal * radius;

			// bottom offset
			var downoffset = (offset + Vector3.Down * radius * 0.85f).Normal * radius;

			if ( i == 0 )
			{
				// init last and first segment info
				lasttop = position + upoffset;
				firsttop = lasttop;

				lastcenter = position + offset;
				firstcenter = lastcenter;

				lastbottom = position + downoffset;
				firstbottom = lastbottom;
			}
			else if ( i == equatorsegments - 1 )
			{
				// Last segment to close the gap
				Line( position + upoffset, firsttop, color, duration, depthtest );
				Line( position + offset, firstcenter, color, duration, depthtest );
				Line( position + downoffset, firstbottom, color, duration, depthtest );

				// regular gap
				Line( position + upoffset, lasttop, color, duration, depthtest );
				Line( position + offset, lastcenter, color, duration, depthtest );
				Line( position + downoffset, lastbottom, color, duration, depthtest );
			}
			else
			{
				// close gap between two radius segments
				Line( position + upoffset, lasttop, color, duration, depthtest );
				Line( position + offset, lastcenter, color, duration, depthtest );
				Line( position + downoffset, lastbottom, color, duration, depthtest );

				// update last segment
				lasttop = position + upoffset;
				lastcenter = position + offset;
				lastbottom = position + downoffset;
			}

			// connect center ring to top and bottom ring
			Line( position + offset, position + upoffset, color, duration, depthtest );
			Line( position + offset, position + downoffset, color, duration, depthtest );

			// connect top and bottom ring to top and bottom of sphere
			Line( position + upoffset, top, color, duration, depthtest );
			Line( position + downoffset, bottom, color, duration, depthtest );
		}
	}

	private class DebugLine
	{
		public DebugVertex start;
		public DebugVertex end;
		public float duration;
		public bool depthtest;
		public TimeSince TimeSinceSpawned;

		public struct DebugVertex
		{
			public Vector3 position;
			public Color color;

			public DebugVertex( Vector3 position, Color color )
			{
				this.position = position;
				this.color = color;
			}
		}

		public DebugLine( Vector3 start, Vector3 end, Color color, float duration, bool depthtest )
		{
			this.start = new DebugVertex( start, color );
			this.end = new DebugVertex( end, color );
			this.duration = duration;
			this.depthtest = depthtest;

			TimeSinceSpawned = 0;
		}

		public override string ToString()
		{
			return $"start: {start} end: {end} duration: {duration} depth: {depthtest}";
		}
	}

	private static readonly IList<DebugLine> DebugLines = new List<DebugLine>();

	private static readonly Material Material = Material.Load( "materials/core/debugline.vanmat" );

	// depth
	private static int dvao, dvbo, debo;
	// no depth
	private static int nvao, nvbo, nebo;

	/// <summary>
	/// Init both no-depth and depth buffers
	/// </summary>
	public static void Init()
	{
		InitDepth();
		InitNoDepth();
	}

	/// <summary>
	/// Init the buffers used for depth debug lines
	/// </summary>
	private static void InitDepth()
	{
		Material.Use();

		GLUtil.CreateBuffer( "Debuglin depth VBO", out dvbo );
		GL.BindBuffer( BufferTarget.ArrayBuffer, dvbo );

		GLUtil.CreateVertexArray( "Debugline depth VAO", out dvao );
		GL.BindVertexArray( dvao );

		// vertex positions
		var vertexPositionLocation = Material.GetAttribLocation( "vPosition" );
		if ( vertexPositionLocation >= 0 )
		{
			GL.EnableVertexAttribArray( vertexPositionLocation );
			GL.VertexAttribPointer( vertexPositionLocation, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf( typeof( DebugLine.DebugVertex ) ), 0 );
		}

		// vertex color
		var vertexColorLocation = Material.GetAttribLocation( "vColor" );
		if ( vertexColorLocation >= 0 )
		{
			GL.EnableVertexAttribArray( vertexColorLocation );
			GL.VertexAttribPointer( vertexColorLocation, 4, VertexAttribPointerType.Float, false, Marshal.SizeOf( typeof( DebugLine.DebugVertex ) ), Marshal.OffsetOf( typeof( DebugLine.DebugVertex ), "color" ) );
		}

		GLUtil.CreateBuffer( "Debugline depth EBO", out debo );
		GL.BindBuffer( BufferTarget.ElementArrayBuffer, debo );
	}

	/// <summary>
	/// Init the buffers used for no-depth debug lines
	/// </summary>
	private static void InitNoDepth()
	{
		Material.Use();

		GLUtil.CreateBuffer( "Debuglin depth VBO", out nvbo );
		GL.BindBuffer( BufferTarget.ArrayBuffer, nvbo );

		GLUtil.CreateVertexArray( "Debugline depth VAO", out nvao );
		GL.BindVertexArray( nvao );

		// vertex positions
		var vertexPositionLocation = Material.GetAttribLocation( "vPosition" );
		if ( vertexPositionLocation >= 0 )
		{
			GL.EnableVertexAttribArray( vertexPositionLocation );
			GL.VertexAttribPointer( vertexPositionLocation, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf( typeof( DebugLine.DebugVertex ) ), 0 );
		}

		// vertex color
		var vertexColorLocation = Material.GetAttribLocation( "vColor" );
		if ( vertexColorLocation >= 0 )
		{
			GL.EnableVertexAttribArray( vertexColorLocation );
			GL.VertexAttribPointer( vertexColorLocation, 4, VertexAttribPointerType.Float, false, Marshal.SizeOf( typeof( DebugLine.DebugVertex ) ), Marshal.OffsetOf( typeof( DebugLine.DebugVertex ), "color" ) );
		}

		GLUtil.CreateBuffer( "Debugline depth EBO", out nebo );
		GL.BindBuffer( BufferTarget.ElementArrayBuffer, nebo );
	}

	/// <summary>
	/// Draw all debug lines with depth enabled
	/// </summary>
	public static void DrawDepthLines()
	{
		Material.Use();

		GL.BindVertexArray( dvao );

		// update vertex positions
		GL.BindBuffer( BufferTarget.ArrayBuffer, dvbo );
		GL.BufferData( BufferTarget.ArrayBuffer, DepthLineVertices.Count * Marshal.SizeOf( typeof( DebugLine.DebugVertex ) ), DepthLineVertices.ToArray(), BufferUsageHint.StaticDraw );

		var indices = new uint[DepthLineVertices.Count * 2];
		for ( uint i = 0; i < DepthLineVertices.Count * 2; i++ )
		{
			indices[i] = i;
		}

		GL.BindBuffer( BufferTarget.ElementArrayBuffer, debo );
		GL.BufferData( BufferTarget.ElementArrayBuffer, indices.Length * sizeof( uint ), indices, BufferUsageHint.StaticDraw );

		GL.DrawElements( PrimitiveType.Lines, indices.Length, DrawElementsType.UnsignedInt, 0 );
	}

	/// <summary>
	/// Draw all debug lines with depth disabled
	/// </summary>
	public static void DrawNoDepthLines()
	{
		GL.Disable( EnableCap.DepthTest );
		Material.Use();

		GL.BindVertexArray( nvao );

		// update vertex positions
		GL.BindBuffer( BufferTarget.ArrayBuffer, nvbo );
		GL.BufferData( BufferTarget.ArrayBuffer, NoDepthLineVertices.Count * Marshal.SizeOf( typeof( DebugLine.DebugVertex ) ), NoDepthLineVertices.ToArray(), BufferUsageHint.StaticDraw );

		var indices = new uint[NoDepthLineVertices.Count * 2];
		for ( uint i = 0; i < NoDepthLineVertices.Count * 2; i++ )
		{
			indices[i] = i;
		}

		GL.BindBuffer( BufferTarget.ElementArrayBuffer, nebo );
		GL.BufferData( BufferTarget.ElementArrayBuffer, indices.Length * sizeof( uint ), indices, BufferUsageHint.StaticDraw );

		GL.DrawElements( PrimitiveType.Lines, indices.Length, DrawElementsType.UnsignedInt, 0 );
		GL.Enable( EnableCap.DepthTest );
	}

	private static readonly IList<DebugLine.DebugVertex> DepthLineVertices = new List<DebugLine.DebugVertex>();
	private static readonly IList<DebugLine.DebugVertex> NoDepthLineVertices = new List<DebugLine.DebugVertex>();

	/// <summary>
	/// Prepare all currently alive debug lines for drawing. Sorts them into their respective lists for buffer population, depending on their depthtest variable.
	/// </summary>
	public static void PrepareDraw()
	{
		// clear prev line vertices
		DepthLineVertices.Clear();
		NoDepthLineVertices.Clear();

		foreach ( var line in DebugLines.Reverse() )
		{
			// sort lines into respective buffers, one for depth testing, one for no depth testing
			if ( line.depthtest )
			{
				DepthLineVertices.Add( line.start );
				DepthLineVertices.Add( line.end );
			}
			else
			{
				NoDepthLineVertices.Add( line.start );
				NoDepthLineVertices.Add( line.end );
			}
		}

		// remove expired lines
		(DebugLines as List<DebugLine>)?.RemoveAll( x => x.TimeSinceSpawned >= x.duration );
	}
}
