#include shaders/common/common.frag

#material sampler2D diffuse
#material sampler2D specular
#material sampler2D normal
#material float gloss

vec3 CalcPointLight(PointLight light, BasicMaterial mat, vec3 fragPos, vec3 viewDir) {
    vec3 lightDir = normalize(light.Position.xyz - fragPos);

    float diff = max(dot(mat.Normal, lightDir), 0.0);

    vec3 halfwayDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(mat.Normal, halfwayDir), 0.0), mat.Gloss);

    float distance = length(light.Position.xyz - fragPos);
    float attenuation = 1.0 / (light.Attenuation.x + light.Attenuation.y * distance + light.Attenuation.z * (distance * distance));
    attenuation = clamp(attenuation * light.Attenuation.w, 0, 1); // brightness

    vec3 ambient = light.Color.rgb * mat.Diffuse;
    vec3 diffuse = light.Color.rgb * diff * mat.Diffuse;
    vec3 specular = light.Color.rgb * spec * mat.Specular;

    ambient *= attenuation;
    ambient *= (dot(mat.Normal, lightDir) + 1) / 2; // make "shadows" darker
    diffuse *= attenuation;
    specular *= attenuation;

    return (ambient + diffuse + specular);
}

vec3 CalcSpotLight(SpotLight light, BasicMaterial mat, vec3 fragPos, vec3 viewDir) {
    vec3 lightDir = normalize(light.Position.xyz - fragPos);

    float diff = max(dot(-mat.Normal, light.Direction.xyz), 0.0);

    vec3 halfwayDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(mat.Normal, halfwayDir), 0.0), mat.Gloss);

    float distance = length(light.Position.xyz - fragPos);
    float attenuation = 1.0 / (light.Attenuation.x + light.Attenuation.y * distance + light.Attenuation.z * (distance * distance));
    attenuation = clamp(attenuation * light.Attenuation.w, 0, 1); // brightness

    float innerangle = cos(light.Params.x);
    float outerangle = cos(light.Params.y);

    float theta = dot(lightDir, normalize(-light.Direction.xyz));
    float epsilon = innerangle - outerangle;
    float intensity = clamp((theta - outerangle) / epsilon, 0.0, 1.0);

    vec3 ambient = light.Color.rgb * mat.Diffuse;
    vec3 diffuse = light.Color.rgb * diff * mat.Diffuse;
    vec3 specular = light.Color.rgb * spec * mat.Specular;

    ambient *= attenuation * intensity;
    ambient *= (dot(-mat.Normal, light.Direction.xyz) + 1) / 2; // make "shadows" darker
    diffuse *= attenuation * intensity;
    specular *= attenuation * intensity;

    return (ambient + diffuse + specular);
}

vec3 CalcDirLight(DirLight light, BasicMaterial mat, vec3 viewDir)
{
    vec3 lightDir = normalize(-light.Direction.xyz);

    // diffuse shading
    float diff = max(dot(mat.Normal, lightDir), 0.0);

    // specular shading
    vec3 reflectDir = reflect(-lightDir, mat.Normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), mat.Gloss);

    // combine results
    vec3 ambient  = light.Color.rgb * mat.Diffuse;
    ambient *= (dot(-mat.Normal, light.Direction.xyz) + 1) / 2; // make "shadows" darker
    vec3 diffuse  = light.Color.rgb * diff * mat.Diffuse;
    vec3 specular = light.Color.rgb * spec * mat.Specular;
    return (ambient + diffuse + specular);
}  

vec3 CommonPhongLighting(BasicMaterial mat, vec3 fragPos, vec3 viewDir) {
    vec3 returncol = mat.Diffuse * g_vAmbientLightingColor.rgb;
    
    // calc dir lights
    for(int i = 0; i <= g_nNumDirlights - 1; i++) {
        DirLight dLight = g_DirLights[i];
        returncol.rgb += CalcDirLight(dLight, mat, viewDir);
    }

    // calc point lights
    for(int i = 0; i <= g_nNumPointlights - 1; i++) {
        PointLight pLight  = g_PointLights[i];
        returncol.rgb += CalcPointLight(pLight, mat, fs_in.vPositionWs, viewDir);
    }

    // calc spot lights
    for(int i = 0; i <= g_nNumSpotlights - 1; i++) {
        SpotLight sLight  = g_SpotLights[i];
        returncol.rgb += CalcSpotLight(sLight, mat, fs_in.vPositionWs, viewDir);
    }
    return returncol;
}