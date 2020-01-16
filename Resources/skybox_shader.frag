#version 460 core
out vec4 FragColor;

in vec3 TexCoords;

uniform samplerCube skybox;

const vec3 fogColour = vec3(0.5, 0.5, 0.6);
const float lowerLimit = 0.0;
const float upperLimit = 1.0;

void main()
{    
    vec4 finalColour = texture(skybox, TexCoords);
	float factor = (TexCoords.y - lowerLimit) / (upperLimit - lowerLimit);
	factor = clamp(factor, 0.0, 1.0);
	FragColor = mix(vec4(fogColour, 1.0), finalColour, factor);
}