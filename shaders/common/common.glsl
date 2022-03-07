uniform float curTime;
uniform vec3 cameraPos;

vec4 tex2D(sampler2D tex, vec2 uv) {
    return textureLod(tex, uv, textureQueryLod(tex, uv).x);
}