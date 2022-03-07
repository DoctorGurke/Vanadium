#version 400 core

#include shaders/common/common.frag

#material sampler2D tex

void main()
{
    vec4 col = tex2D(tex, fTexCoord0).rgba;
    //float mask = tex2D(tintMask, fTexCoord0).r;
    //col *= tintColor * (1 - mask);
    if(col.a <= 0.0)
        discard;
    gl_Color = col;
}