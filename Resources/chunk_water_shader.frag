#version 460 core
out vec4 FragColor;

in vec2 texCoord;
flat in int layer;
in float visibility;

const vec3 skyColour = vec3(0.7, 0.7, 0.8);

uniform sampler2DArray texture0;

void main()
{
	vec4 texel = vec4(texture(texture0, vec3(texCoord.xy, layer)));
	texel = mix(vec4(skyColour, 1.0), texel, visibility);
	FragColor = texel;
}