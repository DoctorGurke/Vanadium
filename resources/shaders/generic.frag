#version 400 core

#include shaders/common/common.frag

#material sampler2D tex

void main()
{
    vec4 base = tex2D(tex, fs_in.vTexCoord0).rgba;
    vec4 tint = base * renderColor;

    vec4 col = mix(base, tint, tintAmount);
    if(col.a <= 0.0)
        discard;
    gl_Color = col;
}