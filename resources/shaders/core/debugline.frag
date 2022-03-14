#version 400 core

in VS_OUT {
	vec3 vPositionWs;
	vec4 vVertexColor;
} fs_in;

out vec4 gl_Color;

void main()
{
    gl_Color = fs_in.vVertexColor;
}