#version 330 core

out vec4 outputColor;

in vec2 texCoord0;
in vec3 vertexColor;

uniform sampler2D texture0;
uniform sampler2D texture1;
uniform float tintAmount;

void main()
{
    vec4 col = texture(texture0, texCoord0);
    float tintmask = texture(texture1, texCoord0).r;
    outputColor = vec4(mix(col.rgb, vertexColor, tintAmount * tintmask), 1.0);
}