#version 400 core

#include shaders/common/common.frag

#material vec3 color

void main()
{
    gl_Color = vec4(color, 1.0);
}