in vec3 vPosition;
in vec3 vNormal;
in vec2 vTexCoord0;
in vec3 vColor;

out vec3 fPosition;
out vec3 fNormal;
out vec2 fTexCoord0;
out vec3 fColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

vec4 CommonVertexProcessing(void) {
	fPosition = vec3(vec4(vPosition, 1.0) * model);
	fNormal = vNormal * mat3(transpose(inverse(model)));
	fTexCoord0 = vTexCoord0;
	fColor = vColor;

    return vec4(vPosition, 1.0) * model * view * projection;
}