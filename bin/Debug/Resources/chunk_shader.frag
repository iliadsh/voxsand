#version 330 core
out vec4 FragColor;

in vec2 texCoord;
flat in int layer;
in vec3 Normal;
in vec4 FragPosInLightSpace;

uniform sampler2DArray texture0;
uniform sampler2D texture1;

float ShadowCalculation(vec4 fragPosLightSpace) 
{
	//// perform perspective divide
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    //// transform to [0,1] range
    projCoords = projCoords * 0.5 + 0.5;
    //// get closest depth value from light's perspective (using [0,1] range fragPosLight as coords)
    float closestDepth = texture(texture1, projCoords.xy).r; 
    //// get depth of current fragment from light's perspective
    float currentDepth = projCoords.z;
    //// check whether current frag pos is in shadow
    float bias = 0;//0.00005;

	float shadow = 0.0;
	vec2 texelSize = 1.0 / textureSize(texture1, 0);
	for(int x = -1; x <= 1; ++x)
	{
		for(int y = -1; y <= 1; ++y)
		{
		    float pcfDepth = texture(texture1, projCoords.xy + vec2(x, y) * texelSize).r; 
		    shadow += currentDepth - bias > pcfDepth ? 1.0 : 0.0;        
		}    
	}
	shadow /= 9.0;

	if(projCoords.z > 1.0)
        shadow = 0.0;

    return shadow;
}

void main()
{
	vec3 norm = normalize(Normal);
	
	vec4 texel = vec4(texture(texture0, vec3(texCoord.xy, layer)));
	vec3 tempxl = texel.xyz;
	float shadow = ShadowCalculation(FragPosInLightSpace);
	tempxl *= (0.5 + (1.0 - shadow));
	texel = vec4(tempxl, texel.w);
	if (texel.a < 0.7)
		discard;

	FragColor = texel;
}