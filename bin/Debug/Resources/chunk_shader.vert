#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in float aLayer;
layout (location = 3) in vec3 aNormal;

out vec2 texCoord;
flat out int layer;
out vec3 Normal;
out vec4 FragPosInLightSpace;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform mat4 lightProjection;
uniform mat4 lightView;

void main() 
{
	layer = int(aLayer);
	texCoord = aTexCoord;
	Normal = aNormal * mat3(transpose(inverse(model)));

	FragPosInLightSpace = vec4(aPosition, 1) * model * lightView * lightProjection;

	gl_Position = vec4(aPosition, 1.0) * model * view * projection;
}