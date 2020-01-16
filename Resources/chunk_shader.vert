#version 460 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in float aLayer;
layout (location = 3) in vec3 aNormal;

out vec2 texCoord;
flat out int layer;
out vec3 Normal;
out vec4 FragPosInLightSpace;
out float visibility;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform mat4 lightProjection;
uniform mat4 lightView;

const float density = 0.015;
const float gradient  = 4.0;

void main() 
{
	layer = int(aLayer);
	texCoord = aTexCoord;
	Normal = aNormal * mat3(transpose(inverse(model)));

	FragPosInLightSpace = vec4(aPosition, 1) * model * lightView * lightProjection;

	vec4 positionRelativeToCam = vec4(aPosition, 1) * model * view;
	float distance = length(positionRelativeToCam.xyz);
	visibility = exp(-pow((distance*density),gradient));
	visibility = clamp(visibility, 0.0, 1.0);

	gl_Position = positionRelativeToCam * projection;
}