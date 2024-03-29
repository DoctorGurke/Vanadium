﻿#include shaders/common/common.frag

#material sampler2D albedo
#material sampler2D normal
#material sampler2D roughness
#material sampler2D metallic
#material sampler2D ao

const float PI = 3.14159265359;

vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}

float DistributionGGX(vec3 N, vec3 H, float roughness)
{
    float a      = roughness * roughness;
    float a2     = a * a;
    float NdotH  = max(dot(N, H), 0.0);
    float NdotH2 = NdotH * NdotH;
	
    float num   = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;
	
    return num / denom;
}

float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r * r) / 8.0;

    float num   = NdotV;
    float denom = NdotV * (1.0 - k) + k;
	
    return num / denom;
}

float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2  = GeometrySchlickGGX(NdotV, roughness);
    float ggx1  = GeometrySchlickGGX(NdotL, roughness);
	
    return ggx1 * ggx2;
}

vec3 Reflectance(Material mat) 
{
    vec3 F0 = vec3(0.04);
    F0 = mix(F0, mat.Albedo, mat.Metallic);
    return F0;
}

vec3 CalcLight(vec3 radiance, Material material, vec3 viewDir, vec3 F0, vec3 L, vec3 H) 
{
    // cook-torrance brdf
    float NDF = DistributionGGX(material.Normal, H, material.Roughness);        
    float G   = GeometrySmith(material.Normal, viewDir, L, material.Roughness);      
    vec3 F    = fresnelSchlick(max(dot(H, viewDir), 0.0), F0);       
        
    vec3 kS = F;
    vec3 kD = vec3(1.0) - kS;
    kD *= 1.0 - material.Metallic;
        
    vec3 numerator    = NDF * G * F;
    float denominator = 4.0 * max(dot(material.Normal, viewDir), 0.0) * max(dot(material.Normal, L), 0.0) + 0.0001;
    vec3 specular     = numerator / denominator;  
            
    // add to outgoing radiance Lo
    float NdotL = max(dot(material.Normal, L), 0.0);                
    return ((kD * material.Albedo / PI + specular) * radiance * NdotL);
}

vec3 CalcPointLight(PointLight light, Material material, vec3 fragPos, vec3 viewDir)
{
    vec3 F0 = Reflectance(material);

    // calculate per-light radiance
    vec3 L = normalize(light.Position.xyz - fragPos);
    vec3 H = normalize(viewDir + L);

    float distance    = length(light.Position.xyz - fragPos);
    float attenuation = 1.0 / (light.Constant + light.Linear * distance + light.Quadratic * (distance * distance));
    attenuation = clamp(attenuation * light.Brightness, 0, 1); // brightness
    vec3 radiance = light.Color.rgb * attenuation;
    
    return CalcLight(radiance, material, viewDir, F0, L, H);
}

vec3 CalcSpotLight(SpotLight light, Material material, vec3 fragPos, vec3 viewDir) 
{
    vec3 F0 = Reflectance(material);

    // calculate per-light radiance
    vec3 L = normalize(light.Position.xyz - fragPos);
    vec3 H = normalize(viewDir + L);

    float distance    = length(light.Position.xyz - fragPos);
    float attenuation = 1.0 / (light.Constant + light.Linear * distance + light.Quadratic * (distance * distance));
    attenuation = clamp(attenuation * light.Brightness, 0, 1); // brightness

    float innerangle = cos(light.InnerAngle);
    float outerangle = cos(light.OuterAngle);

    vec3 relDir = normalize(light.Position.xyz - fragPos);
    float theta = dot(relDir, normalize(-light.Direction.xyz));
    float epsilon = innerangle - outerangle;
    float intensity = clamp((theta - outerangle) / epsilon, 0.0, 1.0);

    vec3 radiance = light.Color.rgb * attenuation * intensity;
    
    return CalcLight(radiance, material, viewDir, F0, L, H);
}

vec3 CalcDirLight(DirLight light, Material material, vec3 fragPos, vec3 viewDir) 
{
    vec3 F0 = Reflectance(material);

    // calculate per-light radiance
    vec3 L = -light.Direction.xyz;
    vec3 H = normalize(viewDir + L);
    vec3 radiance = light.Color.rgb;
    
    return CalcLight(radiance, material, viewDir, F0, L, H);
}


vec3 CommonPbrLighting(Material material, vec3 fragPos, vec3 viewDir)
{
    // reflectance equation
    vec3 Lo = vec3(0.0);

    // calc point lights
    for(int i = 0; i <= g_nNumDirlights - 1; i++) 
    {
        DirLight dLight  = g_DirLights[i];
        Lo.rgb += CalcDirLight(dLight, material, fs_in.vPositionWs, viewDir) * dLight.Brightness;
    }

    // calc point lights
    for(int i = 0; i <= g_nNumPointlights - 1; i++) 
    {
        PointLight pLight  = g_PointLights[i];
        Lo.rgb += CalcPointLight(pLight, material, fs_in.vPositionWs, viewDir) * pLight.Brightness;
    }

    // calc spot lights
    for(int i = 0; i <= g_nNumSpotlights - 1; i++) 
    {
        SpotLight sLight  = g_SpotLights[i];
        Lo.rgb += CalcSpotLight(sLight, material, fs_in.vPositionWs, viewDir) * sLight.Brightness;
    }

    // apply ao
    vec3 ambient = g_vAmbientLightingColor.rgb * material.Albedo * material.AmbientOcclusion;
    vec3 returncol = ambient + Lo;

    returncol = returncol / (returncol + vec3(1.0));

    return returncol;
}