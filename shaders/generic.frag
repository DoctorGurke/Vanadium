#version 330 core

out vec4 outputColor;

in vec2 texCoord0;
in vec3 vertexColor;

uniform sampler2D texture0;
uniform float tintAmount;

void main()
{
    vec4 col = texture(texture0, texCoord0);
    outputColor = vec4(mix(vertexColor, col.rgb, tintAmount), 1.0);
}