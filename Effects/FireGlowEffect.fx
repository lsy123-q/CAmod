sampler2D uImage0 : register(s0);

float4 uColor; // (r,g,b,a) 발광색이다
float uIntensity; // 발광 강도이다
float uBlurRadius; // 블러 반경(픽셀 단위 느낌)이다
float uTime; // 시간이다
float2 uTextureSize; // 텍스처 크기(px)이다

float4x4 MatrixTransform;

struct VertexInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};

struct VertexOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};

VertexOutput VertexShaderFunction(VertexInput input)
{
    VertexOutput output;
    output.Position = mul(input.Position, MatrixTransform); // SpriteBatch 변환을 적용한다
    output.TexCoord = input.TexCoord;
    output.Color = input.Color;
    return output;
}

float4 PixelShaderFunction(VertexOutput input) : COLOR0
{
    float2 uv = input.TexCoord;

    // 텍셀 크기를 텍스처 크기 기반으로 만든다
    float2 texel = (1.0 / max(uTextureSize, 1.0)) * uBlurRadius;

    float4 original = tex2D(uImage0, uv);

    // 4방향 블러(가벼운 편)이다
    float4 b = 0;
    b += tex2D(uImage0, uv + float2(texel.x, 0));
    b += tex2D(uImage0, uv + float2(-texel.x, 0));
    b += tex2D(uImage0, uv + float2(0, texel.y));
    b += tex2D(uImage0, uv + float2(0, -texel.y));
    b *= 0.25;

    // 펄스는 아주 약하게만 준다
    float pulse = 1.0 + sin(uTime * 3.0) * 0.08;

    // 알파 기반으로 바깥쪽만 발광시키는 값이다
    float edge = saturate((b.a - original.a) * 4.0);

    float3 glowRGB = uColor.rgb * (uIntensity * pulse) * edge;

    float4 result;
    result.rgb = original.rgb + glowRGB; // 쉐이더 내부에서 더한다
    result.a = saturate(original.a + edge * 0.35); // 알파를 살짝 올린다

    // 스프라이트 입력 색을 곱한다
    result *= input.Color;

    return result;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}