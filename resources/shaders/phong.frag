#version 410 core

#material samplerCube envmap

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

    vec3 dir = normalize(fs_in.vPositionWs - g_vCameraPositionWs);
    vec3 reflect = reflect(dir, fs_in.vNormalWs);
    vec3 env = texture(envmap, reflect).rgb;

    // tint envmap by color of the material
    env *= col.rgb;

    col.rgb = mix(col.rgb, env, spec);

    col.rgb = CommonPhongLighting(col.rgb, fs_in.vNormalWs, fs_in.vPositionWs, viewDir, col.rgb, spec, gloss);
    col = GammaCorrect(col, 2.2);

    gl_Color = col;
}

