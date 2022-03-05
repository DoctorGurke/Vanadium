#version 330 core

#include shaders/common/common.vert

#material float asd

void main(void)
{
	vec4 pos = CommonVertexProcessing();
	gl_Position = pos;
}