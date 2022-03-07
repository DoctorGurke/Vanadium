#version 410 core

layout(location = 0) in vec3 CubeCoords;

#material samplerCube skybox

out vec4 gl_Color;

void main()
{   
    vec3 col = texture(skybox, CubeCoords.xyz).rgb;
    gl_Color = vec4(col, 1.0);
}