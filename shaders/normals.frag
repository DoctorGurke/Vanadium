﻿#version 400 core

#include shaders/common/common.frag

void main()
{
    gl_Color = vec4(fs_in.vNormalWs, 1.0);
}