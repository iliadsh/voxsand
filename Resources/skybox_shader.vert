#version 330 core
in vec3 aPos;

out vec3 TexCoords;

uniform mat4 projection;
uniform mat4 view;

void main()
{
    TexCoords = aPos;
	vec4 pos = vec4(aPos, 1.0) * view * projection;
    gl_Position = pos.xyww;
}  