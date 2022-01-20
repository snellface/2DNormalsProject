// Input parameters.
float2   ViewportSize    : register(c0);
float2   TextureSize     : register(c1);
float4x4 MatrixTransform : register(c2);

float SpriteElevation : register(c3);
bool AlwaysOverwrite : register(c4);

texture LayerSpecialMap;
sampler LayerSpecialMapSampler : register(s1) = sampler_state
{
	texture = <LayerSpecialMap>;
	MipFilter = LINEAR;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

texture SpriteSpecialMap;
sampler SpriteSpecialMapSampler : register(s0) = sampler_state
{
	texture = <SpriteSpecialMap>;
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
void RenderToTargetPixelShader(inout float4 color : COLOR0, 
				float2 texCoord : TEXCOORD0,
				float2 vPos : VPOS)
{
	float4 layerSpecialMap = tex2D(LayerSpecialMapSampler, vPos/ViewportSize);
	float4 spriteSpecialMap = tex2D(SpriteSpecialMapSampler, texCoord);

	if(spriteSpecialMap.g == 0)
		discard;
	
	float spritePixelHeight = spriteSpecialMap.g + (SpriteElevation / 255);

	float overwriteHighElevation = (spritePixelHeight  >= layerSpecialMap.g || AlwaysOverwrite) * (spriteSpecialMap.g > 0);
	float overwriteLowElevation = (spritePixelHeight  <= layerSpecialMap.r || AlwaysOverwrite) * (spriteSpecialMap.g > 0);


	float lowElevation = (spriteSpecialMap.r + SpriteElevation/255) * overwriteLowElevation + layerSpecialMap.r * (overwriteLowElevation != 1);
	float highElevation = (spriteSpecialMap.g + SpriteElevation/255) * overwriteHighElevation + layerSpecialMap.g * (overwriteHighElevation != 1);
	float2 newSpecularAndAlpha = spriteSpecialMap.ba * overwriteHighElevation  + layerSpecialMap.ba * (overwriteHighElevation  != 1);
	color = float4(lowElevation, highElevation, newSpecularAndAlpha);
}

technique SpriteBatch
{
	pass
	{
		VertexShader = compile vs_3_0 SpriteVertexShader();
		PixelShader  = compile ps_3_0 RenderToTargetPixelShader();
	}
}