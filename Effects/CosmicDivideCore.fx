// Ported from the supplied Unity DualLaserCore shader.
float Thickness = 0.085;
float Blur = 0.007;
float BlurPower = 1.0;
float Opacity = 1.0;

float4 PixelShaderFunction(float4 vertexColor : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    float distanceToCenter = abs(uv.x - 0.5);
    float coreRadius = Thickness * 0.5;
    float mask = 1.0 - smoothstep(coreRadius, coreRadius + max(Blur, 0.0001), distanceToCenter);
    mask = pow(saturate(mask), BlurPower);
    float endFade = smoothstep(0.0, 0.025, uv.y) * (1.0 - smoothstep(0.975, 1.0, uv.y));
    return float4(0.0, 0.0, 0.0, mask * Opacity * endFade);
}

technique CosmicDivideCore
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
