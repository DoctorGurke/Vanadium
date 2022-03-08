﻿#include shaders/common/common.glsl

in VS_OUT {
	vec3 vPositionWs;
	vec3 vNormalWs;
	vec3 vTangentWs;
	vec3 vBitangentWs;
	vec2 vTexCoord0;
	vec2 vTexCoord1;
	vec2 vTexCoord2;
	vec2 vTexCoord3;
	vec3 vVertexColor;
} fs_in;

out vec4 gl_Color;