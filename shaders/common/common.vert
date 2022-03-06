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

out vec3 fPosition;
out vec3 fNormal;
out vec3 fTangent;
out vec3 fBitangent;
out vec2 fTexCoord0;
out vec2 fTexCoord1;
out vec2 fTexCoord2;
out vec2 fTexCoord3;
out vec3 fVertColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

vec4 CommonVertexProcessing(void) {
	fPosition = vec3(vec4(vPosition, 1.0) * model);
	fNormal = vNormal * mat3(transpose(inverse(model)));
	fTangent = vTangent;
	fTexCoord0 = vTexCoord0;
	fTexCoord1 = vTexCoord1;
	fTexCoord2 = vTexCoord2;
	fTexCoord3 = vTexCoord3;
	fVertColor = vColor;

    return vec4(vPosition, 1.0) * model * view * projection;
}