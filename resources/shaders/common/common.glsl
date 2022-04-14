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
    vec4 Position;
    vec4 Color;
    vec4 Attenuation;
};

struct SpotLight {
    vec4 Position;
    vec4 Direction;
    vec4 Color;
    vec4 Attenuation;
    vec4 Params; // inner angle, out angle
};

struct DirLight {
    vec4 Direction;
    vec4 Color;
};

#define MAX_POINT_LIGHTS 128
#define MAX_SPOT_LIGHTS 64
#define MAX_DIR_LIGHTS 16

layout (std140) uniform PerViewLightingUniformBuffer {
    vec4 g_vAmbientLightingColor;
    int g_nNumPointlights;
    int g_nNumSpotlights;
    int g_nNumDirlights;
    PointLight[MAX_POINT_LIGHTS] g_PointLights;
    SpotLight[MAX_SPOT_LIGHTS] g_SpotLights;
    DirLight[MAX_DIR_LIGHTS] g_DirLights;
};

struct Material {
    vec3 Albedo;
    float Opacity;
    vec3 Normal;
    float Roughness;
    float Metallic;
    float AmbientOcclusion;
};

struct BasicMaterial {
    vec3 Diffuse;
    float Opacity;
    vec3 Normal;
    float Specular;
    float Gloss;
};