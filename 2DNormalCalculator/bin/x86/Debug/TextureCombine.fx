// Input parameters.
float2   ViewportSize    : register(c0);
float2   TextureSize     : register(c1);
float4x4 MatrixTransform : register(c2);

bool Additive : register(c3);

texture PrimaryTexture;
sampler PrimaryTextureSampler : register(s0) = sampler_state
{
	texture = <PrimaryTexture>;
};

texture SecondTexture;
sampler SecondTextureSampler : register(s1) = sampler_state
{
	texture = <SecondTexture>;
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
void CombinePixelShader(inout float4 color : COLOR0, 
			float2 texCoord : TEXCOORD0)
{
	float4 tex1Col = tex2D(PrimaryTextureSampler, texCoord);
	float4 tex2Col = tex2D(SecondTextureSampler, texCoord);
	
	if(Additive)
		//color = float4(((tex1Col.xyz * tex1Col.a) + (tex2Col.xyz * tex1Col.a)), 1);
		color = tex1Col + tex2Col;
	else
		color = float4((tex1Col.xyz * tex1Col.a) * (tex2Col.xyz * tex1Col.a), 1);
}

technique SpriteBatch
{
	pass
	{
		VertexShader = compile vs_3_0 SpriteVertexShader();
		PixelShader  = compile ps_3_0 CombinePixelShader();
	}
}