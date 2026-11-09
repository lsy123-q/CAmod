// Ported from the supplied Unity DualLaser_EXP shader.
float LineSpacing = 0.11;
float InnerRange = 0.015;
float OuterRange = 0.075;
float Intensity = 1.6;
float Opacity = 1.0;
float3 BeamColor = float3(0.5, 0.9, 1.0);

float LaserLine(float x, float center, float direction)
{
    float offset = x - center;
    float range = offset * direction <= 0.0 ? InnerRange : OuterRange;
    return exp(-abs(offset) / max(range, 0.0001));
}

float4 PixelShaderFunction(float4 vertexColor : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    float x = uv.x - 0.5;
    float left = LaserLine(x, -LineSpacing * 0.5, -1.0);
    float right = LaserLine(x, LineSpacing * 0.5, 1.0);
    float edgeFade = 1.0 - smoothstep(0.4, 0.5, abs(x));
    float endFade = smoothstep(0.0, 0.025, uv.y) * (1.0 - smoothstep(0.975, 1.0, uv.y));
    float alpha = saturate((left + right) * Intensity) * saturate(Opacity) * edgeFade * endFade;
    // Premultiplied output for AlphaBlend: transparent edges carry no extra RGB.
    return float4(BeamColor * alpha, alpha);
}

technique CosmicDivideGlow
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}

// RGB already contains the opacity. Alpha=1 prevents Additive from applying it a second time.
float4 AdditivePixelShaderFunction(float4 vertexColor : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    float4 glow = PixelShaderFunction(vertexColor, uv);
    return float4(glow.rgb, 1.0);
}

technique CosmicDivideGlowAdditive
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 AdditivePixelShaderFunction();
    }
}
