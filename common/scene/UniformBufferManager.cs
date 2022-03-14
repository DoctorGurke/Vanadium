﻿using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace Vanadium;

public class UniformBufferManager
{
	public int PerViewUniformBufferHandle;
	public int PerViewLightingUniformBufferHandle;
	public static UniformBufferManager Current;

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
		var perviewlightingbuffersize = (Marshal.SizeOf( typeof( Vector4 ) ) + sizeof( int ) + Marshal.SizeOf( typeof( Light ) ) * SceneLightManager.MaxPointLights).RoundUpToMultipleOf( 16 );
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
			g_matWorldToProjection = Camera.ActiveCamera.ProjectionMatrix,
			g_matWorldToView = Camera.ActiveCamera.ViewMatrix,
			g_vCameraPositionWs = Camera.ActiveCamera.Position,
			g_vCameraDirWs = Camera.ActiveCamera.Rotation.Forward,
			g_vCameraUpDirWs = Camera.ActiveCamera.Rotation.Up,
			g_flTime = Time.Now,
			g_flNearPlane = Camera.ActiveCamera.ZNear,
			g_flFarPlane = Camera.ActiveCamera.ZFar,
			g_vViewportSize = Screen.Size
		};
		// put data in buffer
		GL.BufferData( BufferTarget.UniformBuffer, Marshal.SizeOf( perviewuniformbuffer ).RoundUpToMultipleOf( 16 ), ref perviewuniformbuffer, BufferUsageHint.StaticDraw );
	}

	public void UpdateAmbientLightColor( Color col )
	{
		// update light uniform buffer
		GL.BindBuffer( BufferTarget.UniformBuffer, PerViewLightingUniformBufferHandle );

		GL.BufferSubData( BufferTarget.UniformBuffer, IntPtr.Zero, Marshal.SizeOf( typeof( Vector4 ) ), ref col );
	}

	public void UpdatePointlights( Light[] lights, int num )
	{
		// update light uniform buffer
		GL.BindBuffer( BufferTarget.UniformBuffer, PerViewLightingUniformBufferHandle );

		GL.BufferSubData( BufferTarget.UniformBuffer, (IntPtr)Marshal.SizeOf( typeof( Vector4 ) ), sizeof( int ), ref num );
		GL.BufferSubData( BufferTarget.UniformBuffer, (IntPtr)(Marshal.SizeOf( typeof( Vector4 ) ) + sizeof( int ) * 4), Marshal.SizeOf( typeof( Light ) ) * SceneLightManager.MaxPointLights, lights );
	}

	public struct PerViewUniformBuffer
	{
		public Matrix4 g_matWorldToProjection;  // 16	(col0)
												// 16	(col1)
												// 16	(col2)
												// 16	(col3)
		public Matrix4 g_matWorldToView;        // 16	(col0)
												// 16	(col1)
												// 16	(col2)
												// 16	(col3)
		public Vector3 g_vCameraPositionWs;     // 12	
		public float pad1;                      // 4	(padding)
		public Vector3 g_vCameraDirWs;          // 12
		public float pad2;                      // 4	(padding)
		public Vector3 g_vCameraUpDirWs;        // 12
		public float pad3;                      // 4	(padding)
		public Vector2 g_vViewportSize;         // 8
		public float g_flTime;                  // 4
		public float g_flNearPlane;             // 4
		public float g_flFarPlane;              // 4
	}

	public struct Light
	{
		public Vector4 Position;
		public Vector4 Color;
		public Vector4 Params;
	}
}