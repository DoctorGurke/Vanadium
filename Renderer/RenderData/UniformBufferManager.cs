﻿using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;

namespace Vanadium.Renderer.RenderData;

public class UniformBufferManager
{
	public int PerViewUniformBufferHandle;
	public int PerViewLightingUniformBufferHandle;
	public static UniformBufferManager? Current { get; private set; }

	public UniformBufferManager()
	{
		Current = this;
	}

	public void Init()
	{
		// setup per view uniform buffer
		GLUtil.CreateBuffer( "PerViewUniformBuffer", out PerViewUniformBufferHandle );
		GL.BindBuffer( BufferTarget.UniformBuffer, PerViewUniformBufferHandle );
		var perviewbuffersize = Marshal.SizeOf( typeof( PerViewUniformBuffer ) ).RoundUpToMultipleOf( 16 );
		GL.BufferData( BufferTarget.UniformBuffer, perviewbuffersize, IntPtr.Zero, BufferUsageHint.StaticDraw );
		GL.BindBuffer( BufferTarget.UniformBuffer, 0 );
		GL.BindBufferRange( BufferRangeTarget.UniformBuffer, 0, PerViewUniformBufferHandle, IntPtr.Zero, perviewbuffersize );

		// setup per view lighting uniform buffer
		GLUtil.CreateBuffer( "PerViewLightingUniformBuffer", out PerViewLightingUniformBufferHandle );
		GL.BindBuffer( BufferTarget.UniformBuffer, PerViewLightingUniformBufferHandle );
		var perviewlightingbuffersize = 0;
		perviewlightingbuffersize += Marshal.SizeOf( typeof( OpenTKMath.Vector4 ) );
		perviewlightingbuffersize += sizeof( int ) * 4;
		perviewlightingbuffersize += Marshal.SizeOf( typeof( SceneLightManager.PointLight ) ) * SceneLightManager.MaxPointLights;
		perviewlightingbuffersize += Marshal.SizeOf( typeof( SceneLightManager.SpotLight ) ) * SceneLightManager.MaxSpotLights;
		perviewlightingbuffersize += Marshal.SizeOf( typeof( SceneLightManager.DirLight ) ) * SceneLightManager.MaxDirLights;
		perviewlightingbuffersize = perviewlightingbuffersize.RoundUpToMultipleOf( 16 );
		GL.BufferData( BufferTarget.UniformBuffer, perviewlightingbuffersize, IntPtr.Zero, BufferUsageHint.StaticDraw );
		GL.BindBuffer( BufferTarget.UniformBuffer, 1 );
		GL.BindBufferRange( BufferRangeTarget.UniformBuffer, 1, PerViewLightingUniformBufferHandle, IntPtr.Zero, perviewlightingbuffersize );
	}

	public void UpdatePerViewUniformBuffer()
	{
		// update per view uniform buffer
		GL.BindBuffer( BufferTarget.UniformBuffer, PerViewUniformBufferHandle );
		// prepare data
		var perviewuniformbuffer = new PerViewUniformBuffer
		{
			g_matWorldToProjection = Camera.ActiveCamera?.ProjectionMatrix ?? OpenTKMath.Matrix4.CreatePerspectiveFieldOfView( 0, 0, 0, 0 ),
			g_matWorldToView = Camera.ActiveCamera?.ViewMatrix ?? OpenTKMath.Matrix4.LookAt( new Vector3(), new Vector3(), new Vector3() ),
			g_vCameraPositionWs = Camera.ActiveCamera?.Position ?? new Vector3(),
			g_vCameraDirWs = Camera.ActiveCamera?.Rotation.Forward ?? new Vector3(),
			g_vCameraUpDirWs = Camera.ActiveCamera?.Rotation.Up ?? new Vector3(),
			g_flTime = Time.Now,
			g_flNearPlane = Camera.ActiveCamera?.ZNear ?? 0.0f,
			g_flFarPlane = Camera.ActiveCamera?.ZFar ?? 0.0f,
			g_vViewportSize = Screen.Size,
			g_flGamma = DebugOverlay.Gamma
		};
		// put data in buffer
		GL.BufferData( BufferTarget.UniformBuffer, Marshal.SizeOf( perviewuniformbuffer ).RoundUpToMultipleOf( 16 ), ref perviewuniformbuffer, BufferUsageHint.StaticDraw );
	}

	public void UpdateAmbientLightColor( Color col )
	{
		// update light uniform buffer
		GL.BindBuffer( BufferTarget.UniformBuffer, PerViewLightingUniformBufferHandle );

		GL.BufferSubData( BufferTarget.UniformBuffer, IntPtr.Zero, Marshal.SizeOf( typeof( OpenTKMath.Vector4 ) ), ref col );
	}

	public void UpdatePointlights( SceneLightManager.PointLight[] lights, int num )
	{
		// update light uniform buffer
		GL.BindBuffer( BufferTarget.UniformBuffer, PerViewLightingUniformBufferHandle );
		
		// point lights num
		GL.BufferSubData( BufferTarget.UniformBuffer, (IntPtr)Marshal.SizeOf( typeof( OpenTKMath.Vector4 ) ), sizeof( int ), ref num );
		// light array data
		GL.BufferSubData( BufferTarget.UniformBuffer, (IntPtr)(Marshal.SizeOf( typeof( OpenTKMath.Vector4 ) ) + sizeof( int ) * 4), Marshal.SizeOf( typeof( SceneLightManager.PointLight ) ) * SceneLightManager.MaxPointLights, lights );
	}

	public void UpdateSpotlights( SceneLightManager.SpotLight[] lights, int num )
	{
		// update light uniform buffer
		GL.BindBuffer( BufferTarget.UniformBuffer, PerViewLightingUniformBufferHandle );

		// spot lights num
		GL.BufferSubData( BufferTarget.UniformBuffer, (IntPtr)Marshal.SizeOf( typeof( OpenTKMath.Vector4 ) ) + sizeof( int ), sizeof( int ), ref num );
		
		var offset = 0;
		offset += Marshal.SizeOf( typeof( OpenTKMath.Vector4 ) );
		offset += sizeof( int ) * 4; // number of lights (+ pad)
		offset += Marshal.SizeOf( typeof( SceneLightManager.PointLight ) ) * SceneLightManager.MaxPointLights;
		var size = Marshal.SizeOf( typeof( SceneLightManager.SpotLight ) ) * SceneLightManager.MaxSpotLights;
		// lights array data
		GL.BufferSubData( BufferTarget.UniformBuffer, (IntPtr)offset, size, lights );
	}

	public void UpdateDirlights( SceneLightManager.DirLight[] lights, int num )
	{
		// update light uniform buffer
		GL.BindBuffer( BufferTarget.UniformBuffer, PerViewLightingUniformBufferHandle );

		// dir lights num
		GL.BufferSubData( BufferTarget.UniformBuffer, (IntPtr)Marshal.SizeOf( typeof( OpenTKMath.Vector4 ) ) + sizeof( int ) * 2, sizeof( int ), ref num );
		
		var offset = 0;
		offset += Marshal.SizeOf( typeof( OpenTKMath.Vector4 ) );
		offset += sizeof( int ) * 4; // number of lights (+ pad)
		offset += Marshal.SizeOf( typeof( SceneLightManager.PointLight ) ) * SceneLightManager.MaxPointLights;
		offset += Marshal.SizeOf( typeof( SceneLightManager.SpotLight ) ) * SceneLightManager.MaxSpotLights;
		var size = Marshal.SizeOf( typeof( SceneLightManager.DirLight ) ) * SceneLightManager.MaxDirLights;
		// lights array data
		GL.BufferSubData( BufferTarget.UniformBuffer, (IntPtr)offset, size, lights );
	}

	public struct PerViewUniformBuffer
	{
		public OpenTKMath.Matrix4 g_matWorldToProjection;  // 16	(col0)
												// 16	(col1)
												// 16	(col2)
												// 16	(col3)
		public OpenTKMath.Matrix4 g_matWorldToView;        // 16	(col0)
												// 16	(col1)
												// 16	(col2)
												// 16	(col3)
		public Vector3 g_vCameraPositionWs;     // 12	
		public float pad1;                      // 4	(padding)
		public Vector3 g_vCameraDirWs;          // 12
		public float pad2;                      // 4	(padding)
		public Vector3 g_vCameraUpDirWs;        // 12
		public float pad3;                      // 4	(padding)
		public OpenTKMath.Vector2 g_vViewportSize;         // 8
		public float g_flTime;                  // 4
		public float g_flNearPlane;             // 4
		public float g_flFarPlane;              // 4
		public float g_flGamma;                 // 4
	}
}