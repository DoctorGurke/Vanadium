layout (std140) uniform SceneUniformBuffer {
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
    float Constant;
    vec3 Color;
    float Linear;
    float Quadratic;
    float Brightness;
};

struct SpotLight {
    vec3 Position;
    float Constant;
    vec3 Direction;
    float Linear;
    vec3 Color;
    float Quadratic;
    float Brightness;
    float InnerAngle;
    float OuterAngle;
};

struct DirLight {
    vec3 Direction;
    float Brightness;
    vec3 Color;
};

#define MAX_POINT_LIGHTS 128
#define MAX_SPOT_LIGHTS 128
#define MAX_DIR_LIGHTS 16

#include shaders/common/material.glsl

layout (std140) uniform SceneLightingUniformBuffer {
    vec4 g_vAmbientLightingColor;
    int g_nNumPointlights;
    int g_nNumSpotlights;
    int g_nNumDirlights;
    PointLight[MAX_POINT_LIGHTS] g_PointLights;
    SpotLight[MAX_SPOT_LIGHTS] g_SpotLights;
    DirLight[MAX_DIR_LIGHTS] g_DirLights;
};