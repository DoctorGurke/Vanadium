#version 400 core

#include shaders/common/common.frag

void main()
{
    float sin = ((sin(g_flTime * 10) + 1.0) / 4.0) + 0.5;
    gl_Color = vec4(sin, 0.0, 0.0, 1.0);
}