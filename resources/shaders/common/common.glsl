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
    float g_flGamma;
};

struct PointLight {
    vec3 Position;
    vec4 Color;
    vec3 Attenuation;
};

struct SpotLight {
    vec3 Position;
    vec3 Direction;
    vec4 Color;
    vec3 Attenuation;
    vec2 Params; // inner angle, out angle
};

struct DirLight {
    vec3 Direction;
    vec4 Color;
};

#define MAX_POINT_LIGHTS 128
#define MAX_SPOT_LIGHTS 64
#define MAX_DIR_LIGHTS 16

#include shaders/common/material.glsl

layout (std140) uniform PerViewLightingUniformBuffer {
    vec4 g_vAmbientLightingColor;
    int g_nNumPointlights;
    int g_nNumSpotlights;
    int g_nNumDirlights;
    PointLight[MAX_POINT_LIGHTS] g_PointLights;
    SpotLight[MAX_SPOT_LIGHTS] g_SpotLights;
    DirLight[MAX_DIR_LIGHTS] g_DirLights;
};