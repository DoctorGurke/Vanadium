#version 400 core

#include shaders/common/common.frag

#material sampler2D diffuse
#material sampler2D specular
#material float gloss

vec3 CalcPointLight(vec3 lightPos, vec3 lightCol, vec3 attenuationparams, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 baseDiffuse, vec3 baseSpecular, float shininess) {
    vec3 lightDir = normalize(lightPos - fragPos);

    float diff = max(dot(normal, lightDir), 0.0);

    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), shininess);

    float distance = length(lightPos - fragPos);
    float attenuation = 1.0 / (attenuationparams.x + attenuationparams.y * distance + attenuationparams.z * (distance * distance));

    vec3 ambient = lightCol * baseDiffuse;
    vec3 diffuse = lightCol * diff * baseDiffuse;
    vec3 specular = lightCol * spec * baseSpecular;

    ambient *= attenuation;
    diffuse *= attenuation;
    specular *= attenuation;

    return (ambient + diffuse + specular);
}

vec3 CalcSpotLight(vec3 spotPos, vec3 spotDir, vec3 spotCol, vec3 attenuationparams, float innerangle, float outerangle, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 baseDiffuse, vec3 baseSpecular, float shininess) {
    vec3 lightDir = normalize(spotPos - fragPos);

    float diff = max(dot(normal, spotDir), 0.0);

    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), shininess);

    float distance = length(spotPos - fragPos);
    float attenuation = 1.0 / (attenuationparams.x + attenuationparams.y * distance + attenuationparams.z * (distance * distance));

    float theta = dot(lightDir, normalize(-spotDir));
    float epsilon = innerangle - outerangle;
    float intensity = clamp((theta - outerangle) / epsilon, 0.0, 1.0);

    vec3 ambient = spotCol * baseDiffuse;
    vec3 diffuse = spotCol * diff * baseDiffuse;
    vec3 specular = spotCol * spec * baseSpecular;
    ambient *= attenuation * intensity;
    diffuse *= attenuation * intensity;
    specular *= attenuation * intensity;
    return (ambient + diffuse + specular);
}

void main() {
    vec2 uv = fs_in.vTexCoord0;
    vec4 base = tex2D(diffuse, uv).rgba;
    vec4 tint = base * renderColor;

    vec4 col = mix(base, tint, tintAmount);
    vec3 spec = tex2D(specular, uv).rgb;

    if(col.a <= 0.0)
        discard;

    col *= g_vAmbientLightingColor;

    vec3 viewDir = normalize(g_vCameraPositionWs - fs_in.vPositionWs);

    for(int i = 0; i <= g_nNumPointlights - 1; i++) {
        pointlight pLight  = g_Pointlights[i];
        vec3 lightpos = pLight.Position.xyz;
        vec3 lightcol = pLight.Color.rgb;
        vec3 lightparams = pLight.Attenuation.xyz;

        col.rgb += CalcPointLight(lightpos, lightcol, lightparams, fs_in.vNormalWs, fs_in.vPositionWs, viewDir, col.rgb, spec, gloss);
    }

    for(int i = 0; i <= g_nNumSpotlights - 1; i++) {
        spotlight sLight  = g_Spotlights[i];
        vec3 lightpos = sLight.Position.xyz;
        vec3 lightdir = sLight.Direction.xyz;
        vec3 lightcol = sLight.Color.rgb;
        vec3 lightparams = sLight.Attenuation.xyz;
        float inner = sLight.Params.x;
        float outer = sLight.Params.y;

        col.rgb += CalcSpotLight(lightpos, lightdir, lightcol, lightparams, inner, outer, fs_in.vNormalWs, fs_in.vPositionWs, viewDir, col.rgb, spec, gloss);
    }

    gl_Color = col;
}

