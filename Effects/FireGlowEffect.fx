sampler2D Texture : register(s0);
float2 uImageSize0;
float uThicknessPx;
float4 uOutlineColor;
float uOpacity;
float4x4 MatrixTransform;

struct VSInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

struct VSOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

VSOutput VertexShaderFunction(VSInput input)
{
    VSOutput o;
    o.Position = mul(input.Position, MatrixTransform);
    o.Color = input.Color;
    o.TexCoord = input.TexCoord;
    return o;
}

// 4방향만 샘플링 (총 4회) - ps_2_0 안전
float4 PixelShaderFunction(VSOutput i) : COLOR0
{
    float2 texel = 1.0 / max(uImageSize0, float2(1.0, 1.0));
    float4 src = tex2D(Texture, i.TexCoord);
    
    // 4방향 직접 샘플링 (상하좌우)
    float2 offset = texel * uThicknessPx;
    
    float a1 = tex2D(Texture, i.TexCoord + float2(offset.x, 0)).a;
    float a2 = tex2D(Texture, i.TexCoord - float2(offset.x, 0)).a;
    float a3 = tex2D(Texture, i.TexCoord + float2(0, offset.y)).a;
    float a4 = tex2D(Texture, i.TexCoord - float2(0, offset.y)).a;
    
    // 대각선 4방향 추가 (총 8회)
    float2 diag = offset * 0.707; // 1/sqrt(2)
    float a5 = tex2D(Texture, i.TexCoord + float2(diag.x, diag.y)).a;
    float a6 = tex2D(Texture, i.TexCoord - float2(diag.x, diag.y)).a;
    float a7 = tex2D(Texture, i.TexCoord + float2(diag.x, -diag.y)).a;
    float a8 = tex2D(Texture, i.TexCoord - float2(diag.x, -diag.y)).a;
    
    // 평균 블러 알파
    float blurA = (a1 + a2 + a3 + a4 + a5 + a6 + a7 + a8) / 8.0;
    
    // 외곽선만 추출
    float outlineA = saturate(blurA - src.a);
    
    float4 col = uOutlineColor;
    col.a *= outlineA * uOpacity;
    col.rgb *= col.a; // Premultiplied alpha
    col *= i.Color;
    
    return col;
}

technique Technique1
{
    pass P0
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}