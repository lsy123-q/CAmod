sampler uImage0 : register(s0);

float2 uImageSize0; // (texWidth, texHeight)
float uThicknessPx; // 10.0
float4 uOutlineColor; // (r,g,b,a) 0~1
float uOpacity; // 1.0
float4x4 MatrixTransform; // SpriteBatch가 자동으로 넣는 변환행렬이다
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
    o.Position = mul(input.Position, MatrixTransform); // 화면좌표를 클립좌표로 변환한다
    o.Color = input.Color;
    o.TexCoord = input.TexCoord;
    return o;
}

float4 PixelShaderFunction(VSOutput i) : COLOR0
{
    float2 texel = 1.0 / max(uImageSize0, float2(1.0, 1.0));

    float4 src = tex2D(uImage0, i.TexCoord);
    float a0 = src.a;

    // 2링으로 블러 느낌 만든다 (가볍게)
    float r1 = uThicknessPx * 0.55;
    float r2 = uThicknessPx * 1.00;

    float sum = 0.0;
    float wsum = 0.0;

    // 8방향 샘플링한다
    // (cos/sin 대신 상수 방향을 직접 써도 되지만 ps_2_0 const 제한 피하려고 각도 계산을 쓴다)
    for (int k = 0; k < 8; k++)
    {
        float angle = 6.2831853 * (k / 8.0); // 2PI * (k/8) 이다
        float2 dir = float2(cos(angle), sin(angle));

        float a1 = tex2D(uImage0, i.TexCoord + dir * texel * r1).a;
        float a2 = tex2D(uImage0, i.TexCoord + dir * texel * r2).a;

        float w1 = 0.60;
        float w2 = 0.40;

        sum += a1 * w1 + a2 * w2;
        wsum += w1 + w2;
    }

    float blurA = sum / max(wsum, 0.0001);

    // 내부는 지우고 외곽만 남긴다
    float outlineA = saturate(blurA - a0);

    float4 col = uOutlineColor;
    col.a *= outlineA * uOpacity;

    // 프리멀티플라이드 알파로 만든다
    col.rgb *= col.a;

    // 스프라이트 배치 컬러(조명색) 곱한다
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