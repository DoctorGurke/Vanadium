#version 400 core

#include shaders/common/common.frag

#material sampler2D tex

void main()
{
    //vec3 col = tex2D(tex, fTexCoord0).rgb;
    //float mask = tex2D(tintMask, fTexCoord0).r;
    //gl_Color = vec4(mix(col, tintColor, tintAmount * mask), 1.0);
    gl_Color = vec4(fNormal, 1.0);
}