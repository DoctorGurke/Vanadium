#version 400 core

out vec3 CubeCoords;

#include shaders/common/common.vert

void main()
{
    CubeCoords = vPosition.xyz;
    vec4 pos = projection * mat4(mat3(view)) * vec4(vPosition.xyz, 1.0);
    gl_Position = pos.xyww;
}  