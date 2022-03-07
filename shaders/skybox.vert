#version 410 core

layout(location = 0) out vec3 CubeCoords;

in vec3 vPosition;

uniform mat4 projection;
uniform mat4 view;

void main()
{
    CubeCoords = vPosition.xyz;
    vec4 pos = projection * view * vec4(vPosition.xyz, 1.0);
    gl_Position = pos.xyww;
}  