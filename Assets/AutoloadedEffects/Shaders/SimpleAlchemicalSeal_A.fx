
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

float4 _OuterRingColor = float4(1.0, 1.0, 1.0, 1.0);
float4 _InnerOuterRingColor = float4(1.0, 1.0, 1.0, 1.0);
float4 _StarColor = float4(0.55, 0.95, 1.0, 1.0);

float _LineWidth = 0.0246;
float _Radius = 1.0;
float _GlowIntensity = 5.06;
float _Alpha = 0.453;
float _RotationSpeed = 0.0;
float _OuterRingGap = 0.439;

#define PI 3.14159265359
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

float Hexagram(float2 p, float radius, float width)
{
    float a = PolarTriangleOutline(
        p, radius, PI * 0.5, width
    );

    float b = PolarTriangleOutline(
        p, radius, -PI * 0.5, width
    );

    return max(a, b);
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

    float ring = Circle(
        r,
        _Radius * 0.95 - _OuterRingGap,
        _LineWidth
    );

    float starTotal = 0.0;

    for (int s = 0; s < 3; s++)
    {
        float scaleFactor = 1.0 - s * 0.15;
        float alphaFactor = 1.0 - s * 0.35;

        starTotal += Hexagram(
            uv,
            _Radius * 0.75 * scaleFactor,
            _LineWidth
        ) * alphaFactor;
    }

    float colorWeight = ring + starTotal;

    float3 colorSum =
        _InnerOuterRingColor.rgb * ring +
        _StarColor.rgb * starTotal;

    float3 color = colorSum / max(colorWeight, 0.0001);
    color *= 1.0 + _GlowIntensity * 0.15;

    float alpha = saturate(colorWeight * _Alpha) * bounds;

    return float4(
        color * bounds * sampleColor.rgb,
        alpha * sampleColor.a
    );
}

technique SimpleAlchemicalSeal_A
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
