﻿#include shaders/common/common.glsl

in vec3 vPosition;
in vec3 vNormal;
in vec3 vTangent;
in vec3 vBitangent;
in vec2 vTexCoord0;
in vec2 vTexCoord1;
in vec2 vTexCoord2;
in vec2 vTexCoord3;
in vec3 vColor;

out VS_OUT {
	vec3 vPositionWs;
	vec3 vNormalWs;
	vec3 vTangentWs;
	vec3 vBitangentWs;
	vec2 vTexCoord0;
	vec2 vTexCoord1;
	vec2 vTexCoord2;
	vec2 vTexCoord3;
	vec3 vVertexColor;
} vs_out;

uniform mat4 transform;

vec4 CommonVertexProcessing(void) {
	vs_out.vPositionWs = vec3(vec4(vPosition, 1.0) * transform);
	vs_out.vNormalWs = vNormal * mat3(transpose(inverse(transform)));
	vs_out.vBitangentWs = vBitangent;
	vs_out.vTangentWs = vTangent;
	vs_out.vTexCoord0 = vTexCoord0;
	vs_out.vTexCoord1 = vTexCoord1;
	vs_out.vTexCoord2 = vTexCoord2;
	vs_out.vTexCoord3 = vTexCoord3;
	vs_out.vVertexColor = vColor;

    return vec4(vPosition, 1.0) * transform * g_matWorldToView * g_matWorldToProjection;
}