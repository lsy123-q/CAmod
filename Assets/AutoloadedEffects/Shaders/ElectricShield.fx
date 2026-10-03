sampler2D TextureSampler : register(s0);

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


// 원형 영역의 중심으로부터 거리를 계산하여 원형 마스크를 만든다
float CircleMask(float2 uv, float radius, float blur)
{
    float distanceFromCenter =
        length(uv - float2(0.5, 0.5));

    return 1.0 -
        smoothstep(
            radius - blur,
            radius + blur,
            distanceFromCenter);
}


// 해시 기반의 의사 난수를 생성한다
float Hash21(float2 p)
{
    p = frac(p * float2(123.34, 456.21));

    p += dot(
        p,
        p + 45.32);

    return frac(p.x * p.y);
}


// 두 방향의 값을 보간하여 부드러운 노이즈를 생성한다
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
        f.y);
}


// 두 단계의 노이즈를 합쳐 전기 움직임용 FBM을 생성한다
float FastFBM(float2 p)
{
    float value = Noise(p) * 0.65;

    p *= 2.0;

    value += Noise(p) * 0.35;

    return value;
}


// 전기 보호막의 픽셀 색상을 계산한다
float4 PixelShaderFunction(
    float4 color : COLOR0,
    float2 uv : TEXCOORD0) : COLOR0
{
    // UV를 중심 기준 좌표로 변환한다
    float2 centeredUV = uv - float2(0.5, 0.5);

    // 중심으로부터의 거리를 계산한다
    float distanceFromCenter = length(centeredUV);

    // Unity 원본과 동일한 원형 마스크를 계산한다
    float circleMask =
        1.0 -
        smoothstep(
            CircleRadius - CircleSoftness,
            CircleRadius,
            distanceFromCenter);

    // 원 밖의 픽셀은 그리지 않는다
    if (circleMask <= 0.001)
        discard;

    // 중심으로부터의 상대적인 반지름을 계산한다
    float radial =
        saturate(distanceFromCenter / CircleRadius);

    // 내부에서 바깥쪽으로 페이드되는 노이즈 영역을 만든다
    float noiseMask =
        smoothstep(
            NoiseInnerRadius,
            NoiseInnerRadius + NoiseInnerFade,
            radial);

    // 외곽으로 갈수록 노이즈를 제거한다
    noiseMask *=
        1.0 -
        smoothstep(
            NoiseOuterRadius - NoiseOuterFade,
            NoiseOuterRadius,
            radial);

    // Unity 원본과 동일하게 UV 크기를 2배로 만든다
    float2 shieldUV =
        centeredUV * 2.0;

    // 나선 계산에 사용할 반지름을 계산한다
    float radius =
        max(length(shieldUV), 0.0001);

    // 중심을 기준으로 현재 각도를 계산한다
    float angle =
        atan2(
            shieldUV.y,
            shieldUV.x);

    // 시간에 따른 나선 이동량을 계산한다
    float time =
        Time * ElectricSpeed;

    // 역로그 형태의 나선 각도를 계산한다
    float logarithmicAngle =
        log(1.0 + radius * SpiralFrequency)
        * SpiralStrength
        + time;

    // 첫 번째 나선의 각도를 계산한다
    float spiralAngle1 =
        angle + logarithmicAngle;

    // 첫 번째 나선의 극좌표를 직교좌표로 변환한다
    float2 spiralUV1 =
        float2(
            cos(spiralAngle1),
            sin(spiralAngle1))
        * radius;

    // 첫 번째 전기 노이즈의 좌표를 확대한다
    float2 electricUV1 =
        spiralUV1 * ElectricScale;

    // 두 번째 나선의 각도를 계산한다
    float spiralAngle2 =
        angle - logarithmicAngle * 0.73;

    // 두 번째 나선의 극좌표를 직교좌표로 변환한다
    float2 spiralUV2 =
        float2(
            cos(spiralAngle2),
            sin(spiralAngle2))
        * radius;

    // 두 번째 전기 노이즈의 좌표를 확대한다
    float2 electricUV2 =
        spiralUV2 *
        (ElectricScale * 1.37);

    // 첫 번째 나선의 FBM 노이즈를 계산한다
    float noise1 =
        FastFBM(electricUV1);

    // 두 번째 나선의 FBM 노이즈를 계산한다
    float noise2 =
        FastFBM(electricUV2);

    // 두 나선의 노이즈를 서로 다른 비율로 합친다
    float electricNoise =
        noise1 * 0.6 +
        noise2 * 0.4;

    // 노이즈의 중앙값을 기준으로 전기 줄기를 추출한다
    float electricLine =
        1.0 -
        abs(electricNoise - 0.5) * 2.0;

    // 얇은 전기 줄기만 남긴다
    electricLine =
        smoothstep(
            1.0 - ElectricThickness,
            1.0,
            electricLine);

    // 노이즈 영역과 전기 줄기를 결합한다
    float electric =
        electricLine *
        noiseMask;

    // 외곽으로 갈수록 강해지는 프레넬 효과를 계산한다
    float fresnel =
        pow(
            radial,
            FresnelPower);

    // 기본 발광 색상을 계산한다
    float3 baseEmission =
        BaseColor.rgb * 0.2;

    // 전기 줄기의 발광 색상을 계산한다
    float3 electricEmission =
        ElectricColor.rgb *
        electric *
        ElectricIntensity;

    // 외곽 프레넬 발광 색상을 계산한다
    float3 fresnelEmission =
        ElectricColor.rgb *
        fresnel *
        FresnelStrength;

    // 모든 발광 색상을 합친다
    float3 finalColor =
        baseEmission +
        electricEmission +
        fresnelEmission;

    // Unity 원본과 동일한 알파를 계산한다
    float alpha =
        Alpha +
        fresnel * Alpha +
        electric * Alpha * 0.75;

    // 원형 마스크를 알파에 적용한다
    alpha =
        saturate(
            alpha * circleMask);

    return float4(
        finalColor,
        alpha);
}


// SpriteBatch의 기본 Vertex Shader를 사용하고
// PS 3.0 픽셀 셰이더만 적용한다
technique ElectricShield
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}