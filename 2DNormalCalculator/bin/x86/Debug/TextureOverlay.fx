// Input parameters.
float2   ViewportSize    : register(c0);
float2   TextureSize     : register(c1);
float4x4 MatrixTransform : register(c2);

texture PrimaryTexture;
sampler PrimaryTextureSampler : register(s0) = sampler_state
{
	texture = <PrimaryTexture>;
};

texture OverlayTexture;
sampler OverlayTextureSampler : register(s1) = sampler_state
{
	texture = <OverlayTexture>;
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
void OverlayPixelShader(inout float4 color : COLOR0, 
			float2 texCoord : TEXCOORD0)
{
	float4 tex1Col = tex2D(PrimaryTextureSampler, texCoord);
	float4 tex2Col = tex2D(OverlayTextureSampler, texCoord);
	
	// Color = base texture - how much alpha overlay has
	if(tex2Col.r == 1 && tex2Col.g == 0 && tex2Col.b == 1)
		color = tex1Col;
	else
		color = tex2Col;
}

technique SpriteBatch
{
	pass
	{
		AlphaBlendEnable = True;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;

		VertexShader = compile vs_3_0 SpriteVertexShader();
		PixelShader  = compile ps_3_0 OverlayPixelShader();
	}
}