#include shaders/common/common.glsl

in VS_OUT {
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
} fs_in;

uniform vec4 renderColor;
uniform float tintAmount;

out vec4 vColor;

vec3 GammaCorrect(vec3 col, float gamma) {
	return pow(col.rgb, vec3(1.0 / gamma));
}