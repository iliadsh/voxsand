#version 460 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in float aLayer;

out vec2 texCoord;
flat out int layer;

uniform mat4 lightProjection;
uniform mat4 lightView;
uniform mat4 model;

void main()
{
	layer = int(aLayer);
	texCoord = aTexCoord;
    gl_Position =  vec4(aPosition, 1.0) * model * lightView * lightProjection;
}  