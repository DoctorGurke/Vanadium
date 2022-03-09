layout (std140) uniform PerViewUniformBuffer {
    layout(row_major) mat4 g_matWorldToProjection;  // 64
    layout(row_major) mat4 g_matWorldToView;        // 64
    vec3 g_vCameraPositionWs;                       // 12
    vec3 g_vCameraDirWs;                            // 12
    float g_flTime;                                 // 4
};

vec4 tex2D(sampler2D tex, vec2 uv) {
    return textureLod(tex, uv, textureQueryLod(tex, uv).x);
}