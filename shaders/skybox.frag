#version 330 core

in vec3 TexCoords;

out vec4 gl_Color;

#material samplerCube skybox

void main()
{    
    gl_Color = texture(skybox, TexCoords);
}