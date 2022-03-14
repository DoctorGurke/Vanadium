#version 330 core

in vec3 CubeCoords;

#material samplerCube skybox

out vec4 gl_Color;

void main()
{   
    gl_Color = texture(skybox, CubeCoords.xyz);
}