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

texture LayerSpecialMap_1;
sampler LayerSpecialMapSampler_1 : register(s0) = sampler_state
{
	texture = <LayerSpecialMap_1>;
	MipFilter = LINEAR;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

texture LayerSpecialMap_2;
sampler LayerSpecialMapSampler_2 : register(s1) = sampler_state
{
	texture = <LayerSpecialMap_2>;
	MipFilter = LINEAR;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

texture LayerSpecialMap_3;
sampler LayerSpecialMapSampler_3 : register(s2) = sampler_state
{
	texture = <LayerSpecialMap_3>;
	MipFilter = LINEAR;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

texture LayerSpecialMap_4;
sampler LayerSpecialMapSampler_4 : register(s3) = sampler_state
{
	texture = <LayerSpecialMap_4>;
	MipFilter = LINEAR;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

texture LayerSpecialMap_5;
sampler LayerSpecialMapSampler_5 : register(s4) = sampler_state
{
	texture = <LayerSpecialMap_5>;
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

float2 PixelPosToUV(float2 pixelPos)
{
	return float2(pixelPos * InverseViewportDimensions + InverseViewportDimensions*0.5);
}

float IsPixelObscuringLight(in int x, in int y, in int z)
{
	float2 texCoord = PixelPosToUV(float2(x, y));

	float4 pixel1 = tex2Dlod(LayerSpecialMapSampler_1, float4(texCoord, 0, 1));
	float4 pixel2 = tex2Dlod(LayerSpecialMapSampler_2, float4(texCoord, 0, 1));
	float4 pixel3 = tex2Dlod(LayerSpecialMapSampler_3, float4(texCoord, 0, 1));
	float4 pixel4 = tex2Dlod(LayerSpecialMapSampler_4, float4(texCoord, 0, 1));
	float4 pixel5 = tex2Dlod(LayerSpecialMapSampler_5, float4(texCoord, 0, 1));

	float obscured = (z >= pixel1.r * 255 && z+1 < pixel1.g * 255) + (z >= pixel2.r * 255 && z+1 < pixel2.g * 255) + (z >= pixel3.r * 255 && z+1 < pixel3.g * 255) + (z >= pixel4.r * 255 && z+1 < pixel4.g * 255) + (z >= pixel5.r * 255 && z+1 < pixel5.g * 255);

	return obscured;
}

// Vertex shader for rendering sprites on Windows.
void SpriteVertexShader(inout float4 position : POSITION0, 
			inout float4 color : COLOR0, 
			inout float2 texCoord : TEXCOORD0)
{
    	// Apply the matrix transform.
    	position = mul(position, transpose(MatrixTransform));
    
	// Half pixel offset for correct texel centering.
	position.xy -= 0.5;

	// Viewport adjustment.
	position.xy /= ViewportSize;
	position.xy *= float2(2, -2);
	position.xy -= float2(1, -1);

	// Compute the texture coordinate.
	texCoord /= TextureSize;
}

float ObscurityCount(in float3 pixelCoord, in float3 lightCoord)
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

	float obscured = 0;

	if ((l >= m) && (l >= n)) // Y
	{
		err_1 = dy2 - l; err_2 = dz2 - l;

		[loop]
		for (i = 0; i < l; i++) 
		{
			obscured  += IsPixelObscuringLight(px, py, pz);

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
                	obscured  += IsPixelObscuringLight(px, py, pz);

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
                	obscured  += IsPixelObscuringLight(px, py, pz);

                	if (err_1 > 0) {
                 		py += y_inc; err_1 -= dz2;
                    	}
                    	if (err_2 > 0) {
                        	px += x_inc; err_2 -= dz2;
                    	}
                   	err_1 += dy2; err_2 += dx2; pz += z_inc;
                }
        }

	return obscured;
}

// Pixel shader for rendering sprites (shared between Windows and Xbox).
void RenderShadeMapPixelShader(inout float4 color : COLOR0, 
			float2 texCoord : TEXCOORD0,
			float2 vPos : VPOS)
{
color = float4(1,1,0,1);
return;
	float4 layerSpecialMap_1 = tex2D(LayerSpecialMapSampler_1, texCoord);
	float4 layerSpecialMap_2 = tex2D(LayerSpecialMapSampler_2, texCoord);
	float4 layerSpecialMap_3 = tex2D(LayerSpecialMapSampler_3, texCoord);
	float4 layerSpecialMap_4 = tex2D(LayerSpecialMapSampler_4, texCoord);
	float4 layerSpecialMap_5 = tex2D(LayerSpecialMapSampler_5, texCoord);

	float4 topmostLayer = layerSpecialMap_1;
	float newLayerIsTopmost = (layerSpecialMap_2.g > topmostLayer.g);
	topmostLayer = layerSpecialMap_2 * newLayerIsTopmost + topmostLayer * !newLayerIsTopmost;
	topmostLayer = layerSpecialMap_3 * newLayerIsTopmost + topmostLayer * !newLayerIsTopmost;
	topmostLayer = layerSpecialMap_4 * newLayerIsTopmost + topmostLayer * !newLayerIsTopmost;
	topmostLayer = layerSpecialMap_5 * newLayerIsTopmost + topmostLayer * !newLayerIsTopmost;

	float pixelElevation = max(max(max(max(layerSpecialMap_1.g,layerSpecialMap_2.g), layerSpecialMap_3.g), layerSpecialMap_4.g), layerSpecialMap_5.g);

	float shadowCount = ObscurityCount(float3(vPos.x, vPos.y, pixelElevation), LightPos);
	float lum = 0;
	lum = shadowCount > 0;

	color = float4(1,lum,0,1);
}

technique SpriteBatch
{
	pass
	{
		VertexShader = compile vs_3_0 SpriteVertexShader();
		PixelShader  = compile ps_3_0 RenderShadeMapPixelShader();
	}
}