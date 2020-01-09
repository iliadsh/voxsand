#version 330 core
in vec3 aPosition;
in vec2 aTexCoord;
in float aLayer;
in vec3 aNormal;

out vec2 texCoord;
flat out int layer;
out vec3 Normal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main() 
{
	layer = int(aLayer);
	texCoord = aTexCoord;
	Normal = aNormal * mat3(transpose(inverse(model)));

	gl_Position = vec4(aPosition, 1.0) * model * view * projection;
}