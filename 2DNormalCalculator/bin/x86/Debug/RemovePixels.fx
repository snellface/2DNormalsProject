// Input parameters.
float2   ViewportSize    : register(c0);
float2   TextureSize     : register(c1);
float4x4 MatrixTransform : register(c2);

texture PrimaryTexture;
sampler PrimaryTextureSampler : register(s0) = sampler_state
{
	texture = <PrimaryTexture>;
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
void ChannelPixelShader(inout float4 color : COLOR0, 
			float2 texCoord : TEXCOORD0)
{
	color = tex2D(PrimaryTextureSampler, texCoord);
	if(color.r == 1 || color.g == 1 || color.b == 1)
		color = float4(1,0,1,1);
	else if(color.r == 0 && color.g == 0 && color.b == 0)
		color = float4(1,0,1,1);
}

technique SpriteBatch
{
	pass
	{
		AlphaBlendEnable = True;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;

		VertexShader = compile vs_3_0 SpriteVertexShader();
		PixelShader  = compile ps_3_0 ChannelPixelShader();
	}
}