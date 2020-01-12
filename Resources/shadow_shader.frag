#version 330 core

#define BIAS 0.01

in vec2 texCoord;
flat in int layer;

uniform sampler2DArray texture0;

void main()
{         
	vec4 texel = vec4(texture(texture0, vec3(texCoord.xy, layer)));
	if (texel.a < 0.3)
		discard;

	gl_FragDepth = gl_FragCoord.z;
	gl_FragDepth += gl_FrontFacing ? BIAS : 0.0;
}  