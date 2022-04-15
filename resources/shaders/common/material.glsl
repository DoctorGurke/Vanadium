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