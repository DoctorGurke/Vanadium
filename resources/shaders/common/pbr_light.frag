#include shaders/common/common.frag

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
    float a      = roughness*roughness;
    float a2     = a*a;
    float NdotH  = max(dot(N, H), 0.0);
    float NdotH2 = NdotH*NdotH;
	
    float num   = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;
	
    return num / denom;
}

float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r*r) / 8.0;

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

vec3 CalcPointLight(vec3 lightPos, vec3 lightCol, vec4 attenuationparams, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 baseAlbedo, float baseRoughness, float baseMetallic)
{
    vec3 F0 = g_vAmbientLightingColor.rgb;
    F0 = mix(F0, baseAlbedo, baseMetallic);

    // calculate per-light radiance
    vec3 L = normalize(lightPos - fragPos);
    vec3 H = normalize(viewDir + L);
    float distance    = length(lightPos - fragPos);
    float attenuation = 1.0 / (attenuationparams.x + attenuationparams.y * distance + attenuationparams.z * (distance * distance));
    attenuation = clamp(attenuation * attenuationparams.w, 0, 1); // brightness
    vec3 radiance     = lightCol * attenuation;    
        
    // cook-torrance brdf
    float NDF = DistributionGGX(normal, H, baseRoughness);        
    float G   = GeometrySmith(normal, viewDir, L, baseRoughness);      
    vec3 F    = fresnelSchlick(max(dot(H, viewDir), 0.0), F0);       
        
    vec3 kS = F;
    vec3 kD = vec3(1.0) - kS;
    kD *= 1.0 - baseMetallic;
        
    vec3 numerator    = NDF * G * F;
    float denominator = 4.0 * max(dot(normal, viewDir), 0.0) * max(dot(normal, L), 0.0) + 0.0001;
    vec3 specular     = numerator / denominator;  
            
    // add to outgoing radiance Lo
    float NdotL = max(dot(normal, L), 0.0);                
    return ((kD * baseAlbedo / PI + specular) * radiance * NdotL);
}

vec3 CommonPbrLighting(vec3 baseAlbedo, vec3 normal, float baseRoughness, float baseMetallic, float baseAo, vec3 fragPos, vec3 viewDir)
{
    // reflectance equation
    vec3 Lo = vec3(0.0);

    // calc point lights
    for(int i = 0; i <= g_nNumPointlights - 1; i++) {
        PointLight pLight  = g_PointLights[i];
        vec3 lightpos = pLight.Position.xyz;
        vec3 lightcol = pLight.Color.rgb;
        vec4 lightparams = pLight.Attenuation.xyzw;

        Lo.rgb += CalcPointLight(lightpos, lightcol, lightparams, normal, fs_in.vPositionWs, viewDir, baseAlbedo, baseRoughness, baseMetallic);
    }

    // apply ao
    vec3 ambient = g_vAmbientLightingColor.rgb * baseAlbedo * baseAo;
    vec3 returncol = ambient + Lo;

    return returncol;
}