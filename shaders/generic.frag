#version 400 core

#include shaders/common/common.frag

#material sampler2D tex

void main()
{
    vec3 col = tex2D(tex, fTexCoord0).rgb;
    //float mask = tex2D(tintMask, fTexCoord0).r;
    //col *= tintColor * (1 - mask);
    gl_Color = vec4(col, 1.0);
}