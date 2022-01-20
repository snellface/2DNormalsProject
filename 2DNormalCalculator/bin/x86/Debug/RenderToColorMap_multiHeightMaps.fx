// Input parameters.
float2   ViewportSize    : register(c0);
float2   TextureSize     : register(c1);
float4x4 MatrixTransform : register(c2);

float SpriteElevation : register(c3);
bool AlwaysOverwrite : register(c4);

texture LayerSpecialMap_1;
sampler LayerSpecialMapSampler_1 : register(s2) = sampler_state
{
	texture = <LayerSpecialMap_1>;
	MipFilter = LINEAR;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

texture LayerSpecialMap_2;
sampler LayerSpecialMapSampler_2 : register(s3) = sampler_state
{
	texture = <LayerSpecialMap_2>;
	MipFilter = LINEAR;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

texture LayerSpecialMap_3;
sampler LayerSpecialMapSampler_3 : register(s4) = sampler_state
{
	texture = <LayerSpecialMap_3>;
	MipFilter = LINEAR;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

texture LayerSpecialMap_4;
sampler LayerSpecialMapSampler_4 : register(s5) = sampler_state
{
	texture = <LayerSpecialMap_4>;
	MipFilter = LINEAR;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

texture LayerSpecialMap_5;
sampler LayerSpecialMapSampler_5 : register(s6) = sampler_state
{
	texture = <LayerSpecialMap_5>;
	MipFilter = LINEAR;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

texture SpriteSpecialMap;
sampler SpriteSpecialMapSampler : register(s1) = sampler_state
{
	texture = <SpriteSpecialMap>;
	MipFilter = LINEAR;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

texture SpriteColorMap;
sampler SpriteColorMapSampler : register(s0) = sampler_state
{
	texture = <SpriteColorMap>;
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
	float4 layerSpecialMap_1 = tex2D(LayerSpecialMapSampler_1, vPos/ViewportSize);
	float4 layerSpecialMap_2 = tex2D(LayerSpecialMapSampler_2, vPos/ViewportSize);
	float4 layerSpecialMap_3 = tex2D(LayerSpecialMapSampler_3, vPos/ViewportSize);
	float4 layerSpecialMap_4 = tex2D(LayerSpecialMapSampler_4, vPos/ViewportSize);
	float4 layerSpecialMap_5 = tex2D(LayerSpecialMapSampler_5, vPos/ViewportSize);

	float4 spriteSpecialMap = tex2D(SpriteSpecialMapSampler, texCoord);
	float4 spriteColorMap = tex2D(SpriteColorMapSampler, texCoord);

	float spritePixelElevation = spriteSpecialMap.g + (SpriteElevation / 255);

	float overwrite = ((spritePixelElevation >= layerSpecialMap_1.g && spritePixelElevation >= layerSpecialMap_2.g && spritePixelElevation >= layerSpecialMap_3.g && spritePixelElevation >= layerSpecialMap_4.g && spritePixelElevation >= layerSpecialMap_5.g) || AlwaysOverwrite);

	color *= spriteColorMap * overwrite;
}

technique SpriteBatch
{
	pass
	{
		VertexShader = compile vs_3_0 SpriteVertexShader();
		PixelShader  = compile ps_3_0 RenderToTargetPixelShader();
	}
}