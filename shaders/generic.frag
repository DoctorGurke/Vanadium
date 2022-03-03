#version 330 core

out vec4 outputColor;

in vec3 vertexColor;

uniform float tintAmount;

void main()
{
    outputColor = vec4(mix(vertexColor, vec3(0.0, 0.0, 0.0), tintAmount), 1.0);
}