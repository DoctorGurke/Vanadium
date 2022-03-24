#include shaders/common/common.frag

#material sampler2D diffuse
#material sampler2D specular
#material sampler2D normal
#material float gloss

vec3 CalcPointLight(vec3 lightPos, vec3 lightCol, vec3 attenuationparams, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 baseDiffuse, vec3 baseSpecular, float shininess) {
    vec3 lightDir = normalize(lightPos - fragPos);

    float diff = max(dot(normal, lightDir), 0.0);

    vec3 halfwayDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(normal, halfwayDir), 0.0), shininess);

    float distance = length(lightPos - fragPos);
    float attenuation = 1.0 / (attenuationparams.x + attenuationparams.y * distance + attenuationparams.z * (distance * distance));

    vec3 ambient = baseDiffuse * lightCol;
    vec3 diffuse = lightCol * diff * baseDiffuse;
    vec3 specular = lightCol * spec * baseSpecular;

    ambient *= attenuation;
    ambient *= (dot(normal, lightDir) + 1) / 2;
    diffuse *= attenuation;
    specular *= attenuation;

    return (ambient + diffuse + specular);
}

vec3 CalcSpotLight(vec3 spotPos, vec3 spotDir, vec3 spotCol, vec3 attenuationparams, float innerangle, float outerangle, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 baseDiffuse, vec3 baseSpecular, float shininess) {
    vec3 lightDir = normalize(spotPos - fragPos);

    float diff = max(dot(-normal, spotDir), 0.0);

    vec3 halfwayDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(normal, halfwayDir), 0.0), shininess);

    float distance = length(spotPos - fragPos);
    float attenuation = 1.0 / (attenuationparams.x + attenuationparams.y * distance + attenuationparams.z * (distance * distance));

    float theta = dot(lightDir, normalize(-spotDir));
    float epsilon = innerangle - outerangle;
    float intensity = clamp((theta - outerangle) / epsilon, 0.0, 1.0);

    vec3 ambient = baseDiffuse * spotCol;
    vec3 diffuse = spotCol * diff * baseDiffuse;
    vec3 specular = spotCol * spec * baseSpecular;

    ambient *= attenuation * intensity;
    ambient *= (dot(-normal, spotDir) + 1) / 2;
    diffuse *= attenuation * intensity;
    specular *= attenuation * intensity;

    return (ambient + diffuse + specular);
}

vec3 CommonPhongLighting(vec3 col, vec3 baseDiffuse, vec3 baseSpecular, float gloss, vec3 normal, vec3 fragPos, vec3 viewDir) {
    // apply global ambient light color
    vec3 returncol = col * g_vAmbientLightingColor.rgb;

    // calc point lights
    for(int i = 0; i <= g_nNumPointlights - 1; i++) {
        PointLight pLight  = g_PointLights[i];
        vec3 lightpos = pLight.Position.xyz;
        vec3 lightcol = pLight.Color.rgb;
        vec3 lightparams = pLight.Attenuation.xyz;

        returncol.rgb += CalcPointLight(lightpos, lightcol, lightparams, fs_in.vNormalWs, fs_in.vPositionWs, viewDir, baseDiffuse, baseSpecular, gloss);
    }

    // calc spot lights
    for(int i = 0; i <= g_nNumSpotlights - 1; i++) {
        SpotLight sLight  = g_SpotLights[i];
        vec3 lightpos = sLight.Position.xyz;
        vec3 lightdir = sLight.Direction.xyz;
        vec3 lightcol = sLight.Color.rgb;
        vec3 lightparams = sLight.Attenuation.xyz;
        float inner = cos(sLight.Params.x);
        float outer = cos(sLight.Params.y);

        returncol.rgb += CalcSpotLight(lightpos, lightdir, lightcol, lightparams, inner, outer, fs_in.vNormalWs, fs_in.vPositionWs, viewDir, baseDiffuse, baseSpecular, gloss);
    }
    return returncol;
}