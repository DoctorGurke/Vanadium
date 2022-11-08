using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Vanadium.RenderSystem.RenderData;

public class FrameBuffer
{
	private uint fbo;

	public FrameBuffer( Vector2i size, bool srgb = true )
	{
		GL.CreateFramebuffers( 1, out fbo );
	}

	public void Bind()
	{
		GL.BindFramebuffer( FramebufferTarget.Framebuffer, fbo );
		if ( GL.CheckFramebufferStatus( FramebufferTarget.Framebuffer ) != FramebufferErrorCode.FramebufferComplete )
		{
			throw new InvalidOperationException( $"Trying to bind incomplete framebuffer!" );
		}
	}
}
