﻿#version 330 core
out vec4 FragColor;

in vec2 texCoord;
flat in int layer;

uniform sampler2DArray texture0;

void main()
{
	vec4 texel = vec4(texture(texture0, vec3(texCoord.xy, layer)));
	if (texel.a < 0.6)
		discard;
	FragColor = texel;
}