#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 2) in vec2 aTexCoord0;
layout(location = 1) in vec3 aColor; 

out vec2 texCoord0;
out vec3 vertexColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main(void)
{
	texCoord0 = aTexCoord0;
	vertexColor = aColor;

    gl_Position = vec4(aPosition, 1.0) * model * view * projection;
}