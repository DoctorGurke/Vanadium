#include shaders/common/common.glsl

layout(location = 0)in vec3 vPosition;
layout(location = 1)in vec3 vNormal;
layout(location = 2)in vec3 vTangent;
layout(location = 3)in vec3 vBitangent;
layout(location = 4)in vec2 vTexCoord0;
layout(location = 5)in vec2 vTexCoord1;
layout(location = 6)in vec2 vTexCoord2;
layout(location = 7)in vec2 vTexCoord3;
layout(location = 8)in vec3 vColor;

out VS_OUT {
	vec3 vPositionWs;
	vec3 vTangentWs;
	vec3 vBitangentWs;
	vec3 vNormalWs;
	mat3 mTBN;
	vec2 vTexCoord0;
	vec2 vTexCoord1;
	vec2 vTexCoord2;
	vec2 vTexCoord3;
	vec3 vVertexColor;
} vs_out;

uniform mat4 transform;

vec4 CommonVertexProcessing(void) {
	vs_out.vPositionWs = vec3(vec4(vPosition, 1.0) * transform);
	vs_out.vTangentWs = vTangent;
	vs_out.vBitangentWs = vBitangent;
	vs_out.vNormalWs = mat3(inverse(transform)) * vNormal;
	vs_out.mTBN = mat3(vs_out.vTangentWs, vs_out.vBitangentWs, vs_out.vNormalWs);
	vs_out.vTexCoord0 = vTexCoord0;
	vs_out.vTexCoord1 = vTexCoord1;
	vs_out.vTexCoord2 = vTexCoord2;
	vs_out.vTexCoord3 = vTexCoord3;
	vs_out.vVertexColor = vColor;

    return vec4(vPosition, 1.0) * transform * g_matWorldToView * g_matWorldToProjection;
}