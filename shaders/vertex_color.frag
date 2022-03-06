#version 400 core

#include shaders/common/common.frag

void main()
{
    gl_Color = vec4(fVertColor, 1.0);
}