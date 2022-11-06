using OpenTK.Graphics.OpenGL4;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Vanadium.RenderSystem.Util;

static class GLUtil
{

	[Conditional( "DEBUG" )]
	public static void CheckGLError( string title )
	{
		var error = GL.GetError();
		if ( error != ErrorCode.NoError )
		{
			Debug.Print( $"{title}: {error}" );
		}
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void LabelObject( ObjectLabelIdentifier objLabelIdent, int glObject, string name )
	{
		GL.ObjectLabel( objLabelIdent, glObject, name.Length, name );
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void CreateTexture( TextureTarget target, string Name, out int Texture )
	{
		GL.CreateTextures( target, 1, out Texture );
		LabelObject( ObjectLabelIdentifier.Texture, Texture, $"Texture2D: {Name}" );
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void CreateTextureCube( TextureTarget target, string Name, out int Texture )
	{
		GL.CreateTextures( target, 1, out Texture );
		LabelObject( ObjectLabelIdentifier.Texture, Texture, $"TextureCube: {Name}" );
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void CreateProgram( string Name, out int Program )
	{
		Program = GL.CreateProgram();
		LabelObject( ObjectLabelIdentifier.Program, Program, $"Program: {Name}" );
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void CreateShader( ShaderType type, string typename, string Name, out int Shader )
	{
		Shader = GL.CreateShader( type );
		LabelObject( ObjectLabelIdentifier.Shader, Shader, $"Shader: {typename}: {Name}" );
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void CreateBuffer( string Name, out int Buffer )
	{
		GL.CreateBuffers( 1, out Buffer );
		LabelObject( ObjectLabelIdentifier.Buffer, Buffer, $"Buffer: {Name}" );
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void CreateVertexBuffer( string Name, out int Buffer ) => CreateBuffer( $"VBO: {Name}", out Buffer );

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void CreateElementBuffer( string Name, out int Buffer ) => CreateBuffer( $"EBO: {Name}", out Buffer );

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void CreateVertexArray( string Name, out int VAO )
	{
		GL.CreateVertexArrays( 1, out VAO );
		LabelObject( ObjectLabelIdentifier.VertexArray, VAO, $"VAO: {Name}" );
	}
}
