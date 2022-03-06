#include shaders/common/common.glsl

in vec3 fPosition;
in vec3 fNormal;
in vec2 fTexCoord0;
in vec3 fVertColor;

out vec4 gl_Color;

#material vec3 tintColor;
#material float tintAmount;
#material sampler2D tintMask;