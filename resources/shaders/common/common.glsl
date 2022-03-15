layout (std140) uniform PerViewUniformBuffer {
    layout(row_major) mat4 g_matWorldToProjection;
    layout(row_major) mat4 g_matWorldToView;
    vec3 g_vCameraPositionWs;
    vec3 g_vCameraDirWs;
    vec3 g_vCameraUpDirWs;
    vec2 g_vViewportSize;
    float g_flTime;
    float g_flNearPlane;
    float g_flFarPlane;
};

struct pointlight {
    vec4 Position;
    vec4 Color;
    vec4 Attenuation;
};

struct spotlight {
    vec4 Position;
    vec4 Direction;
    vec4 Color;
    vec4 Attenuation;
    vec4 Params; // inner angle, out angle
};

#define MAX_POINT_LIGHTS 256
#define MAX_SPOT_LIGHTS 256

layout (std140) uniform PerViewLightingUniformBuffer {
    vec4 g_vAmbientLightingColor;
    int g_nNumPointlights;
    int g_nNumSpotlights;
    pointlight[MAX_POINT_LIGHTS] g_Pointlights;
    spotlight[MAX_SPOT_LIGHTS] g_Spotlights;
};

vec4 tex2D(sampler2D tex, vec2 uv) {
    return textureLod(tex, uv, textureQueryLod(tex, uv).x);
}