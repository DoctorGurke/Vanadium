#include shaders/common/common.frag

#material sampler2D diffuse
#material sampler2D specular
#material sampler2D normal
#material float gloss

vec3 CalcPointLight(vec3 lightPos, vec3 lightCol, vec4 attenuationparams, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 baseDiffuse, float baseSpecular, float shininess) {
    vec3 lightDir = normalize(lightPos - fragPos);

    float diff = max(dot(normal, lightDir), 0.0);

    vec3 halfwayDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(normal, halfwayDir), 0.0), shininess);

    float distance = length(lightPos - fragPos);
    float attenuation = 1.0 / (attenuationparams.x + attenuationparams.y * distance + attenuationparams.z * (distance * distance));
    attenuation = clamp(attenuation * attenuationparams.w, 0, 1); // brightness

    vec3 ambient = lightCol * baseDiffuse;
    vec3 diffuse = lightCol * diff * baseDiffuse;
    vec3 specular = lightCol * spec * baseSpecular;

    ambient *= attenuation;
    ambient *= (dot(normal, lightDir) + 1) / 2; // make "shadows" darker
    diffuse *= attenuation;
    specular *= attenuation;

    return (ambient + diffuse + specular);
}

vec3 CalcSpotLight(vec3 spotPos, vec3 spotDir, vec3 spotCol, vec4 attenuationparams, float innerangle, float outerangle, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 baseDiffuse, float baseSpecular, float shininess) {
    vec3 lightDir = normalize(spotPos - fragPos);

    float diff = max(dot(-normal, spotDir), 0.0);

    vec3 halfwayDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(normal, halfwayDir), 0.0), shininess);

    float distance = length(spotPos - fragPos);
    float attenuation = 1.0 / (attenuationparams.x + attenuationparams.y * distance + attenuationparams.z * (distance * distance));
    attenuation = clamp(attenuation * attenuationparams.w, 0, 1); // brightness

    float theta = dot(lightDir, normalize(-spotDir));
    float epsilon = innerangle - outerangle;
    float intensity = clamp((theta - outerangle) / epsilon, 0.0, 1.0);

    vec3 ambient = spotCol * baseDiffuse;
    vec3 diffuse = spotCol * diff * baseDiffuse;
    vec3 specular = spotCol * spec * baseSpecular;

    ambient *= attenuation * intensity;
    ambient *= (dot(-normal, spotDir) + 1) / 2; // make "shadows" darker
    diffuse *= attenuation * intensity;
    specular *= attenuation * intensity;

    return (ambient + diffuse + specular);
}

vec3 CalcDirLight(vec3 dir, vec3 col, vec3 normal, vec3 viewDir, vec3 baseDiffuse, float baseSpecular, float shininess)
{
    vec3 lightDir = normalize(-dir);

    // diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);

    // specular shading
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), shininess);

    // combine results
    vec3 ambient  = col * baseDiffuse;
    ambient *= (dot(-normal, dir) + 1) / 2; // make "shadows" darker
    vec3 diffuse  = col * diff * baseDiffuse;
    vec3 specular = col * spec * baseSpecular;
    return (ambient + diffuse + specular);
}  

vec3 CommonPhongLighting(vec3 baseDiffuse, float baseSpecular, float gloss, vec3 normal, vec3 fragPos, vec3 viewDir) {
    vec3 returncol = baseDiffuse;
    
    // calc dir lights
    for(int i = 0; i <= g_nNumDirlights - 1; i++) {
        DirLight dLight = g_DirLights[i];
        vec3 lightdir = dLight.Direction.xyz;
        vec3 lightcol = dLight.Color.rgb;

        returncol.rgb += CalcDirLight(lightdir, lightcol, normal, viewDir, baseDiffuse, baseSpecular, gloss);
    }

    // calc point lights
    for(int i = 0; i <= g_nNumPointlights - 1; i++) {
        PointLight pLight  = g_PointLights[i];
        vec3 lightpos = pLight.Position.xyz;
        vec3 lightcol = pLight.Color.rgb;
        vec4 lightparams = pLight.Attenuation.xyzw;

        returncol.rgb += CalcPointLight(lightpos, lightcol, lightparams, normal, fs_in.vPositionWs, viewDir, baseDiffuse, baseSpecular, gloss);
    }

    // calc spot lights
    for(int i = 0; i <= g_nNumSpotlights - 1; i++) {
        SpotLight sLight  = g_SpotLights[i];
        vec3 lightpos = sLight.Position.xyz;
        vec3 lightdir = sLight.Direction.xyz;
        vec3 lightcol = sLight.Color.rgb;
        vec4 lightparams = sLight.Attenuation.xyzw;
        float inner = cos(sLight.Params.x);
        float outer = cos(sLight.Params.y);

        returncol.rgb += CalcSpotLight(lightpos, lightdir, lightcol, lightparams, inner, outer, normal, fs_in.vPositionWs, viewDir, baseDiffuse, baseSpecular, gloss);
    }
    return returncol;
}