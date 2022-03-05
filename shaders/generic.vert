#version 330 core

#include shaders/common/common.vert

void main(void)
{
	vec4 pos = CommonVertexProcessing();
	gl_Position = pos;
}