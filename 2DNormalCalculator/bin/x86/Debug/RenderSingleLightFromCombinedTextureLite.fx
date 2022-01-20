// Input parameters.
float2   ViewportSize    : register(c0);
float2   TextureSize     : register(c1);
float4x4 MatrixTransform : register(c2);

float PerLayerHeight : register(c4);
float ShadowMapCount : register(c5);

float3 LightPos;
float3 LightColor;
float3 EyeLocation;
float3 EyeDirection;
float LightIntensity;
float2 InverseViewportDimensions;

texture Sprite;
sampler SpriteSampler : register(s0) = sampler_state
{
	texture = <Sprite>;
	MipFilter = LINEAR;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

float3 ReflectVector(in float3 ray, in float3 normal)
{
	float3 reflection = normal * dot(ray, normal);
	return ray - (reflection * 2);
}

float2 VPosToUV(float2 pixelPos)
{
	return float2(pixelPos * InverseViewportDimensions + InverseViewportDimensions * 0.5);
}

float HitTest(int x, int y, int z)
{
	float2 layerCoord = VPosToUV(float2(x, y)); // Colormap coord;
	layerCoord.y += (PerLayerHeight / TextureSize.y); // normalmap coord
	float hits = 0;
	float4 layerData;
	int i;
	[loop]
	for(i = 0; i < ShadowMapCount; i++)
	{
		layerCoord.y += (PerLayerHeight / TextureSize.y); // we start on normalmap, so we must make jump from normal to shadow layer 0

		layerData = tex2Dlod(SpriteSampler, float4(layerCoord, 0, 1));
		hits += (z >= layerData.r * 255 && z+1 < layerData.g * 255);
	}
	return hits;
}

// Do shadowtest using BresenhamLine3D (in utility.cs for c# ref.)
float PixelLitTest(in float3 pixelCoord, in float3 lightCoord)
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

	float hits = 0;

	if ((l >= m) && (l >= n)) // Y
	{
		err_1 = dy2 - l; err_2 = dz2 - l;

		[loop]
		for (i = 0; i < l; i++) 
		{
			hits += HitTest(px, py, pz);

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
                	hits += HitTest(px, py, pz);

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
                	hits += HitTest(px, py, pz);

                	if (err_1 > 0) {
                 		py += y_inc; err_1 -= dz2;
                    	}
                    	if (err_2 > 0) {
                        	px += x_inc; err_2 -= dz2;
                    	}
                   	err_1 += dy2; err_2 += dx2; pz += z_inc;
                }
        }

	return hits;
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
			float2 vPos : VPOS,
			float3 lightToPixel : TEXCOORD1)
{
	float4 pixelColor = tex2D(SpriteSampler, texCoord);
	if(pixelColor.a == 0)
	{
		color = float4(1, 0, 0, 1);
		return;
	}

	float2 shadowMapCoord = texCoord; // color coord
	shadowMapCoord.y += (PerLayerHeight / TextureSize.y); // normalmap coord
	float4 topmostPixelFromHeightData = float4(0,0,0,0);
	float4 thisPixelFromHeightData;
	int i;
	[loop]
	for(i = 0; i < ShadowMapCount; i++)
	{
		shadowMapCoord.y += (PerLayerHeight / TextureSize.y); // we start on normalmap, so we must make jump from normal to shadow layer 0
		thisPixelFromHeightData = tex2Dlod(SpriteSampler, float4(shadowMapCoord, 0, 1));

		if(thisPixelFromHeightData.g > topmostPixelFromHeightData.g)
			topmostPixelFromHeightData = thisPixelFromHeightData;
		
	}

	float3 pixelCoord = float3(vPos.x, vPos.y, topmostPixelFromHeightData.g * 255);

	float shadows = 0;
	shadows = PixelLitTest(pixelCoord, LightPos);
	if(shadows > 0)
	{
		color = float4(0,0,0,1);
		return;
	}

	float3 normal = tex2D(SpriteSampler, texCoord + float2(0, PerLayerHeight / TextureSize.y)).xyz;
	normal = (normal * 2) - 1;
	normal.y = -normal.y;
	normal = normalize(normal);

	//float3 lightToPixel = float3(pixelCoord.x - LightPos.x, pixelCoord.y - LightPos.y, pixelCoord.z - LightPos.z);
	lightToPixel = float3(lightToPixel.xy, pixelCoord.z - LightPos.z);

	float distanceFromLight = length(lightToPixel);
	distanceFromLight = distanceFromLight * distanceFromLight;

	lightToPixel = normalize(lightToPixel);
	float lambertTerm = dot(-lightToPixel, normal);	
	if(lambertTerm < 0)
	{
		color = float4(0,0,0,1);
		return;
	}

	color = float4(LightColor, 0) * LightIntensity * lambertTerm / distanceFromLight;

	if(topmostPixelFromHeightData.b > 0)
	{
		float3 pixelToEye = normalize( pixelCoord - EyeLocation );
		float3 reflectionVector = ReflectVector(-lightToPixel, normal);

		float reflectionDotProduct = dot(pixelToEye, reflectionVector);

		if(reflectionDotProduct > 0)
		{
			float specularIllumination = pow(abs(reflectionDotProduct), 20) * topmostPixelFromHeightData.b;
			color += float4(LightColor, 0) * specularIllumination;
		}

	}

	color.a = 1;	
}

technique SpriteBatch
{
	pass
	{
		VertexShader = compile vs_3_0 SpriteVertexShader();
		PixelShader  = compile ps_3_0 PointLightPixelShader();
	}
}