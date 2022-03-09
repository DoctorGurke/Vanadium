uniform float curTime;
uniform vec3 cameraPos;

layout (std140) uniform Matrices {
    mat4 projection;    // 64
    mat4 view;          // 64
};

vec4 tex2D(sampler2D tex, vec2 uv) {
    return textureLod(tex, uv, textureQueryLod(tex, uv).x);
}