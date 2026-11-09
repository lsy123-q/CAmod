sampler2D TextureSampler : register(s0);

float4x4 MatrixTransform;

float4 BaseColor = float4(0.03, 0.15, 1.0, 1.0);
float4 ElectricColor = float4(0.2, 0.8, 1.0, 1.0);

float Alpha = 0.3;

float CircleRadius = 0.48;
float CircleSoftness = 0.02;

float NoiseInnerRadius = 0.35;
float NoiseInnerFade = 0.15;

float NoiseOuterRadius = 0.95;
float NoiseOuterFade = 0.08;

float ElectricSpeed = 2.0;
float ElectricScale = 12.0;
float ElectricThickness = 0.15;
float ElectricIntensity = 6.0;

float SpiralFrequency = 20.0;
float SpiralStrength = 4.0;

float FresnelPower = 3.0;
float FresnelStrength = 3.0;

float Time;


struct VertexInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 UV : TEXCOORD0;
};

struct VertexOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 UV : TEXCOORD0;
};


VertexOutput VertexShaderFunction(VertexInput input)
{
    VertexOutput output;

    output.Position = mul(input.Position, MatrixTransform);
    output.Color = input.Color;
    output.UV = input.UV;

    return output;
}


float Hash21(float2 p)
{
    p = frac(p * float2(123.34, 456.21));
    p += dot(p, p + 45.32);

    return frac(p.x * p.y);
}


float Noise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);

    f = f * f * (3.0 - 2.0 * f);

    float a = Hash21(i);
    float b = Hash21(i + float2(1.0, 0.0));
    float c = Hash21(i + float2(0.0, 1.0));
    float d = Hash21(i + float2(1.0, 1.0));

    return lerp(
        lerp(a, b, f.x),
        lerp(c, d, f.x),
        f.y
    );
}


// 2단 옥타브 FBM으로 전기 노이즈를 생성한다
float FastFBM(float2 p)
{
    float value = Noise(p) * 0.65;

    p *= 2.0;

    value += Noise(p) * 0.35;

    return value;
}


float4 PixelShaderFunction(VertexOutput input) : COLOR0
{
    float2 centeredUV = input.UV - 0.5;

    float distanceFromCenter = length(centeredUV);


    // 원형 영역 밖의 픽셀을 제거한다
    float circleMask =
        1.0 -
        smoothstep(
            CircleRadius - CircleSoftness,
            CircleRadius,
            distanceFromCenter
        );

    if (circleMask <= 0.001)
        discard;


    float radial =
        saturate(distanceFromCenter / CircleRadius);


    // 내부와 외부를 동시에 페이드한다
    float noiseMask =
        smoothstep(
            NoiseInnerRadius,
            NoiseInnerRadius + NoiseInnerFade,
            radial
        );

    noiseMask *=
        1.0 -
        smoothstep(
            NoiseOuterRadius - NoiseOuterFade,
            NoiseOuterRadius,
            radial
        );


    // UV를 중심 기준 좌표로 변환한다
    float2 shieldUV = centeredUV * 2.0;

    float radius =
        max(length(shieldUV), 0.0001);

    float angle =
        atan2(shieldUV.y, shieldUV.x);


    // 시간에 따라 나선이 회전한다
    float animatedTime =
        Time * ElectricSpeed;


    // 역로그 형태로 나선 각도를 계산한다
    float logarithmicAngle =
        log(1.0 + radius * SpiralFrequency)
        * SpiralStrength
        + animatedTime;


    // 첫 번째 나선
    float spiralAngle1 =
        angle + logarithmicAngle;

    float2 spiralUV1 =
        float2(
            cos(spiralAngle1),
            sin(spiralAngle1)
        ) * radius;

    float2 electricUV1 =
        spiralUV1 * ElectricScale;


    // 두 번째 나선
    float spiralAngle2 =
        angle - logarithmicAngle * 0.73;

    float2 spiralUV2 =
        float2(
            cos(spiralAngle2),
            sin(spiralAngle2)
        ) * radius;

    float2 electricUV2 =
        spiralUV2 *
        (ElectricScale * 1.37);


    // 두 개의 전기 노이즈를 합친다
    float noise1 =
        FastFBM(electricUV1);

    float noise2 =
        FastFBM(electricUV2);

    float electricNoise =
        noise1 * 0.6 +
        noise2 * 0.4;


    // 노이즈에서 밝은 전기 줄기만 추출한다
    float electricLine =
        1.0 -
        abs(electricNoise - 0.5) * 2.0;

    electricLine =
        smoothstep(
            1.0 - ElectricThickness,
            1.0,
            electricLine
        );


    float electric =
        electricLine * noiseMask;


    // 중심에서 바깥쪽으로 프레넬 효과를 만든다
    float fresnel =
        pow(radial, FresnelPower);


    // 기본 방출 색상
    float3 baseEmission =
        BaseColor.rgb * 0.2;


    // 전기 줄기 방출 색상
    float3 electricEmission =
        ElectricColor.rgb
        * electric
        * ElectricIntensity;


    // 외곽 프레넬 방출 색상
    float3 fresnelEmission =
        ElectricColor.rgb
        * fresnel
        * FresnelStrength;


    float3 finalColor =
        baseEmission
        + electricEmission
        + fresnelEmission;


    // 전체 투명도를 계산한다
    float alphaShape =
        1.0
        + fresnel
        + electric * 0.75;

    float alpha = alphaShape * Alpha;

    alpha =
        saturate(alpha * circleMask);


    // SpriteBatch의 원본 색상과 텍스처 알파를 적용한다
    float4 texColor =
        tex2D(TextureSampler, input.UV);

    float finalAlpha =
        alpha *
        texColor.a *
        input.Color.a;


    return float4(
        finalColor * input.Color.rgb,
        finalAlpha
    );
}


technique ElectricShield
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}