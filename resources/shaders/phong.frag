#version 400 core

#include shaders/common/common.glsl

in VS_OUT {
	vec3 vPositionWs;
	vec3 vNormalWs;
	vec3 vTangentWs;
	vec3 vBitangentWs;
	vec2 vTexCoord0;
	vec2 vTexCoord1;
	vec2 vTexCoord2;
	vec2 vTexCoord3;
	vec3 vVertexColor;
} fs_in;

uniform vec4 renderColor;
uniform float tintAmount;

out vec4 gl_Color;

vec4 GammaCorrect(vec4 col, float gamma) {
	return vec4(pow(col.rgb, vec3(1.0 / gamma)), col.a);
}

#material sampler2D diffuse
#material sampler2D specular
#material float gloss

// params = constant, linear, quadratic attenuation
vec3 CalcPointLight(vec3 lightPos, vec3 lightCol, vec3 params, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 baseDiffuse, vec3 baseSpecular, float shininess) {
    vec3 lightDir = normalize(lightPos - fragPos);

    float diff = max(dot(normal, lightDir), 0.0);

    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), shininess);

    float distance = length(lightPos - fragPos);
    float attenuation = 1.0 / (params.x + params.y * distance + params.z * (distance * distance));

    vec3 ambient = g_vAmbientLightingColor.rgb * baseDiffuse;
    vec3 diffuse = lightCol * diff * baseDiffuse;
    vec3 specular = lightCol * spec * baseSpecular;

    ambient *= attenuation;
    diffuse *= attenuation;
    specular *= attenuation;

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
        light pLight  = g_Pointlights[i];
        vec3 lightpos = pLight.Position.xyz;
        vec3 lightcol = pLight.Color.rgb;
        vec3 lightparams = pLight.Params.xyz;

        col.rgb += CalcPointLight(lightpos, lightcol, lightparams, fs_in.vNormalWs, fs_in.vPositionWs, viewDir, col.rgb, spec, gloss);
    }

    gl_Color = col;
}

