#version 400 core

#include shaders/common/common.frag

#material sampler2D tex

void main()
{
    vec4 col = tex2D(tex, fs_in.vTexCoord0).rgba;
    if(col.a <= 0.0)
        discard;
    gl_Color = col;
}