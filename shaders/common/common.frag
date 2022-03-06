#include shaders/common/common.glsl

in vec3 fPosition;
in vec3 fNormal;
in vec3 fTangent;
in vec3 fBitangent;
in vec2 fTexCoord0;
in vec2 fTexCoord1;
in vec2 fTexCoord2;
in vec2 fTexCoord3;
in vec3 fVertColor;

out vec4 gl_Color;

#material vec3 tintColor
#material float tintAmount
#material sampler2D tintMask