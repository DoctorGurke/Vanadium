#version 330 core

#include shaders/common/common.frag

uniform float tintAmount;

void main()
{
    vec4 col = texture(texture0, fTexCoord0);
    float tintmask = texture(texture1, fTexCoord0).r;
    gl_Color = vec4(mix(col.rgb, vec3(1.0, 0.0, 0.0), tintAmount * tintmask), 1.0);
}