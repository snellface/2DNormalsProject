// Input parameters.
float2   ViewportSize    : register(c0);
float2   TextureSize     : register(c1);
float4x4 MatrixTransform : register(c2);

float SpriteElevation : register(c5);
float PerLayerHeight : register(c4);

float FlipHorizontally;
float FlipVertically;

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


// Pixel shader for rendering sprites (shared between Windows and Xbox).
void RenderToCombinedDataTexturePixelShader(inout float4 color : COLOR0, 
			float2 texCoord : TEXCOORD0,
			float2 vPos : VPOS)
{
	if(FlipHorizontally)
		texCoord.x = 1 - texCoord.x;
	if(FlipVertically)
		texCoord.y = 1 - texCoord.y;

	float4 texColor = tex2D(SpriteSampler, texCoord);
	// Rendering color
	if(vPos.y < PerLayerHeight)
	{
		if(texColor.a < 1)
		{
			discard;
			return;
		}
		color *= float4(texColor.rgb, 1);		
	}
	else if(vPos.y >= PerLayerHeight && vPos.y < (2*PerLayerHeight)) // Else if normal map
	{
		if((texColor.r == 0 && texColor.g == 0 && texColor.b == 0) || (texColor.r == 1 && texColor.g == 0 && texColor.b == 1) || texColor.a == 0)
		{
			discard;
			return;
		}
		float3 normal = texColor.rgb;

		if(FlipHorizontally)
			normal.x = 1 - normal.x;
		if(FlipVertically)
			normal.y = 1 - normal.y;

		normal = normal * 2 - 1;

		normal = normalize(normal);
		normal = (normal + 1) / 2;

		color = float4(normal, 1);
	}
	else // Else drawing on one of the shadow maps
	{
		if(texColor.r > texColor.g)
		{
			discard;
			return;
		}
		color = texColor;
		color.r += SpriteElevation;
		color.g += SpriteElevation;
	}
}

technique SpriteBatch
{
	pass
	{
		VertexShader = compile vs_3_0 SpriteVertexShader();
		PixelShader  = compile ps_3_0 RenderToCombinedDataTexturePixelShader();
	}
}