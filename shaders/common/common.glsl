uniform float curTime;

vec4 tex2D(sampler2D tex, vec2 uv) {
    return textureLod(tex, uv, textureQueryLod(tex, uv).x);
}