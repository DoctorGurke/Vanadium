#include shaders/common/common_data.vert

vec4 CommonVertexProcessing(void) {
	fPosition = vec3(vec4(vPosition, 1.0) * model);
	fNormal = vNormal * mat3(transpose(inverse(model)));
	fTangent = vTangent;
	fTexCoord0 = vTexCoord0;
	fTexCoord1 = vTexCoord1;
	fTexCoord2 = vTexCoord2;
	fTexCoord3 = vTexCoord3;
	fVertColor = vColor;

    return vec4(vPosition, 1.0) * model * view * projection;
}