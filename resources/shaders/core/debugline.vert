#version 400 core

#include shaders/common/common.glsl

in vec3 vPosition;
in vec4 vColor;

out VS_OUT {
	vec3 vPositionWs;
	vec4 vVertexColor;
} vs_out;

void main(void)
{
	vs_out.vPositionWs = vPosition;
	vs_out.vVertexColor = vColor;
	gl_Position = vec4(vPosition, 1.0) * g_matWorldToView * g_matWorldToProjection;
}