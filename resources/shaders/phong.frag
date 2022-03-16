﻿#version 410 core

#material samplerCube envmap

#include shaders/common/phong_light.frag

void main() {
    vec2 uv = fs_in.vTexCoord0;
    
    // grab diffuse and alpha
    vec4 base = tex2D(diffuse, uv).rgba;
    vec3 diff = base.rgb;
    float alpha = base.a;

    // don't bother with invisible fragments
    if(alpha <= 0.0)
        discard;

    // tint diffuse
    vec3 tint = diff * renderColor.rgb;
    diff = mix(diff, tint, tintAmount);
    
    // grab specular
    vec3 spec = tex2D(specular, uv).rgb;

    // get ws normals from normal map
    vec3 normal = tex2D(normal, uv).rgb;
    normal = normal * 2.0 - 1.0;
    normal = normalize(fs_in.mTBN * normal);

    // view dir to fragment
    vec3 viewDir = normalize(g_vCameraPositionWs - fs_in.vPositionWs);

    // env map sampling
    vec3 reflect = reflect(-viewDir, normal);
    vec3 env = texture(envmap, reflect).rgb;

    // tint envmap by diffuse of the material
    env *= diff;

    // apply cubemap according to specular of the material
    vec3 col = mix(diff, env, spec);

    // get lighting results
    col = CommonPhongLighting(col, normal, fs_in.vPositionWs, viewDir, diff, spec, gloss);

    // gamma correct
    col = GammaCorrect(col, 2.2);

    gl_Color = vec4(col, alpha);
}

