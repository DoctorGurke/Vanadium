layout (std140) uniform PerViewUniformBuffer {
    layout(row_major) mat4 g_matWorldToProjection;
    layout(row_major) mat4 g_matWorldToView;
    vec3 g_vCameraPositionWs;
    vec3 g_vCameraDirWs;
    float g_flTime;
};

vec4 tex2D(sampler2D tex, vec2 uv) {
    return textureLod(tex, uv, textureQueryLod(tex, uv).x);
}