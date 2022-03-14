﻿#version 400 core

out vec3 CubeCoords;

#include shaders/common/common.vert

void main()
{
    CubeCoords = vPosition.xyz;
    vec4 pos =  vec4(vPosition.xyz, 1.0) * mat4(mat3(g_matWorldToView)) * g_matWorldToProjection;
    gl_Position = pos.xyww;
}  