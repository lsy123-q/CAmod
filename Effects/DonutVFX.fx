/* ������Ÿ���� ���� �׷��� ���̴��Դϴ� 
 �ٽ��� atan2 �� ������ ��ġ�Ѵ��� 
 ������ ���൵�� ���� ������ ���� �׸��ϴ� */


Texture2D uImage0Texture : register(t0);
sampler2D uImage0 : register(s0) = sampler_state
{
    Texture = <uImage0Texture>;
};

Texture2D uImage1Texture : register(t1);
sampler2D uImage1 : register(s1) = sampler_state
{
    Texture = <uImage1Texture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

float3 uColor, uSecondaryColor, uLightSource;
float uOpacity, uSaturation, uRotation, uTime, uDirection;
float4 uSourceRect, uLegacyArmorSourceRect;
float2 uWorldPosition, uImageSize0, uImageSize1, uTargetPosition, uLegacyArmorSheetSize;

float4 BackgroundColor;
float Progress;
float2 ImageScale;
float ImageAlpha;
float GlobalAlpha;

float4 Color1;
float4 Color2;
float4 Color3;

float Color1Position;
float Color2Position;
float Color3Position;

static const float BACKGROUND_RADIUS = 0.257f;
static const float BACKGROUND_BLUR = 0.0197f;

static const float GAUGE_RADIUS = 0.232f;
static const float GAUGE_THICKNESS = 1.0f;
static const float GAUGE_BLUR = 0.02f;

static const float PIXEL_GRID = 256.0f;

#define PI 3.14159265359
#define TWO_PI 6.28318530718

float CircleMask(float2 uv, float radius, float blur)
{
    float distanceFromCenter = length(uv - float2(0.5, 0.5));
    return 1.0 - smoothstep(radius - blur, radius + blur, distanceFromCenter);
}

float RingMask(float2 uv, float radius, float thickness, float blur)
{
    float distanceFromCenter = length(uv - float2(0.5, 0.5));

    float outer = 1.0 - smoothstep(radius - blur, radius + blur, distanceFromCenter);
    float inner = 1.0 - smoothstep(radius - thickness - blur, radius - thickness + blur, distanceFromCenter);

    return saturate(outer - inner);
}

float GetAngle(float2 uv)
{
    float2 center = uv - float2(0.5, 0.5);
    float angle = atan2(center.x, center.y);

    if (angle < 0.0)
        angle += TWO_PI;

    return angle / TWO_PI;
}

float GetGaugeMask(float2 uv, float progress)
{
    return step(GetAngle(uv), progress);
}

float4 GetGaugeColor(float angle)
{
    float safeProgress = max(Progress, 0.0001);
    float colorProgress = saturate(angle / safeProgress);

    if (colorProgress < Color2Position)
    {
        float t = smoothstep(Color1Position, Color2Position, colorProgress);
        return lerp(Color1, Color2, t);
    }

    float t2 = smoothstep(Color2Position, Color3Position, colorProgress);
    return lerp(Color2, Color3, t2);
}

float2 PixelateUV(float2 uv)
{
    return floor(uv * PIXEL_GRID + 0.5) / PIXEL_GRID;
}

float4 GetCenterImage(float2 uv, out float iconCoverage)
{
    float2 centeredUV = (uv - float2(0.5, 0.5)) / ImageScale;
    float2 imageUV = centeredUV + float2(0.5, 0.5);

    float imageBoundsMask = step(0.0, imageUV.x) * step(0.0, imageUV.y) * step(imageUV.x, 1.0) * step(imageUV.y, 1.0);

    float4 image = tex2D(uImage1, imageUV);
    iconCoverage = image.a * imageBoundsMask;
    image.a *= imageBoundsMask * ImageAlpha;

    return image;
}

float4 CircularGaugePS(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    uv = PixelateUV(uv);
    float2 shapeUV = float2(uv.x, 1.0 - uv.y);

    float backgroundMask = CircleMask(shapeUV, BACKGROUND_RADIUS, BACKGROUND_BLUR);

    float ringMask = RingMask(shapeUV, GAUGE_RADIUS, GAUGE_THICKNESS, GAUGE_BLUR);
    float angle = GetAngle(shapeUV);
    float gaugeProgressMask = GetGaugeMask(shapeUV, Progress);
    float gaugeMask = ringMask * gaugeProgressMask;

    float4 gaugeColor = GetGaugeColor(angle);

    float4 baseResult = lerp(BackgroundColor * backgroundMask, gaugeColor, gaugeMask);
    float backgroundAlpha = backgroundMask * BackgroundColor.a;
    float gaugeAlpha = gaugeMask * gaugeColor.a;

    baseResult.a = max(backgroundAlpha, gaugeAlpha);

    baseResult.rgb *= color.rgb;
    baseResult.a *= color.a * GlobalAlpha;

    float iconCoverage;
    float4 centerImage = GetCenterImage(uv, iconCoverage);
    float finalCenterAlpha = centerImage.a;

    // Cut the gauge out using the icon's source alpha, independent of ImageAlpha.
    // Transparent icon pixels keep the gauge, while opaque source pixels never show it through.
    baseResult.rgb *= 1.0 - iconCoverage;
    baseResult.a *= 1.0 - iconCoverage;

    float4 finalResult = lerp(baseResult, centerImage, finalCenterAlpha);
    finalResult.a = max(baseResult.a, finalCenterAlpha);

    return finalResult;
}

technique CircularGauge
{
    pass CircularGaugePass
    {
        PixelShader = compile ps_2_a CircularGaugePS();
    }
}

