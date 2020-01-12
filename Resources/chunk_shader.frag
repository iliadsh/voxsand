#version 330 core
out vec4 FragColor;

in vec2 texCoord;
flat in int layer;
in vec3 Normal;

uniform sampler2DArray texture0;

void main()
{
	vec3 norm = normalize(Normal);
	
	vec4 texel = vec4(texture(texture0, vec3(texCoord.xy, layer)));
	texel = vec4(texel.xyz * 1.1f, texel.w);
	if (texel.a < 0.7)
		discard;

	FragColor = texel;
}