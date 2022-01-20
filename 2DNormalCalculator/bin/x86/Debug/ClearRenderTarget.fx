// Input parameters.
float2   ViewportSize    : register(c0);
float4x4 MatrixTransform : register(c2);
sampler  TextureSampler  : register(s0);

// Vertex shader for rendering sprites on Windows.
void SpriteVertexShader(inout float4 position : POSITION0)
{
    	// Apply the matrix transform.
    	position = mul(position, transpose(MatrixTransform));

	// Half pixel offset for correct texel centering.
	position.xy -= 0.5;

	// Viewport adjustment.
	position.xy /= ViewportSize;
	position.xy *= float2(2, -2);
	position.xy -= float2(1, -1);
}


// Pixel shader for rendering sprites (shared between Windows and Xbox).
float4 ClearSurfacePixelShader() : COLOR0 
{
	return float4(0,0,0,1);
}

technique SpriteBatch
{
	pass
	{
		VertexShader = compile vs_3_0 SpriteVertexShader();
		PixelShader  = compile ps_3_0 ClearSurfacePixelShader();
	}
}