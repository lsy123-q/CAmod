
sampler uImage0 : register(s0);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uSaturation;
float uRotation;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
float2 uTargetPosition;
float4 uLegacyArmorSourceRect;
float2 uLegacyArmorSheetSize;

float4 _NodeCircleColor = float4(1.0, 1.0, 1.0, 1.0);
float4 _MiniStarColor = float4(0.55, 0.95, 1.0, 1.0);
float4 _GlowColor = float4(0.0, 1.0, 1.0, 1.0);

float _LineWidth = 0.0246;
float _Radius = 1.0;
float _GlowIntensity = 5.06;
float _Alpha = 0.453;
float _RotationSpeed = 0.0;
float _NodeSize = 0.132;

float _InwardGlowPower = 3.88;
float _InwardGlowStrength = 2.0;

#define PI 3.14159265359
#define TAU 6.28318530718
#define TRIANGLE_PERIOD 2.09439510239
#define TRIANGLE_HALF_PERIOD 1.04719755120

float2 Rotate2D(float2 p, float a)
{
    float s = sin(a);
    float c = cos(a);

    return float2(
        c * p.x - s * p.y,
        s * p.x + c * p.y
    );
}

float Circle(float r, float target, float w)
{
    return 1.0 - smoothstep(
        w * 0.5,
        w * 1.5,
        abs(r - target)
    );
}

float PolarTriangleOutline(
    float2 p,
    float radius,
    float rotation,
    float width)
{
    float r = length(p);
    float theta = atan2(p.y, p.x) - rotation - PI / 3.0;

    float delta = theta -
        TRIANGLE_PERIOD *
        floor(
            (theta + TRIANGLE_HALF_PERIOD) /
            TRIANGLE_PERIOD
        );

    float apothem = radius * 0.5;
    float signedDistance = r * cos(delta) - apothem;

    return 1.0 - smoothstep(
        width * 0.5,
        width * 1.5,
        abs(signedDistance)
    );
}

float4 PixelShaderFunction(
    float4 sampleColor : COLOR0,
    float2 coords : TEXCOORD0) : COLOR0
{
    float2 uv = coords * 2.0 - 1.0;
    uv = Rotate2D(uv, uTime * _RotationSpeed);

    float r = length(uv);

    float bounds = 1.0 - smoothstep(
        _Radius - 0.01,
        _Radius + 0.01,
        r
    );

    float nodeCirclesTotal = 0.0;
    float miniStarsTotal = 0.0;

    for (int i = 0; i < 6; i++)
    {
        float angle = PI * 0.5 + TAU * i / 6.0;

        float2 center = float2(
            cos(angle),
            sin(angle)
        ) * (_Radius * 0.75);

        float2 q = uv - center;
        float qr = length(q);

        nodeCirclesTotal += Circle(
            qr,
            _NodeSize,
            _LineWidth * 0.8
        );

        float miniStar1 = PolarTriangleOutline(
            q,
            _NodeSize * 0.6,
            uTime * 0.2,
            _LineWidth * 0.5
        );

        float miniStar2 = PolarTriangleOutline(
            q,
            _NodeSize * 0.6,
            -uTime * 0.2 + PI,
            _LineWidth * 0.5
        );

        miniStarsTotal += max(miniStar1, miniStar2);
    }

    float normalizedDist = saturate(r / _Radius);

    float inwardGlow = pow(
        1.0 - normalizedDist,
        _InwardGlowPower
    ) * _InwardGlowStrength;

    float colorWeight =
        nodeCirclesTotal +
        miniStarsTotal +
        inwardGlow;

    float3 colorSum =
        _NodeCircleColor.rgb * nodeCirclesTotal +
        _MiniStarColor.rgb * miniStarsTotal +
        _GlowColor.rgb * inwardGlow;

    float3 color = colorSum / max(colorWeight, 0.0001);
    color *= 1.0 + _GlowIntensity * 0.15;

    float alpha = saturate(colorWeight * _Alpha) * bounds;

    return float4(
        color * bounds * sampleColor.rgb,
        alpha * sampleColor.a * uOpacity
    );
}

technique SimpleAlchemicalSeal_B
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
