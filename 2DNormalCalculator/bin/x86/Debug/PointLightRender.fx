// Input parameters.
float2   ViewportSize    : register(c0);
float2   TextureSize     : register(c1);
float4x4 MatrixTransform : register(c2);

float3 LightPos;
float3 LightColor;
float3 EyeLocation;
float3 EyeDirection;
float LightIntensity;
float2 InverseViewportDimensions;

texture LightMap;
sampler LightMapSampler : register(s0) = sampler_state
{
	texture = <LightMap>;
};

texture NormalMap;
sampler NormalMapSampler : register(s1) = sampler_state
{
	texture = <NormalMap>;
};

texture SpecialMap;
sampler SpecialMapSampler : register(s2) = sampler_state
{
	texture = <SpecialMap>;
};


float3 ReflectVector(in float3 ray, in float3 normal)
{
	float3 reflection = normal * dot(ray, normal);
	return ray - (reflection * 2);
}

float2 PixelPosToUV(float2 pixelPos)
{
	return float2(pixelPos * InverseViewportDimensions + InverseViewportDimensions*0.5);
}

bool IsPixelObscuringLight(in int x, in int y, in int z)
{
	float2 texCoord = PixelPosToUV(float2(x, y));

	float4 pixel = tex2Dlod(SpecialMapSampler, float4(texCoord, 0, 1));

	return (z >= pixel.r * 255 && z+1 < pixel.g * 255);
}

// Do shadowtest using BresenhamLine3D (in utility.cs for c# ref.)
float ShadowTest(in float3 pixelCoord, in float3 lightCoord)
{
	int x2 = pixelCoord.x; int y2 = pixelCoord.y; int z2 = pixelCoord.z;

	int x1 = lightCoord.x; int y1 = lightCoord.y; int z1 = lightCoord.z;

	int i, dx, dy, dz, l, m, n, x_inc, y_inc, z_inc, err_1, err_2, dx2, dy2, dz2;

	int px = x1; int py = y1; int pz = z1;

	dx = x2 - x1; dy = y2 - y1; dz = z2 - z1;

	x_inc = (dx < 0) ? -1 : 1;
	l = abs(dx);

	y_inc = (dy < 0) ? -1 : 1;
	m = abs(dy);

	z_inc = (dz < 0) ? -1 : 1;
	n = abs(dz);

	dx2 = l * 2; dy2 = m * 2; dz2 = n * 2;

	if ((l >= m) && (l >= n)) // Y
	{
		err_1 = dy2 - l; err_2 = dz2 - l;

		[loop]
		for (i = 0; i < l; i++) 
		{
			if(IsPixelObscuringLight(px, py, pz))
				return 0;

			if (err_1 > 0)  {
				py += y_inc; err_1 -= dx2;
			}
			if (err_2 > 0)  {
				pz += z_inc; err_2 -= dx2;
			}
			err_1 += dy2; err_2 += dz2; px += x_inc;

                }
	}
	else if ((m >= l) && (m >= n)) // X
       	{
        	err_1 = dx2 - m; err_2 = dz2 - m;

		[loop]
                for (i = 0; i < m; i++) 
                {
                	if(IsPixelObscuringLight(px, py, pz))
				return 0;

                	if (err_1 > 0)  {
                		px += x_inc; err_1 -= dy2;
                    	}
                    	if (err_2 > 0)  {
                        	pz += z_inc; err_2 -= dy2;
                    	}
                    	err_1 += dx2; err_2 += dz2; py += y_inc;
               	}
        }
	else // Z
        {
        	err_1 = dy2 - n; err_2 = dx2 - n;
		[loop]
                for (i = 0; i < n; i++)
                {
                	if(IsPixelObscuringLight(px, py, pz))
				return 0;

                	if (err_1 > 0) {
                 		py += y_inc; err_1 -= dz2;
                    	}
                    	if (err_2 > 0) {
                        	px += x_inc; err_2 -= dz2;
                    	}
                   	err_1 += dy2; err_2 += dx2; pz += z_inc;
                }
        }

	return 1;
}

// Vertex shader for rendering sprites on Windows.
void SpriteVertexShader(inout float4 position : POSITION0, 
			inout float4 color : COLOR0, 
			inout float2 texCoord : TEXCOORD0,
			out float3 lightToPixel : TEXCOORD1)
{
    	// Apply the matrix transform.
    	position = mul(position, transpose(MatrixTransform));

	lightToPixel = float3(position.x - LightPos.x, position.y - LightPos.y, position.z - LightPos.z);    

	// Half pixel offset for correct texel centering.
	position.xy -= 0.5;

	// Viewport adjustment.
	position.xy /= ViewportSize;
	position.xy *= float2(2, -2);
	position.xy -= float2(1, -1);

	// Compute the texture coordinate.
	texCoord /= TextureSize;
}


// Pixel shader for rendering sprites (shared between Windows and Xbox).
void PointLightPixelShader(inout float4 color : COLOR0, 
			float2 texCoord : TEXCOORD0, 
			float2 pixelPos : VPOS,
			float3 lightToPixel : TEXCOORD1)
{
	float4 pixelColor = tex2D(LightMapSampler, texCoord);
	float4 pixelData = tex2D(SpecialMapSampler, texCoord);

	if((pixelColor.r == 1 && pixelColor.g == 0 && pixelColor.b == 1) || pixelColor.a == 0)
	{
		discard;
		return;
	}

	color = float4(0,0,0, 1);

// Data prep (turn 0..1 into 0..255 height values)
	pixelData.r *= 255;
	pixelData.g *= 255;

// Check if current pixel sees current light light
	float shadowTest = ShadowTest(float3(pixelPos, pixelData.g), LightPos);

	if(shadowTest > 0)
	{
		float3 surfaceNormal = tex2D(NormalMapSampler, texCoord).xyz;	

// Normals prep	
		surfaceNormal = float3(surfaceNormal * 2 - 1);
		surfaceNormal.y = -surfaceNormal.y;
		surfaceNormal = normalize(surfaceNormal);

		lightToPixel = float3(lightToPixel.xy, pixelData.g - LightPos.z);

// DIFFUSE LIGHT
		float lightDistance = length(lightToPixel);
		lightDistance = lightDistance * lightDistance;
	
		lightToPixel = normalize(lightToPixel);
		float lambertTerm = dot(-lightToPixel, surfaceNormal);	

		if(lambertTerm > 0)
		{
			if(lightDistance < 0.001)
				lightDistance = 0.001;
	
			color += float4(LightColor, 0) * LightIntensity * lambertTerm / lightDistance;
		}
		else
		{
			//discard;.
			return;
		}

// SPECULAR LIGHT
		if(pixelData.b > 0)
		{
			float3 pixelToEye = normalize( float3(pixelPos, pixelData.g) - EyeLocation );
			float3 reflectionVector = ReflectVector(-lightToPixel, surfaceNormal);

			float reflectionDotProduct = dot(pixelToEye, reflectionVector);

			if(reflectionDotProduct > 0)
			{
				float specularIllumination = pow(abs(reflectionDotProduct), 20) * pixelData.b;
				color += float4(LightColor, 0) * specularIllumination;
			}

		}
	}
	else
	{
		//discard;
		return;
	}
}

technique SpriteBatch
{
	pass
	{
		VertexShader = compile vs_3_0 SpriteVertexShader();
		PixelShader  = compile ps_3_0 PointLightPixelShader();
	}
}