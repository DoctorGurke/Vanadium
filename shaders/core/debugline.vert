#version 400 core

#include shaders/common/common.vert

void main(void)
{
	gl_Position = vec4(vPosition, 1.0) * g_matWorldToView * g_matWorldToProjection;
}