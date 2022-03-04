in vec3 vPosition;
in vec2 vTexCoord0;
in vec3 vColor;

out vec3 fPosition;
out vec2 fTexCoord0;
out vec3 fColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void CommonVertexProcessing(void) {
	fPosition = vPosition;
	fTexCoord0 = vTexCoord0;
	fColor = vColor;

    gl_Position = vec4(vPosition, 1.0) * model * view * projection;
}