#version 400 core

#include shaders/common/phong_light.frag

void main() {
    vec2 uv = fs_in.vTexCoord0;
    vec4 base = tex2D(diffuse, uv).rgba;
    vec4 tint = base * renderColor;

    vec4 col = mix(base, tint, tintAmount);
    vec3 spec = tex2D(specular, uv).rgb;

    if(col.a <= 0.0)
        discard;

    vec3 viewDir = normalize(g_vCameraPositionWs - fs_in.vPositionWs);

    col.rgb = CommonPhongLighting(col.rgb, fs_in.vNormalWs, fs_in.vPositionWs, viewDir, col.rgb, spec, gloss);

    gl_Color = col;
}

